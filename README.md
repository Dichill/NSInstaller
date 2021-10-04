# NSInstaller
Install Hekate, Atmosphere, and Signature Patches in one click!

**Start Installation** will check for the latest <a href="https://github.com/CTCaer/hekate">Hekate</a>, <a href="https://github.com/Atmosphere-NX/Atmosphere">Atmosphere</a>, and [Signature Patch](https://github.com/ITotalJustice/patches) releases and will automatically extract it for you in just one click! DO note that you still have to manually back up and create an emuMMC via hekate, this app only helps if you are in a hurry for installing or updating Hekate or Atmosphere. I would still recommend installing these stuff manually but if its too complicated for you, you can use this tool to help you.

## Images
![image](https://user-images.githubusercontent.com/48512644/135766310-29f597d4-5393-48bd-b1bd-f80360c0c3a4.png)

## How to use
1. First of, plug your micro sd into your computer.
2. Format the micro sd and make sure its Fat32. (Skip this part if its already formatted and its Fat32, ExFat32 is also acceptable but to data corruption we will go for Fat32.)
3. Open NSInstaller and click Open File Location.
4. Locate the path of your Micro SD Folder and click Select Folder.
5. Click on Start Installation and you can relax and sit back and wait for it to finish.
7. Once done you can now Eject it and use [TegraRCMGui](https://github.com/eliboa/TegraRcmGUI) to boot Hekate/Fuuse.
8. Do note that sig patches are already installed into your atmosphere, you don't need to use the Tools since it does it for you already.

**IMPORTANT**
- If you're having a tough time with **No. 7** I recommend you to watch this video: 
- Also don't forget to backup your **Nand**! You can follow this guide right here: https://switchway.xyz/backup-nand-%26-keys

## Tools
- **Install Sig Patches** - If you have Atmosphere already, you can install Sigpatches if games/homebrew applications keeps crashing.
- **Fix Atmosphere Crashes** - Will install the latest Sig Patches and deletes the `\atmosphere\contents\01000000001000`
- **Update Hekate** - Updates Hekate to its latest version.
- **Update Atmosphere** - Backups folders and extracts the latest version of Atmosphere including Sig Patches.

## Credits
- [benruehl](https://github.com/benruehl) for the best looking themes [Adonis UI WPF](https://github.com/benruehl/adonis-ui/)
- [Ookii](https://github.com/ookii-dialogs) for the amazing folder dialogs! [Ookii Dialogs WPF](https://github.com/ookii-dialogs/ookii-dialogs-wpf)
- [Anonymous](#) for the amazing [XCI-Explorer!](https://github.com/StudentBlake/XCI-Explorer)
- [CTCaer](https://github.com/CTCaer) for the amazing bootloader! [Hekate](https://github.com/CTCaer/hekate)
- [SciresM](https://github.com/Atmosphere-NX) for the amazing custom firmware! [Atmosphere](https://github.com/Atmosphere-NX/Atmosphere)
- [ITotalJustice](https://github.com/ITotalJustice) for the amazing sig patches! [Patches](https://github.com/ITotalJustice/patches)
- [Switchway](https://switchway.xyz/) if it wasn't for their switch modding guides, I wouldn't be where I'm today!
