// This file is part of BitCollectors Plink Service.
// Copyright 2013 Adam Plocher (BitCollectors)
// 
// BitCollectors Plink Service is free software: you can redistribute it and/or 
// modify it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// BitCollectors Plink Service is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
// or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for 
// more details.
// 
// You should have received a copy of the GNU General Public License along with 
// BitCollectors Plink Service.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using BitCollectors.PlinkService.Remoting;
using BitCollectors.PlinkService.Remoting.Entities;
using BitCollectors.PlinkService.Remoting.Enums;
using BitCollectors.PlinkService.Remoting.Helpers;
using BitCollectors.PlinkService.UI.Properties;
using BitCollectors.PlinkService.UI.Remoting;

namespace BitCollectors.PlinkService.UI
{
    public partial class MainForm : Form
    {
        #region Fields
        private System.Timers.Timer _pingTimer;
        private DateTime _lastTunnelValidationPingTime = DateTime.MinValue;
        private ServiceSettings _currentSettings = null;
        private bool _toolTipShowing = false;
        #endregion

        #region Constructors
        public MainForm()
        {
            InitializeComponent();

            lblAboutName.Text = "BitCollectors Plink Service (v." + Assembly.GetExecutingAssembly().GetName().Version + ")";
        }
        #endregion

        #region Properties
        public bool ApplicationExiting
        {
            get;
            set;
        }
        #endregion

        #region Private Methods
        private void AppendLog(string text)
        {
            txtLog.Invoke(new MethodInvoker(() =>
            {
                txtLog.Text += text + Environment.NewLine;
                ScrollLogDown();
            }));
        }

        private void AppendLog(string titleText, string eventText)
        {
            AppendLog(string.Format("{0} - {1} - {2}", titleText, DateTime.Now, eventText));
        }

        private void AppendPlinkLog(string text)
        {
            txtLog.Invoke(new MethodInvoker(() =>
            {
                txtPlinkLog.Text += text + Environment.NewLine;
                ScrollPlinkLogDown();
            }));
        }

        private void AppendPlinkLog(string titleText, string eventText, DateTime eventTime)
        {
            AppendPlinkLog(string.Format("{0} - {1} - {2}", titleText, DateTime.Now, eventText));
        }

        private void btnBrowsePlink_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "plink.exe|plink.exe";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    txtPathToPlink.Text = openFileDialog.FileName;
                }
            }
        }

        private void btnChangeTunnelStatus_Click(object sender, EventArgs e)
        {
            if (btnChangeTunnelStatus.Text == "Start Tunnel" && (string.IsNullOrWhiteSpace(_currentSettings.PlinkExecutable) || string.IsNullOrWhiteSpace(_currentSettings.SshHostname) || string.IsNullOrWhiteSpace(_currentSettings.SshUsername) || _currentSettings.SshPort == 0))
            {
                MessageBox.Show("Before you can start the tunnel you must first fill in your SSH connection information on the Settings tab", "Connection info missing", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                if (btnChangeTunnelStatus.Text == "Start Tunnel" && _currentSettings.IsDirty)
                {
                    DialogResult dialogResult = MessageBox.Show("You have unsaved changes.  Would you like to save them before starting the tunnel?", "Save changes?", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                    if (dialogResult == DialogResult.Yes)
                    {
                        bool saveSuccess = Save();

                        if (!saveSuccess)
                        {
                            return;
                        }
                    }
                    else if (dialogResult == DialogResult.Cancel)
                    {
                        return;
                    }
                }

                btnChangeTunnelStatus.Enabled = false;

                if (btnChangeTunnelStatus.Text == "Stop Tunnel")
                {
                    WcfClientHelper.ExecuteRemoteAction(x => x.ChangeTunnelStatus(TunnelStatuses.Paused), true);
                }
                else if (btnChangeTunnelStatus.Text == "Start Tunnel")
                {
                    WcfClientHelper.ExecuteRemoteAction(x => x.ChangeTunnelStatus(TunnelStatuses.Started), true);
                }
            }
        }

        private void btnClearLog_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to clear all the text from the log?", "Are you sure?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) ==
                DialogResult.Yes)
            {
                txtLog.Text = string.Empty;
            }
        }

        private void btnCopyLog_Click(object sender, EventArgs e)
        {
            txtLog.SelectAll();
            txtLog.Copy();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            Save();
        }

        private bool Save()
        {
            if (this.ValidateChildren())
            {
                AppendLog("Client", "Saving settings");
                btnSave.Enabled = false;

                bool restartPlink = false;

                if (btnChangeTunnelStatus.Text == "Stop Tunnel")
                {
                    restartPlink = (MessageBox.Show("Would you like to reset plink.exe with the settings you specified?  If no, it will use the settings next time Plink.exe gets recycled or when you restart the service", "Restart plink.exe?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) ==
                        DialogResult.Yes);
                }

                WcfClientHelper.ExecuteRemoteAction(x => x.SaveSettings(_currentSettings, restartPlink), true);

                return true;
            }
            else
            {
                MessageBox.Show("Validation errors exist.  Please verify that you have entered valid information into each form field.", "Form validation error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                return false;
            }
        }

        private void btnTestSshConnection_Click(object sender, EventArgs e)
        {
            picLoadingTestConnection.Visible = true;
            btnTestSshConnection.Enabled = false;

            Task.Factory.StartNew(() =>
            {
                int sshPort = 22;
                int.TryParse(txtSshPort.Text, out sshPort);

                WcfClientHelper.ExecuteRemoteAction(x => x.TestConnection(new ServiceSettings() { PlinkExecutable = txtPathToPlink.Text, SshHostname = txtSshHost.Text, SshPort = sshPort, SshUsername = txtSshUsername.Text, SshPassword = txtSshPassword.Text }));
            });
        }

        private void CrossThreadMessageBox(string text, string caption = null, MessageBoxButtons messageBoxButtons = MessageBoxButtons.OK, MessageBoxIcon messageBoxIcon = MessageBoxIcon.None)
        {
            this.Invoke(new MethodInvoker(() => MessageBox.Show(text, caption, messageBoxButtons, messageBoxIcon)));
        }

        private void Disconnect(bool unregister = true)
        {
            try
            {
                this.Invoke(new MethodInvoker(() =>
                    {
                        this.Cursor = Cursors.WaitCursor;
                    }));

                WcfClientHelper.DisconnectIpc(unregister);
            }
            catch
            {
            }
            finally
            {
                _pingTimer.Stop();

                this.Invoke(new MethodInvoker(() =>
                    {
                        this.Cursor = Cursors.Default;
                    }));
            }
        }

        private void HideLoadingTestConnection()
        {
            this.Invoke(new MethodInvoker(() =>
                {
                    picLoadingTestConnection.Visible = false;
                    btnTestSshConnection.Enabled = true;
                }));
        }

        private bool IsPortValid(string portString)
        {
            int portInt;

            return
                portString.Length > 0 &&
                int.TryParse(portString, out portInt) &&
                portInt >= 1 &&
                portInt <= 65535;
        }

        private void lnkBitCollectors_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            OpenUrl("http://www.bitcollectors.com");
        }

        private void lnkBlog_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            OpenUrl("http://aplocher.wordpress.com");
        }

        private void lnkPlink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            OpenUrl("http://www.chiark.greenend.org.uk/~sgtatham/putty/download.html");
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Disconnect();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            this.ApplicationExiting = true;

            ServiceRemotingCallback.ClearAllEventHandlers();

            ServiceRemotingCallback.ServerStoppingEvent += ServiceRemotingCallback_ServerStoppingEvent;
            ServiceRemotingCallback.EventToLogEvent += ServiceRemotingCallback_EventToLogEvent;
            ServiceRemotingCallback.PingEvent += ServiceRemotingCallback_PingEvent;
            ServiceRemotingCallback.TestConnectionCallbackEvent += ServiceRemotingCallback_TestConnectionCallbackEvent;
            ServiceRemotingCallback.TunnelStatusChangedEvent += ServiceRemotingCallback_TunnelStatusChangedEvent;
            ServiceRemotingCallback.PlinkTextOutputEvent += ServiceRemotingCallback_PlinkTextOutputEvent;
            ServiceRemotingCallback.SaveSettingsCallbackEvent += ServiceRemotingCallback_SaveSettingsCallbackEvent;

            AppendLog("Client", "UI started - connected to Windows Service");
            _currentSettings = WcfClientHelper.RemotingObject.GetSettings();
            _currentSettings.IsDirty = false;
            bindingSource1.DataSource = _currentSettings;

            AppendLog("Client", "Windows Service settings retrieved and bound");

            WcfClientHelper.RemotingObject.RegisterCallbackClient();

            _pingTimer = new System.Timers.Timer(Constants.ClientPingInterval * 1000);
            _pingTimer.Elapsed += pingTimer_Elapsed;
            _pingTimer.AutoReset = true;
            _pingTimer.Start();
        }

        private void OpenUrl(string url)
        {
            Process process = new Process();
            process.StartInfo.FileName = "explorer.exe";
            process.StartInfo.Arguments = url;
            process.Start();
        }

        private void ServiceRemotingCallback_SaveSettingsCallbackEvent(object sender, ServiceSettings serviceSettings)
        {
            btnSave.Invoke(new MethodInvoker(() =>
                {
                    btnSave.Enabled = true;

                    _currentSettings = serviceSettings;
                    _currentSettings.IsDirty = false;

                    bindingSource1.DataSource = _currentSettings;

                    AppendLog("Client", "Settings have been saved to service");
                }));
        }

        private void ServiceRemotingCallback_PlinkTextOutputEvent(object sender, string eventText, DateTime eventTime)
        {
            AppendPlinkLog("Plink.exe", eventText, eventTime);
        }

        private void pingTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (this.IsDisposed)
                return;

            WcfClientHelper.ExecuteRemoteAction(x => x.Ping(), true);

            if ((DateTime.Now - ServiceRemotingCallback.LastPingDateTime).TotalSeconds > Constants.ClientPingTimeout)
            {
                AppendLog("Client", "Re-Register Client Callback");
                WcfClientHelper.ExecuteRemoteAction(x => x.RegisterCallbackClient(), true);
            }
        }

        private void ScrollLogDown()
        {
            txtLog.SelectionStart = txtLog.Text.Length;
            txtLog.ScrollToCaret();
        }

        private void ScrollPlinkLogDown()
        {
            txtPlinkLog.SelectionStart = txtPlinkLog.Text.Length;
            txtPlinkLog.ScrollToCaret();
        }

        private void ServiceRemotingCallback_EventToLogEvent(object sender, string eventText, DateTime eventTime)
        {
            AppendLog("Service", eventText);
        }

        private void ServiceRemotingCallback_PingEvent(object sender, bool plinkRunning, bool tunnelValidationRunning, DateTime tunnelValidationLastRan)
        {
            this.Invoke(new MethodInvoker(() =>
                {
                    if (plinkRunning)
                    {
                        picPlinkStatus.Image = Resources.tick;
                        toolTip1.SetToolTip(picPlinkStatus, "Plink.exe is running");
                    }
                    else
                    {
                        picPlinkStatus.Image = Resources.exclamation;
                        toolTip1.SetToolTip(picPlinkStatus, "Plink.exe is not running");
                    }

                    if (tunnelValidationRunning)
                    {
                        picTunnelValStatus.Image = Resources.tick;
                        _lastTunnelValidationPingTime = tunnelValidationLastRan;
                    }
                    else
                    {
                        picTunnelValStatus.Image = Resources.exclamation;
                        _lastTunnelValidationPingTime = DateTime.MinValue;
                    }
                }));
        }

        private void ServiceRemotingCallback_ServerStoppingEvent(object sender, EventArgs eventArgs)
        {
            ApplicationExiting = false;

            this.Disconnect(false);

            CrossThreadMessageBox("Server has been stopped.  Client has been disconnected.", "Service Stopped", MessageBoxButtons.OK, MessageBoxIcon.Error);

            this.Invoke(new MethodInvoker(Close));
        }

        private void ServiceRemotingCallback_TestConnectionCallbackEvent(object sender, PlinkStatus plinkStatus)
        {
            HideLoadingTestConnection();

            string messageText = "";
            MessageBoxIcon messageBoxIcon = MessageBoxIcon.Warning;

            switch (plinkStatus)
            {
                case PlinkStatus.Success:
                    messageText = "Success!";
                    messageBoxIcon = MessageBoxIcon.Information;
                    break;

                case PlinkStatus.ExecutableNotFound:
                    messageText = "Plink.exe was not found";
                    break;

                case PlinkStatus.ExecutableNotRecognized:
                    messageText = "Plink.exe executable was not recognized";
                    break;

                case PlinkStatus.InvalidHostname:
                    messageText = "Could not connect. This appears to be an invalid hostname or IP";
                    break;

                case PlinkStatus.InvalidPort:
                    messageText = "Could not connect. This appears to be an invalid port";
                    break;

                case PlinkStatus.InvalidUserOrPass:
                    messageText = "Could not connect. Invalid username or password";
                    break;

                case PlinkStatus.TimedOut:
                    messageText = "Connection timed out. This could be due to an invalid hostname or port. Check your internet connection and SSH server settings and try again";
                    break;

                default:
                    messageText = "Unknown error";
                    break;
            }

            CrossThreadMessageBox(messageText, "Test connection status", MessageBoxButtons.OK, messageBoxIcon);
        }

        private void ServiceRemotingCallback_TunnelStatusChangedEvent(object sender, TunnelStatuses tunnelStatus)
        {
            btnChangeTunnelStatus.Invoke(new MethodInvoker(() =>
            {
                if (tunnelStatus == TunnelStatuses.Started)
                {
                    btnChangeTunnelStatus.Image = Resources.control_pause_blue;
                    btnChangeTunnelStatus.Text = "Stop Tunnel";
                }
                else if (tunnelStatus == TunnelStatuses.Paused)
                {
                    btnChangeTunnelStatus.Image = Resources.control_play_blue;
                    btnChangeTunnelStatus.Text = "Start Tunnel";
                }

                btnChangeTunnelStatus.Enabled = true;
            }));
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControl1.SelectedIndex == 0)
            {
                if (tabControl2.SelectedIndex == 0)
                {
                    ScrollLogDown();
                }
                else if (tabControl2.SelectedIndex == 1)
                {
                    ScrollPlinkLogDown();
                }
            }
        }

        private void txtTunnelValidationLocalPort_Validating(object sender, CancelEventArgs e)
        {
            if (chkEnableTunnelValidation.Checked)
            {
                if (txtTunnelValidationLocalPort.Text.Trim().Length == 0)
                {
                    errorProvider1.SetError(txtTunnelValidationLocalPort, "Port is required when using tunnel validation");
                    e.Cancel = true;
                }
                else if (!IsPortValid(txtTunnelValidationLocalPort.Text))
                {
                    errorProvider1.SetError(txtTunnelValidationLocalPort, Constants.ValidationPortInvalid);
                    e.Cancel = true;
                }
                else if (txtTunnelValidationLocalPort.Text == txtTunnelValidationRemotePort.Text)
                {
                    errorProvider1.SetError(txtTunnelValidationLocalPort, "Local and remote ports must be different");
                    e.Cancel = true;
                }
                else
                {
                    errorProvider1.SetError(txtTunnelValidationLocalPort, string.Empty);
                    e.Cancel = false;
                }
            }
            else
            {
                errorProvider1.SetError(txtTunnelValidationLocalPort, string.Empty);
                e.Cancel = false;
            }
        }

        private void toolTip1_Popup(object sender, PopupEventArgs e)
        {
            if (!_toolTipShowing)
            {
                _toolTipShowing = true;

                switch (e.AssociatedControl.Name)
                {
                    case "picTunnelValStatus":
                        if (_lastTunnelValidationPingTime == DateTime.MinValue)
                        {
                            toolTip1.SetToolTip(picTunnelValStatus, "Tunnel Validator is not running");
                        }
                        else
                        {
                            toolTip1.SetToolTip(picTunnelValStatus, string.Format("Tunnel Validation is running. Last successful ping: {0} secs ago", Math.Floor((DateTime.Now - _lastTunnelValidationPingTime).TotalSeconds)));
                        }
                        break;
                }

                _toolTipShowing = false;
            }
        }

        private void txtPathToPlink_Validating(object sender, CancelEventArgs e)
        {
            if (txtPathToPlink.Text.Trim().Length == 0)
            {
                errorProvider1.SetError(txtPathToPlink, "Plink.exe executable is required");
                e.Cancel = true;
            }
            else
            {
                errorProvider1.SetError(txtPathToPlink, string.Empty);
                e.Cancel = false;
            }
        }

        private void txtSshHost_Validating(object sender, CancelEventArgs e)
        {
            if (txtSshHost.Text.Trim().Length == 0)
            {
                errorProvider1.SetError(txtSshHost, "Hostname/IP is required");
                e.Cancel = true;
            }
            else
            {
                errorProvider1.SetError(txtSshHost, string.Empty);
                e.Cancel = false;
            }
        }

        private void txtSshPort_Validating(object sender, CancelEventArgs e)
        {
            if (txtSshPort.Text.Trim().Length == 0)
            {
                errorProvider1.SetError(txtSshPort, "Port is required");
                e.Cancel = true;
            }
            else if (!IsPortValid(txtSshPort.Text))
            {
                errorProvider1.SetError(txtSshPort, Constants.ValidationPortInvalid);
                e.Cancel = true;
            }
            else
            {
                errorProvider1.SetError(txtSshPort, string.Empty);
                e.Cancel = false;
            }
        }

        private void txtTunnelValidationRemotePort_Validating(object sender, CancelEventArgs e)
        {
            if (chkEnableTunnelValidation.Checked)
            {
                if (txtTunnelValidationRemotePort.Text.Trim().Length == 0)
                {
                    errorProvider1.SetError(txtTunnelValidationRemotePort, "Port is required when using tunnel validation");
                    e.Cancel = true;
                }
                else if (!IsPortValid(txtTunnelValidationRemotePort.Text))
                {
                    errorProvider1.SetError(txtTunnelValidationRemotePort, Constants.ValidationPortInvalid);
                    e.Cancel = true;
                }
                else if (txtTunnelValidationLocalPort.Text == txtTunnelValidationRemotePort.Text)
                {
                    errorProvider1.SetError(txtTunnelValidationRemotePort, "Local and remote ports must be different");
                    e.Cancel = true;
                }
                else
                {
                    errorProvider1.SetError(txtTunnelValidationRemotePort, string.Empty);
                    e.Cancel = false;
                }
            }
            else
            {
                errorProvider1.SetError(txtTunnelValidationRemotePort, string.Empty);
                e.Cancel = false;
            }
        }

        private void txtTunnelValidationPingInterval_Validating(object sender, CancelEventArgs e)
        {
            if (chkEnableTunnelValidation.Checked)
            {
                int pingInterval;

                if (txtTunnelValidationPingInterval.Text.Trim().Length == 0)
                {
                    errorProvider1.SetError(txtTunnelValidationPingInterval, "Ping interval is required");
                    e.Cancel = true;
                }
                else if (int.TryParse(txtTunnelValidationPingInterval.Text, out pingInterval))
                {
                    if (pingInterval < 5)
                    {
                        errorProvider1.SetError(txtTunnelValidationPingInterval, "Ping interval should not be less than 5 seconds");
                        e.Cancel = true;
                    }
                    else if (pingInterval > 7200)
                    {
                        errorProvider1.SetError(txtTunnelValidationPingInterval, "Ping interval should not be more than 2 hours");
                        e.Cancel = true;
                    }
                    else
                    {
                        errorProvider1.SetError(txtTunnelValidationPingInterval, string.Empty);
                        e.Cancel = false;
                    }
                }
                else
                {
                    errorProvider1.SetError(txtTunnelValidationPingInterval, "Ping interval must be numeric");
                    e.Cancel = true;
                }
            }
            else
            {
                errorProvider1.SetError(txtTunnelValidationPingInterval, string.Empty);
                e.Cancel = false;
            }
        }

        private void chkEnableSocks_Validating(object sender, CancelEventArgs e)
        {
            if (!chkEnableSocks.Checked)
            {
                errorProvider1.SetError(txtSocksPort, string.Empty);
            }
        }

        private void chkEnableTunnelValidation_Validating(object sender, CancelEventArgs e)
        {
            if (!chkEnableTunnelValidation.Checked)
            {
                errorProvider1.SetError(txtTunnelValidationLocalPort, string.Empty);
                errorProvider1.SetError(txtTunnelValidationRemotePort, string.Empty);
                errorProvider1.SetError(txtTunnelValidationPingInterval, string.Empty);
            }
        }

        private void tabControl2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControl1.SelectedIndex == 0)
            {
                if (tabControl2.SelectedIndex == 0)
                {
                    ScrollLogDown();
                }
                else if (tabControl2.SelectedIndex == 1)
                {
                    ScrollPlinkLogDown();
                }
            }
        }

        private void btnClearPlinkLog_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to clear all the text from the log?", "Are you sure?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) ==
                DialogResult.Yes)
            {
                txtPlinkLog.Text = string.Empty;
            }
        }

        private void btnCopyPlinkLog_Click(object sender, EventArgs e)
        {
            txtPlinkLog.SelectAll();
            txtPlinkLog.Copy();
        }

        private void txtSocksPort_Validating(object sender, CancelEventArgs e)
        {
            if (chkEnableSocks.Checked)
            {
                if (txtSocksPort.Text.Trim().Length == 0)
                {
                    errorProvider1.SetError(txtSocksPort, "Port is required when using SOCKS");
                    e.Cancel = true;
                }
                else if (!IsPortValid(txtSocksPort.Text))
                {
                    errorProvider1.SetError(txtSocksPort, Constants.ValidationPortInvalid);
                    e.Cancel = true;
                }
                else
                {
                    errorProvider1.SetError(txtSocksPort, string.Empty);
                    e.Cancel = false;
                }
            }
            else
            {
                errorProvider1.SetError(txtSocksPort, string.Empty);
                e.Cancel = false;
            }
        }
        #endregion
    }
}
