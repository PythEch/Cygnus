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
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using XPTable.Models;

namespace Cygnus
{
    partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            InitializeXPTable(); // Ensure XPTable is loaded before Form_Load
        }

        private void InitializeXPTable()
        {
            listPackages.SelectionStyle = SelectionStyle.Grid;
            listPackages.EnableWordWrap = true;

            listPackages.GridLines = GridLines.None;
            ImageColumn col1 = new ImageColumn(String.Empty, 20) { Editable = false };
            TextColumn col2 = new TextColumn() { Editable = false };

            listPackages.ColumnModel = new ColumnModel(new Column[] { col1, col2 });

            listPackages.TableModel = new TableModel();
        }

        #region Subform Helpers

        private void ShowLoadingView(string loadingText, Action action)
        {
            var loadingView = new LoadingView();

            loadingView.label.Text = loadingText;
            loadingView.Text = loadingText;

            Task.Factory.StartNew(action).ContinueWith((prevTask) =>
            {
                this.Invoke((MethodInvoker)delegate
                {
                    loadingView.Close();
                });
            });

            loadingView.ShowDialog();
        }

        private string ShowAlertView(string text, bool isAdd)
        {
            var alertView = new AlertView();

            alertView.txtSource.Text = text;
            alertView.txtSource.SelectionStart = text.Length;
            alertView.errorProvider1.Clear();
            alertView.btnAdd.Visible = isAdd;
            alertView.btnEdit.Visible = !isAdd;

            alertView.ShowDialog();

            return (alertView.canceled ? null : new Uri(alertView.txtSource.Text).ToString());
        }

        #endregion

        #region Load/Close/Resize the Form

        private void MainForm_Load(object sender, EventArgs e)
        {
            if (!Directory.Exists("repos")) Directory.CreateDirectory("repos");

            AddIcons();

            LoadUserSettings();
            ZoomWebBrowser(); // Formats the 'Zoom: {0}%' label
            LoadXMLSettings();

            statusStrip.Items.Add(new ToolStripControlHost(trackBarZoom));
            //Adds zoom trackbar which isn't possible to do with the Designer

            tabMain_SelectedIndexChanged(null, null);
        }

        private void LoadUserSettings()
        {
            var settings = Properties.Settings.Default;
            this.tabMain.SelectedIndex = settings.tabSelectedIndex;
            this.splitContainer1.SplitterDistance = (this.splitContainer1.Width - settings.webBrowserPanelWidth);
            this.Size = settings.formSize;
            this.CenterToScreen(); // the order of this is important
            this.WindowState = settings.windowState;
            this.trackBarZoom.Value = settings.zoomFactor;
        }

        private void SaveUserSettings()
        {
            var settings = Properties.Settings.Default;
            settings.tabSelectedIndex = this.tabMain.SelectedIndex;
            settings.webBrowserPanelWidth = (this.splitContainer1.Width - this.splitContainer1.SplitterDistance);
            if (this.WindowState != FormWindowState.Maximized)
                settings.formSize = this.Size;
            settings.windowState = (this.WindowState == FormWindowState.Maximized) ? FormWindowState.Maximized : FormWindowState.Normal;
            settings.zoomFactor = trackBarZoom.Value;

            settings.Save();
        }

        private void LoadXMLSettings()
        {
            if (!File.Exists("Cygnus.xml"))
                SaveXMLSettings();

            var sources = XDocument.Load("Cygnus.xml").Element("Settings").Elements("Source");
            foreach (var source in sources)
            {
                string url = new Uri(source.Element("URL").Value).ToString();
                string key = RenameIllegalChars(url);
                string label = source.Element("Label").Value;
                ListViewItem item = listSources.Items.Add(key, url, key);
                item.SubItems.Add(source.Element("Size").Value);
                item.Tag = label;

                allRepos.Add(url, label);
            }
        }

        private void SaveXMLSettings()
        {
            XDocument doc = new XDocument(
                new XElement("Settings",
                    from ListViewItem item in listSources.Items
                    select new XElement("Source",
                        new XElement("URL", item.Text),
                        new XElement("Size", item.SubItems[1].Text),
                        new XElement("Label", item.Tag)
                        )
                    )
                );

            doc.Save("Cygnus.xml");
        }

        private void AddIcons() //to Sources tab
        {
            ImageList imgList = new ImageList();
            imgList.ColorDepth = ColorDepth.Depth24Bit;
            imgList.ImageSize = new Size(32, 32);
            foreach (var file in Directory.GetFiles("repos", "*.png"))
            {
                using (Image image = (Image)new Bitmap(file))
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

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            SaveUserSettings();
        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
            ResizeColumns();
        }

        private void ResizeColumns()
        {
            listSources.Columns[0].Width = listSources.Width - 116; // Perfect constant
            listSources.Columns[1].AutoResize(ColumnHeaderAutoResizeStyle.HeaderSize);

            listPackages.ColumnModel.Columns[1].Width = listPackages.Width - 41;
            // I increased Column.MaximumWidth property from 1024 to 8192 using XPTable source code...
        }

        #endregion

        #region Hotkeys

        // TODO: NEEDS MORE HOTKEYS!

        private void listSources_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                btnDelete_Click(null, null);
            }
        }

        private void txtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                btnSearch_Click(null, null);
            }
        }

        #endregion

        #region Button Events

        private void btnAdd_Click(object sender, EventArgs e)
        {
            string url = ShowAlertView("http://", true);

            if (url == null) return; // means it's cancelled

            VerifyRepoURL(url.ToLowerInvariant()); // See APT.cs
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (listSources.SelectedItems.Count > 0)
            {
                ShowAlertView(listSources.SelectedItems[0].Text, false); // FIXME: do something with the returned text
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (listSources.SelectedItems.Count > 0)
            {
                listSources.SelectedItems[0].Remove();
                SaveXMLSettings();
            }
        }

        private void btnReload_Click(object sender, EventArgs e)
        {
            ShowLoadingView("Reloading Data", () =>
            {
                ReloadData();
            });
        }

        #endregion

        #region Packages Tab

        private void listPackages_SelectionChanged(object sender, XPTable.Events.SelectionEventArgs e)
        {
            if (e.NewSelectedIndicies.Length == 0) return;

            var selected = e.TableModel.Selections.SelectedItems[0];

            selectedPack = (Package)selected.Tag;

            string depiction = selectedPack.depiction;

            if (String.IsNullOrEmpty(depiction))
            {
                // IE7 sucks so bad I had to use that 'X-UA-Compatible' tag again.
                webBrowser1.DocumentText = @"<html>
<head><meta http-equiv='X-UA-Compatible' content='IE=edge'></head>
<body>
<h3 style='font-family:Arial; width:320px; height:0px; margin:auto; position:absolute; top:0; left:0; bottom:0; right:0'>No depiction found for this package</h3>
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

        private void btnSearch_Click(object sender, EventArgs e)
        {
            btnDownload.Enabled = true;
            listPackages.BeginUpdate();
            listPackages.ClearAllData();

            var searchList = SearchPackagesByName(txtSearch.Text);

            if (searchList.Count == 0)
            {
                listPackages.NoItemsText = "No packages found...";
                listPackages.EndUpdate();
                return;
            }

            foreach (var pack in searchList)
            {
                AddPkgToList(pack);
            }
            listPackages.EndUpdate();
        }

        static Package selectedPack;
        private void btnDownload_Click(object sender, EventArgs e)
        {
            if (!Directory.Exists("debs")) Directory.CreateDirectory("debs");

            // TODO: Don't invoke LoadingView while downloading
            // Make another GUI and support multiple downloads simultaneously

            Uri downloadURL = new Uri(new Uri(selectedPack.repo.URL), selectedPack.filename);

            ShowLoadingView("Downloading File", () =>
            {
                string dirPath = Path.Combine("debs", CleanFileName(selectedPack.name));
                string filePath = Path.Combine(dirPath, Path.GetFileName(downloadURL.LocalPath));

                if (!Directory.Exists(dirPath)) Directory.CreateDirectory(dirPath);

                DownloadFile(downloadURL, filePath);

                if (selectedPack.depends != null && MessageBox.Show("Do you want to download dependencies of this package?",
                    "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    // TODO: Use a proper way to ask this, e.g use seperate buttons
                    DownloadAllDependencies(selectedPack, dirPath);
                }
            });
        }

        static CellStyle titleStyle = new CellStyle() { Font = new Font("Microsoft Sans Serif", 11) };
        static CellStyle sectionStyle = new CellStyle() { Font = new Font("Microsoft Sans Serif", 7), ForeColor = Color.Gray };
        static CellStyle descriptionStyle = new CellStyle() { ForeColor = Color.Blue };
        private void AddPkgToList(Package pack)
        {
            Row row = new Row() { Height = 18 };
            row.Cells.Add(new Cell()); // using empty cell for now, I'm going to replace this with Cydia-like Section Icons
            Cell cell = new Cell(pack.name, titleStyle) { ForeColor = (pack.paid ? Color.Blue : Color.Black) };
            row.Cells.Add(cell);
            listPackages.TableModel.Rows.Add(row);

            Row subrow = new Row();
            subrow.Cells.Add(new Cell());
            cell = new Cell(String.Format("from {0} ({1})", pack.repo.label, pack.section), sectionStyle);
            subrow.Cells.Add(cell);
            row.SubRows.Add(subrow);

            subrow = new Row();
            subrow.Cells.Add(new Cell());
            cell = new Cell(pack.description, descriptionStyle);
            subrow.Cells.Add(cell);
            row.SubRows.Add(subrow);

            row.Tag = pack;
        }

        private void trackBarZoom_Scroll(object sender, EventArgs e)
        {
            ZoomWebBrowser();
        }

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

        private void ZoomWebBrowser()
        {
            int zoomFactor = (trackBarZoom.Value <= 10) ? (trackBarZoom.Value * 10) : (trackBarZoom.Value == 11) ? 125 : 150;

            statusLabelZoom.Text = String.Format("Zoom: {0}%", zoomFactor);

            if (webBrowser1.Url == null) return;

            // Saurik's site gets stuck at WebBrowserReadyState.Interactive for some reason
            // The correct way is checking against WebBrowserReadyState.Complete
            while (webBrowser1.ReadyState == WebBrowserReadyState.Uninitialized)
                Application.DoEvents();

            ((SHDocVw.WebBrowser)webBrowser1.ActiveXInstance).ExecWB(SHDocVw.OLECMDID.OLECMDID_OPTICAL_ZOOM,
                SHDocVw.OLECMDEXECOPT.OLECMDEXECOPT_DONTPROMPTUSER, zoomFactor, IntPtr.Zero);

            // Thanks to 'bdrajer' for the solution
            // http://stackoverflow.com/a/12910148
        }

        bool zoomedBefore = false;
        private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            if (!zoomedBefore)
            {
                ZoomWebBrowser();
                zoomedBefore = true;
            }
            // Zoom once to load User Settings
        }

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
            //  • And finally, we won't need to detect IE version to set registry value correctly.
            // It'll use the latest version avaiable on the computer automatically
        }

        private void webBrowser1_NewWindow(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            NavigateWebBrowser(webBrowser1.StatusText);
        }

        #endregion

    }
}
