@echo off
setlocal enabledelayedexpansion

set LIB=rapier_c_bind
set UNITY_LIB=unitybridge

:: Convert USERPROFILE backslashes to forward slashes for emscripten config
set "USER_HOME=%USERPROFILE:\=/%"

set "EM_CONFIG=%USER_HOME%/.emscripten_unity_rust"
set "EM_CACHE=%USER_HOME%/.emscripten_unity_rust_cache"

:: Generate the config if it doesn't exist
if not exist "%USERPROFILE%\.emscripten_unity_rust" (
    echo EMSCRIPTEN_ROOT = 'C:/emscripten/emscripten'  > "%USERPROFILE%\.emscripten_unity_rust"
    echo LLVM_ROOT = 'C:/emscripten/llvm'             >> "%USERPROFILE%\.emscripten_unity_rust"
    echo BINARYEN_ROOT = 'C:/emscripten/binaryen'     >> "%USERPROFILE%\.emscripten_unity_rust"
    echo NODE_JS = 'C:/emscripten/node/node.exe'      >> "%USERPROFILE%\.emscripten_unity_rust"
    echo CACHE = '%USER_HOME%/.emscripten_unity_rust_cache' >> "%USERPROFILE%\.emscripten_unity_rust"
)

:: Build WebGL
set TARGET_WASM=wasm32-unknown-emscripten
set CARGO_TARGET_WASM32_UNKNOWN_EMSCRIPTEN_RUSTFLAGS=-Ctarget-cpu=mvp
set CARGO_TARGET_WASM32_UNKNOWN_EMSCRIPTEN_LINKER=C:/emscripten/emscripten/emcc.bat

cargo +nightly build -Zbuild-std=panic_abort,std -r --target=%TARGET_WASM%
if errorlevel 1 (
    echo Build failed!
    exit /b 1
)

if not exist "..\..\build_bin\WebGL" mkdir "..\..\build_bin\WebGL"

copy /y "target\%TARGET_WASM%\release\lib%LIB%.a" "..\..\build_bin\WebGL\"
copy /y "target\%TARGET_WASM%\release\lib%UNITY_LIB%.a" "..\..\build_bin\WebGL\"

:: Generate C# bindings
cargo run -- ./rapierbind/src

endlocal