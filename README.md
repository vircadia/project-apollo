# Project Apollo

## Goal
To create a opensource metaverse server for Project-Athena.
This metaverse server deals with "domains" and "users".
Other metaverse functions (inventory, chat, sound, assets, ...)
are handled by other servers.

## State of Implementation

This project is not yet complete and has many API calls missing. 
In this early stage, this metaverse server implementation is only
good for development.
Eventually, this project will allow people to create and run
their own personal portion of the Metaverse.

For building and running this code,
refer to [Notes On Development] for that information.

As of 20200616 (June 16, 2020), there is an experimental version
of the Project Apollo metaverse-server running at `http://metaverse.bluestuff.org`.
To use that setup to either test the metaverse-server implementation
or to test a domain-server, follow the information in
[test setup].

## Docker Image

A Docker image is made of the latest release.
As of 20200611, the Docker image is stored at
hub.docker.com with the name "misterblue/vircadia-metaverse-server".
Someday CI will be integrated with this project and then the image may move.

Refer to [instructions for running Docker image] for details.

## Troubleshooting

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
[Notes On Development]: docs/NotesOnDevelopment.md
[instructions for running Docker image]: docs/RunningDockerImage.md
[test setup]: docs/TestSetup.md

