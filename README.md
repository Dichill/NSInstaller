# NSInstaller BETA
Install Hekate, Atmosphere, and Signature Patches in one click!<br>
![image](https://user-images.githubusercontent.com/48512644/135878945-7748cabf-94df-40f8-b54f-d8a223c8ab9f.png)

**Start Installation** will check for the latest <a href="https://github.com/CTCaer/hekate">Hekate</a>, <a href="https://github.com/Atmosphere-NX/Atmosphere">Atmosphere</a>, and [Signature Patch](https://github.com/ITotalJustice/patches) releases and will automatically extract it for you in just one click! DO note that you still have to manually back up and create an emuMMC via hekate, this app only helps if you are in a hurry for installing or updating Hekate or Atmosphere. I would still recommend installing these stuff manually but if its too complicated for you, you can use this tool to help you.

## Folders
![image](https://user-images.githubusercontent.com/48512644/136057195-d5fea062-dc98-44e6-886b-b69256a9a6a1.png)

NSInstaller's Folder can be found at **C:/NSInstaller/** It is where backups/temporary files are stored. (Logs not yet implemented.)

## Releases.txt
![image](https://user-images.githubusercontent.com/48512644/136057415-3451716f-2838-4389-9310-09da5863cf62.png)

This .txt file contains repositories that are needed for switch, you can add another one by line and also adding **api.** before **Github.com**<br>

For Example: https://github.com/CTCaer/hekate/releases -> https://api.github.com/repos/CTCaer/hekate/releases<br>

When you **Start Installation** It will also download the files and extract it to your Micro SD.

## How to use
1. First of, plug your micro sd into your computer.
2. Format the micro sd and make sure its Fat32. (Skip this part if its already formatted and its Fat32, ExFat32 is also acceptable but to avoid data corruption we will go for Fat32.)
3. Open NSInstaller and click Open File Location.
4. Locate the path of your Micro SD Folder and click Select Folder.
5. Click on Start Installation and you can relax and sit back and wait for it to finish.
7. Once done you can now Eject it and use [TegraRCMGui](https://github.com/eliboa/TegraRcmGUI) to boot Hekate/Fuuse.
8. Do note that sig patches are already installed into your atmosphere, you don't need to use the Tools since it does it for you already.

**IMPORTANT**
- For a video explaining how to jailbreak switch using NSInstaller you can check this video out: 
- Also don't forget to backup your **Nand**! You can follow this guide right here: https://switchway.xyz/backup-nand-%26-keys

## Tools
- **Install Sig Patches** - If you have Atmosphere already, you can install Sigpatches if games/homebrew applications keeps crashing.
- **Fix Atmosphere Crashes** - Will install the latest Sig Patches and deletes the `\atmosphere\contents\01000000001000`
- **Update Hekate** - Updates Hekate to its latest version.
- **Update Atmosphere** - Backups folders and extracts the latest version of Atmosphere including Sig Patches.

## Common Errors
Any errors that isn't in the Common Errors list, please file an issue with the picture, or join the support discord.
- **The File C:/NSInstaller/backup/...** - Back up folder still exist, delete everything inside it. If you deleted the backup folder, restart NSInstaller.

## Credits
- [benruehl](https://github.com/benruehl) for the best looking themes [Adonis UI WPF](https://github.com/benruehl/adonis-ui/)
- [Ookii](https://github.com/ookii-dialogs) for the amazing folder dialogs! [Ookii Dialogs WPF](https://github.com/ookii-dialogs/ookii-dialogs-wpf)
- [Anonymous](#) for the amazing [XCI-Explorer!](https://github.com/StudentBlake/XCI-Explorer)
- [CTCaer](https://github.com/CTCaer) for the amazing bootloader! [Hekate](https://github.com/CTCaer/hekate)
- [SciresM](https://github.com/Atmosphere-NX) for the amazing custom firmware! [Atmosphere](https://github.com/Atmosphere-NX/Atmosphere)
- [ITotalJustice](https://github.com/ITotalJustice) for the amazing sig patches! [Patches](https://github.com/ITotalJustice/patches)
- [Switchway](https://switchway.xyz/) if it wasn't for their switch modding guides, I wouldn't be where I'm today!
