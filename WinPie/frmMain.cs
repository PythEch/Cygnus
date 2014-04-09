// WinPie - Cydia-like APT Client for Windows
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
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using XPTable.Models;

namespace WinPie
{
    public partial class frmMain : Form
    {
        public frmMain()
        {
            InitializeComponent();
        }

        #region Load/Close/Resize the Form

        private void frmMain_Load(object sender, EventArgs e)
        {
            LoadUserSettings();
            LoadXMLSettings();

            InitializeXPTable();

            statusStrip.Items.Add(new ToolStripControlHost(trackBarZoom));

            tabMain_SelectedIndexChanged(sender, e);
        }

        private void LoadUserSettings()
        {
            var settings = Properties.Settings.Default;
            this.tabMain.SelectedIndex = settings.tabSelectedIndex;
            this.splitContainer1.SplitterDistance = (int)(settings.splitterDistanceRatio * this.splitContainer1.Width);
            this.Size = settings.formSize;
            this.WindowState = settings.windowState;
            this.trackBarZoom.Value = settings.zoomFactor;
            ZoomWebBrowser(); // Formats the 'Zoom: {0}%' label

            this.CenterToScreen();
        }

        private void SaveUserSettings()
        {
            var settings = Properties.Settings.Default;
            settings.tabSelectedIndex = this.tabMain.SelectedIndex;
            settings.splitterDistanceRatio = (float)this.splitContainer1.SplitterDistance / (float)this.splitContainer1.Width;
            settings.zoomFactor = trackBarZoom.Value;
            settings.windowState = (this.WindowState == FormWindowState.Maximized ? FormWindowState.Maximized : FormWindowState.Normal);
            settings.Save();
        }

        private void LoadXMLSettings()
        {
            if (!Directory.Exists("repos")) Directory.CreateDirectory("repos");
            AddIcons();

            if (!File.Exists("WinPie.xml"))
            {
                using (var sw = new StreamWriter("WinPie.xml"))
                    sw.Write("<?xml version=\"1.0\" encoding=\"utf-8\"?>\n<Settings>\n</Settings>");
            }

            var sources = XDocument.Load("WinPie.xml").Element("Settings").Elements("Source");
            foreach (var source in sources)
            {
                string url = source.Element("URL").Value.TrimEnd('/') + "/";
                string key = RenameIllegalChars(url);
                ListViewItem item = listSources.Items.Add(key, url, key);
                item.SubItems.Add(source.Element("Size").Value);
                item.Tag = source.Element("Label").Value;
            }
        }

        public void SaveXMLSettings()
        {
            // TODO: Use LINQ TO XML while saving too...
            XElement[] content = new XElement[listSources.Items.Count];
            foreach (ListViewItem item in listSources.Items)
            {
                content[item.Index] = new XElement("Source",
                                new XElement("URL", item.Text),
                                new XElement("Size", item.SubItems[1].Text),
                                new XElement("Label", item.Tag)
                                );
            }

            XDocument xDoc = new XDocument(
                        new XDeclaration("1.0", "UTF-8", null),
                        new XElement("Settings",content));

            using (StringWriter sw = new StringWriter())
            using (XmlWriter xWrite = XmlWriter.Create(sw))
            {
                xDoc.Save(xWrite);
            }

            // Save to Disk
            xDoc.Save("winpie.xml");
        }

        private void AddIcons() //to Sources tab
        {
            ImageList imgList = new ImageList();
            imgList.ColorDepth = ColorDepth.Depth24Bit;
            imgList.ImageSize = new Size(32, 32);
            foreach (var file in Directory.GetFiles("repos", "*.png"))
            {
                var image = (Image)new Bitmap(file);
                Bitmap scaled = new Bitmap(32, 32);
                using (Graphics g = Graphics.FromImage(scaled))
                {
                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.High;
                    g.DrawImage(image, new Rectangle(0, 0, 32, 32));
                }
                imgList.Images.Add(Path.GetFileNameWithoutExtension(file), new Bitmap(scaled, 32, 32));
                image.Dispose();
                scaled.Dispose();

            }
            this.listSources.SmallImageList = imgList;
        }

        private void InitializeXPTable()
        {
            listPackages.SelectionStyle = SelectionStyle.Grid;
            listPackages.EnableWordWrap = true;

            listPackages.GridLines = GridLines.None;
            ImageColumn col1 = new ImageColumn(String.Empty, 20);
            TextColumn col2 = new TextColumn(String.Empty, 200);
            listPackages.ColumnModel = new ColumnModel(new Column[] { col1, col2 });

            listPackages.TableModel = new TableModel();
        }

        private void frmMain_FormClosed(object sender, FormClosedEventArgs e)
        {
            SaveUserSettings();
        }

        private void frmMain_Resize(object sender, EventArgs e)
        {
            ResizeColumns();
        }

        private void ResizeColumns()
        {
            listSources.Columns[0].Width = listSources.Width - 116; // Fancy stuff
            listSources.Columns[1].AutoResize(ColumnHeaderAutoResizeStyle.HeaderSize);
        }

        #endregion

        #region Local I/O Stuff

        private string RenameIllegalChars(string text)
        {
            return text.Replace("http://", "").Replace("www.", "").TrimEnd('/').Replace("/", "_");
        }

        private static void ExtractResource(string resName, string path)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();

            using (Stream resourceStream = assembly.GetManifestResourceStream(resName))
            using (BinaryWriter writer = new BinaryWriter(File.Create(path)))
            {
                byte[] resourcesBuffer = new byte[resourceStream.Length];

                resourceStream.Read(resourcesBuffer, 0, resourcesBuffer.Length);

                writer.Write(resourcesBuffer);
            }
        }

        // Very fast and easy way to decompress bz2 archives
        // Managed DLLs are just too slow
        private void ExtractZip(string zipName)
        {
            if (!File.Exists("repos\\7za.exe")) ExtractResource("WinPie.7za.exe", "repos\\7za.exe");

            ProcessStartInfo startInfo = new ProcessStartInfo("repos\\7za.exe", "x -orepos -y repos\\" + zipName);
            startInfo.RedirectStandardOutput = true;
            startInfo.UseShellExecute = false;
            startInfo.CreateNoWindow = true;

            Process.Start(startInfo).WaitForExit();
        }

        #endregion

        #region Remote I/O Stuff

        private bool RemoteFileExists(Uri url)
        {
            try
            {
                HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
                request.Method = WebRequestMethods.Http.Head;
                request.UserAgent = "Cydia/0.9 CFNetwork/548.1.4 Darwin/11.0.0";
                request.AllowAutoRedirect = false;

                using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                {
                    return (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Found);
                }
            }
            catch
            {
                return false;
            }
        }

        private void DownloadFile(Uri uri, string path)
        {
            using (FileStream fStream = File.Create(path))
            {
                DownloadFile(uri, fStream);
            }
        }

        private void DownloadFile(Uri uri, Stream stream)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            request.Method = WebRequestMethods.Http.Get;
            request.UserAgent = "Telesphoreo APT-HTTP/1.0.592";
            //request.Headers.Add("X-Firmware", "7.0.2");
            //request.Headers.Add("X-Machine", "iPhone6,2");
            request.Headers.Add("X-Unique-ID", "0000000000000000000000000000000000000000");
            //some repos require this for some reason
            //TODO: use pseudo-random ID to prevent being banned from some repos in the future

            using (Stream responseStream = request.GetResponse().GetResponseStream())
            {
                byte[] buffer = new byte[4096];
                int bytesRead = 0;

                while ((bytesRead = responseStream.Read(buffer, 0, buffer.Length)) != 0)
                {
                    stream.Write(buffer, 0, bytesRead);
                }
            }

        }

        #endregion

        #region Cydia/APT Related

        // dists/stable, main/binary-iphoneos-arm/
        private readonly string[] oldRepos = new string[]
        { "http://apt.thebigboss.org/repofiles/cydia/", "http://apt.modmyi.com/", "http://cydia.zodttd.com/repo/cydia/" };

        // apt-get update
        private void ReloadData()
        {
            if (!Directory.Exists("repos")) Directory.CreateDirectory("repos");

            foreach (ListViewItem item in listSources.Items)
            {
                string repo = item.Text;
                Uri packagesUri;
                bool bz2Archive = true;

                if (oldRepos.Contains(repo))
                {
                    packagesUri = new Uri(new Uri(repo), "dists/stable/main/binary-iphoneos-arm/Packages.bz2");
                    if (!RemoteFileExists(packagesUri))
                        continue;
                }
                else
                {
                    packagesUri = new Uri(new Uri(repo), "Packages.bz2");
                    if (!RemoteFileExists(packagesUri))
                    {
                        packagesUri = new Uri(new Uri(repo), "Packages.gz");
                        bz2Archive = false;
                        if (!RemoteFileExists(packagesUri))
                            continue;
                    }
                }

                string fileName = RenameIllegalChars(repo) + "_Packages" + (bz2Archive ? ".bz2" : ".gz");

                DownloadFile(packagesUri, "repos\\" + fileName);

                ExtractZip(fileName);
                File.Delete("repos\\" + fileName);

            }
        }

        private void DownloadCydiaIcon(string repo)
        {
            if (!Directory.Exists("repos")) Directory.CreateDirectory("repos");

            Uri iconUri;

            if (oldRepos.Contains(repo))
                iconUri = new Uri(new Uri(repo), "dists/stable/CydiaIcon.png");
            else
                iconUri = new Uri(new Uri(repo), "CydiaIcon.png");

            if (RemoteFileExists(iconUri))
                DownloadFile(iconUri, "repos\\" + RenameIllegalChars(repo) + ".png");

        }

        // Get Label of repo
        private string DownloadRelease(string repo)
        {
            Uri releaseUri;
            string label = null;

            if (oldRepos.Contains(repo))
                releaseUri = new Uri(new Uri(repo), "dists/stable/Release");
            else
                releaseUri = new Uri(new Uri(repo), "Release");

            using (MemoryStream mStream = new MemoryStream())
            {
                DownloadFile(releaseUri, mStream);
                mStream.Position = 0;
                using (StreamReader sReader = new StreamReader(mStream))
                {
                    string line;
                    while ((line = sReader.ReadLine()) != null)
                    {
                        if (line.StartsWith("Label:"))
                        {
                            label = line.Substring(7);
                            break;
                        }
                    }
                }
            }

            return label.Trim();
        }

        private void VerifyURL(Uri uri)
        {
            bool verified = false;
            string label = null;
            Forms.loadingView.label.Text = "Verifying URL";

            Task task = Task.Factory.StartNew(() =>
            {
                if (oldRepos.Contains(uri.ToString()) || RemoteFileExists(new Uri(uri, "Packages.bz2")) || RemoteFileExists(new Uri(uri, "Packages.gz")))
                {
                    verified = true;
                    DownloadCydiaIcon(uri.ToString());
                    label = DownloadRelease(uri.ToString());
                }
            }).ContinueWith((prevTask) =>
            {
                this.Invoke((Action)delegate
                {
                    Forms.loadingView.Close();
                });
            });
            Forms.loadingView.ShowDialog();

            if (verified)
            {
                ListViewItem item = listSources.Items.Add(uri.ToString());
                item.SubItems.Add("?");
                item.Tag = label;
                SaveXMLSettings();
                listSources.Items.Clear();
                LoadXMLSettings();
            }
            else
            {
                MessageBox.Show("Verification failed!"); // FIXME: Display a proper error message
            }
        }

        // Structs are very efficient
        private struct Package
        {
            public string pkg, name, version, description, filename, section, depends, md5sum, depiction;
            public bool paid;
            public int repoIndex;
        }

        List<string> repos;
        List<string> labels;
        private List<Package> ParseData()
        {
            repos = new List<string> { };
            labels = new List<string> { };

            var packs = new List<Package> { };

            foreach (var file in Directory.GetFiles("repos", "*_Packages"))
            {
                var query = File.ReadLines(file).Select(line => line.Split(new char[] { ':' }, 2));
                // Believe me String.Split is the most resource hungry thing you can see here

                var pack = new Package();

                string unRemoveIllegalChars = "http://" + Path.GetFileName(file).Replace("_Packages", String.Empty).Replace("_", "/");
                repos.Add(unRemoveIllegalChars);

                // FIXME: I'm using a super ugly way to do this. I don't have much time
                foreach (ListViewItem item in listSources.Items)
                {
                    if (item.Text.TrimEnd('/') == unRemoveIllegalChars)
                    {
                        string _label = item.Tag.ToString();
                        labels.Add(!String.IsNullOrEmpty(_label) ? _label : unRemoveIllegalChars);
                        break;
                    }
                }

                foreach (var item in query)
                {
                    if (String.IsNullOrWhiteSpace(item[0]))
                    {
                        if (pack.name != null)
                            packs.Add(pack);
                        pack = new Package();
                    }
                    else if (item.Length == 2)
                    {
                        item[1] = item[1].Trim();

                        pack.repoIndex = repos.Count - 1;
                        switch (item[0].ToLower())
                        {
                            case "package":
                                pack.pkg = item[1];
                                break;
                            case "name":
                                pack.name = item[1];
                                break;
                            case "version":
                                pack.version = item[1];
                                break;
                            case "description":
                                pack.description = item[1];
                                break;
                            case "filename":
                                pack.filename = item[1];
                                break;
                            case "section":
                                pack.section = item[1];
                                break;
                            case "depends":
                                pack.depends = item[1];
                                break;
                            case "md5sum":
                                pack.md5sum = item[1];
                                break;
                            case "depiction":
                                pack.depiction = item[1];
                                break;
                            case "tag":
                                pack.paid = (item[1] == "cydia::commercial");
                                break;
                        }
                    }
                }
                // Add the last one
                // Why? There is a bug I don't remember
                if (pack.pkg != null && pack.name != null)
                    packs.Add(pack);

            }

            return packs;
        }

        /* COMPARISON */
        /* This field is not used yet */

        // Ignores many unnecessary fields that is not used for comparison
        // Uses less resources
        private struct FastPackage
        {
            public string pkg, version;
        }

        private List<FastPackage> FastParsePkg(string path)
        {
            var ret = new List<FastPackage> { };
            var pack = new FastPackage();
            foreach (string line in File.ReadLines(path))
            {
                if (line.StartsWith("Package:"))
                    pack.pkg = line.Substring(9);
                else if (line.StartsWith("Version:"))
                {
                    pack.version = line.Substring(9);
                    ret.Add(pack);
                }
            }
            return ret;
        }


        private void Compare(string oldFile, string newFile)
        {
            // TODO: Create a reasonable UI to show changes...
            var temp = FastParsePkg(newFile).Except(FastParsePkg(oldFile));
            foreach (var item in temp)
            {
                MessageBox.Show(item.pkg+"\n"+item.version);
            }
        }

        #endregion

        #region Hotkeys

        // TODO: NEEDS MORE HOTKEYS!

        private void listSources_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                btnDelete_Click(sender, EventArgs.Empty);
            }
        }

        #endregion

        #region Button Events

        private void btnAdd_Click(object sender, EventArgs e)
        {
            Forms.uiAlertView.txtSource.Text = "http://";
            Forms.uiAlertView.txtSource.SelectionStart = 7;
            Forms.uiAlertView.errorProvider1.Clear();
            Forms.uiAlertView.btnAdd.Visible = true;
            Forms.uiAlertView.btnEdit.Visible = false;
            if (!Forms.uiAlertView.Visible)
            {
                Forms.uiAlertView.ShowDialog();
                if (!Forms.uiAlertView.canceled)
                    VerifyURL(new Uri(Forms.uiAlertView.txtSource.Text.TrimEnd('/') + "/"));
            }
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (listSources.SelectedItems.Count > 0)
            {
                Forms.uiAlertView.txtSource.Text = listSources.SelectedItems[0].Text;
                Forms.uiAlertView.txtSource.SelectionStart = listSources.SelectedItems[0].Text.Length;
                Forms.uiAlertView.errorProvider1.Clear();
                Forms.uiAlertView.btnAdd.Visible = false;
                Forms.uiAlertView.btnEdit.Visible = true;
                if (!Forms.uiAlertView.Visible)
                    Forms.uiAlertView.ShowDialog();
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
            Forms.loadingView.label.Text = "Reloading Data";

            Task task = Task.Factory.StartNew(() =>
            {
                ReloadData();
            }).ContinueWith((prevTask) =>
            {
                this.Invoke((Action)delegate
                {
                    Forms.loadingView.Close();
                });
            });

            Forms.loadingView.ShowDialog();
        }

        #endregion

        #region Packages Tab

        private void listPackages_SelectionChanged(object sender, XPTable.Events.SelectionEventArgs e)
        {
            // Ugly workaround
            if (e.NewSelectedIndicies.Length == 0) return;
            int ind = e.NewSelectedIndicies[0];
            switch (ind % 3)
            {
                case 0:
                    e.TableModel.Selections.AddCells(ind + 1, 1, ind + 2, 1);
                    break;
                case 1:
                    e.TableModel.Selections.AddCells(ind - 1, 1, ind + 1, 1);
                    break;
                case 2:
                    e.TableModel.Selections.AddCells(ind - 2, 1, ind - 1, 1);
                    break;
            }
            var urls = (string[])e.TableModel.Selections.SelectedItems[0].Tag;
            //webBrowser1.Navigate(urls[0]);

            string depiction = urls[0];

            // TODO: Disable download button if package is paid
            // TODO2: Use user-configured Cydia IDs to download paid packages
            downloadURL = urls[1];

            if (String.IsNullOrEmpty(depiction))
            {
                // IE7 sucks so bad I had to use that 'X-UA-Compatible' tag again.
                webBrowser1.DocumentText = @"<html>
<head><meta http-equiv='X-UA-Compatible' content='IE=edge'></head>
<body>
<h3 style='font-family:Arial; width:320px; height:0px; margin:auto; position:absolute; top:0; left:0; bottom:0; right:0'>No depiction found for this package</h3>
</body>
</html>";
                return;
            }

            // FIXME: Find a way to set Content-Type to 'text/html'
            // Stupid IE tries to download iframe src sometimes

            if (urls[0].EndsWith("/"))
                urls[0] += "index.html";

            // SUPER HACKY WORKAROUND TO SOLVE STUPID WebBrowser PROBLEM
            webBrowser1.DocumentText = @"<html>
<head><meta http-equiv='X-UA-Compatible' content='IE=edge'></head>
<body>
<iframe src='" + urls[0] + @"' style='border:0; position:absolute; top:0; left:0; right:0; bottom:0; width:100%; height:100%'></iframe>
</body>
</html>";
            // Problem:
            //     ieframe.dll based WebBrowser control loads webpages using IE7 compatibility mode by default
            //
            // I've found a workaround by myself. This solves three problems:
            //  • We won't need to have Administrator privileges and do a stupid registry trick:
            // http://msdn.microsoft.com/en-us/library/ie/ee330730%28v=vs.85%29.aspx
            //
            //  • And other than that, for some reason, this WebBrowser control doesn't render
            // Modmyi even after that trick.
            //
            //  • And finally, we won't need to detect IE version to set registry value properly.
            // It'll use the latest version avaiable on the computer automatically

        }


        private void btnSearch_Click(object sender, EventArgs e)
        {
            btnDownload.Enabled = true;
            listPackages.BeginUpdate();
            listPackages.ClearAllData();
            string searchVal = txtSearch.Text;
            var search = ParseData().Where(pack => pack.name.ToLower().Contains(searchVal.ToLower())).ToArray(); // FIXME: Don't use ToArray()

            if (search.Length == 0)
            {
                listPackages.NoItemsText = "No packages found...";
                listPackages.EndUpdate();
                return;
            }

            for (int i = 0; i < (search.Length < 250 ? search.Length: 250); i++)
            {
                var pack = search[i];
                AddPkgToList(pack.name, repos[pack.repoIndex], labels[pack.repoIndex],
                    pack.filename, pack.section, pack.description, pack.depiction, pack.paid);
            }
            listPackages.EndUpdate();
        }

        string downloadURL = "";
        private void btnDownload_Click(object sender, EventArgs e)
        {

            if (!Directory.Exists("debs")) Directory.CreateDirectory("debs");


            // TODO: Don't invoke LoadingView while downloading
            // Make another GUI and support multiple downloads simultaneously
            var dlURI = new Uri(downloadURL);

            Forms.loadingView.label.Text = "Downloading File";

            Task task = Task.Factory.StartNew(() =>
            {
                DownloadFile(dlURI, "debs\\" + Path.GetFileName(dlURI.LocalPath));
            }).ContinueWith((prevTask) =>
            {
                this.Invoke((Action)delegate
                {
                    Forms.loadingView.Close();
                });
            });

            Forms.loadingView.ShowDialog();
        }

        Font bigFont = new Font("Microsoft Sans Serif", 11);
        Font smallFont = new Font("Microsoft Sans Serif", 7);
        private void AddPkgToList(string name, string repoURL, string repoLabel, string filename, string section, string description, string depiction, bool paid)
        {
            Row row = new Row();
            row.Cells.Add(new Cell());
            Cell cell = new Cell(name);
            cell.WordWrap = true;
            cell.Editable = false;
            cell.Font = bigFont;
            cell.ForeColor = (paid ? Color.Blue : Color.Black);
            row.Cells.Add(cell);
            listPackages.TableModel.Rows.Add(row);

            Row subrow = new Row();
            subrow.Cells.Add(new Cell());
            cell = new Cell(String.Format("from {0} ({1})", repoLabel, section));
            cell.WordWrap = true;
            cell.Editable = false;
            cell.Font = smallFont;
            cell.ForeColor = Color.Gray;
            subrow.Cells.Add(cell);
            row.SubRows.Add(subrow);

            subrow = new Row();
            subrow.Cells.Add(new Cell());
            cell = new Cell(description);
            cell.Editable = false;
            cell.ForeColor = Color.Blue;
            subrow.Cells.Add(cell);
            row.SubRows.Add(subrow);

            row.Tag = new string[] { depiction, repoURL + "/" + filename };
        }

        private void trackBarZoom_Scroll(object sender, EventArgs e)
        {
            ZoomWebBrowser();
        }

        private void tabMain_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (tabMain.SelectedIndex)
            {
                case 0: // Sources
                    ResizeColumns();
                    statusLabelZoom.Visible = false;
                    trackBarZoom.Visible = false;
                    break;
                case 1: // Packages
                    statusLabelZoom.Visible = true;
                    trackBarZoom.Visible = true;
                    break;
                case 2: // Changes
                    statusLabelZoom.Visible = false;
                    trackBarZoom.Visible = false;
                    break;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            MessageBox.Show(listPackages.ColumnModel.Columns[2].Width.ToString());
        }

        private void ZoomWebBrowser()
        {
            int zoomFactor = trackBarZoom.Value <= 10 ? trackBarZoom.Value * 10 : (100 + (trackBarZoom.Value - 10)*25);

            statusLabelZoom.Text = String.Format("Zoom: {0}%", zoomFactor);

            if (webBrowser1.Url == null) return;
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
        }

        #endregion
    }
}
