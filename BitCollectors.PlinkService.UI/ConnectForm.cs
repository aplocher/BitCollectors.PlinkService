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
using System.Drawing;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Windows.Forms;
using BitCollectors.PlinkService.Remoting.Helpers;
using BitCollectors.PlinkService.UI.Controls;
using BitCollectors.PlinkService.UI.Remoting;

namespace BitCollectors.PlinkService.UI
{
    public partial class ConnectForm : Form
    {
        private MainForm _mainForm = null;
        private LoadingControl _loadingControl = null;

        public ConnectForm()
        {
            InitializeComponent();
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;

            WcfClientHelper.FaultMessage = null;

            if (_loadingControl == null)
            {
                _loadingControl = new LoadingControl();

                int loadingControlX = (this.Width / 2) - (_loadingControl.Width / 2);
                int loadingControlY = ((this.Height / 2) - (_loadingControl.Height / 2)) - 20;
                _loadingControl.Location = new Point(loadingControlX, loadingControlY);

                this.Controls.Add(_loadingControl);
            }

            _loadingControl.BringToFront();
            _loadingControl.Visible = true;

            Task connectingTask = Task.Factory.StartNew<bool>(() =>
            {
                ServiceRemotingCallback callback = new ServiceRemotingCallback();

                WcfClientHelper.ConnectIpc(callback);
                ((ICommunicationObject)WcfClientHelper.PipeFactory).Closing += PipeFactory_Closing;
                ((ICommunicationObject)WcfClientHelper.PipeFactory).Faulted += PipeFactory_Faulted;

                return WcfClientHelper.IsConnected;
            }).ContinueWith(x =>
            {
                HandleConnectionResult(x.Result);
            });
        }

        private void HandleConnectionResult(bool connected)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new MethodInvoker(() => HandleConnectionResult(connected)));
            }
            else
            {
                this.Cursor = Cursors.Default;

                _loadingControl.SendToBack();
                _loadingControl.Visible = false;

                if (connected)
                {
                    this.Hide();

                    _mainForm = new MainForm();
                    _mainForm.FormClosed += mainForm_FormClosed;
                    _mainForm.ShowDialog();
                }
                else
                {
                    MessageBox.Show("Can't connect to Windows Service.  Verify that it's running.", "Cannot connect", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void mainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (_mainForm.ApplicationExiting)
            {
                this.Close();
            }
            else
            {
                this.Show();
            }
        }

        private void PipeFactory_Faulted(object sender, EventArgs e)
        {
            MessageBox.Show("PipeFactory_Faulted");
        }

        private void PipeFactory_Closing(object sender, EventArgs e)
        {
            this.Invoke(new MethodInvoker(() =>
                {
                    if (_mainForm != null && !_mainForm.IsDisposed && _mainForm.Visible)
                    {
                        if (!string.IsNullOrEmpty(WcfClientHelper.FaultMessage))
                            _mainForm.ApplicationExiting = false;

                        _mainForm.Close();
                    }

                    if (!string.IsNullOrEmpty(WcfClientHelper.FaultMessage))
                    {
                        MessageBox.Show("Connection to Windows Service faulted.  Closing user interface.  If the issue persists, restart the Windows Service." + Environment.NewLine + Environment.NewLine + WcfClientHelper.FaultMessage, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }));
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
