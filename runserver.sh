#!/usr/bin/env bash
rm -rf bin/Content.Server
dotnet run --project Content.Server
read -p "Press enter to continue"
