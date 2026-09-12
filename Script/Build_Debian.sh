#!/bin/sh
cd ..
rm -rf CsharpBooru/bin/Release/net9.0/linux-x64/publish
dotnet publish -c Release -r linux-x64 --self-contained false

rm -rf Debian/opt/csharpbooru
cp -r  CsharpBooru/bin/Release/net9.0/linux-x64/publish/ Debian/opt/csharpbooru

rm CsharpBooru.deb
dpkg-deb --build Debian
mv Debian.deb CsharpBooru.deb
