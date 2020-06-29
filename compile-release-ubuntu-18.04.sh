echo ////////////////////////////////////////////////////////////
echo ////////////////////////////////////////////////////////////
echo Warning
echo This script will Check to see if the necessary components
echo are installed and install them if they are not And then build
echo Necessary components.
echo snapd
echo dotnet
echo Would you like to continue?
echo ////////////////////////////////////////////////////////////
echo ////////////////////////////////////////////////////////////
read -p "Continue (y/n)?" CONT
if [ "$CONT" = "y" ]; then
  echo "yaaa";
else
  exit
fi
apt-get -y update
dpkg -s snapd
if [ $? -eq 0 ]; then
    echo "snapd is installed!"
else
    sudo apt install snapd
fi
dpkg -s dotnet
if [ $? -eq 0 ]; then
    echo "dotnet is installed!"
else
    sudo snap install dotnet-sdk --classic
    sudo snap alias dotnet-sdk.dotnet dotnet
fi
dotnet build -c Release -r linux-x64
echo Creating file Run-project-apollo-ubuntu-18.04.sh
cat > Run-project-apollo-ubuntu-18.04.sh << EOF1
cd ProjectApollo/bin/Release/netcoreapp3.1/linux-x64
./ProjectApollo
EOF1
chmod +x Run-project-apollo-ubuntu-18.04.sh
echo
echo You can now run the project-apollo with.
echo ./Run-project-apollo-ubuntu-18.04.sh