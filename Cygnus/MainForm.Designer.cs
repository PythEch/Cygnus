// <copyright file="MainForm.Designer.cs">
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
    public partial class MainForm
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
            this.components = new System.ComponentModel.Container();
            XPTable.Models.DataSourceColumnBinder dataSourceColumnBinder1 = new XPTable.Models.DataSourceColumnBinder();
            XPTable.Renderers.DragDropRenderer dragDropRenderer1 = new XPTable.Renderers.DragDropRenderer();
            XPTable.Models.DataSourceColumnBinder dataSourceColumnBinder2 = new XPTable.Models.DataSourceColumnBinder();
            XPTable.Renderers.DragDropRenderer dragDropRenderer2 = new XPTable.Renderers.DragDropRenderer();
            XPTable.Models.DataSourceColumnBinder dataSourceColumnBinder3 = new XPTable.Models.DataSourceColumnBinder();
            XPTable.Renderers.DragDropRenderer dragDropRenderer3 = new XPTable.Renderers.DragDropRenderer();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.tabMain = new System.Windows.Forms.TabControl();
            this.pageSource = new System.Windows.Forms.TabPage();
            this.btnReload = new System.Windows.Forms.Button();
            this.listSources = new System.Windows.Forms.ListView();
            this.colURL = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colPackages = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.btnEdit = new System.Windows.Forms.Button();
            this.btnDelete = new System.Windows.Forms.Button();
            this.btnAdd = new System.Windows.Forms.Button();
            this.pagePackages = new System.Windows.Forms.TabPage();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.tablePackages = new XPTable.Models.Table();
            this.webBrowser1 = new System.Windows.Forms.WebBrowser();
            this.btnSearch = new System.Windows.Forms.Button();
            this.txtSearch = new System.Windows.Forms.TextBox();
            this.pageQueue = new System.Windows.Forms.TabPage();
            this.tableQueue = new XPTable.Models.Table();
            this.pageChanges = new System.Windows.Forms.TabPage();
            this.tableChanges = new XPTable.Models.Table();
            this.trackBarZoom = new System.Windows.Forms.TrackBar();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.statusBarProgressbar = new System.Windows.Forms.ToolStripProgressBar();
            this.statusLabelDownload = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.statusLabelZoom = new System.Windows.Forms.ToolStripStatusLabel();
            this.contextMenuQueue = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.deleteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openFolderToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripDropDownButton1 = new System.Windows.Forms.ToolStripDropDownButton();
            this.tabMain.SuspendLayout();
            this.pageSource.SuspendLayout();
            this.pagePackages.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tablePackages)).BeginInit();
            this.pageQueue.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tableQueue)).BeginInit();
            this.pageChanges.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tableChanges)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarZoom)).BeginInit();
            this.statusStrip.SuspendLayout();
            this.contextMenuQueue.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabMain
            // 
            this.tabMain.Controls.Add(this.pageSource);
            this.tabMain.Controls.Add(this.pagePackages);
            this.tabMain.Controls.Add(this.pageQueue);
            this.tabMain.Controls.Add(this.pageChanges);
            this.tabMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabMain.Location = new System.Drawing.Point(0, 0);
            this.tabMain.Name = "tabMain";
            this.tabMain.SelectedIndex = 0;
            this.tabMain.Size = new System.Drawing.Size(500, 348);
            this.tabMain.TabIndex = 0;
            this.tabMain.SelectedIndexChanged += new System.EventHandler(this.tabMain_SelectedIndexChanged);
            // 
            // pageSource
            // 
            this.pageSource.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(254)))), ((int)(((byte)(254)))), ((int)(((byte)(254)))));
            this.pageSource.Controls.Add(this.btnReload);
            this.pageSource.Controls.Add(this.listSources);
            this.pageSource.Controls.Add(this.btnEdit);
            this.pageSource.Controls.Add(this.btnDelete);
            this.pageSource.Controls.Add(this.btnAdd);
            this.pageSource.Location = new System.Drawing.Point(4, 22);
            this.pageSource.Name = "pageSource";
            this.pageSource.Padding = new System.Windows.Forms.Padding(3);
            this.pageSource.Size = new System.Drawing.Size(492, 322);
            this.pageSource.TabIndex = 0;
            this.pageSource.Text = "Sources";
            // 
            // btnReload
            // 
            this.btnReload.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnReload.Location = new System.Drawing.Point(411, 93);
            this.btnReload.Name = "btnReload";
            this.btnReload.Size = new System.Drawing.Size(75, 23);
            this.btnReload.TabIndex = 4;
            this.btnReload.Text = "&Reload";
            this.btnReload.UseVisualStyleBackColor = true;
            this.btnReload.Click += new System.EventHandler(this.btnReload_Click);
            // 
            // listSources
            // 
            this.listSources.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listSources.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colURL,
            this.colPackages});
            this.listSources.FullRowSelect = true;
            this.listSources.GridLines = true;
            this.listSources.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.listSources.Location = new System.Drawing.Point(8, 6);
            this.listSources.MultiSelect = false;
            this.listSources.Name = "listSources";
            this.listSources.Size = new System.Drawing.Size(399, 283);
            this.listSources.TabIndex = 0;
            this.listSources.UseCompatibleStateImageBehavior = false;
            this.listSources.View = System.Windows.Forms.View.Details;
            this.listSources.KeyDown += new System.Windows.Forms.KeyEventHandler(this.listSources_KeyDown);
            // 
            // colURL
            // 
            this.colURL.Text = "URL";
            this.colURL.Width = 185;
            // 
            // colPackages
            // 
            this.colPackages.Text = "Number of Packages";
            this.colPackages.Width = 112;
            // 
            // btnEdit
            // 
            this.btnEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnEdit.Location = new System.Drawing.Point(411, 64);
            this.btnEdit.Name = "btnEdit";
            this.btnEdit.Size = new System.Drawing.Size(75, 23);
            this.btnEdit.TabIndex = 3;
            this.btnEdit.Text = "&Edit";
            this.btnEdit.UseVisualStyleBackColor = true;
            this.btnEdit.Click += new System.EventHandler(this.btnEdit_Click);
            // 
            // btnDelete
            // 
            this.btnDelete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnDelete.Location = new System.Drawing.Point(411, 35);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(75, 23);
            this.btnDelete.TabIndex = 2;
            this.btnDelete.Text = "&Delete";
            this.btnDelete.UseVisualStyleBackColor = true;
            this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
            // 
            // btnAdd
            // 
            this.btnAdd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAdd.Location = new System.Drawing.Point(411, 6);
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.Size = new System.Drawing.Size(75, 23);
            this.btnAdd.TabIndex = 1;
            this.btnAdd.Text = "&Add";
            this.btnAdd.UseVisualStyleBackColor = true;
            this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);
            // 
            // pagePackages
            // 
            this.pagePackages.Controls.Add(this.splitContainer1);
            this.pagePackages.Controls.Add(this.btnSearch);
            this.pagePackages.Controls.Add(this.txtSearch);
            this.pagePackages.Location = new System.Drawing.Point(4, 22);
            this.pagePackages.Name = "pagePackages";
            this.pagePackages.Padding = new System.Windows.Forms.Padding(3);
            this.pagePackages.Size = new System.Drawing.Size(492, 322);
            this.pagePackages.TabIndex = 1;
            this.pagePackages.Text = "Packages";
            this.pagePackages.UseVisualStyleBackColor = true;
            // 
            // splitContainer1
            // 
            this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainer1.Location = new System.Drawing.Point(8, 32);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.tablePackages);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.webBrowser1);
            this.splitContainer1.Size = new System.Drawing.Size(473, 255);
            this.splitContainer1.SplitterDistance = 227;
            this.splitContainer1.TabIndex = 4;
            this.splitContainer1.SplitterMoved += new System.Windows.Forms.SplitterEventHandler(this.splitContainer1_SplitterMoved);
            // 
            // tablePackages
            // 
            this.tablePackages.BorderColor = System.Drawing.Color.Black;
            this.tablePackages.ColumnResizing = false;
            this.tablePackages.DataMember = null;
            this.tablePackages.DataSourceColumnBinder = dataSourceColumnBinder1;
            this.tablePackages.Dock = System.Windows.Forms.DockStyle.Fill;
            dragDropRenderer1.ForeColor = System.Drawing.Color.Red;
            this.tablePackages.DragDropRenderer = dragDropRenderer1;
            this.tablePackages.FamilyRowSelect = true;
            this.tablePackages.FullRowSelect = true;
            this.tablePackages.GridLinesContrainedToData = false;
            this.tablePackages.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.tablePackages.Location = new System.Drawing.Point(0, 0);
            this.tablePackages.Name = "tablePackages";
            this.tablePackages.NoItemsText = "";
            this.tablePackages.ShowSelectionRectangle = false;
            this.tablePackages.Size = new System.Drawing.Size(227, 255);
            this.tablePackages.TabIndex = 0;
            this.tablePackages.UnfocusedBorderColor = System.Drawing.Color.Black;
            this.tablePackages.SelectionChanged += new XPTable.Events.SelectionEventHandler(this.listPackages_SelectionChanged);
            // 
            // webBrowser1
            // 
            this.webBrowser1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webBrowser1.Location = new System.Drawing.Point(0, 0);
            this.webBrowser1.MinimumSize = new System.Drawing.Size(20, 20);
            this.webBrowser1.Name = "webBrowser1";
            this.webBrowser1.ScriptErrorsSuppressed = true;
            this.webBrowser1.ScrollBarsEnabled = false;
            this.webBrowser1.Size = new System.Drawing.Size(242, 255);
            this.webBrowser1.TabIndex = 0;
            this.webBrowser1.NewWindow += new System.ComponentModel.CancelEventHandler(this.webBrowser1_NewWindow);
            // 
            // btnSearch
            // 
            this.btnSearch.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSearch.Location = new System.Drawing.Point(404, 4);
            this.btnSearch.Name = "btnSearch";
            this.btnSearch.Size = new System.Drawing.Size(77, 24);
            this.btnSearch.TabIndex = 3;
            this.btnSearch.Text = "&Search";
            this.btnSearch.UseVisualStyleBackColor = true;
            this.btnSearch.Click += new System.EventHandler(this.btnSearch_Click);
            // 
            // txtSearch
            // 
            this.txtSearch.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtSearch.Location = new System.Drawing.Point(8, 6);
            this.txtSearch.Name = "txtSearch";
            this.txtSearch.Size = new System.Drawing.Size(392, 20);
            this.txtSearch.TabIndex = 2;
            this.txtSearch.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtSearch_KeyDown);
            // 
            // pageQueue
            // 
            this.pageQueue.Controls.Add(this.tableQueue);
            this.pageQueue.Location = new System.Drawing.Point(4, 22);
            this.pageQueue.Name = "pageQueue";
            this.pageQueue.Size = new System.Drawing.Size(492, 322);
            this.pageQueue.TabIndex = 3;
            this.pageQueue.Text = "Queue";
            this.pageQueue.UseVisualStyleBackColor = true;
            // 
            // tableQueue
            // 
            this.tableQueue.AllowRMBSelection = true;
            this.tableQueue.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableQueue.BorderColor = System.Drawing.Color.Black;
            this.tableQueue.ColumnResizing = false;
            this.tableQueue.DataMember = null;
            this.tableQueue.DataSourceColumnBinder = dataSourceColumnBinder2;
            dragDropRenderer2.ForeColor = System.Drawing.Color.Red;
            this.tableQueue.DragDropRenderer = dragDropRenderer2;
            this.tableQueue.EnableHeaderContextMenu = false;
            this.tableQueue.FamilyRowSelect = true;
            this.tableQueue.FullRowSelect = true;
            this.tableQueue.GridLinesContrainedToData = false;
            this.tableQueue.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.tableQueue.Location = new System.Drawing.Point(8, 6);
            this.tableQueue.Name = "tableQueue";
            this.tableQueue.NoItemsText = "Queue is empty...";
            this.tableQueue.ShowSelectionRectangle = false;
            this.tableQueue.Size = new System.Drawing.Size(473, 281);
            this.tableQueue.TabIndex = 1;
            this.tableQueue.UnfocusedBorderColor = System.Drawing.Color.Black;
            this.tableQueue.MouseDown += new System.Windows.Forms.MouseEventHandler(this.tableQueue_MouseDown);
            // 
            // pageChanges
            // 
            this.pageChanges.Controls.Add(this.tableChanges);
            this.pageChanges.Location = new System.Drawing.Point(4, 22);
            this.pageChanges.Name = "pageChanges";
            this.pageChanges.Padding = new System.Windows.Forms.Padding(3);
            this.pageChanges.Size = new System.Drawing.Size(492, 322);
            this.pageChanges.TabIndex = 2;
            this.pageChanges.Text = "Changes";
            this.pageChanges.UseVisualStyleBackColor = true;
            // 
            // tableChanges
            // 
            this.tableChanges.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableChanges.BorderColor = System.Drawing.Color.Black;
            this.tableChanges.ColumnResizing = false;
            this.tableChanges.DataMember = null;
            this.tableChanges.DataSourceColumnBinder = dataSourceColumnBinder3;
            dragDropRenderer3.ForeColor = System.Drawing.Color.Red;
            this.tableChanges.DragDropRenderer = dragDropRenderer3;
            this.tableChanges.FamilyRowSelect = true;
            this.tableChanges.FullRowSelect = true;
            this.tableChanges.GridLinesContrainedToData = false;
            this.tableChanges.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.tableChanges.Location = new System.Drawing.Point(8, 6);
            this.tableChanges.Name = "tableChanges";
            this.tableChanges.NoItemsText = "No changes found...";
            this.tableChanges.ShowSelectionRectangle = false;
            this.tableChanges.Size = new System.Drawing.Size(473, 281);
            this.tableChanges.TabIndex = 1;
            this.tableChanges.UnfocusedBorderColor = System.Drawing.Color.Black;
            // 
            // trackBarZoom
            // 
            this.trackBarZoom.AutoSize = false;
            this.trackBarZoom.LargeChange = 2;
            this.trackBarZoom.Location = new System.Drawing.Point(399, 2);
            this.trackBarZoom.Maximum = 12;
            this.trackBarZoom.Minimum = 1;
            this.trackBarZoom.Name = "trackBarZoom";
            this.trackBarZoom.Size = new System.Drawing.Size(87, 25);
            this.trackBarZoom.TabIndex = 6;
            this.trackBarZoom.Value = 10;
            this.trackBarZoom.Scroll += new System.EventHandler(this.trackBarZoom_Scroll);
            // 
            // statusStrip
            // 
            this.statusStrip.AutoSize = false;
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.statusBarProgressbar,
            this.statusLabelDownload,
            this.toolStripStatusLabel1,
            this.statusLabelZoom});
            this.statusStrip.Location = new System.Drawing.Point(0, 322);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(500, 26);
            this.statusStrip.TabIndex = 1;
            // 
            // statusBarProgressbar
            // 
            this.statusBarProgressbar.Name = "statusBarProgressbar";
            this.statusBarProgressbar.Size = new System.Drawing.Size(100, 20);
            this.statusBarProgressbar.Visible = false;
            // 
            // statusLabelDownload
            // 
            this.statusLabelDownload.Name = "statusLabelDownload";
            this.statusLabelDownload.Size = new System.Drawing.Size(111, 21);
            this.statusLabelDownload.Text = "{0}% Complete ({1})";
            this.statusLabelDownload.Visible = false;
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(416, 21);
            this.toolStripStatusLabel1.Spring = true;
            // 
            // statusLabelZoom
            // 
            this.statusLabelZoom.Name = "statusLabelZoom";
            this.statusLabelZoom.Size = new System.Drawing.Size(69, 21);
            this.statusLabelZoom.Text = "Zoom: {0}%";
            // 
            // contextMenuQueue
            // 
            this.contextMenuQueue.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.deleteToolStripMenuItem,
            this.openFolderToolStripMenuItem});
            this.contextMenuQueue.Name = "contextMenuQueue";
            this.contextMenuQueue.Size = new System.Drawing.Size(198, 48);
            // 
            // deleteToolStripMenuItem
            // 
            this.deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
            this.deleteToolStripMenuItem.Size = new System.Drawing.Size(197, 22);
            this.deleteToolStripMenuItem.Text = "Delete";
            this.deleteToolStripMenuItem.Click += new System.EventHandler(this.deleteToolStripMenuItem_Click);
            // 
            // openFolderToolStripMenuItem
            // 
            this.openFolderToolStripMenuItem.Name = "openFolderToolStripMenuItem";
            this.openFolderToolStripMenuItem.Size = new System.Drawing.Size(197, 22);
            this.openFolderToolStripMenuItem.Text = "Open containing folder";
            this.openFolderToolStripMenuItem.Click += new System.EventHandler(this.openFolderToolStripMenuItem_Click);
            // 
            // toolStripDropDownButton1
            // 
            this.toolStripDropDownButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripDropDownButton1.Image = ((System.Drawing.Image)(resources.GetObject("toolStripDropDownButton1.Image")));
            this.toolStripDropDownButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripDropDownButton1.Name = "toolStripDropDownButton1";
            this.toolStripDropDownButton1.Size = new System.Drawing.Size(29, 20);
            this.toolStripDropDownButton1.Text = "toolStripDropDownButton1";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(500, 348);
            this.Controls.Add(this.statusStrip);
            this.Controls.Add(this.tabMain);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Cygnus";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainForm_FormClosed);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.Resize += new System.EventHandler(this.MainForm_Resize);
            this.tabMain.ResumeLayout(false);
            this.pageSource.ResumeLayout(false);
            this.pagePackages.ResumeLayout(false);
            this.pagePackages.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.tablePackages)).EndInit();
            this.pageQueue.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.tableQueue)).EndInit();
            this.pageChanges.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.tableChanges)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarZoom)).EndInit();
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.contextMenuQueue.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabMain;
        private System.Windows.Forms.TabPage pageSource;
        private System.Windows.Forms.TabPage pagePackages;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.Button btnEdit;
        private System.Windows.Forms.Button btnDelete;
        private System.Windows.Forms.Button btnAdd;
        private System.Windows.Forms.ColumnHeader colURL;
        private System.Windows.Forms.ColumnHeader colPackages;
        public System.Windows.Forms.ListView listSources;
        private System.Windows.Forms.Button btnReload;
        private System.Windows.Forms.TabPage pageChanges;
        private XPTable.Models.Table tablePackages;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Button btnSearch;
        private System.Windows.Forms.TextBox txtSearch;
        private System.Windows.Forms.TrackBar trackBarZoom;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.ToolStripStatusLabel statusLabelZoom;
        private System.Windows.Forms.WebBrowser webBrowser1;
        private System.Windows.Forms.ToolStripProgressBar statusBarProgressbar;
        private System.Windows.Forms.ToolStripStatusLabel statusLabelDownload;
        private System.Windows.Forms.TabPage pageQueue;
        private XPTable.Models.Table tableQueue;
        private XPTable.Models.Table tableChanges;
        private System.Windows.Forms.ContextMenuStrip contextMenuQueue;
        private System.Windows.Forms.ToolStripMenuItem deleteToolStripMenuItem;
        private System.Windows.Forms.ToolStripDropDownButton toolStripDropDownButton1;
        private System.Windows.Forms.ToolStripMenuItem openFolderToolStripMenuItem;
    }
}
