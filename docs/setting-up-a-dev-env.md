# Setting Up A Development Environment

## Prerequisites
- **Windows 10 (ideally)** - The game is written in the .NET Framework, which can only be ran natively on Windows. The MSBuild targets used by the projects have only been tested with Windows 10. Older versions of Windows may work and other operating systems using Mono may also work, but neither of these are tested.
- **OpenGL 4.x** - A minimum version of OpenGL 4.0 is required to run.

## Libraries

The following is a list of library builds that should be distributed with a build of the game. These need to be placed under `src/Natives/<32|64>/windows/library.dll`.

### GLFW
A [GLFW](http://www.glfw.org/) 3.x version of at least v3.1.2 is needed to run the game.

**Windows Example**

A compiled DLL of GLFW should be placed at `src/Natives/32/windows/glfw3.dll`.

## Error Handler
Release client builds require a build of the game's error handler. Compile either `src/ErrorHandler` or `src/Dash.ErrorHandler` and place the resulting `ErrorHandler.exe` in the `src` folder at `src/ErrorHandler.exe`.

## Tools

### Angel Code Font Tool
Fonts are loaded into the game from the [Angel Code Bitmap Font format](http://www.angelcode.com/products/bmfont/). It is recommened to use [Hiero from the libGDX team](https://github.com/libgdx/libgdx/wiki/Hiero) for creating fonts for the game.
