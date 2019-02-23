# pixel-splitter
A pixel based auto splitter for LiveSplit
LiveSplit.PixelSplitter is a component for LiveSplit to do auto splits using visual cues. Bringing auto splitting to console speedruns.

## How to install
Before you can use this, you will need to install [OBS-VirtualCam](https://obsproject.com/forum/resources/obs-virtualcam.539/) as it will be used for the image stream that PixelSplitter use.

When ready, you can download a build or build it yourself. If you download the build, just copy the files into the LiveSplit components folder.

To add the PixelSplitter component, you will have to
1. Open LiveSplit
2. Right-click and select edit layout
3. Click on the + sign and select Control > Pixel based Auto Splitter

## How to configure
After installing PixelSplitter, it will need to be configured to work for your game.

## Build
To build, you will need Visual Studio 2017 or newer. 
Then make sure the references to **LiveSplit.Core** and **LiveSplit.Plugin** is available. 
They should exist in the same folder as your livesplit program.


## Changelog
### Version 1.0
Initial release