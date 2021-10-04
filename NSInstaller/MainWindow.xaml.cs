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

namespace NSInstaller
{
    public partial class MainWindow : AdonisWindow
    {

        public MainWindow()
        {          
            InitializeComponent();

            this.Title = "NSInstaller v" + getAssemblyVersion();                     

            // The Installation Folder where we dump the downloaded files.
            if (!Directory.Exists("temp")) Directory.CreateDirectory("temp");
            if (!Directory.Exists("logs")) Directory.CreateDirectory("logs");

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
                // Check First the Folder.
                startBttn.Content = "Checking...";
                proglabel.Text = "Status: Checking Folder Path.....";                

                try
                {
                    var directory = new DirectoryInfo(filePathTxt.Text);
                    var files = directory.GetFiles();
                    if (files.Length == 0) StartInstallation();
                    else if (files.Length > 1) { MessageBox.Show("Please format your SD Card First before doing the Installation. (There are files in the Micro SD)", "NSInstaller", MessageBoxButton.OK, MessageBoxImage.Warning); startBttn.Content = "Start Installation"; proglabel.Text = "Failed: SD Card is not Empty!"; }
                }
                catch (Exception error) {
                    MessageBox.Show("Folder Location not found! Make sure that the SD Card is plugged in!", "NSInstaller", MessageBoxButton.OK, MessageBoxImage.Warning); startBttn.Content = "Start Installation"; proglabel.Text = "Error: Directory does not Exist!";
                    Console.WriteLine("Error Occured: " + error.ToString()) ;
                    proglabel.Text = error.ToString();
                }                
            }
        }

        private void StartInstallation()
        {
            setAllButtonIsEnabled(false);

            startBttn.Content = "Installing...";
            proglabel.Text = "Status: Starting Installation....";
        }

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
