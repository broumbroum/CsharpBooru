#!/bin/sh
cd ..

# Build CsharpBooru
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

rm CsharpBooru.AppImage
./appimagetool-x86_64.AppImage AppImage CsharpBooru.AppImage

