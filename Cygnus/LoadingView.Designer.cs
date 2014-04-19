// <copyright file="LoadingView.Designer.cs">
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
    public partial class LoadingView
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param Name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }

            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LoadingView));
            this.label = new System.Windows.Forms.Label();
            this.pboxSpinner = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.pboxSpinner)).BeginInit();
            this.SuspendLayout();
            // 
            // Label
            // 
            this.label.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.label.ForeColor = System.Drawing.Color.White;
            this.label.Location = new System.Drawing.Point(0, 53);
            this.label.Name = "Label";
            this.label.Size = new System.Drawing.Size(135, 18);
            this.label.TabIndex = 1;
            this.label.Text = "Reloading Data";
            this.label.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // pboxSpinner
            // 
            this.pboxSpinner.BackColor = System.Drawing.Color.Black;
            this.pboxSpinner.Image = global::Cygnus.Properties.Resources.spinner;
            this.pboxSpinner.Location = new System.Drawing.Point(49, 12);
            this.pboxSpinner.Name = "pboxSpinner";
            this.pboxSpinner.Size = new System.Drawing.Size(36, 36);
            this.pboxSpinner.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pboxSpinner.TabIndex = 2;
            this.pboxSpinner.TabStop = false;
            // 
            // LoadingView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.ClientSize = new System.Drawing.Size(135, 86);
            this.Controls.Add(this.pboxSpinner);
            this.Controls.Add(this.label);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "LoadingView";
            this.Opacity = 0.8D;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "LoadingView";
            ((System.ComponentModel.ISupportInitialize)(this.pboxSpinner)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pboxSpinner;
        public System.Windows.Forms.Label label;
    }
}
