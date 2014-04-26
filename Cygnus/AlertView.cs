// <copyright file="AlertView.cs">
//    Copyright (C) 2014  PythEch
//
//    This program is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either Version 3 of the License, or
//    (at your option) any later Version.
//
//    This program is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU General Public License for more details.
//
//    You should have received a copy of the GNU General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// <summary>Cygnus - Cydia-like APT Client for Windows</summary>

namespace Cygnus
{
    using System;
    using System.Windows.Forms;

    public partial class AlertView : Form
    {
        #region Fields

        /// <summary>
        /// The boolean that indicates if user clicked Cancel button.
        /// </summary>
        public bool Canceled = false;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="AlertView"/> class.
        /// </summary>
        public AlertView()
        {
            this.InitializeComponent();
            this.Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, this.Width, this.Height, 15, 15));
        }

        #endregion Constructors

        #region Methods

        //Credits: http://stackoverflow.com/questions/10674228/form-with-rounded-borders-in-c
        [System.Runtime.InteropServices.DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn(
            int nLeftRect, // x-coordinate of upper-left corner
            int nTopRect, // y-coordinate of upper-left corner
            int nRightRect, // x-coordinate of lower-right corner
            int nBottomRect, // y-coordinate of lower-right corner
            int nWidthEllipse, // height of ellipse
            int nHeightEllipse // width of ellipse
            );

        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (Uri.IsWellFormedUriString(this.txtSource.Text, UriKind.Absolute))
            {
                this.Canceled = false;
                this.Close();
            }
            else
            {
                errorProvider1.Clear();
                errorProvider1.SetError(this.txtSource, "URL is not valid!");
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Canceled = true;
            this.Close();
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            btnAdd_Click(null, null);
            // I want to do this explictly,
            // yes I do know I could point Event handler
            // to btnAdd_Click from the designer.
        }

        private void txtSource_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (this.btnAdd.Visible)
                    this.btnAdd_Click(null, null);
                else
                    this.btnEdit_Click(null, null);
            }
            else if (e.KeyCode == Keys.Escape)
                this.btnCancel_Click(null, null);
        }

        #endregion Methods
    }
}
