# WinkAtHome
C# Web Application to control your Wink Hub and Devices via the Wink/Quirky API

Before first use, if you want to make settings more secure, go into web.config and change "encyrptionKey" to something unique.  Upon the first run, the app with automatically go into the settings screen.  Here, you should set your username and password.

INSTRUCTIONS:

To install IIS<br>
1) Click the Start button , click Control Panel, click Programs, and then click Turn Windows features on or off. 
2) In the list of Windows features, select Internet Information Services, and then click OK.

To Install WinkAtHome
1) unzip the contents of this release and copy the WinkAtHome folder toyour inetpub\wwwroot folder. This is typically c:\inetpub\wwwroot.

To Configure IIS
1) Click the Start button , click Control Panel, click Administrative Tools, dbl-click Computer Management.
2) Expand Services and Application
3) Click Internet Information Services
4) expand until you are in Default web site
5) WinkAtHome should be listed. Right-click it and choose Convert to Application

To Test
open your browser and navigate to http://localhost/WinkAtHome

Troubleshooting and discussion can be found on g+...
https://plus.google.com/+CraigTrunzo/posts/6nzLFegJxyY

Totally optional and not even encouraged, but any donations made will be used to purchase additional wink products to flush out my system and allow me to more easily add and test new types of devices.

[![PayPayl donate button](https://www.paypalobjects.com/en_US/i/btn/btn_donate_LG.gif)](https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=5NLDWXRPQXSN6 "Donate once-off to this project using Paypal")




