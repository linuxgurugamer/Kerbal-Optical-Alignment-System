# Kerbal-Optical-Alignment-System
## Synopsis

Adds an optical sight and target to stock docking ports in Kerbal Space Program(tm).  

## Motivation

I wanted to 'kick it old school'.  Back before KSP I experienced this sort of seat of the pants docking in an Apollo simulation created for Orbiter (a great open source space simulator) and I wanted to be able to do that in KSP.  I also felt like something was lacking in the stock game when it came to docking.  Magnetic ports do forgive some amount of inaccuracy, but I felt we should be able to be more precise.  

I made every effort to make this mod a incognito as possible, try to add it in to the game so it seemd like Squad did it themselves.  

## Installation

**Be sure to have the latest version of [ModuleManger](http://forum.kerbalspaceprogram.com/index.php?/topic/50533-110-module-manager-2622-april-19th-with-even-more-sha-and-less-bug/) installed!**

Upzip it into your GameData folder, as with most other mods.

## API Reference

If you wish to add this mod to your own parts (or your favorite parts) follow these ModuleManager config examples:

Adding the Camera
```
@PART[dockingPort1]
{
  MODULE
  {
    name = DPCamera
    cameraName = DPCam
    cameraForward = 0, 1, 0
    cameraUp = 0, 0, -1
    cameraPosition = 0, 0.12, 0.0
    cameraFoVMax = 80
    cameraFoVMin = 80
    cameraMode = 1
	}
}
```

Adding the Target

```
@PART[dockingPort1]
{
	MODEL
	{
		model = FP_KOAS/Parts/DockingTarget/COAS_Target
		position = 0,0.11,0
		scale = 1,1,1
		rotation = 0,0,0
	}
	MODEL
	{
		model = Squad/Parts/Utility/dockingPortShielded/model
	}
	!mesh=DELETE
}
```

**Note** that in both cases the values in "Position" will likely need to be adjusted to get the eyepoint and target models in the right place.

## How-To-Use

Simply right-click the port of the craft you are flying, choose "Control from Here" so the manuvering keys work correctly, and then "View from Here" to be taken into "Docking View".

To exit, just do as the message on screen said and hit the "Camera Key" (default 'c') as a bonus, the next / previous vessel keys (default '[',']') will also "pop" you back into the external view.


## Contributors

Guru's who answered my questions:
- JPLRepo
- sarbian
- Warezcrawler
-	Padishar
	
Authors whose plugins were studied for clues and code:
- Albert VDS
- bernierm

## License

GNU GENERAL PUBLIC LICENSE V3 dated 6/29/2007
