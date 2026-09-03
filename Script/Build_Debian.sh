#!/bin/sh
cd ..
dotnet publish -c Release -r linux-x64 --self-contained false
cp -r  CsharpBooru/bin/Release/net9.0/linux-x64 Debian/opt/csharpbooru
dpkg-deb --build Debian
mv Debian.deb CsharpBooru.deb
