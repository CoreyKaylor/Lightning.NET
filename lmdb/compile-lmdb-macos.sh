#!/opt/homebrew/bin/bash

if [ ! -d "lmdb" ]; then
  git clone https://git.openldap.org/openldap/openldap.git lmdb
fi
cd ./lmdb/libraries/liblmdb || exit
# Pinned LMDB 1.0.1: includes Windows build fixes (ITS#10539, ITS#10553) and
# post-1.0.0 work (encryption/checksum/backup, ITS#10515/10518-10523/10538/10542,
# ITS#10529/10551). ITS#10553 fixed the mingw SIZE_T MAP() issue that previously
# needed mingw-map-len-type.patch.
git fetch origin tag LMDB_1.0.1 --no-tags
git checkout -f LMDB_1.0.1

declare -A build_outputs
declare -A supported_targets=(
  [ios-arm64/native/lmdb.dylib]="make CC='xcrun --sdk iphoneos clang -arch arm64 -miphoneos-version-min=12.0' LDFLAGS='-s' XCFLAGS='-DNDEBUG' VERSION_OPT='-Wl,-current_version,1.0'"
  [iossimulator-arm64/native/lmdb.dylib]="make CC='xcrun --sdk iphonesimulator clang -arch arm64 -mios-simulator-version-min=12.0' LDFLAGS='-s' XCFLAGS='-DNDEBUG' VERSION_OPT='-Wl,-current_version,1.0'"
  [iossimulator-x64/native/lmdb.dylib]="make CC='xcrun --sdk iphonesimulator clang -arch x86_64 -mios-simulator-version-min=12.0' LDFLAGS='-s' XCFLAGS='-DNDEBUG' VERSION_OPT='-Wl,-current_version,1.0'"
  [osx-arm64/native/lmdb.dylib]="make LDFLAGS='-s' XCFLAGS='-DNDEBUG' VERSION_OPT='-Wl,-current_version,1.0'"
  [osx/native/lmdb.dylib]="make CC='clang -mmacosx-version-min=10.15 -arch x86_64' LDFLAGS='-s' XCFLAGS='-DNDEBUG' VERSION_OPT='-Wl,-current_version,1.0'"
  [linux-arm/native/liblmdb.so]="docker run --mount type=bind,source=$(pwd),target=/lmdb --rm --platform=linux/arm/7 -w /lmdb gcc:latest make LDFLAGS='-s' XCFLAGS='-DNDEBUG'"
  [linux-arm64/native/liblmdb.so]="docker run --mount type=bind,source=$(pwd),target=/lmdb --rm --platform=linux/arm64 -w /lmdb gcc:latest make LDFLAGS='-s' XCFLAGS='-DNDEBUG'"
  [linux-x64/native/liblmdb.so]="docker run --mount type=bind,source=$(pwd),target=/lmdb --rm --platform=linux/amd64 -w /lmdb gcc:latest make LDFLAGS='-s' XCFLAGS='-DNDEBUG'"
  [win-x64/native/lmdb.dll]="make CC='x86_64-w64-mingw32-gcc' AR='x86_64-w64-mingw32-gcc-ar' LDFLAGS='-s' XCFLAGS='-DNDEBUG' LDL= VERSION_OPT="
  [win-x86/native/lmdb.dll]="make CC='i686-w64-mingw32-gcc' AR='i686-w64-mingw32-gcc-ar' LDFLAGS='-s' XCFLAGS='-DNDEBUG' LDL= VERSION_OPT="
  [win-arm64/native/lmdb.dll]="docker run --mount type=bind,source='$(pwd)',target=/lmdb --rm -w /lmdb dockcross/windows-arm64 bash -c 'make CC=aarch64-w64-mingw32-gcc AR=aarch64-w64-mingw32-ar LDFLAGS=-s XCFLAGS=-DNDEBUG LDL= VERSION_OPT='"
  [android-arm64/native/liblmdb.so]="make CC=$NDK/toolchains/llvm/prebuilt/darwin-x86_64/bin/aarch64-linux-android21-clang AR=$NDK/toolchains/llvm/prebuilt/darwin-x86_64/bin/llvm-ar LDFLAGS='-s' XCFLAGS='-UMDB_USE_ROBUST -DMDB_USE_POSIX_MUTEX -DANDROID -DNDEBUG'"
  [android-arm/native/liblmdb.so]="make CC=$NDK/toolchains/llvm/prebuilt/darwin-x86_64/bin/armv7a-linux-androideabi21-clang AR=$NDK/toolchains/llvm/prebuilt/darwin-x86_64/bin/llvm-ar LDFLAGS='-s' XCFLAGS='-UMDB_USE_ROBUST -DMDB_USE_POSIX_MUTEX -DANDROID -DNDEBUG'"
  [android-x86/native/liblmdb.so]="make CC=$NDK/toolchains/llvm/prebuilt/darwin-x86_64/bin/i686-linux-android21-clang AR=$NDK/toolchains/llvm/prebuilt/darwin-x86_64/bin/llvm-ar LDFLAGS='-s' XCFLAGS='-UMDB_USE_ROBUST -DMDB_USE_POSIX_MUTEX -DANDROID -DNDEBUG'"
  [android-x64/native/liblmdb.so]="make CC=$NDK/toolchains/llvm/prebuilt/darwin-x86_64/bin/x86_64-linux-android21-clang AR=$NDK/toolchains/llvm/prebuilt/darwin-x86_64/bin/llvm-ar LDFLAGS='-s' XCFLAGS='-UMDB_USE_ROBUST -DMDB_USE_POSIX_MUTEX -DANDROID -DNDEBUG'"
  [browser-wasm/native/liblmdb.wasm]="emcc -O2 -pthread -fPIC -DNDEBUG -sSIDE_MODULE=1 -o liblmdb.so mdb.c midl.c module.c"
)

function compile_lib() {
  echo "Build starting for $2"
  make clean
  if ! eval "$1"
  then
    echo "Build failed for $2"
    exit 1
  fi
  echo "Build succeeded for $2"
  output_hash=$(md5 ./liblmdb.so)
  echo "$2 $output_hash"
  build_outputs["$output_hash"]="$2"
  cp ./liblmdb.so ../../../../src/LightningDB/runtimes/"$2"
  sleep 10
  #seems to be a stateful race condition on the docker run processes so this allows everything to succeed
}

RUNTIMES_DIR=../../../../src/LightningDB/runtimes
IOS_DIR=../../../../src/LightningDB/ios

function write_framework_plist() {
  # $1 = framework dir, $2 = supported platform (iPhoneOS | iPhoneSimulator)
  cat > "$1/Info.plist" <<PLIST
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
	<key>CFBundleDevelopmentRegion</key>
	<string>en</string>
	<key>CFBundleExecutable</key>
	<string>lmdb</string>
	<key>CFBundleIdentifier</key>
	<string>io.github.coreykaylor.lmdb</string>
	<key>CFBundleInfoDictionaryVersion</key>
	<string>6.0</string>
	<key>CFBundleName</key>
	<string>lmdb</string>
	<key>CFBundlePackageType</key>
	<string>FMWK</string>
	<key>CFBundleShortVersionString</key>
	<string>1.0.0</string>
	<key>CFBundleVersion</key>
	<string>1.0.0</string>
	<key>CFBundleSupportedPlatforms</key>
	<array>
		<string>$2</string>
	</array>
	<key>MinimumOSVersion</key>
	<string>12.0</string>
</dict>
</plist>
PLIST
}

# iOS requires @rpath install names and code signatures; bare dylibs with the
# Makefile's default liblmdb.so install name load in dev but fail release builds.
function package_apple_artifacts() {
  for rid in ios-arm64 iossimulator-arm64 iossimulator-x64; do
    install_name_tool -id @rpath/lmdb.dylib "$RUNTIMES_DIR/$rid/native/lmdb.dylib"
    codesign --force --sign - "$RUNTIMES_DIR/$rid/native/lmdb.dylib"
  done

  local scratch
  scratch=$(mktemp -d)
  mkdir -p "$scratch/device/lmdb.framework" "$scratch/simulator/lmdb.framework"
  cp "$RUNTIMES_DIR/ios-arm64/native/lmdb.dylib" "$scratch/device/lmdb.framework/lmdb"
  lipo -create \
    "$RUNTIMES_DIR/iossimulator-arm64/native/lmdb.dylib" \
    "$RUNTIMES_DIR/iossimulator-x64/native/lmdb.dylib" \
    -output "$scratch/simulator/lmdb.framework/lmdb"
  write_framework_plist "$scratch/device/lmdb.framework" iPhoneOS
  write_framework_plist "$scratch/simulator/lmdb.framework" iPhoneSimulator
  for fw in "$scratch/device/lmdb.framework" "$scratch/simulator/lmdb.framework"; do
    install_name_tool -id @rpath/lmdb.framework/lmdb "$fw/lmdb"
    codesign --force --sign - "$fw"
  done

  rm -rf "$IOS_DIR/lmdb.xcframework"
  mkdir -p "$IOS_DIR"
  xcodebuild -create-xcframework \
    -framework "$scratch/device/lmdb.framework" \
    -framework "$scratch/simulator/lmdb.framework" \
    -output "$IOS_DIR/lmdb.xcframework"
  rm -rf "$scratch"
}

function verify_apple_artifacts() {
  local failed=0
  local binaries=(
    "$RUNTIMES_DIR/ios-arm64/native/lmdb.dylib"
    "$RUNTIMES_DIR/iossimulator-arm64/native/lmdb.dylib"
    "$RUNTIMES_DIR/iossimulator-x64/native/lmdb.dylib"
    "$IOS_DIR"/lmdb.xcframework/*/lmdb.framework/lmdb
  )
  for bin in "${binaries[@]}"; do
    if ! otool -D "$bin" | grep -q "@rpath/lmdb"; then
      echo "FAIL: $bin install name is not @rpath-based: $(otool -D "$bin" | tail -1)"
      failed=1
    fi
    # arm64 simulator slices are floored at 14.0 by the toolchain (no arm64
    # simulators existed before iOS 14), so accept either minimum.
    if ! otool -l "$bin" | grep -A4 LC_BUILD_VERSION | grep -Eq "minos (12|14)\.0"; then
      echo "FAIL: $bin minos is not 12.0/14.0"
      failed=1
    fi
    if ! codesign -dv "$bin" 2>/dev/null; then
      echo "FAIL: $bin is not code signed"
      failed=1
    fi
  done
  if ! lipo -info "$IOS_DIR"/lmdb.xcframework/ios-*-simulator/lmdb.framework/lmdb | grep -q "x86_64 arm64"; then
    echo "FAIL: simulator framework slice is not a fat x86_64+arm64 binary"
    failed=1
  fi
  if [ $failed -eq 0 ]; then
    echo "All iOS artifacts verified (install names, minos, signatures, simulator archs)"
  else
    exit 1
  fi
}

for key in "${!supported_targets[@]}"; do
  compile_lib "${supported_targets[$key]}" $key
done

if [ ${#supported_targets[@]} -eq ${#build_outputs[@]} ]; then
    echo "All builds for lmdb supported targets have succeeded"
else
    echo "Not all supported targets have produced unique output"
fi

package_apple_artifacts
verify_apple_artifacts
