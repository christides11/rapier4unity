LIB="rapier_c_bind"
UNITY_LIB="unitybridge"

# Convert USERPROFILE backslashes to forward slashes
USER_HOME=$(echo "$USERPROFILE" | sed 's|\\|/|g')

export EM_CONFIG="$USER_HOME/.emscripten_unity_rust"
export EM_CACHE="$USER_HOME/.emscripten_unity_rust_cache"

# Generate the config if it doesn't exist
if [ ! -f "$EM_CONFIG" ]; then
cat > "$EM_CONFIG" << EOF
EMSCRIPTEN_ROOT = 'C:/emscripten/emscripten'
LLVM_ROOT = 'C:/emscripten/llvm'
BINARYEN_ROOT = 'C:/emscripten/binaryen'
NODE_JS = 'C:/emscripten/node/node.exe'
CACHE = '$USER_HOME/.emscripten_unity_rust_cache'
EOF
fi

#Generate the config if it doesn't exist
if [ ! -f "$EM_CONFIG" ]; then
  "$EMSCRIPTEN_ROOT/emcc.bat" --generate-config
fi

# Build WebGL
TARGET_WASM="wasm32-unknown-emscripten"
export CARGO_TARGET_WASM32_UNKNOWN_EMSCRIPTEN_RUSTFLAGS="-Ctarget-cpu=mvp"
#export CARGO_TARGET_WASM32_UNKNOWN_EMSCRIPTEN_LINKER="C:/Program Files/Unity/Hub/Editor/6000.3.8f1/Editor/Data/PlaybackEngines/WebGLSupport/BuildTools/Emscripten/emscripten/emcc.bat"
export CARGO_TARGET_WASM32_UNKNOWN_EMSCRIPTEN_LINKER="C:/emscripten/emscripten/emcc.bat"
cargo +nightly build -Zbuild-std=panic_abort,std -r --target=$TARGET_WASM
mkdir ../../build_bin/WebGL
cp target/${TARGET_WASM}/release/lib${LIB}.a ../../build_bin/WebGL/
cp target/${TARGET_WASM}/release/lib${UNITY_LIB}.a ../../build_bin/WebGL/

# Generate C# bindings
cargo run -- ./rapierbind/src