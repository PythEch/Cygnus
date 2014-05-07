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
    using System.Collections.Generic;

    using Cygnus.Extensions;
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
        private static readonly CellStyle TitleStyle = new CellStyle() { Font = new Font("Microsoft Sans Serif", 11) };
        private static readonly CellStyle SectionStyle = new CellStyle() { Font = new Font("Microsoft Sans Serif", 7), ForeColor = Color.Gray };
        private static readonly CellStyle DescriptionStyle = new CellStyle() { ForeColor = Color.Blue };

        /// <summary>
        /// The latest selected package by the user.
        /// Used to download and display depiction.
        /// </summary>
        private static Package selectedPack;

        /// <summary>
        /// Used to store last selected package in Queue page.
        /// </summary>
        private static Queue selectedQueue;

        private static HtmlElement cssBtnDownload, cssBtnDownloadFully, cssBtnBack, cssFrameDepiction;

        private static readonly Dictionary<string, Bitmap> SectionIcons = new Dictionary<string, Bitmap>
        {
            {"Addons", Properties.Resources.Addons},
            {"Administration", Properties.Resources.Administration},
            {"Archiving", Properties.Resources.Archiving},
            {"Books", Properties.Resources.Books},
            {"Carrier_Bundles", Properties.Resources.Carrier_Bundles},
            {"Data_Storage", Properties.Resources.Data_Storage},
            {"Development", Properties.Resources.Development},
            {"Dictionaries", Properties.Resources.Dictionaries},
            {"Education", Properties.Resources.Education},
            {"Entertainment", Properties.Resources.Entertainment},
            {"Fonts", Properties.Resources.Fonts},
            {"Games", Properties.Resources.Games},
            {"Health_and_Fitness", Properties.Resources.Health_and_Fitness},
            {"Java", Properties.Resources.Java},
            {"Keyboards", Properties.Resources.Keyboards},
            {"Localization", Properties.Resources.Localization},
            {"Messaging", Properties.Resources.Messaging},
            {"Multimedia", Properties.Resources.Multimedia},
            {"Navigation", Properties.Resources.Navigation},
            {"Networking", Properties.Resources.Networking},
            {"Packaging", Properties.Resources.Packaging},
            {"Productivity", Properties.Resources.Productivity},
            {"Repositories", Properties.Resources.Repositories},
            {"Ringtones", Properties.Resources.Ringtones},
            {"Scripting", Properties.Resources.Scripting},
            {"Security", Properties.Resources.Security},
            {"Site-Specific_Apps", Properties.Resources.Site_Specific_Apps},
            {"Social", Properties.Resources.Social},
            {"Soundboards", Properties.Resources.Soundboards},
            {"System", Properties.Resources.System},
            {"Terminal_Support", Properties.Resources.Terminal_Support},
            {"Text_Editors", Properties.Resources.Text_Editors},
            {"Themes", Properties.Resources.Themes},
            {"Toys", Properties.Resources.Toys},
            {"Tweaks", Properties.Resources.Tweaks},
            {"Utilities", Properties.Resources.Utilities},
            {"Wallpaper", Properties.Resources.Wallpaper},
            {"Widgets", Properties.Resources.Widgets},
            {"X_Window", Properties.Resources.X_Window},
        };

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
            // == Packages XPTable == //
            this.tablePackages.SelectionStyle = SelectionStyle.Grid;
            this.tablePackages.GridLines = GridLines.None;
            this.tablePackages.EnableWordWrap = true;

            ImageColumn col1 = new ImageColumn(String.Empty, 32) { Editable = false, Resizable = false };
            TextColumn col2 = new TextColumn() { Editable = false, Resizable = false };

            this.tablePackages.ColumnModel = new ColumnModel(new Column[] { col1, col2 });
            this.tablePackages.TableModel = new TableModel();

            // == Changes XPTable == //
            this.tableChanges.SelectionStyle = SelectionStyle.Grid;
            this.tableChanges.GridLines = GridLines.None;
            this.tableChanges.EnableWordWrap = true;

            ImageColumn col3 = new ImageColumn(String.Empty, 32) { Editable = false, Resizable = false };
            TextColumn col4 = new TextColumn() { Editable = false, Resizable = false };

            this.tableChanges.ColumnModel = new ColumnModel(new Column[] { col3, col4 });
            this.tableChanges.TableModel = new TableModel();

            // == Queue XPTable == //
            this.tableQueue.SelectionStyle = SelectionStyle.Grid;
            this.tableQueue.GridLines = GridLines.Rows;
            this.tableQueue.EnableWordWrap = true;

            TextColumn col5 = new TextColumn() { Editable = false, Resizable = false };
            ProgressBarColumn col6 = new ProgressBarColumn() { Editable = false, Resizable = false };

            this.tableQueue.ColumnModel = new ColumnModel(new Column[] { col5, col6 });
            this.tableQueue.TableModel = new TableModel() { RowHeight = 20 };
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

            return alertView.Canceled ? null : alertView.txtSource.Text.toValidURL();
        }

        /// <summary>
        /// The helper method that shows <see cref="LoadingView"/>.
        /// </summary>
        /// <param name="loadingText">The text that is written below spinner.</param>
        /// <param name="action">The task (lambda) to run while <see cref="LoadingView"/> is being displayed.</param>
        private async void ShowLoadingView(string loadingText, Action action)
        {
            var loadingView = new LoadingView();

            loadingView.label.Text = loadingText;
            loadingView.Text = loadingText;

            var task = Task.Factory.StartNew(action).ContinueWith((prevTask) =>
            {
                this.Invoke((MethodInvoker)delegate
                {
                    loadingView.Close();
                });
            });

            loadingView.ShowDialog();

            await task; // unwraps exceptions automatically unlike Task.Wait()
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

            var settings = XDocument.Load("Cygnus.xml").Element("Settings");

            txtUDID.Text = settings.Element("UDID").Value;
            boxiDevice.SelectedIndex = settings.Element("iDevice").Value.ToInt32(-1);
            boxVersion.SelectedIndex = settings.Element("Version").Value.ToInt32(-1);

            foreach (var source in settings.Elements("Source"))
            {
                string repoUrl = source.Element("URL").Value.toValidURL();
                string key = URLToFilename(repoUrl);
                string repoLabel = source.Element("Label").Value;

                ListViewItem item = this.listSources.Items.Add(key, repoUrl, key);

                item.SubItems.Add(source.Element("Size").Value);
                item.Tag = repoLabel;

                allRepos.Add(new Repo() { URL = repoUrl, Label = repoLabel });
            }
        }

        /// <summary>
        /// Saves the repositories added by the user.
        /// </summary>
        private void SaveXMLSettings()
        {
            XDocument doc = new XDocument(
                new XElement("Settings",
                    new XElement("UDID", txtUDID.Text),
                    new XElement("iDevice", boxiDevice.SelectedIndex),
                    new XElement("Version", boxVersion.SelectedIndex),

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
        /// Auto-resizes Column widths of ListViews and XPTables.
        /// </summary>
        /// <remarks>
        /// I had to increase Column.MaximumWidth property from 1024 to 8192 using XPTable source code...
        /// </remarks>
        private void ResizeColumns()
        {
            this.listSources.Columns[0].Width = this.listSources.Width - 116; // Perfect constant
            this.listSources.Columns[1].AutoResize(ColumnHeaderAutoResizeStyle.HeaderSize);

            this.tablePackages.ColumnModel.Columns[1].Width = this.tablePackages.Width - 53;

            this.tableChanges.ColumnModel.Columns[0].Width = this.tableChanges.Width - 41;

            this.tableQueue.Refresh(); // this is necessary for some reason
            this.tableQueue.AutoResizeColumnWidths();
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

            this.webBrowser1.Navigate("about:blank");
            this.webBrowser1.Document.Write(String.Empty); // Stupid workaround, thanks: http://stackoverflow.com/a/21608474
            this.webBrowser1.DocumentText = Properties.Resources.WebBrowserStyles;

            while (this.webBrowser1.ReadyState != WebBrowserReadyState.Complete)
                Application.DoEvents();

            cssBtnDownload = this.webBrowser1.Document.GetElementById("download");
            cssBtnDownloadFully = this.webBrowser1.Document.GetElementById("downloadfully");
            cssBtnBack = this.webBrowser1.Document.GetElementById("back");
            cssFrameDepiction = this.webBrowser1.Document.GetElementById("depiction");

            cssBtnDownload.Enabled = cssBtnDownloadFully.Enabled = false;

            cssBtnDownload.Click += delegate { DownloadPackage(false); };
            cssBtnDownloadFully.Click += delegate { DownloadPackage(true); };
            cssBtnBack.Click += delegate { NavigateWebBrowser(selectedPack.Depiction); };

            ZoomWebBrowser();

            tabMain_SelectedIndexChanged(null, null);
        }

        /// <summary>
        /// Resize event of MainForm.
        /// </summary>
        private void MainForm_Resize(object sender, EventArgs e)
        {
            ResizeColumns();

            optionsPanel.Left = (this.ClientSize.Width - optionsPanel.Width) / 2;
            optionsPanel.Top = (this.ClientSize.Height - optionsPanel.Height) / 2 - 35;
        }

        /// <summary>
        /// Handles the Closed event of MainForm.
        /// </summary>
        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            SaveUserSettings();
            SaveXMLSettings();
        }


        /// <summary>
        /// Adds a "Package" to the tablePackages (XPTable).
        /// </summary>
        /// <param name="pack">Package to add.</param>
        private void UpdatePackagesTable(Package pack)
        {
            Row row = new Row() { Height = 18 };
            row.Cells.Add(new Cell());
            row.Cells.Add(new Cell(pack.Name, TitleStyle) { ForeColor = pack.Paid ? Color.Blue : Color.Black });
            this.tablePackages.TableModel.Rows.Add(row);

            Bitmap sectionImage = SectionIcons.FirstOrDefault(x => pack.Section.Contains(x.Key)).Value;

            Row subrow = new Row();
            subrow.Cells.Add(new Cell(String.Empty, sectionImage) { Padding = new CellPadding(0, -16, 0, 0), ImageSizeMode = ImageSizeMode.NoClip });
            // Overflow the bounds of the image to make it fit two rows instead of one.
            subrow.Cells.Add(new Cell("from {0} ({1})".FormatWith(pack.Repo.Label, pack.Section), SectionStyle));
            row.SubRows.Add(subrow);

            subrow = new Row();
            subrow.Cells.Add(new Cell());
            subrow.Cells.Add(new Cell(pack.Description, DescriptionStyle));
            row.SubRows.Add(subrow);

            row.Tag = pack;
        }

        /// <summary>
        /// Adds a "Package" to the tableChanges.
        /// </summary>
        /// <param name="pack">Package to add.</param>
        private void UpdateChangesTable(Package pack)
        {
            Row row = new Row() { Height = 18 };
            row.Cells.Add(new Cell()); // using empty cell for now, I'm going to replace this with Cydia-like Section Icons
            row.Cells.Add(new Cell(pack.Name, TitleStyle) { ForeColor = pack.Paid ? Color.Blue : Color.Black });
            this.tableChanges.TableModel.Rows.Add(row);

            Row subrow = new Row();
            subrow.Cells.Add(new Cell());
            subrow.Cells.Add(new Cell("from {0} ({1})".FormatWith(pack.Repo.Label, pack.Section), SectionStyle));
            row.SubRows.Add(subrow);

            subrow = new Row();
            subrow.Cells.Add(new Cell());
            subrow.Cells.Add(new Cell(pack.Description, DescriptionStyle));
            row.SubRows.Add(subrow);
        }

        /// <summary>
        /// Adds a Package to the tableQueue.
        /// </summary>"
        /// <param name="packName">Package name to display.</param>
        /// <returns>The row index of the package.</returns>
        private Queue UpdateQueueTable(string packName, Uri downloadUri)
        {
            Row row = new Row();
            row.Cells.Add(new Cell(packName, TitleStyle)); // Package name text
            row.Cells.Add(new Cell()); // Progressbar
            this.tableQueue.TableModel.Rows.Add(row);

            Queue queue = new Queue() { DownloadUri = downloadUri, TableRow = row };
            row.Tag = queue;
            return queue;
        }

        /// <summary>
        /// Adds a new repo to the list.
        /// <see cref="MainForm.APT.VerifyRepoURL"/>
        /// </summary>
        private void btnAdd_Click(object sender, EventArgs e)
        {
            string url = ShowAlertView("http://", true);

            if (url == null) return; // means it's cancelled

            ListViewItem item = this.listSources.Items.Add(String.Empty);
            bool verified = VerifyRepoURL(url, item);

            if (!verified) item.Remove();
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

                VerifyRepoURL(url, selectedItem);
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

            SaveXMLSettings();
        }

        /// <summary>
        /// Searches packages by title.
        /// <see cref="MainForm.APT.SearchPackgesByName"/>
        /// </summary>
        private void btnSearch_Click(object sender, EventArgs e)
        {
            cssBtnDownload.Enabled = cssBtnDownloadFully.Enabled = true;
            this.tablePackages.BeginUpdate();
            this.tablePackages.ClearAllData();

            var searchList = SearchPackagesByName(txtSearch.Text);

            if (searchList.Count == 0)
            {
                this.tablePackages.NoItemsText = "No packages found...";
                this.tablePackages.EndUpdate();
                cssBtnDownload.Enabled = cssBtnDownloadFully.Enabled = false;
                return;
            }

            foreach (var pack in searchList)
            {
                UpdatePackagesTable(pack);
            }

            this.tablePackages.EndUpdate();
        }

        /// <summary>
        /// Downloads the last selected package asynchronously.
        /// </summary>
        /// <remarks>
        /// This code is pretty much self-explanatory.
        /// </remarks>
        private async void DownloadPackage(bool downloadDependencies)
        {
            Uri downloadUri = new Uri(new Uri(selectedPack.Repo.URL), selectedPack.Filename);
            string selectedPackName = selectedPack.Name;
            //Copy selectedPack locally to avoid problems with async

            if (allQueues.Any(x => x.DownloadUri == downloadUri)) return;
            // It's already in queue

            Queue queue = UpdateQueueTable(selectedPackName, downloadUri);
            allQueues.Add(queue);

            // Wait until the queue is finished
            while (allQueues.IndexOf(queue) >= 1)
            {
                await TaskEx.Delay(500); //500 ms optimal
            }

            if (!allQueues.Contains(queue)) return;
            //Prevent race condition that happens when user cancels the queue

            string dirPath = Path.Combine("debs", ValidFilename(selectedPackName));
            string filePath = Path.Combine(dirPath, Path.GetFileName(downloadUri.LocalPath));

            Directory.CreateDirectory(dirPath);

            statusBarProgressbar.Visible = true;
            statusLabelDownload.Visible = true;

            statusLabelDownload.Text = "0% Complete ({0})".FormatWith(selectedPackName);

            await Task.Factory.StartNew(() =>
            {
                DownloadQueue(downloadUri, filePath, selectedPackName, queue.TableRow.Cells[1]);

                if (downloadDependencies)
                {
                    DownloadAllDependencies(selectedPack, dirPath);
                }
            });

            this.Text = "Cygnus";

            allQueues.Remove(queue);
        }

        private void btnDownload_Click(object sender, EventArgs e)
        {
            DownloadPackage(false);
        }

        private void btnDownloadFully_Click(object sender, EventArgs e)
        {
            DownloadPackage(true);
        }

        /// <summary>
        /// Navigates the WebBrowser control using latest IE engine.
        /// </summary>
        /// <param name="url">The URL to navigate.</param>
        private void NavigateWebBrowser(string url)
        {
            cssFrameDepiction.SetAttribute("src", url.toValidURL());

            ShowDownloadButtons();
        }

        /// <summary>
        /// <para>
        /// SelectionChanged event handler of listPackages.
        /// </para><para>
        /// Basically, first this function navigates to the depiction of
        /// selected package. Then it changes selectedPack variable to make
        /// cssBtnDownload can determine which package was selected.
        /// </para>
        /// </summary>
        private void listPackages_SelectionChanged(object sender, XPTable.Events.SelectionEventArgs e)
        {
            if (e.NewSelectedIndicies.Length == 0) return;

            var selected = e.TableModel.Selections.SelectedItems[0];

            selectedPack = (Package)selected.Tag;

            string depiction = selectedPack.Depiction;

            if (depiction.IsNullOrWhitespace())
            {
                depiction = new Uri(new Uri("http://cydia.saurik.com/package/"), selectedPack.Pkg).ToString();
            }

            NavigateWebBrowser(depiction);
        }

        /// <summary>
        /// Makes zoom controls visible only in Packages tab and autoresizes columns.
        /// </summary>
        private void tabMain_SelectedIndexChanged(object sender, EventArgs e)
        {
            ResizeColumns();

            statusLabelZoom.Visible = trackBarZoom.Visible = (tabMain.SelectedIndex == 1);
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

            statusLabelZoom.Text = "Zoom: {0}%".FormatWith(zoomFactor);

            if (this.webBrowser1.Url == null) return;

            ((SHDocVw.WebBrowser)this.webBrowser1.ActiveXInstance).ExecWB(
                SHDocVw.OLECMDID.OLECMDID_OPTICAL_ZOOM,
                SHDocVw.OLECMDEXECOPT.OLECMDEXECOPT_DONTPROMPTUSER,
                zoomFactor,
                IntPtr.Zero);

        }

        /// <summary>
        /// This prevents WebBrowser from opening (real) Internet Explorer and then
        /// navigates to the clicked link.
        /// </summary>
        private void webBrowser1_NewWindow(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            NavigateWebBrowser(this.webBrowser1.StatusText);
            HideDownloadButtons();
        }

        private void ShowDownloadButtons()
        {
            cssBtnDownload.Style = cssBtnDownloadFully.Style = String.Empty;
            cssBtnBack.Style = "display: none;";
        }

        private void HideDownloadButtons()
        {
            cssBtnDownload.Style = cssBtnDownloadFully.Style = "display: none;";
            cssBtnBack.Style = String.Empty;
        }

        /// <summary>
        /// The scroll event handler of Zoom Trackbar.
        /// Calls ZoomWebBrowser() right away.
        /// </summary>
        private void trackBarZoom_Scroll(object sender, EventArgs e)
        {
            ZoomWebBrowser();
        }

        private void tableQueue_MouseDown(object sender, MouseEventArgs e)
        {
            Row row;
            if (e.Button == System.Windows.Forms.MouseButtons.Right && (row = tableQueue.TableModel.RowAt(e.Y)) != null)
            {
                selectedQueue = (Queue)row.Tag;
                contextMenuQueue.Show(Cursor.Position);
            }
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            allQueues.Remove(selectedQueue);

            int? progressBarPercentage = (int?)selectedQueue.TableRow.Cells[1].Data;
            if (progressBarPercentage.HasValue && progressBarPercentage != 100)
            {
                isDownloadCanceled = true;
                statusBarProgressbar.Visible = statusLabelDownload.Visible = false;
            }

            tableQueue.TableModel.Rows.Remove(selectedQueue.TableRow);
        }

        private void openFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string packName = selectedQueue.TableRow.Cells[0].Text;
            System.Diagnostics.Process.Start("explorer.exe", Path.Combine("debs", ValidFilename(packName)));
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

        private void label5_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/PythEch/Cygnus");
        }

        private void splitContainer1_SplitterMoved(object sender, SplitterEventArgs e)
        {
            ResizeColumns();
        }

        #endregion Methods

    }
}
