using AdonisUI.Controls;
using AdonisUI.Extensions;
using System.IO;
using System.Linq;
using System.Reflection;
using NSInstaller.Utils;
using NSInstaller.Ookii;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Threading;
using System.Collections.Generic;
using Ionic.Zip;
using Octokit;
using System.Diagnostics;

namespace NSInstaller
{
    public partial class MainWindow : AdonisWindow
    {
        private bool isFinished = false;

        #region Github Version Checker
        // https://stackoverflow.com/questions/25678690/how-can-i-check-github-releases-in-c
        private async System.Threading.Tasks.Task CheckGitHubNewerVersion()
        {          
            try
            {
                //Get all releases from GitHub
                //Source: https://octokitnet.readthedocs.io/en/latest/getting-started/
                GitHubClient client = new GitHubClient(new ProductHeaderValue("ns-installer"));
                IReadOnlyList<Release> releases = await client.Repository.Release.GetAll("Dichill", "NSInstaller");

                //Setup the versions
                Version latestGitHubVersion = new Version(releases[0].TagName);
                Version localVersion = new Version(getAssemblyVersion()); //Replace this with your local version. 
                                                                          //Only tested with numeric values.

                //Compare the Versions
                //Source: https://stackoverflow.com/questions/7568147/compare-version-numbers-without-using-split-function
                int versionComparison = localVersion.CompareTo(latestGitHubVersion);
                if (versionComparison < 0)
                {
                    //The version on GitHub is more up to date than this local release.
                    if (MessageBox.Show("A New update has been found! Do you want to download the new one?", "NSInstaller | New Update!", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    {
                        Process.Start("https://github.com/Dichill/NSInstaller/releases");
                    }
                }
                else if (versionComparison > 0)
                {
                    //This local version is greater than the release version on GitHub.
                    this.Title = "NSInstaller Prebuild | v" + getAssemblyVersion();
                }
                else
                {
                    //This local Version and the Version on GitHub are equal.
                    this.Title = "NSInstaller v" + getAssemblyVersion();
                }
            }    
            catch(Exception e)
            {
                MessageBox.Show("NSInstaller Requires Internet to install the latest files onto your modded switch.", "NSInstaller | No Internet Connection");
                Environment.Exit(0);
            }
        }
        #endregion

        public MainWindow()
        {          
            InitializeComponent();

            this.Title = "NSInstaller v" + getAssemblyVersion();

            // The Installation Folder where we dump the downloaded files.
            if (!Directory.Exists(util.root_folder)) Directory.CreateDirectory(util.root_folder);
            if (!Directory.Exists(util.temp_folder)) Directory.CreateDirectory(util.temp_folder);
            if (!Directory.Exists(util.logs_folder)) Directory.CreateDirectory(util.logs_folder);
            if (!Directory.Exists(util.backup_folder)) Directory.CreateDirectory(util.backup_folder);
            if (!File.Exists("releases.txt")) File.Create("releases.txt");

            setAllButtonIsEnabled(false);
            openFileLocationBttn.IsEnabled = true;
            filePathTxt.IsEnabled = true;
        }

        // Credits to XCI-Explorer!
        private string getAssemblyVersion()
        {
            string assemblyVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            string[] versionArray = assemblyVersion.Split('.');

            assemblyVersion = string.Join(".", versionArray.Take(3));

            return assemblyVersion;
        }

        private void openFileLocationBttn_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            // Credits to Ookii for providing the FolderBrowserDialog
            // Credits to C. Augusto Proiete and CAD bloke for sorting out the files.

            VistaFolderBrowserDialog dialog = new VistaFolderBrowserDialog();
            dialog.Description = "Please select the micro sd folder.";
            dialog.UseDescriptionForTitle = true; // This applies to the Vista style dialog only, not the old dialog.
            if (!VistaFolderBrowserDialog.IsVistaFolderDialogSupported)
                MessageBox.Show(this, "Because you are not using Windows Vista or later, the regular folder browser dialog will be used. Please use Windows Vista to see the new dialog.", "Sample folder browser dialog");
            if ((bool)dialog.ShowDialog(this))
            {
                filePathTxt.Text = dialog.SelectedPath;
                proglabel.Text = "Loaded " + dialog.SelectedPath;
                setAllButtonIsEnabled(true);
            }
        }

        private void startBttn_Click(object sender, System.Windows.RoutedEventArgs e)
        {            
            if (filePathTxt.Text.Length == 0) MessageBox.Show("Please select the micro sd folder before starting the installation.", "NSInstaller", MessageBoxButton.OK, MessageBoxImage.Warning);
            else if (filePathTxt.Text.Length >= 2)
            {                
                try
                {
                    // Check First the Folder.
                    startBttn.Content = "Checking...";
                    proglabel.Text = "Status: Checking Folder Path.....";
                    var directory = new DirectoryInfo(filePathTxt.Text);
                    var files = directory.GetFiles();
                    if (files.Length == 0) StartInstallation();
                    else if (files.Length >= 1)
                    {
                        startBttn.Content = "Start Installation"; proglabel.Text = "Failed: SD Card is not Empty!";
                        MessageBoxResult dialogResult = MessageBox.Show("There are files in your Micro SD, atmosphere and bootloader folder will be backed up, do you want to proceed?", "NSInstaller", MessageBoxButton.YesNoCancel);
                        if (dialogResult == MessageBoxResult.Yes)
                        {
                            BackupFiles();
                            StartInstallation();

                        } else
                        {
                            proglabel.Text = "User did not choose options.";
                        }

                    }
                    else { startBttn.Content = "Start Installation"; proglabel.Text = "Failed: Folder is not accessible!"; MessageBox.Show("Make sure you selected the Micro SD Root Folder!", "NSInstaller", MessageBoxButton.OK, MessageBoxImage.Warning); }
                }
                catch (Exception ex)
                {
                    startBttn.Content = "Start Installation"; proglabel.Text = "Failed: SD Card is not Empty!"; MessageBox.Show("An Error has occured! " + ex.ToString() + "\nSend a screenshot to the support discord so they can help you out!", "NSInstaller", MessageBoxButton.OK, MessageBoxImage.Warning);
                }                
            }                      
        }
        #region Backup Handler
        private void BackupFiles()
        {
            if (Directory.Exists(filePathTxt.Text + "/bootloader/"))
            {
                CopyDirectory(new DirectoryInfo(filePathTxt.Text + "/bootloader"), new DirectoryInfo(util.backup_folder + "/bootloader"));
                DeleteDirectory(filePathTxt.Text + "/bootloader/");
            }
            if (Directory.Exists(filePathTxt.Text + "/atmosphere/"))
            {
                CopyDirectory(new DirectoryInfo(filePathTxt.Text + "/atmosphere"), new DirectoryInfo(util.backup_folder + "/atmosphere"));
                DeleteDirectory(filePathTxt.Text + "/atmosphere/");
            }
        }
        #endregion

        #region Installation Handler, CleanInstallation Handler

        // ~ Stages of Installation ~
        // Stage 1 - Install Hekate
        // Stage 2 - Install Atmosphere
        // Stage 3 - Install Patches
        // Stage 4 - Install Lockpick_RCM
        // ~ FINISHED ~

        private async void StartInstallation()
        {
            setAllButtonIsEnabled(false);
            startBttn.Content = "Installing...";
            proglabel.Text = "Status: Starting Installation....";
            if (isFinished == true) { CleanInstallation(); }
            else if (isFinished == false) await AutoInstall();
        }

        // Clean the files generated by NSInstaller.
        private void CleanInstallation()
        {            
            try 
            {             
                setAllButtonIsEnabled(true);

                Dispatcher.Invoke(() => {                   
                    progressBar.Value = 0;
                    proglabel.Text = "Installation Complete";
                    startBttn.Content = "Install";
                });
            } 
            catch (Exception e)
            {
                Dispatcher.Invoke(() => MessageBox.Show(e.ToString(), "NSInstaller", MessageBoxButton.OK, MessageBoxImage.Error)) ;
            }           
        }
        #endregion

        #region Download Handler / Extraction Handler    

        private async Task AutoInstall()
        {                    
            try {
                foreach (var url in util.github_releases)
                {
                    using (var webClient = new WebClient())
                    {
                        IWebProxy webProxy = WebRequest.DefaultWebProxy;
                        webProxy.Credentials = CredentialCache.DefaultCredentials;
                        webClient.Proxy = webProxy;
                        webClient.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; " +
                                            "Windows NT 5.2; .NET CLR 1.0.3705;)");
                        webClient.DownloadProgressChanged += (s, e) => { Dispatcher.Invoke(() => { progressBar.Value = e.ProgressPercentage; proglabel.Text = $"[{e.ProgressPercentage}%] Fetching API - " + url; }); };
                        webClient.DownloadStringCompleted += async (s, e) =>
                        {
                            dynamic dynObj = JsonConvert.DeserializeObject(e.Result);
                            string browser_url = dynObj[0].assets[0].browser_download_url;

                            await DownloadAsync(browser_url);
                        };
                        await webClient.DownloadStringTaskAsync(new Uri(url)).ConfigureAwait(false);
                        webClient.Dispose();
                    }                        
                }
            }
            catch(Exception e)
            {
                MessageBox.Show(e.ToString(), "NSInstaller", MessageBoxButton.OK, MessageBoxImage.Error);
            }            
        }

        #region Download Single Link (For Tools)
        private async Task DownloadSingleLink(string url)
        {
            try
            {               
                using (var webClient = new WebClient())
                {
                    IWebProxy webProxy = WebRequest.DefaultWebProxy;
                    webProxy.Credentials = CredentialCache.DefaultCredentials;
                    webClient.Proxy = webProxy;
                    webClient.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; " +
                                        "Windows NT 5.2; .NET CLR 1.0.3705;)");
                    webClient.DownloadProgressChanged += (s, e) => { Dispatcher.Invoke(() => { progressBar.Value = e.ProgressPercentage; proglabel.Text = $"[{e.ProgressPercentage}%] Fetching API - " + url;  }); };
                    webClient.DownloadStringCompleted += async (s, e) =>
                    {
                        dynamic dynObj = JsonConvert.DeserializeObject(e.Result);
                        string browser_url = dynObj[0].assets[0].browser_download_url;

                        await DownloadAsync(browser_url);                        
                    };
                    await webClient.DownloadStringTaskAsync(new Uri(url)).ConfigureAwait(false);
                    webClient.Dispose();
                }                
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }
        #endregion

        private async Task DownloadAsync(string url)
        {           
            try
            {                
                using (var webClient = new WebClient())
                {
                    IWebProxy webProxy = WebRequest.DefaultWebProxy;
                    webProxy.Credentials = CredentialCache.DefaultCredentials;
                    webClient.Proxy = webProxy;
                    webClient.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; " +
                                      "Windows NT 5.2; .NET CLR 1.0.3705;)");
                    webClient.DownloadProgressChanged += (s, e) => { Dispatcher.Invoke(() => { progressBar.Value = e.ProgressPercentage; proglabel.Text = $"[{e.ProgressPercentage}%] Downloading - " + url; }); };
                    webClient.DownloadFileCompleted += (s, e) => { 
                        
                        Dispatcher.Invoke(() => { ExtractZips(Path.GetFileName(new Uri(url).LocalPath)); CleanInstallation(); });
                    }; 
                    await webClient.DownloadFileTaskAsync(new Uri(url), util.temp_folder + Path.GetFileName(new Uri(url).LocalPath)).ConfigureAwait(false);
                    webClient.Dispose();
                }
            }
            catch (Exception e)
            {
                Dispatcher.Invoke(() => MessageBox.Show(e.ToString(), "NSInstaller", MessageBoxButton.OK, MessageBoxImage.Error)) ;
            }            
        }

        private void ExtractZips(string v)
        {                       
            using (ZipFile zip = ZipFile.Read(util.temp_folder + v))
            {
                foreach (ZipEntry zipFiles in zip)
                {
                    Dispatcher.Invoke(() => zipFiles.Extract(filePathTxt.Text, ExtractExistingFileAction.OverwriteSilently));                    
                }
            }            
        }
        #endregion

        #region Tools | onClick Events
        private void installSigBttn_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (MessageBox.Show("Do you want to install the Latest Sig Patches to your Switch?", "NSInstaller", MessageBoxButton.YesNoCancel) == MessageBoxResult.Yes)
            {
                if (filePathTxt.Text.Length == 0) MessageBox.Show("Please select the micro sd folder before starting the installation.", "NSInstaller", MessageBoxButton.OK, MessageBoxImage.Warning);
                else if (filePathTxt.Text.Length >= 2)
                {
                    try
                    {
                        proglabel.Text = "Status: Checking Folder Path.....";

                        if (Directory.Exists(filePathTxt.Text + "/Atmosphere/"))
                        {
                            Task task = DownloadSingleLink("https://api.github.com/repos/ITotalJustice/patches/releases");                            
                        }
                        else
                        {
                            CleanInstallation();
                            MessageBox.Show("Atmosphere Folder not Found! Did you install it correctly?", "NSInstaller", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    catch (Exception ex)
                    {
                        startBttn.Content = "Start Installation"; proglabel.Text = "Error occured installing Signature Patches."; MessageBox.Show("An Error has occured! " + ex.ToString() + "\nSend a screenshot to the support discord so they can help you out!", "NSInstaller", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }                    
                }
            }            
        }

        private void fixAtmoBttn_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (MessageBox.Show("Do you want to install the Latest Signature Patches to your Nintendo Switch?", "NSInstaller", MessageBoxButton.YesNoCancel) == MessageBoxResult.Yes)
            {
                if (filePathTxt.Text.Length == 0) MessageBox.Show("Please select the micro sd folder before starting the installation.", "NSInstaller", MessageBoxButton.OK, MessageBoxImage.Warning);
                else if (filePathTxt.Text.Length >= 2)
                {
                    try
                    {
                        proglabel.Text = "Status: Checking Folder Path.....";

                        if (Directory.Exists(filePathTxt.Text + "/Atmosphere/"))
                        {
                            if (Directory.Exists(filePathTxt.Text + "/Atmosphere/contents/01000000001000"))
                            {
                                Directory.Delete(filePathTxt.Text + "/Atmosphere/contents/01000000001000");                                
                            }
                            Task task = DownloadSingleLink("https://api.github.com/repos/ITotalJustice/patches/releases");
                        } 
                        else
                        {
                            CleanInstallation();
                            MessageBox.Show("Atmosphere Folder not Found! Did you install it correctly?", "NSInstaller", MessageBoxButton.OK, MessageBoxImage.Error);
                        }


                    }
                    catch (Exception ex)
                    {
                        startBttn.Content = "Start Installation"; proglabel.Text = "Error occured while installing Atmosphere"; MessageBox.Show("An Error has occured! " + ex.ToString() + "\nSend a screenshot to the support discord so they can help you out!", "NSInstaller", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
            }
        }

        private void updtHekateBttn_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (MessageBox.Show("Do you want to Update Hekate for your Nintendo Switch?", "NSInstaller", MessageBoxButton.YesNoCancel) == MessageBoxResult.Yes)
            {
                if (filePathTxt.Text.Length == 0) MessageBox.Show("Please select the micro sd folder before starting the installation.", "NSInstaller", MessageBoxButton.OK, MessageBoxImage.Warning);
                else if (filePathTxt.Text.Length >= 2)
                {
                    try
                    {
                        proglabel.Text = "Status: Checking Folder Path.....";

                        if (Directory.Exists(filePathTxt.Text + "/bootloader/"))
                        {
                            new List<string>(Directory.GetFiles(filePathTxt.Text)).ForEach(file => { if (file.ToUpper().Contains("hekate".ToUpper())) File.Delete(file); });
                            new List<string>(Directory.GetFiles(filePathTxt.Text)).ForEach(file => { if (!file.ToUpper().Contains("hekate".ToUpper())) { Task task = DownloadSingleLink("https://api.github.com/repos/CTCaer/hekate/releases"); }  });
                        }                        
                        else
                        {
                            CleanInstallation();
                            MessageBox.Show("Bootloader Folder not Found! Did you install it correctly?", "NSInstaller", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    catch (Exception ex)
                    {
                        startBttn.Content = "Start Installation"; proglabel.Text = "Error occured while Installing Hekate"; MessageBox.Show("An Error has occured! " + ex.ToString() + "\nSend a screenshot to the support discord so they can help you out!", "NSInstaller", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
            }
        }

        private void updtAtmoBttn_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (MessageBox.Show("Do you want to Update Atmosphere for your Nintendo Switch? Backups can be found at C:/NSInstaller/backup/", "NSInstaller", MessageBoxButton.YesNoCancel) == MessageBoxResult.Yes)
            {
                if (filePathTxt.Text.Length == 0) MessageBox.Show("Please select the micro sd folder before starting the installation.", "NSInstaller", MessageBoxButton.OK, MessageBoxImage.Warning);
                else if (filePathTxt.Text.Length >= 2)
                {
                    try
                    {
                        proglabel.Text = "Status: Checking Folder Path.....";

                        if (Directory.Exists(filePathTxt.Text + "/bootloader/") && Directory.Exists(filePathTxt.Text + "/atmosphere/"))
                        {
                            // Back it up first, just in case something went wrong.
                            BackupFiles();

                            new List<string>(Directory.GetFiles(filePathTxt.Text)).ForEach(file => { if (file.ToUpper().Contains("hekate".ToUpper())) File.Delete(file); });
                            Directory.Delete(filePathTxt.Text + "/atmosphere/");
                            Directory.Delete(filePathTxt.Text + "/bootloader/");

                            Task task = DownloadSingleLink("https://api.github.com/repos/Atmosphere-NX/Atmosphere/releases");
                            Task task2 = DownloadSingleLink("https://api.github.com/repos/ITotalJustice/patches/releases");
                        }
                        else
                        {
                            CleanInstallation();
                            MessageBox.Show("Atmosphere Folder not Found! Did you install it correctly?", "NSInstaller", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    catch (Exception ex)
                    {
                        startBttn.Content = "Start Installation"; proglabel.Text = "Error occured while updating Atmosphere"; MessageBox.Show("An Error has occured! " + ex.ToString() + "\nSend a screenshot to the support discord so they can help you out!", "NSInstaller", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
            }
        }
        #endregion

        #region Misc | SetButtonIsEnabled | URI Handler | CopyDirectory Handler
        private void setAllButtonIsEnabled(bool isEnabled)
        {
            filePathTxt.IsEnabled = isEnabled;
            openFileLocationBttn.IsEnabled = isEnabled;
            startBttn.IsEnabled = isEnabled;
            installSigBttn.IsEnabled = isEnabled;
            fixAtmoBttn.IsEnabled = isEnabled;
            updtHekateBttn.IsEnabled = isEnabled;
            updtAtmoBttn.IsEnabled = isEnabled;
            homeBrewBttn.IsEnabled = isEnabled;
        }

        // https://stackoverflow.com/questions/677221/copy-folders-in-c-sharp-using-system-io
        static void CopyDirectory(DirectoryInfo source, DirectoryInfo destination)
        {
            if (!destination.Exists)
            {
                destination.Create();
            }

            // Copy all files.
            FileInfo[] files = source.GetFiles();
            foreach (FileInfo file in files)
            {
                file.CopyTo(Path.Combine(destination.FullName,
                    file.Name));
            }

            // Process subdirectories.
            DirectoryInfo[] dirs = source.GetDirectories();
            foreach (DirectoryInfo dir in dirs)
            {
                // Get destination directory.
                string destinationDir = Path.Combine(destination.FullName, dir.Name);

                // Call CopyDirectory() recursively.
                CopyDirectory(dir, new DirectoryInfo(destinationDir));
            }
        }

        // https://stackoverflow.com/questions/329355/cannot-delete-directory-with-directory-deletepath-true
        public static void DeleteDirectory(string target_dir)
        {
            string[] files = Directory.GetFiles(target_dir);
            string[] dirs = Directory.GetDirectories(target_dir);

            foreach (string file in files)
            {
                File.SetAttributes(file, FileAttributes.Normal);
                File.Delete(file);
            }

            foreach (string dir in dirs)
            {
                DeleteDirectory(dir);
            }

            Directory.Delete(target_dir, false);
        }
        #endregion 

        #region Models and Utilities
        Util util = new Util();
        #endregion

        private async void AdonisWindow_Initialized(object sender, EventArgs e)
        {
            await CheckGitHubNewerVersion();
        }
    }
}