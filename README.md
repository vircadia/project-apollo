# Project Apollo

## Disclaimer
This project is not yet complete and has many API calls missing. 

## Goal
To create a opensource metaverse server for Project-Athena

## Build Instructions

```
dotnet restore
dotnet build
```

## Pre-Requisites

Other than dotnet-core, the pre-requisite packages are all NuGet
packages which are installed with the "dotnet restore":

```
dotnet-core 3.1
Newtonsoft.Json
RandomNameGeneratorLibrary
HttpMultipartParser
Microsoft.IO.RecyclableMemoryStream
BCrypt.Net-Next
Nerdbank.GitVersioning
Microsoft.NET.Test.SDK
NUnit
NUnit3TestAdaptor
```

## Running

When you run the server, you must point any domain-servers (Project-Athena) at this
metaverse server. This is accomplished by setting the `HIFI_METAVERSE_URL`:

```sh
export HIFI_METAVERSE_URL=http://HOSTADDRESS:9400
./run_domain-server
```
If you are modifying domain-server sources, the default server address is in
`NetworkingConstants.h`:

```c++

    const QUrl METAVERSE_SERVER_URL_STABLE { "http://127.0.0.1:9400" };
    const QUrl METAVERSE_SERVER_URL_STAGING { "http://127.0.0.1:9400" };

```

## Versioning and Development

This server uses [Nerdbank.GitVersioning] to add versioning information to
the build. The file `version.json` specifies the version of the build. The
Git 'height' is added to the longer version number to uniquify the builds.

The Git branch `master` is used for development and it is updated with the
latest, untested sources and pull requests. Branches are created for the
stable releases. For instance, there will be a branch named `v1.1` that is
created to hold the distributed version 1.1 release.
This release branch will get fixes before release and will receive the post-release patches.

The complete, uniquifying version number (including version, release status, and Git
commit number) is printed on the console and in the log file.

## Configuration

The application reads a configuration file and accepts command line parameters.
Each of the parameters listed below can be in the JSON format configuration file
or specified on the command line (prepended with a double-dash or prepended with "--no" to complement booleans)

| Parameter | Description | Type | Default |
| --------- | ----------- | ---- | ------- |
| Quiet         | Quiet console output | bool | false |
| Verbose       | Excessive console output | bool | false |
| ConsoleLog    | Also log to the console | bool | true |
| ConfigFile    | Per site configuration file | string | "config.json" |
| DefaultIceServer | IP address of ice server. If empty, set to self. | string | "" |
| Listener.Host | HttpListener host | string | "+" |
| Listener.Port | HttpListener port | int | 9400 |
| Listener.Response.Header.Server | What to return as 'Server: header field | string | "1.5" |
| Storage.Dir   | Root of entity storage | string | "Entities" |
| Storage.StaticDir | Directory of static pages served for users | string | "Static" |
| Commerce.MarketplaceKey | Public key for Marketplace access. This is a placeholder. | string | "lksjdlkjskldjflsd" |
| LogLevel       | One of 'warn', 'info', 'debug' | string | "Debug" |
| Logger.RotateMins | Minutes to write to log file before starting next | int | 60 |
| Logger.ForceFlush | Force a flush after each log write | bool | true |
| Logger.LogDirectory | Directory to put logs into | string | "Logs" |
| Debug.Processing | Whether to print each API request processing | bool | false |

A configuration file that changes the listening port looks like:

```json
{
    "Listener.Port": 19400
}
```

While a command line doing the same:

`./ProjectApollo.exe --Listener.Port=19400`

The default console logging can be suppressed with the command line:

`./ProjectApollo.exe --noConsoleLog`

The configuration file and storage directories default to the application's run directory but
can be placed anywhere by specifying the `ConfigFile` or `Storage.Dir` parameters.

## Troubleshooting

### Windows

Much of the development and operation was done on Windows 10 with Visual Studio 2019.

If building from command line,
make sure you have dotnet core 3.1 SDK installed. Most of the development and 
building should be possible from the command line, just by executing the bat files.
Builds are located in `bin/`.

On Windows10, you must add the listening URL to the HTTP ACL:

     `netsh http add urlacl url=http://+:9400/ user=everyone`

### Mac & Linux

Install dotnet core 3.1 from https://dotnet.microsoft.com/download/dotnet-core/3.1


# LICENSING

This code is built on the shoulders of giants and relies on the generous licensing of
the developers who have come before us:

**Project-Apollo** is Copyright 2020 Vircadia contributors and is licensed under [Apache License, Version 2.0].

**NewtonSoft.Json** is licensed under the [MIT License].

**BCrypt.Net-Next** is
Copyright (c) 2006 Damien Miller djm@mindrot.org (jBCrypt),
Copyright (c) 2013 Ryan D. Emerle (.Net port), and
Copyright (c) 2016/2020 Chris McKee (.Net-core port / patches)
and is licensed under the [MIT License].

**RandomNameGeneratorLibrary** is Copyright (c) 2014 Mark Rogers and licensed under the [MIT License].

**HttpMultipartParser** is Copyright (c) 2013 Jake Woods and licensed under the [MIT License].

**Nerdbank.GitVersioning** is Copyright Andrew Arnott and licensed under the [MIT License].

**Microsoft.IO.RecyclableMemoryStream** is Copyright Microsoft and licensed under the [MIT License].

**NUnit** and **NUnit3TestAdaptor** is Copyright (c) 2019 Charlie Poole, Rob Prouse and is licensed
under the [MIT License].

Portions of other code bases have been referenced and built upon:

**OpenSimulator** is Copyright (c) Contributors, http://opensimulator.org/ and is licensed under
the [3-Clause BSD License].

**LookingGlass Viewer** is Copyright 2008, Robert Adams and is licensed under
the [3-Clause BSD License].

[Apache License, Version 2.0]: http://www.apache.org/licenses/LICENSE-2.0
[MIT License]: https://opensource.org/licenses/MIT
[3-Clause BSD License]: https://opensource.org/licenses/BSD-3-Clause
[Nerdbank.GitVersioning]: https://github.com/dotnet/Nerdbank.GitVersioning

