﻿#!/bin/sh
curl -sSL https://dot.net/v1/dotnet-install.sh > dotnet-install.sh
chmod +x dotnet-install.sh
./dotnet-install.sh -c 8.0 -InstallDir ./dotnet8
./dotnet8/dotnet --version
./dotnet8/dotnet restore
./dotnet8/dotnet build
./dotnet8/dotnet publish -c Release -o output MainSample.WebAssembly.csproj