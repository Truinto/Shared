#!/bin/bash

set -Eu
shopt -s lastpipe globstar nullglob dotglob
IFS=$'\n\t'

dotnet build --nologo -p:VersioningTask_AutoIncrease=false -c Release UnityMod.csproj
dotnet build --nologo -p:VersioningTask_AutoIncrease=false -c Release UnityMod-net2.1.csproj
dotnet build --nologo -p:VersioningTask_AutoIncrease=false -c Release UnityMod-net472.csproj

# pause, if not in a shell
(( SHLVL > 1 )) || read -rsn1
