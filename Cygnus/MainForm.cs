// <copyright file="MainForm.cs">
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
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows.Forms;
    using System.Xml.Linq;

    using XPTable.Models;

    /// <summary>
    /// The main form of the application.
    /// This file handles (mostly) the UI parts of MainForm.
    /// <seealso cref="MainForm.APT"/>
    /// </summary>
    public partial class MainForm : Form
    {
        #region Fields

        /// <summary>
        /// Cell styles are just like CSS of the XPTable.
        /// These are global because it's unnecessary to re-initialize them everytime.
        /// </summary>
        private static CellStyle titleStyle = new CellStyle() { Font = new Font("Microsoft Sans Serif", 11) };
        private static CellStyle sectionStyle = new CellStyle() { Font = new Font("Microsoft Sans Serif", 7), ForeColor = Color.Gray };
        private static CellStyle descriptionStyle = new CellStyle() { ForeColor = Color.Blue };

        /// <summary>
        /// The latest selected package by the user.
        /// Used to download and display depiction.
        /// </summary>
        private static Package selectedPack;

        /// <summary>
        /// Used to zoom once after the first website is loaded to load user preferences.
        /// </summary>
        private static bool zoomedBefore = false;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="MainForm"/> class.
        /// </summary>
        public MainForm()
        {
            this.InitializeComponent();
            this.InitializeXPTable(); // Ensure XPTable is loaded before Form_Load
        }

        #endregion Constructors

        #region Methods

        /// <summary>
        /// Initializes XPTable (listPackages) model.
        /// </summary>
        private void InitializeXPTable()
        {
            listPackages.SelectionStyle = SelectionStyle.Grid;
            listPackages.EnableWordWrap = true;

            listPackages.GridLines = GridLines.None;
            ImageColumn col1 = new ImageColumn(String.Empty, 20) { Editable = false, Resizable = false };
            TextColumn col2 = new TextColumn() { Editable = false, Resizable = false };

            listPackages.ColumnModel = new ColumnModel(new Column[] { col1, col2 });

            listPackages.TableModel = new TableModel();
        }

        /// <summary>
        /// The helper method to show <see cref="AlertView"/>.
        /// </summary>
        /// <param name="text">The text to be written in URL textbox.</param>
        /// <param name="isAdd">Used to decide whether it should show Add button or Edit button.</param>
        /// <returns>The text typed by the user. Null if it was canceled.</returns>
        private static string ShowAlertView(string text, bool isAdd)
        {
            var alertView = new AlertView();

            alertView.txtSource.Text = text;
            alertView.txtSource.SelectionStart = text.Length;
            alertView.errorProvider1.Clear();
            alertView.btnAdd.Visible = isAdd;
            alertView.btnEdit.Visible = !isAdd;

            alertView.ShowDialog();

            return alertView.Canceled ? null : new Uri(alertView.txtSource.Text).ToString();
        }

        /// <summary>
        /// The helper method that shows <see cref="LoadingView"/>.
        /// </summary>
        /// <param name="loadingText">The text that is written below spinner.</param>
        /// <param name="action">The task (lambda) to run while <see cref="LoadingView"/> is being displayed.</param>
        private void ShowLoadingView(string loadingText, Action action)
        {
            var loadingView = new LoadingView();

            loadingView.label.Text = loadingText;
            loadingView.Text = loadingText;

            var task = Task.Factory.StartNew(action);

            task.ContinueWith((prevTask) =>
            {
                this.Invoke((MethodInvoker)delegate
                {
                    loadingView.Close();
                });
            });

            loadingView.ShowDialog();

            if (task.IsFaulted)
            {
                foreach (var exception in task.Exception.InnerExceptions)
                {
                    throw exception;
                }
            }
        }

        /// <summary>
        /// Reloads the last state of MainForm.
        /// </summary>
        private void LoadUserSettings()
        {
            var settings = Properties.Settings.Default;
            this.tabMain.SelectedIndex = settings.tabSelectedIndex;
            this.splitContainer1.SplitterDistance = this.splitContainer1.Width - settings.webBrowserPanelWidth;
            this.Size = settings.formSize;
            this.CenterToScreen(); // the order of this is important
            this.WindowState = settings.windowState;
            this.trackBarZoom.Value = settings.zoomFactor;
        }

        /// <summary>
        /// Saves the last state of MainForm before being closed.
        /// </summary>
        private void SaveUserSettings()
        {
            var settings = Properties.Settings.Default;
            settings.tabSelectedIndex = this.tabMain.SelectedIndex;
            settings.webBrowserPanelWidth = this.splitContainer1.Width - this.splitContainer1.SplitterDistance;

            if (this.WindowState != FormWindowState.Maximized)
            {
                settings.formSize = this.Size;
            }

            settings.windowState = (this.WindowState == FormWindowState.Maximized) ? FormWindowState.Maximized : FormWindowState.Normal;
            settings.zoomFactor = trackBarZoom.Value;

            settings.Save();
        }

        /// <summary>
        /// Loads the repo list from /doc:Cygnus.xml
        /// </summary>
        private void LoadXMLSettings()
        {
            if (!File.Exists("Cygnus.xml"))
                SaveXMLSettings();

            this.listSources.Items.Clear();
            allRepos.Clear();

            var sources = XDocument.Load("Cygnus.xml").Element("Settings").Elements("Source");
            foreach (var source in sources)
            {
                string repoUrl = new Uri(source.Element("URL").Value).ToString();
                string key = URLToFilename(repoUrl);
                string repoLabel = source.Element("Label").Value;

                ListViewItem item = this.listSources.Items.Add(key, repoUrl, key);

                item.SubItems.Add(source.Element("Size").Value);
                item.Tag = repoLabel;

                allRepos.Add(new Repo { URL = repoUrl, Label = repoLabel });
            }
        }

        /// <summary>
        /// Saves the repositories added by the user.
        /// </summary>
        private void SaveXMLSettings()
        {
            XDocument doc = new XDocument(
                new XElement("Settings",
                    from ListViewItem item in this.listSources.Items
                    select new XElement("Source",
                        new XElement("URL", item.Text),
                        new XElement("Size", item.SubItems[1].Text),
                        new XElement("Label", item.Tag)
                        )
                    )
                );

            doc.Save("Cygnus.xml");
        }

        /// <summary>
        /// Inserts repo icons to listSources.
        /// </summary>
        private void AddIcons()
        {
            if (!Directory.Exists("repos")) return;

            ImageList imgList = new ImageList();
            imgList.ColorDepth = ColorDepth.Depth24Bit;
            imgList.ImageSize = new Size(32, 32);
            foreach (var file in Directory.GetFiles("repos", "*.png"))
            {
                Image image;
                try
                {
                    image = (Image)new Bitmap(file);
                }
                catch (ArgumentException)
                {
                    // bitmap is invalid
                    continue;
                }

                using (image)
                using (Bitmap scaled = new Bitmap(32, 32))
                {
                    using (Graphics g = Graphics.FromImage(scaled))
                    {
                        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.High;
                        g.DrawImage(image, new Rectangle(0, 0, 32, 32));
                    }

                    imgList.Images.Add(Path.GetFileNameWithoutExtension(file), new Bitmap(scaled, 32, 32));
                }
            }

            this.listSources.SmallImageList = imgList;
        }

        /// <summary>
        /// Auto-resizes Column widths of listSources and listPackages
        /// </summary>
        /// <remarks>
        /// I had to increase Column.MaximumWidth property from 1024 to 8192 using XPTable source code...
        /// </remarks>
        private void ResizeColumns()
        {
            this.listSources.Columns[0].Width = this.listSources.Width - 116; // Perfect constant
            this.listSources.Columns[1].AutoResize(ColumnHeaderAutoResizeStyle.HeaderSize);

            this.listPackages.ColumnModel.Columns[1].Width = this.listPackages.Width - 41;
        }

        /// <summary>
        /// Customizes MainForm's UI.
        /// </summary>
        private void MainForm_Load(object sender, EventArgs e)
        {
            AddIcons();

            LoadUserSettings();
            ZoomWebBrowser(); // Formats the 'Zoom: {0}%' Label
            LoadXMLSettings();

            this.statusStrip.Items.Add(new ToolStripControlHost(this.trackBarZoom));
            //Adds zoom trackbar which isn't possible to do with the Designer

            this.tabMain_SelectedIndexChanged(null, null);
        }

        /// <summary>
        /// Resize event of MainForm.
        /// </summary>
        private void MainForm_Resize(object sender, EventArgs e)
        {
            ResizeColumns();
        }

        /// <summary>
        /// Handles the Closed event of MainForm.
        /// </summary>
        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            SaveUserSettings();
        }

        /// <summary>
        /// Adds a "Package" to the listPackages (XPTable).
        /// </summary>
        /// <param name="pack">Package to add.</param>
        private void AddPkgToList(Package pack)
        {
            Row row = new Row() { Height = 18 };
            row.Cells.Add(new Cell()); // using empty cell for now, I'm going to replace this with Cydia-like Section Icons
            Cell cell = new Cell(pack.Name, titleStyle) { ForeColor = pack.Paid ? Color.Blue : Color.Black };
            row.Cells.Add(cell);
            this.listPackages.TableModel.Rows.Add(row);

            Row subrow = new Row();
            subrow.Cells.Add(new Cell());
            cell = new Cell(String.Format("from {0} ({1})", pack.Repo.Label, pack.Section), sectionStyle);
            subrow.Cells.Add(cell);
            row.SubRows.Add(subrow);

            subrow = new Row();
            subrow.Cells.Add(new Cell());
            cell = new Cell(pack.Description, descriptionStyle);
            subrow.Cells.Add(cell);
            row.SubRows.Add(subrow);

            row.Tag = pack;
        }

        /// <summary>
        /// Adds a new repo to the list.
        /// <see cref="MainForm.APT.VerifyRepoURL"/>
        /// </summary>
        private void btnAdd_Click(object sender, EventArgs e)
        {
            string url = ShowAlertView("http://", true);

            if (url == null) return; // means it's cancelled

            VerifyRepoURL(url.ToLowerInvariant(), this.listSources.Items.Add(String.Empty));
        }

        /// <summary>
        /// Edits the selected repo.
        /// </summary>
        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (this.listSources.SelectedItems.Count > 0)
            {
                var selectedItem = this.listSources.SelectedItems[0];

                string url = ShowAlertView(selectedItem.Text, false);

                if (url == null) return;

                VerifyRepoURL(url.ToLowerInvariant(), selectedItem);
            }
        }

        /// <summary>
        /// Deletes the selected repository.
        /// </summary>
        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (this.listSources.SelectedItems.Count > 0)
            {
                var selectedItem = this.listSources.SelectedItems[0];
                selectedItem.SubItems.Clear();
                selectedItem.Remove();
                SaveXMLSettings();
                LoadXMLSettings();
            }
        }

        /// <summary>
        /// Reloads data from repos. <see cref="MainForm.APT.ReloadData"/> for more info.
        /// </summary>
        private void btnReload_Click(object sender, EventArgs e)
        {
            ShowLoadingView("Reloading Data", () =>
            {
                ReloadData();
            });
        }

        /// <summary>
        /// Searches packages by title. 
        /// <see cref="MainForm.APT.SearchPackgesByName"/>
        /// </summary>
        private void btnSearch_Click(object sender, EventArgs e)
        {
            this.btnDownload.Enabled = true;
            this.listPackages.BeginUpdate();
            this.listPackages.ClearAllData();

            var searchList = SearchPackagesByName(txtSearch.Text);

            if (searchList.Count == 0)
            {
                this.listPackages.NoItemsText = "No packages found...";
                this.listPackages.EndUpdate();
                this.btnDownload.Enabled = false;
                return;
            }

            foreach (var pack in searchList)
            {
                AddPkgToList(pack);
            }

            this.listPackages.EndUpdate();
        }

        /// <summary>
        /// Downloads the last selected package.
        /// </summary>
        /// <remarks>
        /// This code is pretty much self-self-explanatory.
        /// </remarks>
        private void btnDownload_Click(object sender, EventArgs e)
        {
            if (!Directory.Exists("debs")) Directory.CreateDirectory("debs");

            // TODO: Don't invoke LoadingView while downloading
            // Make another GUI and support multiple downloads simultaneously

            Uri downloadURL = new Uri(new Uri(selectedPack.Repo.URL), selectedPack.Filename);

            ShowLoadingView("Downloading File", () =>
            {
                string dirPath = Path.Combine("debs", CleanFileName(selectedPack.Name));
                string filePath = Path.Combine(dirPath, Path.GetFileName(downloadURL.LocalPath));

                if (!Directory.Exists(dirPath)) Directory.CreateDirectory(dirPath);

                DownloadFile(downloadURL, filePath);

                if (selectedPack.Depends != null &&
                    MessageBox.Show("Do you want to download dependencies of this package?",
                                    "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    // TODO: Use a proper way to ask this, e.g use seperate buttons
                    DownloadAllDependencies(selectedPack, dirPath);
                }
            });
        }

        /// <summary>
        /// Navigates the WebBrowser control using latest IE engine.
        /// </summary>
        /// <param name="url">The URL to navigate.</param>
        private void NavigateWebBrowser(string url)
        {
            // SUPER HACKY WORKAROUND TO SOLVE STUPID WebBrowser PROBLEM
            webBrowser1.DocumentText = @"<html>
<head><meta http-equiv='X-UA-Compatible' content='IE=edge'></head>
<body>
<iframe src='" + url + @"' style='border:0; position:absolute; top:0; left:0; right:0; bottom:0; width:100%; height:100%' />
</body>
</html>";
            // Problem:
            //     ieframe.dll based WebBrowser control loads webpages using IE7 compatibility mode by default
            //
            // I've found this workaround by myself. This solves three problems:
            //  • We won't need to have Administrator privileges and do a stupid registry trick:
            // http://msdn.microsoft.com/en-us/library/ie/ee330730%28v=vs.85%29.aspx
            //
            //  • And other than that, for some reason, this WebBrowser control doesn't render
            // Modmyi properly even after that trick.
            //
            //  • And finally, we won't need to detect IE Version to set registry value correctly.
            // It'll use the latest Version avaiable on the computer automatically
        }

        /// <summary>
        /// <para>
        /// SelectionChanged event handler of listPackages.
        /// </para><para>
        /// Basically this navigates to depiction and changed
        /// selectedPack variable so btnDownload can determine
        /// the latest selected package.
        /// </para>
        /// </summary>
        private void listPackages_SelectionChanged(object sender, XPTable.Events.SelectionEventArgs e)
        {
            if (e.NewSelectedIndicies.Length == 0) return;

            var selected = e.TableModel.Selections.SelectedItems[0];

            selectedPack = (Package)selected.Tag;

            string depiction = selectedPack.Depiction;

            if (String.IsNullOrEmpty(depiction))
            {
                // IE7 sucks so bad I had to use that 'X-UA-Compatible' tag again.
                webBrowser1.DocumentText = @"<html>
<head><meta http-equiv='X-UA-Compatible' content='IE=edge'></head>
<body>
<h3 style='font-family:Arial; width:320px; height:0px; margin:auto; position:absolute; top:0; left:0; bottom:0; right:0'>No Depiction found for this package</h3>
</body>
</html>"; // Centers <h3> horizontally and vertically
                return;
            }

            // FIXME: Find a way to set Content-Type to 'text/html'
            // Stupid IE tries to download iframe src sometimes

            if (depiction.EndsWith("/"))
                depiction += "index.html";

            NavigateWebBrowser(depiction);
        }

        /// <summary>
        /// Resizes columns and hides/unhides Zoom controls according to the selected tab.
        /// </summary>
        private void tabMain_SelectedIndexChanged(object sender, EventArgs e)
        {
            ResizeColumns();

            switch (tabMain.SelectedIndex)
            {
                case 0: // Sources
                case 2: // Changes
                    statusLabelZoom.Visible = false;
                    trackBarZoom.Visible = false;
                    break;
                case 1: // Packages
                    statusLabelZoom.Visible = true;
                    trackBarZoom.Visible = true;
                    break;
            }
        }

        /// <summary>
        /// Zooms in and out of WebBrowser (IE) using SHDocVw which is 
        /// referenced as 'Microsoft Internet Controls' for human beings.
        /// </summary>
        /// <remarks>
        /// Credits: http://stackoverflow.com/a/12910148
        /// </remarks>
        private void ZoomWebBrowser()
        {
            int zoomFactor = (this.trackBarZoom.Value <= 10) ? (this.trackBarZoom.Value * 10) : (this.trackBarZoom.Value == 11) ? 125 : 150;

            statusLabelZoom.Text = String.Format("Zoom: {0}%", zoomFactor);

            if (this.webBrowser1.Url == null) return;

            // Saurik's site gets stuck at WebBrowserReadyState.Interactive for some reason
            // The correct way is checking against WebBrowserReadyState.Complete
            while (this.webBrowser1.ReadyState == WebBrowserReadyState.Uninitialized)
            {
                Application.DoEvents();
            }

            ((SHDocVw.WebBrowser)this.webBrowser1.ActiveXInstance).ExecWB(
                SHDocVw.OLECMDID.OLECMDID_OPTICAL_ZOOM,
                SHDocVw.OLECMDEXECOPT.OLECMDEXECOPT_DONTPROMPTUSER,
                zoomFactor,
                IntPtr.Zero);
        }

        /// <summary>
        /// It is necessary to zoom once after the first website is loaded.
        /// Because the default zoom factor is obviously 100%.
        /// </summary>
        private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            if (!zoomedBefore)
            {
                ZoomWebBrowser();
                zoomedBefore = true;
            }
        }

        /// <summary>
        /// This prevents WebBrowser from opening (real) Internet Explorer and then 
        /// navigates to the clicked link.
        /// </summary>
        private void webBrowser1_NewWindow(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            NavigateWebBrowser(this.webBrowser1.StatusText);
        }

        /// <summary>
        /// The scroll event handler of Zoom Trackbar.
        /// </summary>
        private void trackBarZoom_Scroll(object sender, EventArgs e)
        {
            ZoomWebBrowser();
        }

        /// <summary>
        /// listSources hotkey.
        /// </summary>
        private void listSources_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                this.btnDelete_Click(null, null);
            }
        }

        /// <summary>
        /// txtSearch hotkey.
        /// </summary>
        private void txtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                this.btnSearch_Click(null, null);
            }
        }

        #endregion Methods
    }
}