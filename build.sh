#!/bin/bash

dotnet tool restore
dotnet paket restore
dotnet build src/AardvarkSandbox/AardvarkSandbox.fsproj