// <copyright file="MainForm.APT.cs">
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
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using System.Windows.Forms;

    /// <content>
    /// Contains APT related parts of MainForm
    /// </content>
    public partial class MainForm
    {
        #region Fields

        /// <summary>
        /// Default packages installed by evasi0n7 jailbreak.
        /// We don't want to download these packages as dependencies.
        /// </summary>
        private static readonly string[] DefaultPackages = 
        {
            "apr-lib",
            "apt7-key",
            "apt7-lib",
            "base",
            "org.thebigboss.Repo.icons",
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

        /// <summary>
        /// Hardcoded constant of known sources which will be used to fix URLs entered by the user incorrectly.
        /// If URL contains the Key, it will be replaced with the Value.
        /// </summary>
        /// <remarks>
        /// So finally you can add thebigboss to your sources without hassle!
        /// </remarks>
        private static readonly Dictionary<string, string> KnownSources = new Dictionary<string, string> 
        {
            { "bigboss", "http://apt.thebigboss.org/repofiles/cydia/" }, // Y U NO CREATE SYMLINK :(
            { "zodttd", "http://cydia.zodttd.com/Repo/cydia/" },
            { "modmyi", "http://apt.modmyi.com/" },
            { "saurik", "http://apt.saurik.com/" },
            { "ultrasn0w", "http://repo666.ultrasn0w.com/" },
            { "rpetri", "http://rpetri.ch/Repo/" },
            { "filippobiga", "http://filippobiga.me/Repo/" },
            { "getdelta", "http://getdelta.co/" },
            { "if0rce", "http://apt.if0rce.com/" },
            { "angelxwind", "http://cydia.angelxwind.net/" },
            { "radare", "http://cydia.radare.org/" },
            { "pushfix", "http://cydia.pushfix.info/" }
        };

        /// <summary>
        /// The old repos from the time of Installer requires extra care.
        /// We need to handle these exceptions.
        /// </summary>
        /// <remarks>
        /// <para>
        /// ReloadData: dists/stable/main/binary-iphoneos-arm/
        /// DownloadCydiaIcon, DownloadRelease: dists/stable/
        /// </para><para>
        /// Saurik's Repo is an exception here, it uses dists/ios instead...
        /// Yo dawg, I heard you like exceptions, so I put an exception in yo exception array!
        /// </para>
        /// </remarks>
        private static readonly string[] OldRepos = new string[] 
        {
            "http://apt.thebigboss.org/repofiles/cydia/", "http://apt.modmyi.com/", "http://cydia.zodttd.com/Repo/cydia/",
            "http://apt.saurik.com/"
        };

        /// <summary>
        /// Stores all repos on memory to access later.
        /// </summary>
        private static List<Repo> allRepos = new List<Repo> { };

        #endregion Fields

        #region Methods

        /// <summary>
        /// Removes invalid Filename chars from the argument.
        /// </summary>
        /// <param name="fileName">The filename to clean.</param>
        /// <returns>Legal Filename.</returns>
        /// <remarks>
        /// Credits: http://stackoverflow.com/a/7393722
        /// </remarks>
        private static string CleanFileName(string fileName)
        {
            return Path.GetInvalidFileNameChars().Aggregate(fileName, (current, c) => current.Replace(c.ToString(), String.Empty));
        }

        /// <summary>
        /// The Filename alias of the repositories which will be used to store Repo Icons and Packages.
        /// </summary>
        /// <param name="url">The URL to be converted.</param>
        /// <returns>The Filename alias of the Repo.</returns>
        private static string URLToFilename(string url)
        {
            return url.Replace("http://", string.Empty).Replace("www.", string.Empty).TrimEnd('/').Replace('/', '_');
        }

        /// <summary>
        /// Extracts an embedded resource.
        /// </summary>
        /// <param name="resName">The ID of the resource to extract.</param>
        /// <param name="path">The path where resources are going to be extracted.</param>
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

        /// <summary>
        /// Extracts an archive using 7-zip executable (7za.exe).
        /// </summary>
        /// <param name="zipName">The name of the archive to extract.</param>
        /// <remarks>
        /// Very fast and easy way to decompress bz2 archives.
        /// Managed DLLs are just too slow.
        /// </remarks>
        private static void ExtractZip(string zipName)
        {
            if (!File.Exists("repos\\7za.exe")) ExtractResource("Cygnus.7za.exe", "repos\\7za.exe");

            ProcessStartInfo startInfo = new ProcessStartInfo("repos\\7za.exe", "x -orepos -y repos\\" + zipName);
            startInfo.RedirectStandardOutput = true;
            startInfo.UseShellExecute = false;
            startInfo.CreateNoWindow = true;

            Process.Start(startInfo).WaitForExit();
        }

        /// <summary>
        /// Checks if Uri is valid using HTTP HEAD request.
        /// </summary>
        /// <param name="url">The URL to be checked.</param>
        /// <returns>true if URL exists and false if else or an exception occurs.</returns>
        private static bool RemoteUriExists(Uri url)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = WebRequestMethods.Http.Head;
                request.UserAgent = "Cydia/0.9 CFNetwork/548.1.4 Darwin/11.0.0";
                request.AllowAutoRedirect = false;

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    return response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Found;
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Downloads a file from web using an URL and a Stream.
        /// </summary>
        /// <param name="uri">The URL to be downloaded.</param>
        /// <param name="stream">The stream to write.</param>
        private static void DownloadFile(Uri uri, Stream stream)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            request.Method = WebRequestMethods.Http.Get;
            request.UserAgent = "Telesphoreo APT-HTTP/1.0.592";
            ////request.Headers.Add("X-Firmware", "7.0.2");
            ////request.Headers.Add("X-Machine", "iPhone6,2");
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

        /// <summary>
        /// The method overload which will create FileStream using path argument.
        /// </summary>
        /// <param name="uri">URL to download.</param>
        /// <param name="path">The relative path where the file is going to be saved.</param>
        private static void DownloadFile(Uri uri, string path)
        {
            using (FileStream fileStream = File.Create(path))
            {
                DownloadFile(uri, fileStream);
            }
        }

        /// <summary>
        /// Downloads repository's CydiaIcon.png, the logo you see in Cydia>Manage>Sources.
        /// </summary>
        /// <param name="repoURL">The Repo's URL</param>
        private static void DownloadCydiaIcon(string repoURL)
        {
            if (!Directory.Exists("repos")) Directory.CreateDirectory("repos");

            Uri iconUri;

            if (repoURL == "http://apt.saurik.com/")
                iconUri = new Uri(new Uri(repoURL), "dists/ios/CydiaIcon.png");
            else if (OldRepos.Contains(repoURL))
                iconUri = new Uri(new Uri(repoURL), "dists/stable/CydiaIcon.png");
            else
                iconUri = new Uri(new Uri(repoURL), "CydiaIcon.png");

            if (RemoteUriExists(iconUri))
            {
                DownloadFile(iconUri, "repos\\" + URLToFilename(repoURL) + ".png");
            }
        }

        /// <summary>
        /// Downloads Repo's Release file using MemoryStream.
        /// </summary>
        /// <param name="repoURL">The Repo's URL.</param>
        /// <returns>The Label of the Repo.</returns>
        private static string DownloadRelease(string repoURL)
        {
            Uri releaseUri;
            string label = null;

            if (repoURL == "http://apt.saurik.com/")
                releaseUri = new Uri(new Uri(repoURL), "dists/ios/Release");
            else if (OldRepos.Contains(repoURL))
                releaseUri = new Uri(new Uri(repoURL), "dists/stable/Release");
            else
                releaseUri = new Uri(new Uri(repoURL), "Release");

            using (MemoryStream memoryStream = new MemoryStream())
            {
                DownloadFile(releaseUri, memoryStream);
                memoryStream.Position = 0;
                using (StreamReader streamReader = new StreamReader(memoryStream))
                {
                    string line;
                    while ((line = streamReader.ReadLine()) != null)
                    {
                        if (line.StartsWith("Label:", StringComparison.InvariantCulture))
                        {
                            label = line.Substring(7);
                            break;
                        }
                    }
                }
            }

            return label.Trim();
        }

        /// <summary>
        /// Re-downloads packages lists from repositories akin to 'apt-get update'
        /// </summary>
        private static void ReloadData()
        {
            if (!Directory.Exists("repos")) Directory.CreateDirectory("repos");

            foreach (var repo in allRepos)
            {
                Uri packagesUri;
                bool bz2Archive = true;

                if (repo.URL == "http://apt.saurik.com/")
                {
                    packagesUri = new Uri(new Uri(repo.URL), "dists/ios/main/binary-iphoneos-arm/Packages.bz2");
                    if (!RemoteUriExists(packagesUri))
                        continue;
                }
                else if (OldRepos.Contains(repo.URL))
                {
                    packagesUri = new Uri(new Uri(repo.URL), "dists/stable/main/binary-iphoneos-arm/Packages.bz2");
                    if (!RemoteUriExists(packagesUri))
                        continue;
                }
                else
                {
                    packagesUri = new Uri(new Uri(repo.URL), "Packages.bz2");
                    if (!RemoteUriExists(packagesUri))
                    {
                        packagesUri = new Uri(new Uri(repo.URL), "Packages.gz");
                        bz2Archive = false;
                        if (!RemoteUriExists(packagesUri))
                            continue;
                    }
                }

                string fileName = URLToFilename(repo.URL) + ".Packages" + (bz2Archive ? ".bz2" : ".gz");

                try
                {
                    DownloadFile(packagesUri, "repos\\" + fileName);
                }
                catch (WebException webEx)
                {
                    MessageBox.Show(String.Format("An error occured while reloading data from repo: {0}\n\n{1}", repo.URL, webEx.Message),
                                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                ExtractZip(fileName);
                File.Delete("repos\\" + fileName);
            }
        }

        /// <summary>
        /// Finds packages which contains the argument in their title (case-insensitive),
        /// after reading every .Packages files line by line.
        /// This has an hardcoded limit of 250 packages, after that it'll stop searching.
        /// </summary>
        /// <param name="packageNameToSearch">Package Name to search.</param>
        /// <returns>List of Packages that matches the criteria.</returns>
        /// <remarks>
        /// <para>
        /// Can be optimized further, 
        /// like removing the Select() and checking if the package Name contains the argument
        /// on the fly and then adding it to the 'packages' List
        /// </para><para>
        /// String.Split bites my CPU but, do we need optimization though? 
        /// It's already a lot faster than iOS/Cydia on my cheap dual core computer
        /// </para>
        /// </remarks>
        private static List<Package> SearchPackagesByName(string packageNameToSearch)
        {
            packageNameToSearch = packageNameToSearch.ToLowerInvariant();

            var packages = new List<Package> { };

            foreach (var repo in allRepos)
            {
                string filename = "repos\\" + URLToFilename(repo.URL) + ".Packages";

                if (!File.Exists(filename))
                {
                    MessageBox.Show("Could not find file: " + filename, "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
                }

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
                                pack.Pkg = value;
                                break;
                            case "Name":
                                pack.Name = value;
                                break;
                            case "Version":
                                pack.Version = value;
                                break;
                            case "Description":
                                pack.Description = value;
                                break;
                            case "Filename":
                                pack.Filename = value;
                                break;
                            case "Section":
                                pack.Section = value;
                                break;
                            case "Pre-Depends":
                            case "Depends":
                                pack.Depends = (pack.Depends == null) ? value : (pack.Depends + "," + value);
                                break;
                            case "MD5sum":
                                pack.MD5sum = value;
                                break;
                            case "Depiction":
                                pack.Depiction = value;
                                break;
                            case "Tag":
                                pack.Paid = value.Contains("cydia::commercial");
                                break;
                        }
                    }
                    else if (string.IsNullOrWhiteSpace(key))
                    {
                        if (!string.IsNullOrWhiteSpace(pack.Name) && pack.Name.ToLowerInvariant().Contains(packageNameToSearch))
                        {
                            pack.Repo = repo;
                            packages.Add(pack);
                        }

                        pack = new Package();
                    }
                } // end foreach (var substrings in query)

                // Add the last one if we missed one
                if (!string.IsNullOrWhiteSpace(pack.Pkg) && string.IsNullOrWhiteSpace(pack.Name) && pack.Name.ToLowerInvariant().Contains(packageNameToSearch))
                {
                    packages.Add(pack);
                }
            } // end foreach (ListViewItem item in listSources.Items)

            return packages;
        }

        /// <summary>
        /// Searches packages by ID, similar to SearchPackagesByName().
        /// This is used to download dependencies, unlike the former,
        /// which is used for display purposes.
        /// </summary>
        /// <param name="packageIDToSearch">Package ID to search.</param>
        /// <returns>The Package whose ID is the same as the argument.</returns>
        /// <remarks>
        /// This is a bit against DRY
        /// But since this is a compiled language, I don't think that's too much a problem.
        /// </remarks>
        private static Package SearchPackagesByID(string packageIDToSearch)
        {
            packageIDToSearch = packageIDToSearch.ToLowerInvariant();

            Package pack = new Package();

            bool found = false;

            foreach (var repo in allRepos)
            {
                if (found) break;

                string filename = "repos\\" + URLToFilename(repo.URL) + ".Packages";

                if (!File.Exists(filename))
                {
                    MessageBox.Show("Could not find file: " + filename, "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
                }

                foreach (var line in File.ReadLines(filename))
                {
                    if (!found)
                    {
                        if (line.StartsWith("Package:") && line.Substring(9) == packageIDToSearch)
                        {
                            found = true;
                            pack.Pkg = packageIDToSearch;
                        }
                    }
                    else
                    {
                        string[] substrings = line.Split(new char[] { ':' }, 2);

                        if (substrings.Length != 2)
                        {
                            // End of Package
                            pack.Repo = repo;
                            break;
                        }

                        string key = substrings[0];
                        string value = substrings[1].Trim();

                        switch (key.Trim())
                        {
                            case "Filename":
                                pack.Filename = value;
                                break;
                            case "Pre-Depends":
                            case "Depends":
                                pack.Depends = (pack.Depends == null) ? value : (pack.Depends + "," + value);
                                break;
                            case "MD5sum":
                                pack.MD5sum = value;
                                break;
                        }
                    }
                }
            }

            return pack;
        }

        /// <summary>
        /// Downloads all dependencies recursively.
        /// </summary>
        /// <param name="pack">The package of which dependencies are going be downloaded.</param>
        /// <param name="dirPath">The path of the directory where files are going to be saved.</param>
        private static void DownloadAllDependencies(Package pack, string dirPath)
        {
            if (pack.Depends == null) return;

            string[] depends = Regex.Replace(pack.Depends, @"(\(.*?\)|\s+)", string.Empty).Split(',');
            // Remove Version requirements (parentheses) and whitespace then split
            // TODO: Check if Version requirements are met like Cydia

            foreach (var dependency in depends.Where(x => !DefaultPackages.Contains(x)))
            {
                Package dependencyPack = SearchPackagesByID(dependency);
                if (dependencyPack.Pkg == null) return;
                Uri uri = new Uri(new Uri(dependencyPack.Repo.URL), dependencyPack.Filename);

                string downloadPath = Path.Combine(dirPath, Path.GetFileName(uri.LocalPath));

                if (File.Exists(downloadPath)) return;

                DownloadFile(uri, downloadPath);

                DownloadAllDependencies(dependencyPack, dirPath); //yay! recursion
            }
        }

        /// <summary>
        /// Verifies the Repo URL added by the user.
        /// Tries to fix incorrect URLs.
        /// </summary>
        /// <param name="url">The Repo's url.</param>
        /// <param name="item">The ListViewItem to edit.</param>
        private void VerifyRepoURL(string url, ListViewItem item)
        {
            // zodttd has two repos actually
            if (new Uri(url).ToString() != "http://cydiabetas.zodttd.com/")
            {
                foreach (var source in KnownSources)
                {
                    if (url.Contains(source.Key))
                    {
                        // I could use Regex but whatever...
                        url = source.Value;
                        break;
                    }
                }
            }

            bool verified = false;
            string label = null;

            Uri uri = new Uri(url);

            if (allRepos.FirstOrDefault(x => x.URL == uri.ToString()) != null)
            {
                MessageBox.Show("The repository you've entered already exists!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            ShowLoadingView("Verifying URL", () =>
            {
                if (OldRepos.Contains(uri.ToString()) || RemoteUriExists(new Uri(uri, "Packages.bz2")) || RemoteUriExists(new Uri(uri, "Packages.gz")))
                {
                    verified = true;
                    DownloadCydiaIcon(uri.ToString());
                    label = DownloadRelease(uri.ToString());
                }
            });

            if (verified)
            {
                item.Text = uri.ToString();
                item.SubItems.Add("?"); // FIXME: Calculate number of packages.
                item.Tag = label;
                SaveXMLSettings();
                AddIcons();
                LoadXMLSettings();
            }
            else
            {
                MessageBox.Show(uri.ToString() + " is not a valid Cydia repository!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        /// <summary>
        /// THIS IS NOT USED YET!
        /// Loads Package ID and Version to the memory which will be used to
        /// compare two .Packages file.
        /// </summary>
        /// <param name="path">The path of the packages list.</param>
        /// <returns>Package IDs and Versions.</returns>
        /// <remarks>
        /// Pkg will be used to find new packages using SearchPackagesByID()
        /// while Version is only used to check if newer Version exists.
        /// </remarks>
        private static List<FastPackage> FastParsePkg(string path)
        {
            var ret = new List<FastPackage> { };
            var pack = new FastPackage();
            foreach (string line in File.ReadLines(path))
            {
                if (line.StartsWith("Package:"))
                {
                    pack.Pkg = line.Substring(9);
                }
                else if (line.StartsWith("Version:"))
                {
                    pack.Version = line.Substring(9);
                    ret.Add(pack);
                }
            }

            return ret;
        }

        /// <summary>
        /// THIS IS NOT USED YET!
        /// This method compares two .Packages file and is not complete yet.
        /// </summary>
        /// <param name="oldFile">The old .Packages file.</param>
        /// <param name="newFile">The updated .Packages file.</param>
        private static void Compare(string oldFile, string newFile)
        {
            // TODO: Create a reasonable UI to show changes...
            var temp = FastParsePkg(newFile).Except(FastParsePkg(oldFile));
            foreach (var item in temp)
            {
                ////MessageBox.Show(item.Pkg + "\n" + item.Version);
            }
        }

        #endregion Methods

        #region Nested Types

        /// <summary>
        /// Used to store Package information on memory.
        /// </summary>
        private struct Package
        {
            #region Fields

            public bool Paid;
            public string Pkg, Name, Version, Description, Depends, Filename, Section, MD5sum, Depiction;
            public Repo Repo;

            #endregion Fields
        }

        /// <summary>
        /// THIS IS NOT USED YET!
        /// I call this FastPackage because it ignores many unnecessary 
        /// fields that is not used for comparison,  which results better performance.
        /// </summary>
        private struct FastPackage
        {
            #region Fields

            public string Pkg, Version;

            #endregion Fields
        }

        /// <summary>
        /// Used to store Repo URLs and labels.
        /// </summary>
        /// <remarks>
        /// This is a class because it's more logical to create this once and reference after that.
        /// If this was a struct it would be passed as a value which will cause serious performance issues later.
        /// </remarks>
        private class Repo
        {
            #region Fields

            public string URL, Label;

            #endregion Fields
        }

        #endregion Nested Types
    }
}