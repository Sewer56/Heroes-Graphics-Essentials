
<div align="center">
	<h1>Project Reloaded (Mod Template)</h1>
	<img src="https://i.imgur.com/BjPn7rU.png" width="150" align="center" />
	<br/> <br/>
	<strong>All your mods are belong to us.</strong>
	<p>C# based universal mod loader framework compatible with arbitrary processes.</p>
</div>

# Deprecation Notice
This project has been deprecated. 

It has been ported to the newer, [Reloaded-II](https://github.com/Reloaded-Project/Reloaded-II) mod loader.

The new project can be found here: [Heroes.Graphics.Essentials.ReloadedII](https://github.com/Sewer56/Heroes.Graphics.Essentials.ReloadedII).

# About This Project

The following project is a Reloaded Mod Loader mod that provides/implements a set of essential for running Sonic Heroes in 2018; focusing namely on graphical elements.

## How to Use

A. Install Graphics-Essentials like a regular Reloaded mod.
B. Modify settings in Graphics.json before launching.

## Feature Set

### Graphics
```
Width:              The width of the game window, in pixels.
Height:             The height of the game window, in pixels.
Fullscreen:         Sets the game to fullscreen mode.
Borderless:         Sets the game window to borderless if the game is not in fullscreen mode.
Resizable:          Makes the game window resizable if the game is not in fullscreen mode.
Disable2PFrameSkip: Disables the frame skipping behaviour in 2 player mode, allowing you to play at 60 FPS.
AspectRatioLimit:   If the game window is below this aspect ratio, the widescreen hack 
                    scales the window vertically instead of horizontally.

StupidlyFastLoadTimes: 
                    Reimplements a bug to the extreme that happens while running the game in fullscreen on modern hardware.                      https://twitter.com/sewer56lol/status/1031387436683866112

EnableAspectHack:   A hack to stop the game crashing on the stage titlecards with extreme aspect ratios such as 9:16 (W:H) or 168:1 (W:H).

AlternateAspectScaling:
                    An alternative way of scaling the game's FOV.
                    Setting this locks the Aspect Ratio of the in-game HUD to the set AspectRatioLimit.
                    Normal behaviour with this flag off is a HUD shrink.
```

### Graphics (DX9 Settings)
```
Enable: Copies Crosire's D3D8To9 to the game directory if it does not exist and renders the game
        through DirectX 9.

EnableMSAA: Enables Multi Sampling Anti Aliasing to reduce the amount of jagged edges shown on character
            models.
        
EnableAF:   Enables Anisotropic Filtering.

MSAALevel:  Specifies the number of samples for MSAA; this value can be anywhere between 1 and 16.
            Powers of two are recommended.

AFLevel:    Specifies the level of anisotropic filtering used between 1 and 16.

VSync:      Enabling this kills any possible tearing at the expense of possible input latency.

HardwareVertexProcessing: 
            If set to true; the GPU processes the vertex shaders increasing performance.
            Otherwise the CPU processes vertex shaders.
```

### As a Default Launcher Replacement
The following are named using the developers' own original names.

```
Language:           Note: This setting is mostly useless; game stores language per-save.
    0 = Japanese    (Default Launcher Ignores this Language) 
    1 = English
    2 = French
    3 = Spanish
    4 = German
    5 = Italian
    6 = Korean      (Default Launcher Ignores this Language)

SFXVolume: Volume of sound effects from 0 to 100.
BGMVolume: Volume of music from 0 to 100.    
ThreeDimensionalSound: Controls whether enemies such as objects emit positional sound.
SFXOn: Self explanatory.
BGMOn: Self explanatory.
SoftShadows: If this is false, all shadows are "cheap" circles rather than explicit models.
MouseControl: Controls the default mouse movement mode.
CharmyShutup: Disables character action sounds such as Knuckles' famous 'Shoot! Rock! Yeah!'.
```
