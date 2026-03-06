LIB="rapier_c_bind"
UNITY_LIB="unitybridge"

# Build Android
TARGET_ANDROID="aarch64-linux-android"
ANDROID_NDK="C:/Program Files/Unity/Hub/Editor/6000.3.8f1/Editor/Data/PlaybackEngines/AndroidPlayer/NDK"
if [ -z "$ANDROID_NDK" ]; then
    echo "** Android build error: $ANDROID_NDK is not defined."
    exit 1
fi
export CARGO_TARGET_AARCH64_LINUX_ANDROID_LINKER="${ANDROID_NDK}/toolchains/llvm/prebuilt/windows-x86_64/bin/aarch64-linux-android35-clang.cmd"
cargo build -r --target=$TARGET_ANDROID
mkdir ../../build_bin/Android
cp target/${TARGET_ANDROID}/release/lib${LIB}.so ../../build_bin/Android/
cp target/${TARGET_ANDROID}/release/lib${UNITY_LIB}.so ../../build_bin/Android/

# Generate C# bindings
cargo run -- ./rapierbind/src