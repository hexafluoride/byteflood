<#
Build script for Ragnar. Will download and compile Boost and libtorrent.
#>

$LIBS_DIR = Join-Path $PSScriptRoot "packages"
$7ZA_URL = "http://downloads.sourceforge.net/project/sevenzip/7-Zip/9.20/7za920.zip"
$7ZA_PKG = Join-Path $LIBS_DIR "7za920.zip"
$7ZA_ROOT = Join-Path $LIBS_DIR "7za920"
$7ZA_EXE = Join-Path $7ZA_ROOT "7za.exe"
$BOOST_URL = "http://downloads.sourceforge.net/project/boost/boost/1.55.0/boost_1_55_0.zip"
$BOOST_PKG = Join-Path $LIBS_DIR "boost_1_55_0.zip"
$BOOST_ROOT = Join-Path $LIBS_DIR "boost_1_55_0"
$LIBTORRENT_URL = "http://downloads.sourceforge.net/project/libtorrent/libtorrent/libtorrent-rasterbar-1.0.2.tar.gz"
$LIBTORRENT_PKG = Join-Path $LIBS_DIR "libtorrent-rasterbar-1.0.2.tar.gz"
$LIBTORRENT_TAR = Join-Path $LIBS_DIR "libtorrent-rasterbar-1.0.2.tar"
$LIBTORRENT_ROOT = Join-Path $LIBS_DIR "libtorrent-rasterbar-1.0.2"

function Download-File {
    param (
        [string]$url,
        [string]$target
    )

    $webClient = new-object System.Net.WebClient
    $webClient.DownloadFile($url, $target)
}

function Extract-File {
    param (
        [string]$file,
        [string]$target
    )

    [System.Reflection.Assembly]::LoadWithPartialName('System.IO.Compression.FileSystem') | Out-Null
    [System.IO.Compression.ZipFile]::ExtractToDirectory($file, $target)
}

# Create lib directory if it does not exist
if (!(Test-Path $LIBS_DIR)) {
    New-Item -ItemType Directory -Path $LIBS_DIR | Out-Null
}

# Download 7za
if (!(Test-Path $7ZA_PKG)) {
    Write-Host "Downloading 7za"
    Download-File $7ZA_URL $7ZA_PKG
}

# Download Boost
if (!(Test-Path $BOOST_PKG)) {
    Write-Host "Downloading Boost"
    Download-File $BOOST_URL $BOOST_PKG
}

# Download libtorrent
if (!(Test-Path $LIBTORRENT_PKG)) {
    Write-Host "Downloading libtorrent"
    Download-File $LIBTORRENT_URL $LIBTORRENT_PKG
}

# Unpack 7za
if (!(Test-Path $7ZA_EXE)) {
    Write-Host "Unpacking 7za"
    Extract-File $7ZA_PKG $7ZA_ROOT
}

# Unpack Boost
if (!(Test-Path $BOOST_ROOT)) {
    Write-Host "Unpacking Boost. May take a while..."

    # Since Boost packages itself as a directory, unpack to LIBS_DIR
    Extract-File $BOOST_PKG $LIBS_DIR
}

# Unpack libtorrent
if (!(Test-Path $LIBTORRENT_ROOT)) {
    Write-Host "Unpacking libtorrent. May take a while..."
    & "$7ZA_EXE" x $LIBTORRENT_PKG -o"$LIBS_DIR"
    & "$7ZA_EXE" x $LIBTORRENT_TAR -o"$LIBS_DIR"
}

# Bootstrap Boost
Write-Host "Bootstrapping Boost"
$boost_bootstrap = Join-Path $BOOST_ROOT "bootstrap.bat"
Start-Process "$boost_bootstrap" -NoNewWindow -Wait -WorkingDirectory $BOOST_ROOT

# Build Boost
Write-Host "Building Boost. This *WILL* take a while."
$boost_b2 = Join-Path $BOOST_ROOT "b2.exe"
Start-Process "$boost_b2" -ArgumentList "toolset=msvc-12.0 link=shared runtime-link=shared --with-date_time --with-system --with-thread" -NoNewWindow -Wait -WorkingDirectory $BOOST_ROOT

# Build libtorrent
Write-Host "Building libtorrent. May take a while."
$env:BOOST_BUILD_PATH = "$BOOST_ROOT"
$env:BOOST_ROOT = "$BOOST_ROOT"
$env:CL = "/I$BOOST_ROOT"

$bjam = Join-Path $BOOST_ROOT "bjam.exe"
$libtorrent_args = "toolset=msvc-12.0 boost=source boost-link=shared geoip=off encryption=tommath link=shared variant"
Start-Process "$bjam" -ArgumentList "$libtorrent_args=debug" -NoNewWindow -Wait -WorkingDirectory $LIBTORRENT_ROOT
Start-Process "$bjam" -ArgumentList "$libtorrent_args=release" -NoNewWindow -Wait -WorkingDirectory $LIBTORRENT_ROOT