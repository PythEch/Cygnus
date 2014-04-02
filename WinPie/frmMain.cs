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
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Text;
using Microsoft.Win32;
using System.Reflection;
using System.Diagnostics;
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

        #region Form Events

        private void frmMain_Load(object sender, EventArgs e)
        {
            frmMain_Resize(sender, e);
            LoadSettings();

            //FIXME
            button7_Click(sender, e);
        }

        private void frmMain_Resize(object sender, EventArgs e)
        {
            listSources.Columns[0].Width = listSources.Width - 116; // Fancy stuff
            listSources.Columns[1].AutoResize(ColumnHeaderAutoResizeStyle.HeaderSize);
        }
        #endregion

        #region Commonly Used

        private string RenameIllegalChars(string text)
        {
            text = text.Replace("http://", "").Replace("www.", "").TrimEnd('/');
            foreach (var character in Path.GetInvalidFileNameChars())
                text = text.Replace(character, '_');
            return text;
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

        #region Web Requests

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
                SaveSettings();
                listSources.Items.Clear();
                LoadSettings();
            }
            else
            {
                MessageBox.Show("Verification failed!"); // FIXME: Display a proper error message
            }
        }

        #endregion

        #region Settings Handlers

        // dists/stable, main/binary-iphoneos-arm/
        private readonly string[] oldRepos = new string[]
        { "http://apt.thebigboss.org/repofiles/cydia/", "http://apt.modmyi.com/", "http://cydia.zodttd.com/repo/cydia/" };

        private void AddIcons()
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

        private void LoadSettings()
        {

            if (!Directory.Exists("repos")) Directory.CreateDirectory("repos");
            AddIcons();

            var sources = XElement.Load("winpie.xml").Elements("Source");
            foreach (var source in sources)
            {
                string url = source.Element("URL").Value.TrimEnd('/') + "/";
                string key = RenameIllegalChars(url);
                ListViewItem item = listSources.Items.Add(key, url, key);
                item.SubItems.Add(source.Element("Size").Value);
                item.Tag = source.Element("Label").Value;
            }

        }

        public void SaveSettings()
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
                SaveSettings();
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

        #region APT Related Stuff

        // Structs are very efficient
        private struct Package
        {
            public string pkg, name, version, description, filename, section, depends, md5sum, depiction;
            public bool paid;
            public int repoIndex;
        }



        List<string> repos;
        private List<Package> ParseData()
        {

            repos = new List<string> { };

            var packs = new List<Package> { };
          
            foreach (var file in Directory.GetFiles("repos", "*_Packages"))
            {
                var query = File.ReadLines(file).Select(line => line.Split(new char[] { ':' }, 2));
                // Believe me String.Split is the most resource hungry thing you can see here

                var pack = new Package();

                repos.Add("http://" + Path.GetFileName(file).Replace("_Packages", String.Empty).Replace("_", "/"));

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

        Font bigFont = new Font("Microsoft Sans Serif", 11);
        Font smallFont = new Font("Microsoft Sans Serif", 7);
        private void AddPkgToList(TableModel table, string name, string repo, string filename, string section, string description, string depiction, bool paid)
        {
            Row row = new Row();
            row.Cells.Add(new Cell());
            Cell cell = new Cell(name);
            cell.WordWrap = true;
            cell.Editable = false;
            cell.Font = bigFont;
            cell.ForeColor = (paid ? Color.Blue : Color.Black);
            row.Cells.Add(cell);
            table.Rows.Add(row);

            // Add a sub-row that shows just the email subject in grey (single line only)
            Row subrow = new Row();
            subrow.Cells.Add(new Cell());
            cell = new Cell(String.Format("from {0} ({1})", repo, section));
            cell.WordWrap = true;
            cell.Editable = false;
            cell.Font = smallFont;
            cell.ForeColor = Color.Gray;
            cell.ColSpan = 2;
            subrow.Cells.Add(cell);
            row.SubRows.Add(subrow);

            // Add a sub-row that shows just a preview of the email body in blue, and wraps too
            subrow = new Row();
            subrow.Cells.Add(new Cell());
            cell = new Cell(description);
            cell.Editable = false;
            cell.ForeColor = Color.Blue;
            cell.ColSpan = 2;
            subrow.Cells.Add(cell);
            row.SubRows.Add(subrow);

            row.Tag = new string[] {depiction, repo + "/" + filename};
        }

        private void button7_Click(object sender, EventArgs e)
        {
            Table table = listPackages;       // The Table control on a form - already initialised
            table.SelectionStyle = SelectionStyle.Grid;
            table.BeginUpdate();
            table.EnableWordWrap = true;    // If false, then Cell.WordWrap is ignored

            table.GridLines = GridLines.None;
            ImageColumn col1 = new ImageColumn("", 20);
            TextColumn col2 = new TextColumn("", 200);
            table.ColumnModel = new ColumnModel(new Column[] { col1, col2});

            TableModel model = new TableModel();
            AddPkgToList(model, "Null", "Null", "Null", "Null", "Null", "http://Null", false); //FIXME: Find a way to avoid crash without doing this
            table.TableModel = model;

            table.EndUpdate();

        }

        private void listPackages_SelectionChanged(object sender, XPTable.Events.SelectionEventArgs e)
        {
            // Ugly workaround
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
            webBrowser1.Navigate(urls[0]);

            // TODO: Disable download button if package is paid
            // TODO2: Use user-entered Cydia IDs to download paid packages

            downloadURL = urls[1];
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            var model = listPackages.TableModel;
            listPackages.BeginUpdate();
            model.Rows.Clear();
            string searchVal = txtSearch.Text;
            var search = ParseData().Where(pack => pack.name.ToLower().Contains(searchVal.ToLower())).ToArray();
            for (int i = 0; i < (search.Length < 250 ? search.Length: 250); i++)
            {
                var pack = search[i];
                AddPkgToList(model, pack.name, repos[pack.repoIndex], pack.filename, pack.section, pack.description, pack.depiction, pack.paid);
            }
            listPackages.EndUpdate();
        }

        string downloadURL = "";
        private void btnDownload_Click(object sender, EventArgs e)
        {

            if (!Directory.Exists("debs")) Directory.CreateDirectory("debs");


            //MessageBox.Show(downloadURL);
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
    }
}
