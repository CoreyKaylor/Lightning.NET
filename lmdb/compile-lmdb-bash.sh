#!/bin/bash
set -e  # Exit on error, but we'll handle errors in the compile function

#=============================================================================
# LMDB Cross-Platform Compilation Script for Lightning.NET
#=============================================================================
#
# DESCRIPTION:
# This script compiles LMDB (Lightning Memory-Mapped Database) native libraries 
# for multiple target platforms to support Lightning.NET, a .NET wrapper for LMDB.
# The script is designed to run on WSL (Windows Subsystem for Linux) with Ubuntu.
#
# FEATURES:
# - Cross-platform compilation for Windows, Linux, macOS, iOS, Android, and WebAssembly
# - Automatic tool detection and dependency checking
# - Colored logging with build status tracking
# - Hash-based duplicate build detection
# - Comprehensive error handling and reporting
# - Build time measurement and performance metrics
#
# SUPPORTED TARGETS:
# - Windows: x64, x86, ARM64 (using MinGW and Clang)
# - Linux: x64, ARM, ARM64 (using GCC cross-compilers)
# - macOS: x64, ARM64 (using Clang with minimum version targeting)
# - iOS: ARM, ARM64, Simulator x64/ARM64 (using Xcode toolchain)
# - Android: x64, x86, ARM, ARM64 (using Android NDK)
# - WebAssembly: WASM (using Emscripten)
#
# WSL SETUP PREREQUISITES:
# Before running this script on WSL (Ubuntu), ensure the following are installed:
#
# 1. Update system packages and install basic tools:
#    sudo apt update
#    sudo apt install build-essential make git dos2unix gcc-mingw-w64 g++-mingw-w64
#    sudo apt install mingw-w64
#    sudo apt install gcc-aarch64-linux-gnu g++-aarch64-linux-gnu binutils
#    sudo apt install coreutils
#
# 2. Install LLVM MinGW for ARM64 Windows targets:
#    cd ~
#    wget https://github.com/mstorsjo/llvm-mingw/releases/download/20250709/llvm-mingw-20250709-ucrt-ubuntu-22.04-x86_64.tar.xz
#    tar -xJf llvm-mingw-20250105-ucrt-ubuntu-20.04-x86_64.tar.xz
#    mv llvm-mingw-20250105-ucrt-ubuntu-20.04-x86_64 llvm-mingw
#    echo 'export PATH=$HOME/llvm-mingw/bin:$PATH' >> ~/.bashrc
#    source ~/.bashrc
#
# 3. Install Emscripten for WebAssembly targets (optional):
#    cd ~
#    git clone https://github.com/emscripten-core/emsdk.git
#    cd emsdk
#    ./emsdk install latest
#    ./emsdk activate latest
#    source ./emsdk_env.sh
#
# 4. For Android targets (optional):
#    # Download and extract Android NDK
#    # Set NDK environment variable: export NDK=/path/to/android-ndk
#
# 5. For macOS/iOS targets (only on macOS):
#    # Requires Xcode Command Line Tools (not available on WSL)
#
# COMPILATION FLAGS EXPLANATION:
# - CC: Compiler command (gcc, clang, or cross-compiler)
# - AR: Archive tool for creating static libraries
# - LDFLAGS: Linker flags
#   -s: Strip symbol table (reduces binary size)
#   -shared: Create shared library (.so files)
# - XCFLAGS: Extra C compiler flags
#   -O2: Optimization level 2 (recommended for production)
#        Enables most optimizations without significantly increasing compile time
#        Provides good balance between performance and binary size
#   -DNDEBUG: Disable debug assertions (production build)
#   -fPIC: Position Independent Code (required for shared libraries)
#   -DANDROID: Android-specific compilation flag
#   -UMDB_USE_ROBUST: Disable robust mutexes (Android compatibility)
#   -DMDB_USE_POSIX_MUTEX: Use POSIX mutexes (Android/Linux)
#
# OPTIMIZATION LEVELS:
# -O0: No optimization (debug builds)
# -O1: Basic optimizations
# -O2: Standard optimizations (RECOMMENDED for production)
#      - Enables: loop unrolling, instruction scheduling, register allocation
#      - Good performance with reasonable compile time
# -O3: Aggressive optimizations (may increase binary size)
# -Os: Optimize for size
# -Ofast: Aggressive optimizations that may break standards compliance
#
# OUTPUT STRUCTURE:
# The compiled libraries are placed in:
# src/LightningDB/runtimes/{platform}-{arch}/native/{library}
# 
# Examples:
# - src/LightningDB/runtimes/win-x64/native/lmdb.dll
# - src/LightningDB/runtimes/linux-x64/native/liblmdb.so
# - src/LightningDB/runtimes/osx-arm64/native/lmdb.dylib
#
# USAGE:
# ./compile-lmdb-bash.sh
#
# The script will automatically:
# 1. Clone LMDB source code (if not already present)
# 2. Check for required compilation tools
# 3. Build all supported targets
# 4. Copy binaries to appropriate runtime directories
# 5. Generate a comprehensive build report
#
# TROUBLESHOOTING:
# - If builds fail due to missing tools, install the required cross-compilers
# - For permission issues, ensure the script has execute permissions: chmod +x compile-lmdb-bash.sh
# - Check WSL integration if accessing Windows filesystem paths
# - Verify environment variables (NDK for Android, EMSDK for WebAssembly)
#
#=============================================================================

# Script configuration
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
LMDB_REPO="https://git.openldap.org/openldap/openldap.git"
LMDB_VERSION="LMDB_0.9.33"
OUTPUT_BASE="../../../../src/LightningDB/runtimes"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Logging functions
log_info() {
    echo -e "${BLUE}[INFO]${NC} $1"
}

log_success() {
    echo -e "${GREEN}[SUCCESS]${NC} $1"
}

log_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

log_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# Setup LMDB source
setup_lmdb() {
    log_info "Setting up LMDB source..."
    
    if [ ! -d "lmdb" ]; then
        log_info "Cloning LMDB repository..."
        git clone "$LMDB_REPO" lmdb
    fi
    
    cd ./lmdb/libraries/liblmdb || {
        log_error "Failed to enter LMDB directory"
        exit 1
    }
    
    log_info "Checking out LMDB version $LMDB_VERSION..."
    git checkout "$LMDB_VERSION"
}

# Build target definitions
declare -A supported_targets=(
    # iOS targets
    [ios-arm/native/liblmdb.dylib]="make CC='xcrun --sdk iphoneos --toolchain iphoneos clang -arch armv7s' LDFLAGS='-s' XCFLAGS='-DNDEBUG'"
    [ios-arm64/native/liblmdb.dylib]="make CC='xcrun --sdk iphoneos --toolchain iphoneos clang -arch arm64' LDFLAGS='-s' XCFLAGS='-DNDEBUG'"
    [iossimulator-arm64/native/liblmdb.dylib]="make CC='xcrun --sdk iphonesimulator --toolchain iphoneos clang -arch arm64' LDFLAGS='-s' XCFLAGS='-DNDEBUG'"
    [iossimulator-x64/native/liblmdb.dylib]="make CC='xcrun --sdk iphonesimulator --toolchain iphoneos clang -arch x86_64' LDFLAGS='-s' XCFLAGS='-DNDEBUG'"
    
    # macOS targets
    [osx-arm64/native/lmdb.dylib]="make LDFLAGS='-s' XCFLAGS='-DNDEBUG'"
    [osx/native/lmdb.dylib]="make CC='clang -mmacosx-version-min=10.15 -arch x86_64' LDFLAGS='-s' XCFLAGS='-DNDEBUG'"
    
    # Linux targets
    [linux-arm/native/liblmdb.so]="make CC='aarch64-linux-gnu-gcc' AR='aarch64-linux-gnu-ar' LDFLAGS='-s -shared' XCFLAGS='-DNDEBUG -fPIC'"
    [linux-arm64/native/liblmdb.so]="make CC='aarch64-linux-gnu-gcc' AR='aarch64-linux-gnu-ar' LDFLAGS='-s -shared' XCFLAGS='-DNDEBUG -fPIC'"
    [linux-x64/native/liblmdb.so]="make CC='gcc' AR='ar' LDFLAGS='-s -shared' XCFLAGS='-DNDEBUG -fPIC'"
    
    # Windows targets
    [win-x64/native/lmdb.dll]="make CC='x86_64-w64-mingw32-gcc' AR='x86_64-w64-mingw32-gcc-ar' LDFLAGS='-s' XCFLAGS='-DNDEBUG'"
    [win-x86/native/lmdb.dll]="make CC='i686-w64-mingw32-gcc' AR='i686-w64-mingw32-gcc-ar' LDFLAGS='-s' XCFLAGS='-DNDEBUG'"
    [win-arm64/native/lmdb.dll]="make CC='clang --target=aarch64-w64-mingw32' AR='llvm-ar' LDFLAGS='-s' XCFLAGS='-DNDEBUG'"
    
    # Android targets
    [android-arm64/native/liblmdb.so]="make CC=\$NDK/toolchains/llvm/prebuilt/\$(uname -m | sed 's/x86_64/darwin-x86_64/; s/aarch64/darwin-x86_64/')/bin/aarch64-linux-android21-clang AR=\$NDK/toolchains/llvm/prebuilt/\$(uname -m | sed 's/x86_64/darwin-x86_64/; s/aarch64/darwin-x86_64/')/bin/llvm-ar LDFLAGS='-s' XCFLAGS='-UMDB_USE_ROBUST -DMDB_USE_POSIX_MUTEX -DANDROID -DNDEBUG'"
    [android-arm/native/liblmdb.so]="make CC=\$NDK/toolchains/llvm/prebuilt/\$(uname -m | sed 's/x86_64/darwin-x86_64/; s/aarch64/darwin-x86_64/')/bin/armv7a-linux-androideabi21-clang AR=\$NDK/toolchains/llvm/prebuilt/\$(uname -m | sed 's/x86_64/darwin-x86_64/; s/aarch64/darwin-x86_64/')/bin/llvm-ar LDFLAGS='-s' XCFLAGS='-UMDB_USE_ROBUST -DMDB_USE_POSIX_MUTEX -DANDROID -DNDEBUG'"
    [android-x86/native/liblmdb.so]="make CC=\$NDK/toolchains/llvm/prebuilt/\$(uname -m | sed 's/x86_64/darwin-x86_64/; s/aarch64/darwin-x86_64/')/bin/i686-linux-android21-clang AR=\$NDK/toolchains/llvm/prebuilt/\$(uname -m | sed 's/x86_64/darwin-x86_64/; s/aarch64/darwin-x86_64/')/bin/llvm-ar LDFLAGS='-s' XCFLAGS='-UMDB_USE_ROBUST -DMDB_USE_POSIX_MUTEX -DANDROID -DNDEBUG'"
    [android-x64/native/liblmdb.so]="make CC=\$NDK/toolchains/llvm/prebuilt/\$(uname -m | sed 's/x86_64/darwin-x86_64/; s/aarch64/darwin-x86_64/')/bin/x86_64-linux-android21-clang AR=\$NDK/toolchains/llvm/prebuilt/\$(uname -m | sed 's/x86_64/darwin-x86_64/; s/aarch64/darwin-x86_64/')/bin/llvm-ar LDFLAGS='-s' XCFLAGS='-UMDB_USE_ROBUST -DMDB_USE_POSIX_MUTEX -DANDROID -DNDEBUG'"
    
    # WebAssembly target
    [browser-wasm/native/liblmdb.wasm]="emmake make CC=emcc AR=emar XCFLAGS='-O2 -DNDEBUG' && mv liblmdb.a liblmdb.wasm"
)

# Arrays to track build results
declare -A build_outputs
succeeded_targets=()
failed_targets=()
skipped_targets=()

# Check if required tools are available
check_tools() {
    local target="$1"
    local missing_tools=()
    
    case "$target" in
        win-*)
            if [[ "$target" == *"arm64"* ]]; then
                command -v clang >/dev/null 2>&1 || missing_tools+=("clang")
                command -v llvm-ar >/dev/null 2>&1 || missing_tools+=("llvm-ar")
            else
                command -v x86_64-w64-mingw32-gcc >/dev/null 2>&1 || missing_tools+=("mingw32-gcc")
            fi
            ;;
        ios-*|iossimulator-*)
            command -v xcrun >/dev/null 2>&1 || missing_tools+=("xcrun")
            ;;
        osx*)
            command -v clang >/dev/null 2>&1 || missing_tools+=("clang")
            ;;
        linux-arm*)
            command -v aarch64-linux-gnu-gcc >/dev/null 2>&1 || missing_tools+=("aarch64-linux-gnu-gcc")
            ;;
        android-*)
            [ -n "$NDK" ] || missing_tools+=("NDK environment variable")
            ;;
        browser-*)
            command -v emmake >/dev/null 2>&1 || missing_tools+=("emscripten/emmake")
            ;;
    esac
    
    if [ ${#missing_tools[@]} -gt 0 ]; then
        log_warning "Missing tools for $target: ${missing_tools[*]}"
        return 1
    fi
    return 0
}

# Get expected output filename based on target
get_output_filename() {
    local target="$1"
    case "$target" in
        win-*)         echo "lmdb.dll" ;;
        osx*|ios*)     echo "liblmdb.dylib" ;;
        browser-*)     echo "liblmdb.wasm" ;;
        *)             echo "liblmdb.so" ;;
    esac
}

# Calculate file hash (cross-platform)
calculate_hash() {
    local file="$1"
    if command -v sha256sum >/dev/null 2>&1; then
        sha256sum "$file" | cut -d' ' -f1
    elif command -v shasum >/dev/null 2>&1; then
        shasum -a 256 "$file" | cut -d' ' -f1
    elif command -v md5sum >/dev/null 2>&1; then
        md5sum "$file" | cut -d' ' -f1
    elif command -v md5 >/dev/null 2>&1; then
        md5 -q "$file"
    else
        log_warning "No hash command found, using timestamp"
        date +%s
    fi
}

# Compile library for a specific target
compile_lib() {
    local build_cmd="$1"
    local target="$2"
    local start_time=$(date +%s)
    
    log_info "Building $target..."
    
    # Check if required tools are available
    if ! check_tools "$target"; then
        log_warning "Skipping $target due to missing tools"
        skipped_targets+=("$target")
        return
    fi
    
    # Clean previous build artifacts
    make clean >/dev/null 2>&1 || true
    
    # Execute build command
    if ! eval "$build_cmd" 2>&1; then
        log_error "Build failed for $target"
        failed_targets+=("$target")
        return
    fi
    
    # Determine expected output file
    local output_file
    output_file=$(get_output_filename "$target")
    
    # For WebAssembly, try multiple possible output files
    if [[ "$target" == "browser-wasm"* ]]; then
        if [ -f "./liblmdb.wasm" ]; then
            output_file="liblmdb.wasm"
        elif [ -f "./liblmdb.a" ]; then
            log_info "Found liblmdb.a, converting to liblmdb.wasm for WebAssembly target"
            mv "./liblmdb.a" "./liblmdb.wasm"
            output_file="liblmdb.wasm"
        elif [ -f "./lmdb.wasm" ]; then
            output_file="lmdb.wasm"
        fi
    fi
    
    # Check if output file exists
    if [ ! -f "./$output_file" ]; then
        log_error "Expected output $output_file not found for $target!"
        log_info "Available files:"
        ls -la ./*.{dll,dylib,so,wasm,a,o} 2>/dev/null || echo "  No library files found"
        failed_targets+=("$target")
        return
    fi
    
    # Calculate file hash
    local output_hash
    output_hash=$(calculate_hash "./$output_file")
    
    # Check for duplicate builds
    if [ -n "${build_outputs[$output_hash]}" ]; then
        log_warning "Duplicate build detected for $target (same as ${build_outputs[$output_hash]})"
    fi
    
    build_outputs["$output_hash"]="$target"
    
    # Prepare target directory and copy file
    local target_path="$OUTPUT_BASE/$target"
    local target_dir
    target_dir=$(dirname "$target_path")
    
    mkdir -p "$target_dir"
    
    # Remove existing files in target directory to ensure clean state
    rm -f "$target_dir"/*
    
    # Copy output file
    if cp "./$output_file" "$target_path"; then
        local end_time=$(date +%s)
        local duration=$((end_time - start_time))
        log_success "Built $target successfully in ${duration}s (hash: ${output_hash:0:8}...)"
        succeeded_targets+=("$target")
    else
        log_error "Failed to copy output file for $target"
        failed_targets+=("$target")
        return
    fi
    
    # Brief pause to avoid potential race conditions
    sleep 1
}

# Print build summary
print_summary() {
    local total_targets=${#supported_targets[@]}
    local succeeded_count=${#succeeded_targets[@]}
    local failed_count=${#failed_targets[@]}
    local skipped_count=${#skipped_targets[@]}
    
    echo
    echo "========================================"
    echo "           BUILD SUMMARY"
    echo "========================================"
    echo "Total targets: $total_targets"
    echo "Succeeded: $succeeded_count"
    echo "Failed: $failed_count"
    echo "Skipped: $skipped_count"
    echo "========================================"
    
    if [ $succeeded_count -gt 0 ]; then
        echo
        log_success "Successful builds:"
        for target in "${succeeded_targets[@]}"; do
            echo "  ✓ $target"
        done
    fi
    
    if [ $failed_count -gt 0 ]; then
        echo
        log_error "Failed builds:"
        for target in "${failed_targets[@]}"; do
            echo "  ✗ $target"
        done
    fi
    
    if [ $skipped_count -gt 0 ]; then
        echo
        log_warning "Skipped builds:"
        for target in "${skipped_targets[@]}"; do
            echo "  ⚠ $target"
        done
    fi
    
    echo
    if [ $failed_count -eq 0 ] && [ $succeeded_count -gt 0 ]; then
        log_success "All attempted builds completed successfully!"
        exit 0
    elif [ $succeeded_count -eq 0 ]; then
        log_error "No builds succeeded!"
        exit 1
    else
        log_warning "Some builds failed or were skipped."
        exit 1
    fi
}

# Main execution
main() {
    local script_start_time=$(date +%s)
    
    log_info "Starting LMDB compilation script..."
    log_info "Target platforms: ${#supported_targets[@]}"
    
    # Setup LMDB source
    setup_lmdb
    
    # Build all targets
    for target in "${!supported_targets[@]}"; do
        if [ -n "${supported_targets[$target]}" ]; then
            compile_lib "${supported_targets[$target]}" "$target"
        else
            log_warning "No build command for $target, skipping."
            skipped_targets+=("$target (no build command)")
        fi
    done
    
    local script_end_time=$(date +%s)
    local total_duration=$((script_end_time - script_start_time))
    
    echo
    log_info "Total script execution time: ${total_duration}s"
    
    # Print final summary
    print_summary
}

# Run main function
main "$@"
