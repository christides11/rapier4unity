# Build Linux dylibs
TARGET_ARM="aarch64-unknown-linux-gnu"
TARGET_X86="x86_64-unknown-linux-gnu"
cargo build -r --target=$TARGET_ARM
cargo build -r --target=$TARGET_X86
lipo -create -output ${LIB}.bundle \
  target/${TARGET_ARM}/release/lib${LIB}.dylib \
  target/${TARGET_X86}/release/lib${LIB}.dylib
lipo -create -output ${UNITY_LIB}.bundle \
  target/${TARGET_ARM}/release/lib${UNITY_LIB}.dylib \
  target/${TARGET_X86}/release/lib${UNITY_LIB}.dylib
mkdir ../../build_bin/linux
cp ${LIB}.bundle ../../build_bin/linux/
cp ${UNITY_LIB}.bundle ../../build_bin/linux/

# Generate C# bindings
cargo run -- ./rapierbind/src