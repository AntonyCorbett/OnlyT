# OnlyT <img src="https://ci.appveyor.com/api/projects/status/d0wra2jk7o23fagx?svg=true">

Windows Meeting Timer using C#, WPF and custom analogue clock control. Designed for use in Kingdom Halls where the meeting format is predefined, but has a "manual" and "file-based" mode that can be used to configure the timers as required (and so can be used in other settings too).

** 7th Feb 2024 - I've just fixed an issue with the times used by the automatic mode. You may have to delete the locally cached data in order for OnlyT to pick up the fresh feed. To do this, open Windows File Explorer and enter "%AppData%\OnlyT" into the address bar. Delete the "feed.json" file.**

![Main Window](http://cv8.org.uk/soundbox/OnlyT/Images/MainWindow2.png)

![Timer Display](http://cv8.org.uk/soundbox/OnlyT/Images/Monitor02.png)

### System Requirements

* Windows 10
* .NET 5 x86 or later
* 2GB RAM
* 20MB Hard disk space
* Internet connection (for "Automatic" Operating Mode only)

### Download

If you just want to install the application, please download the [OnlyTSetup.exe](https://github.com/AntonyCorbett/OnlyT/releases/latest) file (there is also a portable version if you'd prefer to just copy a folder). If you choose the portable option, you may also need to download and install the Microsoft .NET 5.0 Desktop runtime (x86) from [here](https://dotnet.microsoft.com/download/dotnet/5.0/runtime).

### Help

See the [wiki](https://github.com/AntonyCorbett/OnlyT/wiki) for basic instructions and for information on where to get further help.

See the [FAQ](https://github.com/AntonyCorbett/OnlyT/wiki/FAQ) for frequently asked questions.

### License, etc

OnlyT is Copyright &copy; 2018, 2021 Antony Corbett and other contributors under the [MIT license](LICENSE).

NAudio (Mark Heath) is used under the Microsoft Public License (Ms-PL). MaterialDesign themes (James Willock, Mulholland Software and Contributors) is used under the MIT license. NUglify, Copyright (c) 2016, Alexandre Mutel. QRCode (Raffael Herrmann) is used under MIT. Serilog is used under the Apache License Version 2.0, January 2004. LiteDB (Mauricio David) is used under the MIT License. PDFSharp is used under the MIT License.

With thanks to:
* Crowdin.com for localisation tools
* GitHub for project management tools
* [JetBrains](https://jb.gg/OpenSourceSupport) for Resharper tools
* A team of over 20 translators who have helped localise OnlyT
