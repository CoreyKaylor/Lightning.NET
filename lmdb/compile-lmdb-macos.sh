#!/opt/homebrew/bin/bash

if [ ! -d "lmdb" ]; then
  git clone https://git.openldap.org/openldap/openldap.git lmdb
fi
cd ./lmdb/libraries/liblmdb || exit
git checkout LMDB_0.9.33

declare -A build_outputs
declare -A supported_targets=(
  [ios-arm/native/liblmdb.dylib]="make CC='xcrun --sdk iphoneos --toolchain iphoneos clang -arch armv7s' LDFLAGS='-s' XCFLAGS='-DNDEBUG'"
  [ios-arm64/native/liblmdb.dylib]="make CC='xcrun --sdk iphoneos --toolchain iphoneos clang -arch arm64' LDFLAGS='-s' XCFLAGS='-DNDEBUG'"
  [iossimulator-arm64/native/liblmdb.dylib]="make CC='xcrun --sdk iphonesimulator --toolchain iphoneos clang -arch arm64' LDFLAGS='-s' XCFLAGS='-DNDEBUG'"
  [iossimulator-x64/native/liblmdb.dylib]="make CC='xcrun --sdk iphonesimulator --toolchain iphoneos clang -arch x86_64' LDFLAGS='-s' XCFLAGS='-DNDEBUG'"
  [osx-arm64/native/lmdb.dylib]="make LDFLAGS='-s' XCFLAGS='-DNDEBUG'"
  [osx/native/lmdb.dylib]="make CC='clang -mmacosx-version-min=10.15 -arch x86_64' LDFLAGS='-s' XCFLAGS='-DNDEBUG'"
  [linux-arm/native/liblmdb.so]="docker run --mount type=bind,source=$(pwd),target=/lmdb --rm --platform=linux/arm/7 -w /lmdb gcc:latest make LDFLAGS='-s' XCFLAGS='-DNDEBUG'"
  [linux-arm64/native/liblmdb.so]="docker run --mount type=bind,source=$(pwd),target=/lmdb --rm --platform=linux/arm64 -w /lmdb gcc:latest make LDFLAGS='-s' XCFLAGS='-DNDEBUG'"
  [linux-x64/native/liblmdb.so]="docker run --mount type=bind,source=$(pwd),target=/lmdb --rm --platform=linux/amd64 -w /lmdb gcc:latest make LDFLAGS='-s' XCFLAGS='-DNDEBUG'"
  [win-x64/native/lmdb.dll]="make CC='x86_64-w64-mingw32-gcc' AR='x86_64-w64-mingw32-gcc-ar' LDFLAGS='-s' XCFLAGS='-DNDEBUG'"
  [win-x86/native/lmdb.dll]="make CC='i686-w64-mingw32-gcc' AR='i686-w64-mingw32-gcc-ar' LDFLAGS='-s' XCFLAGS='-DNDEBUG'"
  [win-arm64/native/lmdb.dll]="make CC='aarch64-w64-mingw32-gcc' AR='aarch64-w64-mingw32-gcc-ar' LDFLAGS='-s' XCFLAGS='-DNDEBUG'"
  [android-arm64/native/liblmdb.so]="make CC=$NDK/toolchains/llvm/prebuilt/darwin-x86_64/bin/aarch64-linux-android21-clang AR=$NDK/toolchains/llvm/prebuilt/darwin-x86_64/bin/llvm-ar LDFLAGS='-s' XCFLAGS='-UMDB_USE_ROBUST -DMDB_USE_POSIX_MUTEX -DANDROID -DNDEBUG'"
  [android-arm/native/liblmdb.so]="make CC=$NDK/toolchains/llvm/prebuilt/darwin-x86_64/bin/armv7a-linux-androideabi21-clang AR=$NDK/toolchains/llvm/prebuilt/darwin-x86_64/bin/llvm-ar LDFLAGS='-s' XCFLAGS='-UMDB_USE_ROBUST -DMDB_USE_POSIX_MUTEX -DANDROID -DNDEBUG'"
  [android-x86/native/liblmdb.so]="make CC=$NDK/toolchains/llvm/prebuilt/darwin-x86_64/bin/i686-linux-android21-clang AR=$NDK/toolchains/llvm/prebuilt/darwin-x86_64/bin/llvm-ar LDFLAGS='-s' XCFLAGS='-UMDB_USE_ROBUST -DMDB_USE_POSIX_MUTEX -DANDROID -DNDEBUG'"
  [android-x64/native/liblmdb.so]="make CC=$NDK/toolchains/llvm/prebuilt/darwin-x86_64/bin/x86_64-linux-android21-clang AR=$NDK/toolchains/llvm/prebuilt/darwin-x86_64/bin/llvm-ar LDFLAGS='-s' XCFLAGS='-UMDB_USE_ROBUST -DMDB_USE_POSIX_MUTEX -DANDROID -DNDEBUG'"
  [browser-wasm/native/liblmdb.wasm]="emmake make LDFLAGS='-s' XCFLAGS='-DNDEBUG'"
)

# function compile_lib() {
  # echo "Build starting for $2"
  # make clean
  # if ! eval "$1"
  # then
    # echo "Build failed for $2"
    # exit 1
  # fi
  # echo "Build succeeded for $2"
  # output_hash=$(md5 ./liblmdb.so)
  # echo "$2 $output_hash"
  # build_outputs["$output_hash"]="$2"
  # cp ./liblmdb.so ../../../../src/LightningDB/runtimes/"$2"
  # sleep 10
  # #seems to be a stateful race condition on the docker run processes so this allows everything to succeed
# }
function compile_lib() {
  echo "Build starting for $2"
  make clean
  if ! eval "$1"; then
    echo "Build failed for $2"
    exit 1
  fi
  echo "Build succeeded for $2"

  # Detect correct artifact name
  output_file="liblmdb.so"
  case "$2" in
    win-*)         output_file="lmdb.dll" ;;
    osx*|ios*)     output_file="liblmdb.dylib" ;;
    browser-*)     output_file="liblmdb.wasm" ;;
  esac

  if [ ! -f "./$output_file" ]; then
    echo "Expected output $output_file not found!"
    exit 1
  fi

  # Calculate hash and copy to target
  output_hash=$(md5 -q "./$output_file")
  echo "$2 $output_hash"
  build_outputs["$output_hash"]="$2"

  cp "./$output_file" ../../../../src/LightningDB/runtimes/"$2"

  # Avoid race condition between docker runs
  sleep 10
}

for key in "${!supported_targets[@]}"; do
  compile_lib "${supported_targets[$key]}" $key
done

if [ ${#supported_targets[@]} -eq ${#build_outputs[@]} ]; then
    echo "All builds for lmdb supported targets have succeeded"
else
    echo "Not all supported targets have produced unique output"
fi
