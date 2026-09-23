#!/bin/sh
cd ..
rm -rf CsharpBooru/bin/Release/net9.0/linux-x64/publish
dotnet publish -c Release -r linux-x64 --self-contained false

rm -rf Debian/opt/csharpbooru
cp -r  CsharpBooru/bin/Release/net9.0/linux-x64/publish/ Debian/opt/csharpbooru

# Icons
mkdir -p Debian/usr/share/icons/hicolor/48x48/apps/
cp CsharpBooru/Resources/Logo/'Logo 48x48.png' Debian/usr/share/icons/hicolor/48x48/apps/csharpbooru.png
mkdir -p Debian/usr/share/icons/hicolor/128x128/apps/
cp CsharpBooru/Resources/Logo/'Logo 128x128.png' Debian/usr/share/icons/hicolor/128x128/apps/csharpbooru.png
mkdir -p Debian/usr/share/icons/hicolor/256x256/apps/
cp CsharpBooru/Resources/Logo/'Logo 256x256.png' Debian/usr/share/icons/hicolor/256x256/apps/csharpbooru.png
mkdir -p Debian/usr/share/icons/hicolor/512x512/apps/
cp CsharpBooru/Resources/Logo/'Logo 512x512.png' Debian/usr/share/icons/hicolor/512x512/apps/csharpbooru.png

# Desktop
mkdir -p Debian/usr/share/applications/
cp csharpbooru.desktop Debian/usr/share/applications/csharpbooru.desktop

rm CsharpBooru.deb
dpkg-deb --build Debian
mv Debian.deb CsharpBooru.deb
