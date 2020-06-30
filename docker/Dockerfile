# Dockerfile for building a runnable version of MetaverseServer

# Adding the 'focal' to the tag gets Ubuntu 20.04
# Adding the 'bionic' to the tag gets Ubuntu 18.04
FROM mcr.microsoft.com/dotnet/core/sdk:3.1-bionic as project-apollo-build
# FROM mcr.microsoft.com/dotnet/core/runtime:3.1-focal

# ENV TARGET=Release
ENV TARGET=Debug

# Optout of the .NET Core Telemetry (https://aka.ms/dotnet-cli-telemetry)
ENV DOTNET_CLI_TELEMETRY_OPTOUT=true

LABEL Version=${VERSION}
LABEL Description="Docker container MetaverseServer"

# add the development environment and base tools
RUN apt-get update \
    && apt-get upgrade -y \
    && apt-get install -y \
        curl \
        git \
        vim \
    && apt-get clean \
    && rm -rf /tmp/* var/tmp/* \
    && rm -rf /var/lib/apt/lists/*

# Get the MetaverseServer sources and build
# NOTE: the binaries are built into /home/${USER}/dist
RUN cd /usr/local/src \
    && mkdir -p /usr/local/src/dist \
    && git clone https://github.com/kasenvr/project-apollo.git \
    && cd project-apollo \
    && git checkout master \
    && dotnet restore \
    && dotnet build --configuration ${TARGET} --output /usr/local/src/dist --runtime linux-x64

# Create VERSION file with built project's version info
RUN dotnet tool install --tool-path /usr/local/src/project-apollo nbgv \
    && cd /usr/local/src/project-apollo/ProjectApollo \
    && ../nbgv get-version | sed -e 's/:/=/' > /usr/local/src/dist/VERSION

# ==================================================
FROM mcr.microsoft.com/dotnet/core/runtime:3.1-bionic as project-apollo-run

# Optout of the .NET Core Telemetry (https://aka.ms/dotnet-cli-telemetry)
ENV DOTNET_CLI_TELEMETRY_OPTOUT=true

# An account is created in the docker file for building and running the app.
ENV USER=mvsvr

# Directory configuration and runtime data is stored
ENV VIRDIR=/var/vircadia
ENV VIRDISTDIR=${VIRDIR}/dist
ENV VIRCONTENTDIR=${VIRDIR}/content

# Add a user for the server to run under
RUN adduser --disabled-password --gecos 'MetaverseServer user' ${USER}

# Create the content directory and make it owned by the user
RUN mkdir -p ${VIRDIR} \
    && chown -R ${USER}:${USER} ${VIRDIR} \
    && chmod 775 ${VIRDIR}

# Switch to user who will be running the server
WORKDIR /home/${USER}
USER ${USER}:${USER}

RUN mkdir -p ${VIRDISTDIR} \
    && mkdir -p ${VIRCONTENTDIR}

# Bring over all the built code from the build version
COPY --from=project-apollo-build --chown=${USER}:${USER} /usr/local/src/dist ${VIRDISTDIR}

COPY ./files/run-MetaverseServer.sh /home/${USER}
COPY ./files/GetVersion.sh /home/${USER}

# Default configuration file which can be overwritten by mounting a volume on this directory
COPY ./files/config.json ${VIRCONTENTDIR}

# Mount over this to save entries in persistant storage
VOLUME ${VIRCONTENTDIR}

EXPOSE 9400

ENTRYPOINT [ "./run-MetaverseServer.sh" ]

