#!/usr/bin/env bash
rm -rf bin/Content.Server
dotnet run --project Content.Server --configuration Tools
read -p "Press enter to continue"
