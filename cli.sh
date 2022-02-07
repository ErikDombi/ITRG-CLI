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

if [[ "$1" = "config" ]]
then
  code /usr/local/share/itrg-cli/config/config.json
  return 0
fi

if [[ "$1" = "uninstall" ]]
then
  awk '!/# Add ITRG CLI to path/' ~/.zshrc > ~/.zshrc.tmp && mv ~/.zshrc.tmp ~/.zshrc
  awk '!/export PATH=\/usr\/local\/share\/itrg-cli\/bin\/\:\$PATH/' ~/.zshrc > ~/.zshrc.tmp && mv ~/.zshrc.tmp ~/.zshrc
  rm -rf /usr/local/share/itrg-cli/
  return 0
fi

if [[ "$1" = "update" ]]
then
  echo "Starting cleanup..."
  awk '!/# Add ITRG CLI to path/' ~/.zshrc > ~/.zshrc.tmp && mv ~/.zshrc.tmp ~/.zshrc
  awk '!/export PATH=\/usr\/local\/share\/itrg-cli\/bin\/\:\$PATH/' ~/.zshrc > ~/.zshrc.tmp && mv ~/.zshrc.tmp ~/.zshrc
  rm -f /usr/local/share/itrg-cli/*.*
  rm -rf /usr/local/share/itrg-cli/bin

  zsh <(curl https://raw.githubusercontent.com/ErikDombi/ITRG-CLI/main/install.sh)

  return 0
fi

dotnet /usr/local/share/itrg-cli/CLI.dll
