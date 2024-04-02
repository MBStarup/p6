# Dependencies
requires SDL2 for rendering an input, make sure sdl2.dll is in your path, optionally build with rendering and input disabled.

# Build
first build SDL2-CS by running
```
dotnet publish -c Release -f net7.0 SDL2-CS.Core.csproj
```
in the SDL2-CS directory

make sure the resulting dll is added to oop.csproj,

then run this with
```
dotnet run
```
or
```
dotnet publish && ./bin/Release/net8.0/publish/oop
dotnet publish && .\bin\Release\net8.0\publish\oop
```