#!/usr/bin/env zsh

REPO_NAME=ITRG-CLI
REPO_URL=https://github.com/ErikDombi/ITRG-CLI

if command -v cli &> /dev/null
then
  echo "ERR: itrg-cli is already installed!"
  return 1
fi

echo "Starting install of ITRG CLI..."
echo "Cloning repo..."
cd ~
git clone $REPO_URL
cd $REPO_NAME

if ! command -v dotnet &> /dev/null
then
  echo "Installing .NET"
  brew install --cask dotnet
  echo "Dotnet installed!"
else
  echo ".NET already installed... skipping"
fi

echo "Starting build of ITRG CLI"
cd CLI
dotnet publish -c Release
echo "Build completed"
cd ..

echo "Setting up binary directory..."
mkdir /usr/local/share/itrg-cli/
cp ./CLI/bin/Release/net5.0/publish/*.* /usr/local/share/itrg-cli/
mv ./CLI/bin/Release/net5.0/publish/runtimes /usr/local/share/itrg-cli/runtimes/
mkdir /usr/local/share/itrg-cli/bin
cp ./cli.sh /usr/local/share/itrg-cli/bin/cli
chmod +x /usr/local/share/itrg-cli/bin/cli

echo "Cleaning up..."
cd ~
rm -rf $REPO_NAME

echo "Adding to path..."
echo -e "\n# Add ITRG CLI to path\nexport PATH=/usr/local/share/itrg-cli/bin/:\$PATH" >> ~/.zshrc

echo -e "\nITRG CLI installed!"
echo "Restart terminal for updated PATH"
