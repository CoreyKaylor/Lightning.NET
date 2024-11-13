#!/bin/bash

if [ ! -d "lmdb" ]; then
  git clone https://git.openldap.org/openldap/openldap.git lmdb
fi
cd ./lmdb/libraries/liblmdb || exit
git checkout LMDB_0.9.33
make clean
# Start with the tagged version release.
make CC='xcrun --sdk iphoneos --toolchain iphoneos clang -arch armv7s' XCFLAGS='-DNDEBUG'
mv ./liblmdb.so ../../../../src/LightningDB/runtimes/ios-arm/native/liblmdb.dylib
make clean
make CC='xcrun --sdk iphoneos --toolchain iphoneos clang -arch arm64' XCFLAGS='-DNDEBUG'
mv ./liblmdb.so ../../../../src/LightningDB/runtimes/ios-arm64/native/liblmdb.dylib
make clean
make CC='xcrun --sdk iphonesimulator --toolchain iphoneos clang -arch arm64' XCFLAGS='-DNDEBUG'
mv ./liblmdb.so ../../../../src/LightningDB/runtimes/iossimulator-arm64/native/liblmdb.dylib
make clean
make CC='xcrun --sdk iphonesimulator --toolchain iphoneos clang -arch x86_64' XCFLAGS='-DNDEBUG'
mv ./liblmdb.so ../../../../src/LightningDB/runtimes/iossimulator-x64/native/liblmdb.dylib
make clean
make XCFLAGS='-DNDEBUG'
mv ./liblmdb.so ../../../../src/LightningDB/runtimes/osx-arm64/native/lmdb.dylib
make clean
make CC='clang -mmacosx-version-min=10.15 -arch x86_64' XCFLAGS='-DNDEBUG'
mv ./liblmdb.so ../../../../src/LightningDB/runtimes/osx/native/lmdb.dylib
make clean
podman run --mount type=bind,source="$(pwd)",target=/lmdb --rm --platform=linux/arm/7 -w /lmdb gcc:latest make XCFLAGS='-DNDEBUG'
mv ./liblmdb.so ../../../../src/LightningDB/runtimes/linux-arm/native/liblmdb.so
make clean
podman run --mount type=bind,source="$(pwd)",target=/lmdb --rm --platform=linux/arm64 -w /lmdb gcc:latest make XCFLAGS='-DNDEBUG'
mv ./liblmdb.so ../../../../src/LightningDB/runtimes/linux-arm64/native/liblmdb.so
make clean
podman run --mount type=bind,source="$(pwd)",target=/lmdb --rm --platform=linux/amd64 -w /lmdb gcc:latest make XCFLAGS='-DNDEBUG'
mv ./liblmdb.so ../../../../src/LightningDB/runtimes/linux-x64/native/liblmdb.so
make clean
make CC='x86_64-w64-mingw32-gcc' AR='x86_64-w64-mingw32-gcc-ar' XCFLAGS='-DNDEBUG'
mv ./liblmdb.so ../../../../src/LightningDB/runtimes/win-x64/native/lmdb.dll
make clean
make CC='i686-w64-mingw32-gcc' AR='i686-w64-mingw32-gcc-ar' XCFLAGS='-DNDEBUG'
mv ./liblmdb.so ../../../../src/LightningDB/runtimes/win-x86/native/lmdb.dll
make clean
#Android NDK
make CC=$NDK/toolchains/llvm/prebuilt/darwin-x86_64/bin/aarch64-linux-android21-clang AR=$NDK/toolchains/llvm/prebuilt/darwin-x86_64/bin/llvm-ar XCFLAGS='-UMDB_USE_ROBUST -DMDB_USE_POSIX_MUTEX -DANDROID -DNDEBUG'
mv ./liblmdb.so ../../../../src/LightningDB/runtimes/android-arm64/native/liblmdb.so
make clean
make CC=$NDK/toolchains/llvm/prebuilt/darwin-x86_64/bin/armv7a-linux-androideabi21-clang AR=$NDK/toolchains/llvm/prebuilt/darwin-x86_64/bin/llvm-ar XCFLAGS='-UMDB_USE_ROBUST -DMDB_USE_POSIX_MUTEX -DANDROID -DNDEBUG'
mv ./liblmdb.so ../../../../src/LightningDB/runtimes/android-arm/native/liblmdb.so
make clean
make CC=$NDK/toolchains/llvm/prebuilt/darwin-x86_64/bin/i686-linux-android21-clang AR=$NDK/toolchains/llvm/prebuilt/darwin-x86_64/bin/llvm-ar XCFLAGS='-UMDB_USE_ROBUST -DMDB_USE_POSIX_MUTEX -DANDROID -DNDEBUG'
mv ./liblmdb.so ../../../../src/LightningDB/runtimes/android-x86/native/liblmdb.so
make clean
make CC=$NDK/toolchains/llvm/prebuilt/darwin-x86_64/bin/x86_64-linux-android21-clang AR=$NDK/toolchains/llvm/prebuilt/darwin-x86_64/bin/llvm-ar XCFLAGS='-UMDB_USE_ROBUST -DMDB_USE_POSIX_MUTEX -DANDROID -DNDEBUG'
mv ./liblmdb.so ../../../../src/LightningDB/runtimes/android-x64/native/liblmdb.so
make clean
# Checkout release sha with FIXEDSIZE preprocessor directive to support auto-growing map size on Windows
git checkout 48a7fed59a8aae623deff415dda27097198ca0c1
make CC='x86_64-w64-mingw32-gcc' AR='x86_64-w64-mingw32-gcc-ar' XCFLAGS='-UMDB_FIXEDSIZE -DNDEBUG'
mv ./liblmdb.so ../../../../src/LightningDB/runtimes/win-x64/native/lmdbautoresize.dll
make clean
make CC='i686-w64-mingw32-gcc' AR='i686-w64-mingw32-gcc-ar' XCFLAGS='-UMDB_FIXEDSIZE -DNDEBUG'
mv ./liblmdb.so ../../../../src/LightningDB/runtimes/win-x86/native/lmdbautoresize.dll
make clean
