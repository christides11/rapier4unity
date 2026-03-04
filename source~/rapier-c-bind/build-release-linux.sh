LIB="rapier_c_bind"
UNITY_LIB="unitybridge"

# Build Linux
TARGET_X86="x86_64-unknown-linux-gnu"
cargo build -r --target=$TARGET_X86
mkdir ../../build_bin/Linux
cp ${LIB}.bundle ../../build_bin/Linux/
cp ${UNITY_LIB}.bundle ../../build_bin/Linux/
cp target/${TARGET_X86}/release/lib${LIB}.so ../../build_bin/Linux/
cp target/${TARGET_X86}/release/lib${UNITY_LIB}.so ../../build_bin/Linux/

# Generate C# bindings
cargo run -- ./rapierbind/src