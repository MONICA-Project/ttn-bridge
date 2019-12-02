#!/bin/bash

HOMEDIR=$HOME
ROOT="$HOMEDIR/deb"
OUTPUT="../bin/Release/netcoreapp3.0"

DEBNAME="ttnbridge"
CSPROJFILE="TTN-Bridge_Core.csproj"

EXEC="$ROOT/usr/local/bin/$DEBNAME"
CONFIG="$ROOT/etc/$DEBNAME"
SYSTEMD="$ROOT/lib/systemd/system"
LOGROTATE="$ROOT/etc/logrotate.d"

DEBIAN="$ROOT/DEBIAN"
VMAJOR=$(grep -e "<Version>" ../$CSPROJFILE | cut -d'>' -f 2 | cut -d'<' -f 1 | cut -d'.' -f 1)
VMINOR=$(grep -e "<Version>" ../$CSPROJFILE | cut -d'>' -f 2 | cut -d'<' -f 1 | cut -d'.' -f 2)
VBUILD=$(grep -e "<Version>" ../$CSPROJFILE | cut -d'>' -f 2 | cut -d'<' -f 1 | cut -d'.' -f 3)
ARCHT=$1

mkdir -p $EXEC
mkdir -p $CONFIG
mkdir -p $DEBIAN
mkdir -p $SYSTEMD
mkdir -p $LOGROTATE

cp control-core $DEBIAN/control
cp preinst $DEBIAN
cp postinst $DEBIAN
cp prerm $DEBIAN
sed -i s/Version:\ x\.x-x/"Version: $VMAJOR.$VMINOR-$VBUILD"/ $DEBIAN/control
sed -i s/Architecture:\ any/"Architecture: $ARCHT"/ $DEBIAN/control
chmod 755 $DEBIAN -R

cp "service-$DEBNAME-core" "$SYSTEMD/$DEBNAME.service"
chmod 644 $SYSTEMD/"$DEBNAME.service"

cp $OUTPUT/*.runtimeconfig.json $EXEC/
find $OUTPUT -name \*.dll -exec cp {} $EXEC/ \;
chmod 644 $EXEC/*
chmod 755 $EXEC

cp $OUTPUT/config-example/* $CONFIG
chmod 644 $CONFIG/*
chmod 755 $CONFIG

cp "logrotate-$DEBNAME" "$LOGROTATE/$DEBNAME.conf"
chmod 644 $LOGROTATE/*

dpkg-deb --build $ROOT
mv $HOMEDIR/deb.deb ../../../Builds/"$ARCHT-""$DEBNAME""_$VMAJOR.$VMINOR-$VBUILD.deb"
rm $HOMEDIR/deb -r