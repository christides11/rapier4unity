<#
Commands:
  .\build.ps1 -Platform Windows -Config Release
  .\build.ps1 -Platform Windows -Config Debug
  .\build.ps1 -Platform Android -Config Release -AndroidNdk "C:\path\to\NDK"
  .\build.ps1 -Platform WebGL -Config Release
  .\build.ps1 -Platform Linux -Config Release
  .\build.ps1 -Platform All -Config Release -SkipUnsupported

Options:
  -SkipBindings      Build native binaries without regenerating C# bindings.
  -SkipUnsupported   Skip platforms that cannot build on the current host.

Notes:
  Linux builds from Windows require a Linux linker. This script uses cargo-zigbuild
  automatically when it is installed:
    cargo install cargo-zigbuild
    winget install zig.zig
#>
[CmdletBinding()]
param(
    [ValidateSet('Windows', 'Android', 'Linux', 'WebGL', 'Apple', 'All')]
    [string[]]$Platform = @('Windows'),

    [ValidateSet('Debug', 'Release')]
    [string]$Config = 'Release',

    [string]$AndroidNdk,

    [string]$EmscriptenRoot = 'C:/emscripten/emscripten',
    [string]$LlvmRoot = 'C:/emscripten/llvm',
    [string]$BinaryenRoot = 'C:/emscripten/binaryen',
    [string]$NodeJs = 'C:/emscripten/node/node.exe',

    [switch]$SkipBindings,
    [switch]$SkipUnsupported
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$Lib = 'rapier_c_bind'
$UnityLib = 'unitybridge'
$RepoRoot = $PSScriptRoot
$BuildBin = Resolve-Path -LiteralPath (Join-Path $RepoRoot '..\..\build_bin') -ErrorAction SilentlyContinue
if (-not $BuildBin) {
    $BuildBinPath = Join-Path $RepoRoot '..\..\build_bin'
    New-Item -ItemType Directory -Force -Path $BuildBinPath | Out-Null
    $BuildBin = Resolve-Path -LiteralPath $BuildBinPath
}
$BuildBin = $BuildBin.Path

function Test-IsWindowsHost {
    return [System.Runtime.InteropServices.RuntimeInformation]::IsOSPlatform([System.Runtime.InteropServices.OSPlatform]::Windows)
}

function Test-IsMacOSHost {
    return [System.Runtime.InteropServices.RuntimeInformation]::IsOSPlatform([System.Runtime.InteropServices.OSPlatform]::OSX)
}

function Write-Step {
    param([string]$Message)
    Write-Host "==> $Message" -ForegroundColor Cyan
}

function Get-ProfileDir {
    if ($Config -eq 'Release') { return 'release' }
    return 'debug'
}

function Get-CargoProfileArg {
    if ($Config -eq 'Release') { return @('--release') }
    return @()
}

function Invoke-RequiredCommand {
    param(
        [string]$Command,
        [string[]]$Arguments
    )

    Write-Host "+ $Command $($Arguments -join ' ')" -ForegroundColor DarkGray
    & $Command @Arguments
    if ($LASTEXITCODE -ne 0) {
        throw "Command failed with exit code ${LASTEXITCODE}: $Command $($Arguments -join ' ')"
    }
}

function Ensure-Command {
    param([string]$Command, [string]$InstallHint)

    if (-not (Get-Command $Command -ErrorAction SilentlyContinue)) {
        throw "$Command was not found. $InstallHint"
    }
}

function Ensure-OutputDir {
    param([string]$Name)

    $dir = Join-Path $BuildBin $Name
    New-Item -ItemType Directory -Force -Path $dir | Out-Null
    return $dir
}

function Ensure-EmscriptenConfig {
    if (-not $env:USERPROFILE) {
        throw 'USERPROFILE is not set; pass Emscripten paths explicitly or run from Windows.'
    }

    $userHome = $env:USERPROFILE -replace '\\', '/'
    $env:EM_CONFIG = "$userHome/.emscripten_unity_rust"
    $env:EM_CACHE = "$userHome/.emscripten_unity_rust_cache"

    if (-not (Test-Path -LiteralPath $env:EM_CONFIG)) {
        $config = @"
EMSCRIPTEN_ROOT = '$EmscriptenRoot'
LLVM_ROOT = '$LlvmRoot'
BINARYEN_ROOT = '$BinaryenRoot'
NODE_JS = '$NodeJs'
CACHE = '$userHome/.emscripten_unity_rust_cache'
"@
        Set-Content -LiteralPath $env:EM_CONFIG -Value $config -Encoding ASCII
    }
}

function Copy-RequiredFile {
    param([string]$Source, [string]$DestinationDir)

    if (-not (Test-Path -LiteralPath $Source)) {
        throw "Expected build output was not found: $Source"
    }

    Copy-Item -LiteralPath $Source -Destination $DestinationDir -Force
}

function Get-DefaultAndroidNdk {
    $candidates = @(
        'C:/Program Files/Unity/Hub/Editor/6000.5.2f1/Editor/Data/PlaybackEngines/AndroidPlayer/NDK',
        '/Applications/Unity/Hub/Editor/6000.5.2f1/PlaybackEngines/AndroidPlayer/NDK'
    )

    foreach ($candidate in $candidates) {
        if (Test-Path -LiteralPath $candidate) { return $candidate }
    }

    return $null
}

function Get-AndroidLinker {
    param([string]$NdkPath)

    if (Test-IsWindowsHost) {
        return Join-Path $NdkPath 'toolchains/llvm/prebuilt/windows-x86_64/bin/aarch64-linux-android35-clang.cmd'
    }

    if (Test-IsMacOSHost) {
        return Join-Path $NdkPath 'toolchains/llvm/prebuilt/darwin-x86_64/bin/aarch64-linux-android35-clang'
    }

    return Join-Path $NdkPath 'toolchains/llvm/prebuilt/linux-x86_64/bin/aarch64-linux-android35-clang'
}

function Invoke-Bindings {
    if ($SkipBindings) {
        return
    }

    Write-Step 'Generating C# bindings'
    Invoke-RequiredCommand 'cargo' @('run', '-p', 'export-to-unity-dynamic-binder', '--', './rapierbind/src')
}

function Build-Windows {
    Write-Step "Building Windows $Config"
    $profile = Get-ProfileDir
    $outDir = Ensure-OutputDir 'Windows'

    Invoke-RequiredCommand 'cargo' (@('build') + (Get-CargoProfileArg))

    Copy-RequiredFile (Join-Path $RepoRoot "target/$profile/$Lib.dll") $outDir
    Copy-RequiredFile (Join-Path $RepoRoot "target/$profile/$Lib.pdb") $outDir
    Copy-RequiredFile (Join-Path $RepoRoot "target/$profile/$UnityLib.dll") $outDir
    Copy-RequiredFile (Join-Path $RepoRoot "target/$profile/$UnityLib.pdb") $outDir
}

function Build-Android {
    Write-Step "Building Android $Config"
    $target = 'aarch64-linux-android'
    $profile = Get-ProfileDir
    $outDir = Ensure-OutputDir 'Android'
    $ndk = if ($AndroidNdk) { $AndroidNdk } else { Get-DefaultAndroidNdk }

    if (-not $ndk) {
        throw 'Android NDK was not found. Pass -AndroidNdk "path/to/NDK".'
    }

    $linker = Get-AndroidLinker $ndk
    if (-not (Test-Path -LiteralPath $linker)) {
        throw "Android linker was not found: $linker"
    }

    $env:CARGO_TARGET_AARCH64_LINUX_ANDROID_LINKER = $linker
    Invoke-RequiredCommand 'cargo' (@('build') + (Get-CargoProfileArg) + @('--target', $target))

    Copy-RequiredFile (Join-Path $RepoRoot "target/$target/$profile/lib$Lib.so") $outDir
    Copy-RequiredFile (Join-Path $RepoRoot "target/$target/$profile/lib$UnityLib.so") $outDir
}

function Build-Linux {
    Write-Step "Building Linux $Config"
    $target = 'x86_64-unknown-linux-gnu'
    $profile = Get-ProfileDir
    $outDir = Ensure-OutputDir 'Linux'

    if (Test-IsWindowsHost) {
        if (-not (Get-Command 'cargo-zigbuild' -ErrorAction SilentlyContinue)) {
            throw 'Linux builds from Windows require a Linux linker. Install Zig and cargo-zigbuild, then rerun: cargo install cargo-zigbuild; winget install zig.zig'
        }

        if (-not (Get-Command 'zig' -ErrorAction SilentlyContinue)) {
            throw 'cargo-zigbuild is installed, but Zig was not found on PATH. Install it with: winget install zig.zig. Then open a new terminal and rerun the build.'
        }

        Invoke-RequiredCommand 'cargo' (@('zigbuild') + (Get-CargoProfileArg) + @('--target', $target))
    }
    else {
        Invoke-RequiredCommand 'cargo' (@('build') + (Get-CargoProfileArg) + @('--target', $target))
    }

    Copy-RequiredFile (Join-Path $RepoRoot "target/$target/$profile/lib$Lib.so") $outDir
    Copy-RequiredFile (Join-Path $RepoRoot "target/$target/$profile/lib$UnityLib.so") $outDir
}

function Build-WebGL {
    if ($Config -ne 'Release') {
        throw 'WebGL currently supports Release only because it uses cargo +nightly -Zbuild-std.'
    }

    Write-Step 'Building WebGL Release'
    $target = 'wasm32-unknown-emscripten'
    $profile = 'release'
    $outDir = Ensure-OutputDir 'WebGL'

    Ensure-EmscriptenConfig
    $linker = Join-Path $EmscriptenRoot 'emcc.bat'
    if (-not (Test-Path -LiteralPath $linker)) {
        throw "Emscripten linker was not found: $linker"
    }

    $env:CARGO_TARGET_WASM32_UNKNOWN_EMSCRIPTEN_RUSTFLAGS = '-Ctarget-cpu=mvp'
    $env:CARGO_TARGET_WASM32_UNKNOWN_EMSCRIPTEN_LINKER = $linker

    Invoke-RequiredCommand 'cargo' @('+nightly', 'build', '-Zbuild-std=panic_abort,std', '--release', '--target', $target)

    Copy-RequiredFile (Join-Path $RepoRoot "target/$target/$profile/lib$Lib.a") $outDir
    Copy-RequiredFile (Join-Path $RepoRoot "target/$target/$profile/lib$UnityLib.a") $outDir
}

function Build-Apple {
    if (-not (Test-IsMacOSHost)) {
        throw 'Apple builds require macOS with Apple toolchains and lipo.'
    }

    Write-Step "Building macOS and iOS $Config"
    Ensure-Command 'lipo' 'Install Xcode command line tools.'

    $profile = Get-ProfileDir
    $macOut = Ensure-OutputDir 'macOS'
    $iosOut = Ensure-OutputDir 'iOS'
    $macArm = 'aarch64-apple-darwin'
    $macX86 = 'x86_64-apple-darwin'
    $ios = 'aarch64-apple-ios'

    Invoke-RequiredCommand 'cargo' (@('build') + (Get-CargoProfileArg) + @('--target', $macArm))
    Invoke-RequiredCommand 'cargo' (@('build') + (Get-CargoProfileArg) + @('--target', $macX86))

    Invoke-RequiredCommand 'lipo' @(
        '-create',
        '-output', (Join-Path $RepoRoot "$Lib.bundle"),
        (Join-Path $RepoRoot "target/$macArm/$profile/lib$Lib.dylib"),
        (Join-Path $RepoRoot "target/$macX86/$profile/lib$Lib.dylib")
    )
    Invoke-RequiredCommand 'lipo' @(
        '-create',
        '-output', (Join-Path $RepoRoot "$UnityLib.bundle"),
        (Join-Path $RepoRoot "target/$macArm/$profile/lib$UnityLib.dylib"),
        (Join-Path $RepoRoot "target/$macX86/$profile/lib$UnityLib.dylib")
    )

    Copy-RequiredFile (Join-Path $RepoRoot "$Lib.bundle") $macOut
    Copy-RequiredFile (Join-Path $RepoRoot "$UnityLib.bundle") $macOut

    Invoke-RequiredCommand 'cargo' (@('build') + (Get-CargoProfileArg) + @('--target', $ios))
    Copy-RequiredFile (Join-Path $RepoRoot "target/$ios/$profile/lib$Lib.a") $iosOut
    Copy-RequiredFile (Join-Path $RepoRoot "target/$ios/$profile/lib$UnityLib.a") $iosOut
}

function Invoke-PlatformBuild {
    param(
        [string]$Name,
        [switch]$ContinueOnError
    )

    try {
        switch ($Name) {
            'Windows' { Build-Windows }
            'Android' { Build-Android }
            'Linux' { Build-Linux }
            'WebGL' { Build-WebGL }
            'Apple' { Build-Apple }
            default { throw "Unsupported platform: $Name" }
        }

        return [pscustomobject]@{
            Platform = $Name
            Status   = 'Succeeded'
            Message  = ''
        }
    }
    catch {
        if ($SkipUnsupported) {
            Write-Warning "Skipped ${Name}: $($_.Exception.Message)"
            return [pscustomobject]@{
                Platform = $Name
                Status   = 'Skipped'
                Message  = $_.Exception.Message
            }
        }

        if ($ContinueOnError) {
            Write-Warning "Failed ${Name}: $($_.Exception.Message)"
            return [pscustomobject]@{
                Platform = $Name
                Status   = 'Failed'
                Message  = $_.Exception.Message
            }
        }

        throw
    }
}

function Write-BuildSummary {
    param([object[]]$Results)

    Write-Host ''
    Write-Step 'Build results'

    foreach ($result in $Results) {
        $color = switch ($result.Status) {
            'Succeeded' { 'Green' }
            'Skipped' { 'Yellow' }
            'Failed' { 'Red' }
            default { 'White' }
        }

        $line = "  {0,-8} {1}" -f $result.Platform, $result.Status
        if ($result.Message) {
            $line = "$line - $($result.Message)"
        }

        Write-Host $line -ForegroundColor $color
    }
}

Push-Location $RepoRoot
try {
    Ensure-Command 'cargo' 'Install Rust from https://rustup.rs/.'
    $isAllBuild = $Platform -contains 'All'

    $platforms = foreach ($item in $Platform) {
        if ($item -eq 'All') {
            'Windows'
            'Android'
            'Linux'
            'WebGL'
            'Apple'
        }
        else {
            $item
        }
    }

    $platforms = @($platforms | Select-Object -Unique)
    $results = @()
    foreach ($platformName in $platforms) {
        $results += Invoke-PlatformBuild $platformName -ContinueOnError:$isAllBuild
    }

    if ($isAllBuild) {
        $failed = @($results | Where-Object { $_.Status -eq 'Failed' })
        if ($failed.Count -gt 0) {
            Write-BuildSummary $results
            throw "One or more platform builds failed."
        }
    }

    Invoke-Bindings
    if ($isAllBuild) {
        Write-BuildSummary $results
    }

    Write-Step "Build completed. Outputs are in $BuildBin"
}
finally {
    Pop-Location
}
