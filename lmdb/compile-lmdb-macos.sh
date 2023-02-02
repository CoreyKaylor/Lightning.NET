#!/bin/bash

if [ ! -d "lmdb" ]; then
  git clone https://github.com/LMDB/lmdb.git
fi
cd ./lmdb/libraries/liblmdb
git checkout LMDB_0.9.29
make clean
# Start with the tagged version release.
make
mv ./liblmdb.so ../../../../src/LightningDB/runtimes/osx-arm64/native/lmdb.dylib
make clean
docker run --mount type=bind,source=$(pwd),target=/lmdb --rm --platform=linux/arm/7 -w /lmdb gcc:latest make
mv ./liblmdb.so ../../../../src/LightningDB/runtimes/linux-arm/native/liblmdb.so
make clean
docker run --mount type=bind,source=$(pwd),target=/lmdb --rm --platform=linux/arm64 -w /lmdb gcc:latest make
mv ./liblmdb.so ../../../../src/LightningDB/runtimes/linux-arm64/native/liblmdb.so
make clean
docker run --mount type=bind,source=$(pwd),target=/lmdb --rm --platform=linux/amd64 -w /lmdb gcc:latest make
mv ./liblmdb.so ../../../../src/LightningDB/runtimes/linux-x64/native/liblmdb.so
make clean
make CC='x86_64-w64-mingw32-gcc' AR='x86_64-w64-mingw32-gcc-ar'
mv ./liblmdb.so ../../../../src/LightningDB/runtimes/win-x64/native/lmdb.dll
make clean
make CC='i686-w64-mingw32-gcc' AR='i686-w64-mingw32-gcc-ar'
mv ./liblmdb.so ../../../../src/LightningDB/runtimes/win-x86/native/lmdb.dll
make clean
# Checkout release sha with FIXEDSIZE preprocessor directive to support auto-growing map size on Windows
git checkout 48a7fed59a8aae623deff415dda27097198ca0c1
make CC='x86_64-w64-mingw32-gcc' AR='x86_64-w64-mingw32-gcc-ar' XCFLAGS='-UMDB_FIXEDSIZE'
mv ./liblmdb.so ../../../../src/LightningDB/runtimes/win-x64/native/lmdbautoresize.dll
make clean
make CC='i686-w64-mingw32-gcc' AR='i686-w64-mingw32-gcc-ar'  XCFLAGS='-UMDB_FIXEDSIZE'
mv ./liblmdb.so ../../../../src/LightningDB/runtimes/win-x86/native/lmdbautoresize.dll
make clean

