# Project Apollo

## Disclaimer
This project is not yet complete and has many API calls missing. 

## Goal
To create a opensource metaverse server for Project-Athena

## Build Instructions

```
dotnet build
```

## Pre-Requisites

```
dotnet-core 3.1
Newtonsoft.Json
```


## Troubleshooting

### Windows
Make sure you have dotnet core 3.1 SDK installed. Building should be possible from the command line, just by executing the bat files. Builds are located in bin/

### Mac & Linux
Install dotnet core 3.1 from https://dotnet.microsoft.com/download/dotnet-core/3.1

## Running the server
When you run the server you will need to ensure that the following is set in the Project-Athena source code, or that you have the `HIFI_METAVERSE_URL` environment variable set.

NetworkingConstants.h
```c++

    const QUrl METAVERSE_SERVER_URL_STABLE { "http://127.0.0.1:9400" };
    const QUrl METAVERSE_SERVER_URL_STAGING { "http://127.0.0.1:9401" };

```