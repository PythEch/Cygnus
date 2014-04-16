// Cygnus - Cydia-like APT Client for Windows
// Copyright (C) 2014  PythEch
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Windows.Forms;

namespace Cygnus
{
    partial class AlertView : Form
    {
        //Credits: http://stackoverflow.com/questions/10674228/form-with-rounded-borders-in-c

        [System.Runtime.InteropServices.DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn
        (
            int nLeftRect, // x-coordinate of upper-left corner
            int nTopRect, // y-coordinate of upper-left corner
            int nRightRect, // x-coordinate of lower-right corner
            int nBottomRect, // y-coordinate of lower-right corner
            int nWidthEllipse, // height of ellipse
            int nHeightEllipse // width of ellipse
        );
        public AlertView()
        {
            InitializeComponent();
            Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, Width, Height, 15, 15));
        }

        public bool canceled = false;
        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (Uri.IsWellFormedUriString(txtSource.Text, UriKind.Absolute))
            {
                canceled = false;
                this.Close();
            }
            else
            {
                errorProvider1.Clear();
                errorProvider1.SetError(txtSource, "URL is not valid!");
            }
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (Uri.IsWellFormedUriString(txtSource.Text, UriKind.Absolute))
            {
                canceled = false;
                this.Close();
            }
            else
            {
                errorProvider1.Clear();
                errorProvider1.SetError(txtSource, "URL is not valid!");
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            canceled = true;
            this.Close();
        }

        private void txtSource_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (btnAdd.Visible)
                    btnAdd_Click(null, null);
                else
                    btnEdit_Click(null, null);
            }
            else if (e.KeyCode == Keys.Escape)
                btnCancel_Click(null, null);
        }

    }
}
