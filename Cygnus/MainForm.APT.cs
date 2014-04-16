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
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Cygnus
{
    partial class MainForm
    {
        #region Local I/O Stuff

        private static string CleanFileName(string fileName)
        {
            return Path.GetInvalidFileNameChars().Aggregate(fileName, (current, c) => current.Replace(c.ToString(), string.Empty));
        }

        private static string RenameIllegalChars(string text)
        {
            return text.Replace("http://", String.Empty).Replace("www.", String.Empty).TrimEnd('/').Replace('/', '_');
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
        private static void ExtractZip(string zipName)
        {
            if (!File.Exists("repos\\7za.exe")) ExtractResource("Cygnus.7za.exe", "repos\\7za.exe");

            ProcessStartInfo startInfo = new ProcessStartInfo("repos\\7za.exe", "x -orepos -y repos\\" + zipName);
            startInfo.RedirectStandardOutput = true;
            startInfo.UseShellExecute = false;
            startInfo.CreateNoWindow = true;

            Process.Start(startInfo).WaitForExit();
        }

        #endregion

        #region Remote I/O Stuff

        private static bool RemoteFileExists(Uri url)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = WebRequestMethods.Http.Head;
                request.UserAgent = "Cydia/0.9 CFNetwork/548.1.4 Darwin/11.0.0";
                request.AllowAutoRedirect = false;

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    return (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Found);
                }
            }
            catch
            {
                return false;
            }
        }

        private static void DownloadFile(Uri uri, string path)
        {
            using (FileStream fStream = File.Create(path))
            {
                DownloadFile(uri, fStream);
            }
        }

        private static void DownloadFile(Uri uri, Stream stream)
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

        #region Add Repo

        // ReloadData: dists/stable/main/binary-iphoneos-arm/
        // DownloadCydiaIcon, DownloadRelease: dists/stable/

        static readonly string[] oldRepos = new string[] 
        { 
            "http://apt.thebigboss.org/repofiles/cydia/", "http://apt.modmyi.com/", "http://cydia.zodttd.com/repo/cydia/",
            "http://apt.saurik.com/"
        };

        // Saurik's repo is an exception here, it uses dists/ios instead...
        // Yo dawg, I heard you like exceptions, so I put an exception in yo exception array!

        // apt-get update
        private static void ReloadData()
        {
            if (!Directory.Exists("repos")) Directory.CreateDirectory("repos");

            foreach (var k in allRepos)
            {
                string repo = k.Key;
                Uri packagesUri;
                bool bz2Archive = true;

                if (repo == "http://apt.saurik.com/")
                {
                    packagesUri = new Uri(new Uri(repo), "dists/ios/main/binary-iphoneos-arm/Packages.bz2");
                    if (!RemoteFileExists(packagesUri))
                        continue;
                }
                else if (oldRepos.Contains(repo))
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

                string fileName = RenameIllegalChars(repo) + ".Packages" + (bz2Archive ? ".bz2" : ".gz");

                DownloadFile(packagesUri, "repos\\" + fileName);

                ExtractZip(fileName);
                File.Delete("repos\\" + fileName);

            }
        }

        private static void DownloadCydiaIcon(string repo)
        {
            if (!Directory.Exists("repos")) Directory.CreateDirectory("repos");

            Uri iconUri;

            if (repo == "http://apt.saurik.com/")
                iconUri = new Uri(new Uri(repo), "dists/ios/CydiaIcon.png");
            else if (oldRepos.Contains(repo))
                iconUri = new Uri(new Uri(repo), "dists/stable/CydiaIcon.png");
            else
                iconUri = new Uri(new Uri(repo), "CydiaIcon.png");

            if (RemoteFileExists(iconUri))
            {
                DownloadFile(iconUri, "repos\\" + RenameIllegalChars(repo) + ".png");
            }

        }

        // Get Label of repo
        private static string DownloadRelease(string repo)
        {
            Uri releaseUri;
            string label = null;

            if (repo == "http://apt.saurik.com/")
                releaseUri = new Uri(new Uri(repo), "dists/ios/Release");
            else if (oldRepos.Contains(repo))
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

        // Tries to correct incorrect URLs of well known sources
        // So finally you can add thebigboss to your sources without hassle!
        static Dictionary<string, string> knownSources = new Dictionary<string, string> 
        {
            { "bigboss", "http://apt.thebigboss.org/repofiles/cydia/" }, // Y U NO CREATE SYMLINK :(
            { "zodttd", "http://cydia.zodttd.com/repo/cydia/" },
            { "modmyi", "http://apt.modmyi.com/" },
            { "saurik", "http://apt.saurik.com/" },
            { "ultrasn0w", "http://repo666.ultrasn0w.com/" },
            { "rpetri", "http://rpetri.ch/repo/" },
            { "filippobiga", "http://filippobiga.me/repo/" },
            { "getdelta", "http://getdelta.co/" },
            { "if0rce", "http://apt.if0rce.com/" },
            { "angelxwind", "http://cydia.angelxwind.net/" },
            { "radare", "http://cydia.radare.org/" },
            { "pushfix", "http://cydia.pushfix.info/"}
        };

        private void VerifyRepoURL(string url)
        {
            if (new Uri(url).ToString() != "http://cydiabetas.zodttd.com/") // zodttd has two repos actually
            {
                foreach (var source in knownSources)
                {
                    if (url.Contains(source.Key)) // I could use Regex but whatever...
                    {
                        url = source.Value;
                        break;
                    }
                }
            }

            bool verified = false;
            string label = null;

            Uri uri = new Uri(url);

            ShowLoadingView("Verifying URL", () =>
            {
                if (oldRepos.Contains(uri.ToString()) || RemoteFileExists(new Uri(uri, "Packages.bz2")) || RemoteFileExists(new Uri(uri, "Packages.gz")))
                {
                    verified = true;
                    DownloadCydiaIcon(uri.ToString());
                    label = DownloadRelease(uri.ToString());
                }
            });

            if (verified)
            {
                ListViewItem item = listSources.Items.Add(uri.ToString());
                item.SubItems.Add("?");
                item.Tag = label;
                SaveXMLSettings();
                listSources.Items.Clear();
                AddIcons();
                LoadXMLSettings();
            }
            else
            {
                MessageBox.Show(uri.ToString() + " is not a valid Cydia repository!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }
        #endregion

        #region Search/Download Packages

        static Dictionary<string, string> allRepos = new Dictionary<string, string> { };

        // Value type
        private struct Package
        {
            public string pkg, name, version, description, depends, filename, section, md5sum, depiction;
            public bool paid;
            public Repo repo;
        }

        // Reference type
        private class Repo
        {
            public string URL, label;
        }

        // Can be optimized further, 
        // like removing the Select() and checking if the package name contains the argument
        // on the fly and then adding it to the 'packages' List<>
        //
        // String.Split bites my CPU but, do we need optimization though? 
        // It's already a lot faster than iOS/Cydia on my cheap dual core computer
        private static List<Package> SearchPackagesByName(string packageNameToSearch)
        {
            packageNameToSearch = packageNameToSearch.ToLowerInvariant();

            var packages = new List<Package> { };

            foreach (var k in allRepos)
            {
                string repoURL = k.Key;
                string repoLabel = k.Value;
                string filename = "repos\\" + RenameIllegalChars(repoURL) + ".Packages";

                if (!File.Exists(filename))
                {
                    MessageBox.Show("Could not find file: " + filename, "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
                }

                Repo repoStruct = new Repo() { URL = repoURL, label = repoLabel };
                Package pack = new Package();

                var query = File.ReadLines(filename).Select(line => line.Split(new char[] { ':' }, 2));

                foreach (var substrings in query)
                {
                    if (packages.Count >= 250) // Hardcoded limit
                        break;

                    string key = substrings[0];

                    if (substrings.Length == 2)
                    {
                        string value = substrings[1].Trim();

                        switch (key.Trim())
                        {
                            case "Package":
                                pack.pkg = value;
                                break;
                            case "Name":
                                pack.name = value;
                                break;
                            case "Version":
                                pack.version = value;
                                break;
                            case "Description":
                                pack.description = value;
                                break;
                            case "Filename":
                                pack.filename = value;
                                break;
                            case "Section":
                                pack.section = value;
                                break;
                            case "Pre-Depends":
                            case "Depends":
                                pack.depends = (pack.depends == null) ? value : (pack.depends + "," + value);
                                break;
                            case "MD5sum":
                                pack.md5sum = value;
                                break;
                            case "Depiction":
                                pack.depiction = value;
                                break;
                            case "Tag":
                                pack.paid = value.Contains("cydia::commercial");
                                break;
                        }
                    }
                    else if (String.IsNullOrWhiteSpace(key))
                    {
                        if (!String.IsNullOrWhiteSpace(pack.name) && pack.name.ToLowerInvariant().Contains(packageNameToSearch))
                        {
                            pack.repo = repoStruct;
                            packages.Add(pack);
                        }

                        pack = new Package();
                    }
                } // end foreach (var substrings in query)

                // Add the last one if we missed one
                if (!String.IsNullOrWhiteSpace(pack.pkg) && String.IsNullOrWhiteSpace(pack.name) && pack.name.ToLowerInvariant().Contains(packageNameToSearch))
                {
                    packages.Add(pack);
                }
            } // end foreach (ListViewItem item in listSources.Items)

            return packages;
        }

        // This is a bit against DRY
        // Since this is a compiled language, I don't think that's too much a problem.
        private static Package SearchPackagesByID(string packageIDToSearch)
        {
            packageIDToSearch = packageIDToSearch.ToLowerInvariant();

            Package pack = new Package();

            bool found = false;

            foreach (var k in allRepos)
            {
                if (found) break;

                string repoURL = k.Key;
                string filename = "repos\\" + RenameIllegalChars(repoURL) + ".Packages";

                if (!File.Exists(filename))
                {
                    MessageBox.Show("Could not find file: " + filename, "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
                }

                Repo repoStruct = new Repo() { URL = repoURL };

                foreach (var line in File.ReadLines(filename))
                {
                    if (!found)
                    {
                        if (line.StartsWith("Package:") && line.Substring(9) == packageIDToSearch)
                        {
                            found = true;
                            pack.pkg = packageIDToSearch;
                        }
                    }
                    else
                    {
                        string[] substrings = line.Split(new char[] { ':' }, 2);

                        if (substrings.Length != 2) // End of Package
                        {
                            pack.repo = repoStruct;
                            break;
                        }

                        string key = substrings[0];
                        string value = substrings[1].Trim();

                        switch (key.Trim())
                        {
                            case "Filename":
                                pack.filename = value;
                                break;
                            case "Pre-Depends":
                            case "Depends":
                                pack.depends = (pack.depends == null) ? value : (pack.depends + "," + value);
                                break;
                            case "MD5sum":
                                pack.md5sum = value;
                                break;
                        }
                    }
                }
            }

            return pack;
        }

        static readonly string[] defaultPackages = 
        {
            "apr-lib",
            "apt7-key",
            "apt7-lib",
            "base",
            "org.thebigboss.repo.icons",
            "bash",
            "bzip2",
            "coreutils-bin",
            "cydia",
            "cydia-lproj",
            "darwintools",
            "dpkg",
            "debianutils",
            "diffutils",
            "com.evad3rs.evasi0n7",
            "findutils",
            "firmware",
            "gnupg",
            "grep",
            "gzip",
            "lzma",
            "ncurses",
            "pam",
            "pam-modules",
            "pcre",
            "profile.d",
            "readline",
            "sed",
            "shell-cmds",
            "system-cmds",
            "tar",
            "uikittools"
        };

        private static void DownloadAllDependencies(Package pack, string dirPath)
        {
            if (pack.depends == null) return;

            string[] depends = Regex.Replace(pack.depends, @"(\(.*?\)|\s+)", String.Empty).Split(',');
            // Remove version requirements (parentheses) and whitespace then split
            // TODO: Check if version requirements are met like Cydia

            foreach (var dependency in depends.Where(x => !defaultPackages.Contains(x)))
            {
                Package dependencyPack = SearchPackagesByID(dependency);
                if (dependencyPack.pkg == null) return;
                Uri uri = new Uri(new Uri(dependencyPack.repo.URL), dependencyPack.filename);

                string dlPath = Path.Combine(dirPath, Path.GetFileName(uri.LocalPath));

                if (File.Exists(dlPath)) return;

                DownloadFile(uri, dlPath);

                DownloadAllDependencies(dependencyPack, dirPath); //yay! recursion
            }
        }

        #endregion

        #region Comparison

        /* This field is not used yet */

        // Ignores many unnecessary fields that is not used for comparison
        // Uses less resources
        private struct FastPackage
        {
            public string pkg, version;
        }

        private static List<FastPackage> FastParsePkg(string path)
        {
            var ret = new List<FastPackage> { };
            var pack = new FastPackage();
            foreach (string line in File.ReadLines(path))
            {
                if (line.StartsWith("Package:"))
                {
                    pack.pkg = line.Substring(9);
                }
                else if (line.StartsWith("Version:"))
                {
                    pack.version = line.Substring(9);
                    ret.Add(pack);
                }
            }
            return ret;
        }


        private static void Compare(string oldFile, string newFile)
        {
            // TODO: Create a reasonable UI to show changes...
            var temp = FastParsePkg(newFile).Except(FastParsePkg(oldFile));
            foreach (var item in temp)
            {
                //MessageBox.Show(item.pkg + "\n" + item.version);
            }
        }

        #endregion

    }
}
