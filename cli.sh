#!/usr/bin/env zsh

if [[ "$1" = "projects" ]]
then
  code /usr/local/share/itrg-cli/config/projects.json
  return 0
fi

if [[ "$1" = "servers" ]]
then
  code /usr/local/share/itrg-cli/config/projects.json
  return 0
fi

dotnet /usr/local/share/itrg-cli/CLI.dll
