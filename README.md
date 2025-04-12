# Isolumia bhvrSession Grabber
Very easy and self explaining bhvrSession grabber to use for prestige injection or whatever else.<br><br>

Check out our web based Prestige Injector here: https://injector.isolumia.com/ <br>
*Completely free without limitations*<br><br>


If you play on Steam you require an SSL Bypass since it is a proxy, find that here:
https://github.com/crexpy/Isolumia-Simple-SSL/releases/latest
You just enable Items and click Start, after that start the session grabber
<br><br><br>
If you need help with anything join the Discord :)<br><br>
[![Join our Discord](https://invidget.switchblade.xyz/ZBsJ834qxj)](https://discord.gg/ZBsJ834qxj)

<br>

# Download
You can either download the source code and compile it yourself - or download the already compiled version [here](https://github.com/crexpy/bhvrSession-Grabber/releases/latest).
<br><br>

I recommend to compile it as self contained:<br>
```dotnet publish -c Release -r win-x64 --self-contained /p:PublishSingleFile=true```<br><br>
Location: \bin\Release\net8.0\win-x64\publish

<br>

# Setup
After downloading the tool heres how you use it:
1. Make sure you already have Epic Games / Xbox App open since they won't load with a proxy enabled

2. Start the exe as Adminstrator<br>

3. Accept the Fiddler Root certificate<br>

4. Start your game<br>

5. After making it to the loading screen where it tells you to press any button to continue, simply do so and tab back into the application<br>

6. Now after a few seconds the request should have been catched and you succesfully grabbed your bhvrSession!<br>

7. Either press any button in the application to terminate your game and ensure the bhvrSession stays valid or close the the application to keep your game open<br><br>
Note that if you normally close the game by using Alt + F4 or whatever your bhvrSession will go invalid. If you don't instantly termanite it ensure to do so afterwards by using your Task Manager if you want it to stay valid.

<br>

# Showcase Video
https://github.com/user-attachments/assets/b43a5409-79c1-4fa7-b5e9-3f813dd41f98


<br>
Permanent link if the video goes invalid or something: https://cdn.isolumia.com/bhvrSessionGrabberShowcase.mp4




<br><br>

# Uninstall the Certificate
To uninstall the certificate follow these steps:
1. Head into ```%localappdata%/Isolumia/BhvrSessionGrabber``` and delete ```FiddlerRootCertificate.p12```
![image](https://github.com/user-attachments/assets/9553a54a-07f7-444d-a1a9-204e741d4e96)
<br>

2. Open up the certificate manager by searching for ```Manage user certificates```
3. Go to ```Trusted Root Certification Authorities``` (2nd) -> ```Certificates```
4. Search for ```DO_NOT_TRUST_FiddlerRoot``` -> Right click -> Delete
![image](https://github.com/user-attachments/assets/e00ccd16-cdc2-4d26-b510-ce9b73b28505)
