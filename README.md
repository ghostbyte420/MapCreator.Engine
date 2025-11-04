
<img width="1000" height="545" alt="mcheader" src="https://github.com/user-attachments/assets/0898f4c6-0401-4c3f-ac8d-2c6f7f4976e7" />

```sh
                                                                        Version 5.0 | Build 10212025
```

### Overview

> MapCreator is a Windows Forms-based tool designed for [Ultima Online](http://uo.com)™ custom facet generation. Whether you're an Ultima Online™ server developer or hobbyist, MapCreator simplifies the process of designing and compiling custom maps for your game server...

<br/>

### Features
---
🛠️ 🎨 **Configure Color Tables**<br/><br/>
✔️ allows users to load up and view the Terrain.xml and Altitude.xml which assign colors to the tiles in game<br/><br/>
✔️ users can also customize the color tables and add new game tiles by assigning colors to them<br/><br/> 
<img width="626" height="10" alt="Untitled-2" src="https://github.com/user-attachments/assets/f433a9f7-3927-4ef8-b227-c895be4330a0" /><br/>
🛠️ 🖼️ **Create Map Templates**<br/><br/>
✔️ allows users to output their facet *Terrain.bmp* and *Altitude.bmp*files<br/><br/>
✔️ users can use the default map sizes from the drop-down list OR they can add their own custom sizes to the list<br/><br/>
<img width="626" height="10" alt="Untitled-2" src="https://github.com/user-attachments/assets/f433a9f7-3927-4ef8-b227-c895be4330a0" /><br/>
🛠️ 📐 **Draw A Custom Facet**<br/><br/>
✔️ users can now use a lightweight bitmap editing program designed for facet templates created with MapCreator<br/><br/> 
⚠️ this feature is new and has not been fully implemented yet<br/><br/>
<img width="626" height="10" alt="Untitled-2" src="https://github.com/user-attachments/assets/f433a9f7-3927-4ef8-b227-c895be4330a0" /><br/>
🛠️ 🖼️ **Encode Altitude Bitmap**<br/><br/>
✔️ when users are done with their *Terrain.bmp*, they can use this utility to convert it into an *Altitude.bmp*<br/><br/>
✔️ the conversion will use different RGB values for the land, water, rock, and other<br/><br/>
✔️ users can then load up the *Altitude.bmp* and use those colors to draw in their facet elevations<br/><br/>
<img width="626" height="10" alt="Untitled-2" src="https://github.com/user-attachments/assets/f433a9f7-3927-4ef8-b227-c895be4330a0" /><br/>
🛠️ ⚙️ **Compile Your New Map**<br/><br/>
✔️ when users are done their *Terrain.bmp*, and the *Altitude.bmp* is finished, they can then compile both files<br/><br/>
✔️ this creates 3 files that most Ultima Online™ game clients can load up: **Map0.mul**, **Staidx0.mul**, and **Statics0.mul**<br/><br/>
<img width="626" height="10" alt="Untitled-2" src="https://github.com/user-attachments/assets/f433a9f7-3927-4ef8-b227-c895be4330a0" /><br/>
🛠️ 🧩 **User-Submitted Utilities**<br/><br/> 
✔️ there is a section where users can host utilities that will assist with custom facet creation<br/>

<br/>

### Comumunity
---
*UO Emulation*<br/>
[RunUO](http://runuo.com)<br/>
[ServUO](http://servuo.com)<br/><br/>
*Game Engine*<br/>
[uoAvox](https://uoavox.studio)<br/><br/>
*Custom Client*<br/>
[ClassicUO](https://www.classicuo.eu/)<br/>

<br/>

### Technical Stuff
---
🖥️ *Software Requirements*<br/><br/>

⚡ [Microsoft's Visual Studio Community Edition](https://visualstudio.microsoft.com/vs/community/)<br/><br/>
⚡ **AI** to explain the code if you aren't sure how to read it: **recommended**: [MistralAI](https://mistral.ai/)<br/><br/>

📋 *Visual Studio Synopsis*<br/><br/>

📌 Start up visual studio and create an .sln file. Name it: MapCreator and save it<br/><br/>
📌 Clone this repo on your machine: https://github.com/ghostbyte420/MapCreator<br/><br/>
📌 Clone this repo on your machine: https://github.com/ghostbyte420/MapCreator.Engine<br/><br/>
📌 Link up both projects in your newly created .sln file:<br/> 
❗ make sure the **MapCreator.Engine.dll** is used as a project reference for the MapCreator project<br/><br/>
📌 Compile solution: a 'bin' and 'obj' folder will be created inside the MapCreator projects directories<br/><br/>
📌 Open up the 'Required' directory:<br/> 
❗ copy and paste the directories inside into: **/bin/Debug/net8.0-windows**<br/><br/>


