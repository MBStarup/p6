# B.Sc. final project
## Comparing Impacts of Data and Object Oriented Programming on Energy Consumption in C\#

### Setup
This repo contains two functionally equivalent implementations of a small test game, implemented in different styles, used for measurements for our bachelor project.

Make dure you have [dotnet](https://dotnet.microsoft.com/en-us/download) installed.

Requires [SDL2](https://github.com/libsdl-org/SDL/releases/tag/release-2.32.8), make sure the DLL is available in the PATH, or placed in the working directory or next to the executable.

The project relies on [SDL2-CS](https://github.com/flibitijibibo/SDL2-CS/) to wrap SDL2. Initialize the git submodule and build this:

```bash
git submodule update --init --recursive
dotnet publish -c Release -f net7.0 SDL2-CS\SDL2-CS.Core.csproj
```

Then you should be able to run either version of the game:
```bash
dotnet run -c Release -p oop
dotnet run -c Release -p dop
```

### Replays

The game will by default record a replay of input actions that can be replayed for performance benchmarking.
As an example you could replay the
```bash
dotnet run -c Release --project TestRunner -- 1 -p .\oop\bin\Release\net8.0\oop.exe -n oop_test .\Replays\Short30Sec
```

However, ***PLEASE NOTE*** that this relies on the [`perf`](https://www.man7.org/linux/man-pages/man1/perf.1.html) utility being installed and correctly configured on the system.
