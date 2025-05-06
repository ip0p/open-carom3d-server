"# open-carom3d-server" 

Thank you Comworm and his team (Neoact people as a general), for making the best Pool/Billiards/Snooker game of all times.
I really hope you all are rich and living well.

----------------------------------------------------

What to expect:
Full functionalities from the official Carom3D server;
Extensions to make server customizations enjoyable.

----------------------------------------------------

Incoming for v0.4.0 - ALPHA:
- Everything supported by carom.exe Client?

----------------------------------------------------

How to build:
Before building, open the server_starter.cs file and change the IP you want to expose.
This is a .NET C# project, just use a supporting .NET compatible compiler (like updated versions of Visual Studio, etc).

*Windows only for now, there are plans for Unix-like systems though.

----------------------------------------------------

How to run:
Double click on the generated server executable.

For remote connections (playing with friends), tell your friends to:
- Download the official Carom3D 5.31 (Last official version) executable;

- Edit their etc/hosts file, adding the following line at the end:
YOUR_EXPOSED_IP caromengopening.neoact.com
YOUR_EXPOSED_IP carom3d.gameis.com

- Make sure there is not a ServerAddress folder in their Carom3D installation path;

- Open carom.exe;

- Click online;

- Log in with any id/password;

- Enjoy.

----------------------------------------------------

How to compile the project:
1. Install .NET SDK from https://dotnet.microsoft.com/download
2. Open a terminal and navigate to the project directory
3. Run the following command to compile the project:
   dotnet build

----------------------------------------------------

How to run the server using GitHub Actions:
1. Create a new GitHub repository and push your project code to it.
2. Create a new file in the `.github/workflows` directory named `ci.yml`.
3. Add the following content to the `ci.yml` file:
   ```
   name: CI

   on:
     push:
       branches:
         - main
     pull_request:
       branches:
         - main

   jobs:
     build:
       runs-on: ubuntu-latest

       steps:
       - name: Checkout code
         uses: actions/checkout@v2

       - name: Setup .NET
         uses: actions/setup-dotnet@v1
         with:
           dotnet-version: '5.0.x'

       - name: Restore dependencies
         run: dotnet restore

       - name: Build
         run: dotnet build --no-restore

       - name: Run tests
         run: dotnet test --no-build --verbosity normal
   ```

----------------------------------------------------

Changelog:

v0.3.0 - ALPHA:
~ More stable match system;
+ Most of player commands (/to, /tog, /in, /cover...);
+ More server types: Advanced, Expert... etc;
+ Simple HTTP support: User Profile, Banner, Spotlight and In-Game Player Picture;
~ Many bug fixes.

v0.2.0 - ALPHA:
~ Major refactoring;
+ Rooms;
+ Matches.

v0.1.0 - ALPHA:
+ Login;
+ Channels;
+ Channel messages.

