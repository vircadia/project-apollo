# Project-Apollo API Research Work-In-Progress

This directory contains files being used to capture and define the
APIs for the Project-Apollo MetaverseAPI server.

When [Vircadia] inherited the open-source project from [High Fidelity], 
the APIs for the back-end "Metaverse API" was not documented.
These files capture the reserach done and are being used to
define the initial version of the API.

Future versions of the [Vircadia] API will be documented in the
actual [Vircadia] developer documentation at https://docs.vircadia.com .

## Progress

**20200514**: Moved all work-in-progress files into this directory.

## Files

### CleanedUpURLs.md

A cleaned up version of grep'ing all the [Project Apollo] sources
for "v1" which also included "api". This got most of the scattered
interface refereces (expecting "/api/v1/something").

### API-Research.md

Notes collected from analyzing [CleanedUpURLs.md](./CleanedUpURLS.md).
This includes looking through the sources and finding what fields and
values are being used for the requests.

My plan is to use these notes to create initial [Swagger] interface
defintion files that can be then tested against the existing code
to see what works and doesn't work.

### DomainServer-api.json

Based on the notes from [API-Research.md](./API-Research.md), this
file contains an initial, overall [Swagger] definition of the API.

I expect this definition to be broken up into separate service API
definitions (domain, places, users, groups, ...) and those
actual/final/working definitions will be in the regular "api"
directory and not in this "wip" directory.

### login-logout.md

A capture of the HTTP communication between an Interface and a
DomainServer. The user just logged in and logged out.

This should give some insight to the authentication/authorization setup.

All the actual user names, passwords, keys, and certificates have been
messed with so they are not the original values.

[Vircadia]: https://vircadia.com/
[Project Apollo]: https://github.com/kasenvr/project-apollo
[High Fidelity]: https://en.wikipedia.org/wiki/High_Fidelity_(company)
[Swagger]: https://swagger.io
[Creative Commons Attribution-NonCommercial 4.0 International]: http://creativecommons.org/licenses/by-nc/4.0/

<!-- vim: ts=2 sw=2 et ai
-->

