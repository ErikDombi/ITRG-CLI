#!/usr/bin/env zsh

if [[ "$1" = "projects" ]]
  code /usr/local/share/itrg-cli/config/projects.json
  return 0
fi

if [[ "$1" = "servers" ]]
  code /usr/local/share/itrg-cli/config/projects.json
  return 0
fi

dotnet /usr/local/share/itrg-cli/CLI.dll
