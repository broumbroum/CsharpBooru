#!/bin/sh
cd ..

# Build CsharpBooru
rm -rf CsharpBooru/bin/Release/net9.0/linux-x64/publish
dotnet publish -c Release -r linux-x64 --self-contained true
rm -rf /AppImage/usr/lib/csharpbooru
cp -r CsharpBooru/bin/Release/net9.0/linux-x64/publish/ AppImage/usr/lib/csharpbooru

# Copy VLC
mkdir -p AppImage/usr/lib/vlc
cp -d /usr/lib/x86_64-linux-gnu/libvlc.so* AppImage/usr/lib/
cp -d /usr/lib/x86_64-linux-gnu/libvlccore.so* AppImage/usr/lib/
mkdir -p AppImage/usr/lib/vlc/plugins
cp -r /usr/lib/x86_64-linux-gnu/vlc/plugins AppImage/usr/lib/vlc/plugins


cd AppImage/usr/lib
ln -s libvlc.so.5 libvlc.so
ln -s libvlccore.so.9 libvlccore.so
cd ../../..

# Icons
cp CsharpBooru/Resources/Logo/'Logo 256x256.png' AppImage/csharpbooru.png
mkdir -p AppImage/usr/share/icons/hicolor/48x48/apps/
cp CsharpBooru/Resources/Logo/'Logo 48x48.png' AppImage/usr/share/icons/hicolor/48x48/apps/csharpbooru.png
mkdir -p AppImage/usr/share/icons/hicolor/128x128/apps/
cp CsharpBooru/Resources/Logo/'Logo 128x128.png' AppImage/usr/share/icons/hicolor/128x128/apps/csharpbooru.png
mkdir -p AppImage/usr/share/icons/hicolor/256x256/apps/
cp CsharpBooru/Resources/Logo/'Logo 256x256.png' AppImage/usr/share/icons/hicolor/256x256/apps/csharpbooru.png
mkdir -p AppImage/usr/share/icons/hicolor/512x512/apps/
cp CsharpBooru/Resources/Logo/'Logo 512x512.png' AppImage/usr/share/icons/hicolor/512x512/apps/csharpbooru.png

# Desktop
mkdir -p AppImage/usr/share/applications/
cp csharpbooru.desktop AppImage/usr/share/applications/csharpbooru.desktop
cp csharpbooru.desktop AppImage/csharpbooru.desktop

rm CsharpBooru.AppImage
./appimagetool-x86_64.AppImage AppImage CsharpBooru.AppImage

