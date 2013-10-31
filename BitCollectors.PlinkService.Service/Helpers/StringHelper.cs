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
using System.Collections.Specialized;
using System.Text.RegularExpressions;

namespace BitCollectors.PlinkService.Service.Helpers
{
    public static class StringHelper
    {
        private static readonly Regex validateTunnelString = new Regex(@"^[A-Za-z0-9_\-\:\.]+$");

        public static string[] ParseStringLines(string text, string commentIndicator = "#")
        {
            string[] stringLines = text.Split('\n');
            StringCollection stringCollection = new StringCollection();

            foreach (string line in stringLines)
            {
                string newLine = line.Trim();

                if (line.Contains(commentIndicator))
                {
                    string[] splitLine = line.Split(new[] { commentIndicator }, StringSplitOptions.None);
                    newLine = splitLine[0];
                }

                if (newLine.Trim().Length > 0 && validateTunnelString.IsMatch(newLine.Trim()))
                {
                    stringCollection.Add(newLine.Trim());
                }
            }

            string[] returnValue = new string[stringCollection.Count];

            stringCollection.CopyTo(returnValue, 0);

            return returnValue;
        }
    }
}
