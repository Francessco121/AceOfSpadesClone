# The Ace of Spades Clone
By Ethan Lafrenais


This repository contains my final senior year project for my highschool programming course. My goal was to recreate as much of the game [Ace of Spades Classic](https://www.buildandshoot.com/) as possible.

It's worth mentioning that I did use some textures and recreated some models from the [OpenSpades project](https://github.com/yvt/openspades).

I plan to update this repository with more details on how I made this clone, and the technologies that went into it! But for now, please [see the AoS page on my website](https://francessco.us/projects/ace-of-spades-clone) for more information.

### Commit History
Unforunately, I lost the entire commit history uploading to GitHub. I had to create a new repository since one of the earlier commits contained files over 100mb (which GitHub does not allow). :(

## Content Files

### Audio
Due to licensing issues, none of the game's sound files will be included in this repository. The game however will continue to function without sound.

## Setting Up A Development Environment

### Prerequisites
- **Windows 10 (ideally)** - The game is written in the .NET Framework, which can only be ran natively on Windows. The MSBuild targets used by the projects have only been tested with Windows 10. Older versions of Windows may work and other operating systems using Mono may also work, but neither of these are tested.
- **OpenGL 4.x** - A minimum version of OpenGL 4.0 is required to run.

### Libraries

The following is a list of library builds that should be distributed with a build of the game. These need to be placed under `src/Natives/<32|64>/windows/library.dll`.

#### GLFW
A [GLFW](http://www.glfw.org/) 3.x version of at least v3.1.2 is needed to run the game.

##### Windows Example
A compiled DLL of GLFW should be placed at `src/Natives/32/windows/glfw3.dll`.

### Error Handler
Release client builds require a build of the game's error handler. Compile either `src/ErrorHandler` or `src/Dash.ErrorHandler` and place the resulting `ErrorHandler.exe` in the `src` folder at `src/ErrorHandler.exe`.

### Tools

#### Angel Code Font Tool
Fonts are loaded into the game from the [Angel Code Bitmap Font format](http://www.angelcode.com/products/bmfont/). It is recommened to use [Hiero from the libGDX team](https://github.com/libgdx/libgdx/wiki/Hiero) for creating fonts for the game.
