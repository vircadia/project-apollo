#! /bin/bash
# Run MetaverseServer.

cd /var/vircadia/dist

echo "Doing: dotnet ./ProjectApollo.dll --ConfigFile '/var/vircadia/content/config.json' $@"
dotnet ./ProjectApollo.dll --ConfigFile "/var/vircadia/content/config.json" $@

exit $?
