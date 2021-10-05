using AdonisUI.Controls;
using AdonisUI.Extensions;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using NSInstaller.Utils;
using NSInstaller.Ookii;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Threading.Tasks;

namespace NSInstaller
{
    public partial class MainWindow : AdonisWindow
    {
        private bool isFinished = false;
        private int Stage = 1;

        public MainWindow()
        {          
            InitializeComponent();

            this.Title = "NSInstaller v" + getAssemblyVersion();                     

            // The Installation Folder where we dump the downloaded files.
            if (!Directory.Exists(util.temp_folder)) Directory.CreateDirectory(util.temp_folder);
            if (!Directory.Exists(util.logs_folder)) Directory.CreateDirectory(util.logs_folder);

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
                    else if (files.Length > 1) { startBttn.Content = "Start Installation"; proglabel.Text = "Failed: SD Card is not Empty!";  MessageBox.Show("Please format your SD Card First before doing the Installation. (There are files in the Micro SD)", "NSInstaller", MessageBoxButton.OK, MessageBoxImage.Warning); }
                    else { startBttn.Content = "Start Installation"; proglabel.Text = "Failed: Folder is not accessible!"; MessageBox.Show("Make sure you selected the Micro SD Root Folder!", "NSInstaller", MessageBoxButton.OK, MessageBoxImage.Warning); }
                }
                catch (Exception ex)
                {
                    startBttn.Content = "Start Installation"; proglabel.Text = "Failed: SD Card is not Empty!";  MessageBox.Show("An Error has occured! " + ex.ToString() + "\nSend a screenshot to the support discord so they can help you out!", "NSInstaller", MessageBoxButton.OK, MessageBoxImage.Warning);
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

        private void StartInstallation()
        {
            setAllButtonIsEnabled(false);

            startBttn.Content = "Installing...";
            proglabel.Text = "Status: Starting Installation....";

            this.Title = "NSInstaller v" + getAssemblyVersion() + " | Stage " + Stage.ToString();

            // Let it know that we are in the first stage.
            switch (Stage)
            {
                case 1:
                    Stage += 1;
                    proglabel.Text = "Fetching Github API for Hekate...";
                    GetDownloadLink(util.hekate_releases);

                    // Proceed to the next Stage
                    StartInstallation();
                    break;
                case 2:
                    Stage += 1;
                    proglabel.Text = "Fetching Github API for Atmosphere...";
                    GetDownloadLink(util.atmosphere_releases);

                    StartInstallation();
                    break;
                case 3:
                    Stage += 1;
                    proglabel.Text = "Fetching Github API for Signature Patches...";
                    GetDownloadLink(util.sig_patches);

                    StartInstallation();
                    break;
                case 4:
                    Stage = 0;

                    isFinished = true;
                    break;

                default:
                    break;
            }

            if (isFinished == true) { }
        }

        // Clean the files generated by NSInstaller.
        private void CleanInstallation()
        {

        }
        #endregion

        #region Download Handler / Extraction Handler
        private void GetDownloadLink(string url)
        {
            using (var webClient = new WebClient())
            {
                IWebProxy webProxy = WebRequest.DefaultWebProxy;
                webProxy.Credentials = CredentialCache.DefaultCredentials;
                webClient.Proxy = webProxy;
                webClient.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; " +
                                  "Windows NT 5.2; .NET CLR 1.0.3705;)");
                webClient.DownloadProgressChanged += (s, e) => { progressBar.Value = e.ProgressPercentage; proglabel.Text = $"[{e.ProgressPercentage}%] Fetching Github API - " + url; };
                webClient.DownloadStringCompleted += (s, e) => { proglabel.Text = "Parsing " + url; };
                webClient.DownloadStringAsync(new Uri(url));
                webClient.Dispose();
            }
        }        

        private async void DownloadAsync(string url, string filePath)
        {           
            using (var webClient = new WebClient())
            {
                IWebProxy webProxy = WebRequest.DefaultWebProxy;
                webProxy.Credentials = CredentialCache.DefaultCredentials;
                webClient.Proxy = webProxy;
                webClient.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; " +
                                  "Windows NT 5.2; .NET CLR 1.0.3705;)");
                webClient.DownloadProgressChanged += (s, e) => { progressBar.Value = e.ProgressPercentage; proglabel.Text = $"[{e.ProgressPercentage}%] Downloading - " + url; };
                webClient.DownloadFileCompleted += async (s, e) => { proglabel.Text = "Extracting required files to root path...";  await Task.Run(() => ZipFile.ExtractToDirectory(filePath, filePathTxt.Text)); proglabel.Text = "Finished Extracting!"; };
                await webClient.DownloadFileTaskAsync(new Uri(url), filePath).ConfigureAwait(false);
                webClient.Dispose();
            }
        }       
        #endregion

        #region Tools | onClick Events
        private void installSigBttn_Click(object sender, System.Windows.RoutedEventArgs e)
        {

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

        #region Misc | SetButtonIsEnabled
        private void setAllButtonIsEnabled(bool isEnabled)
        {
            filePathTxt.IsEnabled = isEnabled;
            openFileLocationBttn.IsEnabled = isEnabled;
            startBttn.IsEnabled = isEnabled;
            installSigBttn.IsEnabled = isEnabled;
            fixAtmoBttn.IsEnabled = isEnabled;
            updtHekateBttn.IsEnabled = isEnabled;
            updtAtmoBttn.IsEnabled = isEnabled;
        }
        #endregion

        #region Models and Utilities
        Util util = new Util();
        #endregion
    }
}