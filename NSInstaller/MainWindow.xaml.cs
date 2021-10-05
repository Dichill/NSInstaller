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

namespace NSInstaller
{
    public partial class MainWindow : AdonisWindow
    {
        private bool isFinished = false;        

        public MainWindow()
        {          
            InitializeComponent();

            this.Title = "NSInstaller v" + getAssemblyVersion();

            // The Installation Folder where we dump the downloaded files.
            if (!Directory.Exists(util.root_folder)) Directory.CreateDirectory(util.root_folder);
            if (!Directory.Exists(util.temp_folder)) Directory.CreateDirectory(util.temp_folder);
            if (!Directory.Exists(util.logs_folder)) Directory.CreateDirectory(util.logs_folder);
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
                    else if (files.Length >= 1) { startBttn.Content = "Start Installation"; proglabel.Text = "Failed: SD Card is not Empty!"; MessageBox.Show("Please format your SD Card First before doing the Installation. (There are files in the Micro SD)", "NSInstaller", MessageBoxButton.OK, MessageBoxImage.Warning); }
                    else { startBttn.Content = "Start Installation"; proglabel.Text = "Failed: Folder is not accessible!"; MessageBox.Show("Make sure you selected the Micro SD Root Folder!", "NSInstaller", MessageBoxButton.OK, MessageBoxImage.Warning); }
                }
                catch (Exception ex)
                {
                    startBttn.Content = "Start Installation"; proglabel.Text = "Failed: SD Card is not Empty!"; MessageBox.Show("An Error has occured! " + ex.ToString() + "\nSend a screenshot to the support discord so they can help you out!", "NSInstaller", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }                      
        }

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
                MessageBox.Show(e.ToString(), "NSInstaller", MessageBoxButton.OK, MessageBoxImage.Error);
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
                        ExtractZips(Path.GetFileName(new Uri(url).LocalPath));
                    };
                    await webClient.DownloadFileTaskAsync(new Uri(url), util.temp_folder + Path.GetFileName(new Uri(url).LocalPath)).ConfigureAwait(false);
                    webClient.Dispose();
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "NSInstaller", MessageBoxButton.OK, MessageBoxImage.Error);
            }            
        }

        private void ExtractZips(string v)
        {
            Dispatcher.Invoke(() => progressBar.Value = 0);

            
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
                            CleanInstallation();
                        }
                    }
                    catch (Exception ex)
                    {
                        startBttn.Content = "Start Installation"; proglabel.Text = "Failed: SD Card is not Empty!"; MessageBox.Show("An Error has occured! " + ex.ToString() + "\nSend a screenshot to the support discord so they can help you out!", "NSInstaller", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
            }            
        }

        private void fixAtmoBttn_Click(object sender, System.Windows.RoutedEventArgs e)
        {

        }

        private void updtHekateBttn_Click(object sender, System.Windows.RoutedEventArgs e)
        {

        }

        private void updtAtmoBttn_Click(object sender, System.Windows.RoutedEventArgs e)
        {

        }
        #endregion

        #region Misc | SetButtonIsEnabled | URI Handler
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
        #endregion

        #region Models and Utilities
        Util util = new Util();
        #endregion
    }
}