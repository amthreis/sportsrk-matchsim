# SportsRk - MatchSim

This is a project made with ASP.NET and Godot/C# and receives matches from the main app, simulating them and sending back the results. Godot here is used just so I have an interface while developing, production goes with the API only.

## Usage

The API can be started with a `dotnet restore` and `dotnet run`. It can be opened with Godot 4.2.2 directly, no installation process needed. Execute it with a --headless flag within the cmd so it doesn't render a window. In the project, there's also a debug scene for the match simulator:

![Debugger](https://i.imgur.com/5wAZcRP.png)
