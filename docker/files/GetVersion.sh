#! /bin/bash
# Run MetaverseServer to print out the version

cd /var/vircadia/dist

dotnet ./ProjectApollo.dll --version

exit $?
