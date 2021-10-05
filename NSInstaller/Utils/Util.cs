using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSInstaller.Utils
{
    class Util
    {
        // Credits to CTCaer & Contributors for making this amazing Bootloader!
        // Credits to SciresM & Contributors for making this amazing Custom Firmware!
        // Credits to ITotalJustice for providing Signature Patches!
        // Credits to shchmue & Contributors for making Lockpick RCM!

        // Links must be in order so it will replace the files needed to be patched.
        public string[] github_releases = File.ReadAllLines("releases.txt");

        // Folder Paths related to NS Installer
        public string root_folder = "C:/NSInstaller/";

        public string temp_folder = "C:/NSInstaller/temp/";

        public string logs_folder = "C:/NSInstaller/logs/";

        public string backup_folder = "C:/NSInstaller/backup/";
    }
}
