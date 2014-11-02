$Root = $PSScriptRoot

Properties {
    $Configuration      = "Release"
    $Name               = "Ragnar"
    $SolutionFile       = "Ragnar.sln"

    # Artifacts
    $NuGetPkg           = Join-Path $Root "build/Ragnar.$Version.nupkg"

    # Directories
    $Dir_Artifacts      = Join-Path $Root "build"
    $Dir_Binaries       = Join-Path $Root "build/bin"

    # Tools
    $Tools_NuGet        = Join-Path $Root "tools/nuget.exe"
}

$h = Get-Host
$w = $h.UI.RawUI.WindowSize.Width

FormatTaskName (("-"*$w) + "`r`n[{0}]`r`n" + ("-"*$w))

Task Default -depends Clean, Compile, Output, NuGet-Pack
Task Publish -depends NuGet-Push

Task Clean {
    Write-Host "Cleaning and creating build artifacts folder."

    If (Test-Path $Dir_Artifacts)
    {
        Remove-Item $Dir_Artifacts -Recurse -Force | Out-Null
    }

    New-Item $Dir_Artifacts -ItemType directory | Out-Null
    New-Item $Dir_Binaries  -ItemType directory | Out-Null

    Exec { msbuild $SolutionFile /t:Clean "/p:Configuration=$Configuration" /p:VisualStudioVersion=12.0 /v:quiet } 
}

Task Compile -depends Clean {
    Exec { msbuild $SolutionFile /t:Build "/p:Configuration=$Configuration" /p:VisualStudioVersion=12.0 /v:quiet }
}

Task Output -depends Compile {
    # Copying relevant files to binaries directory
    $dll = Join-Path $Root "$Configuration/Ragnar.dll"

    Copy-Item $dll $Dir_Binaries

    # Copy boost DLLs
    Copy-Item "packages/boost_1_55_0/stage/lib/boost_chrono-vc120-mt-1_55.dll" $Dir_Binaries
    Copy-Item "packages/boost_1_55_0/stage/lib/boost_date_time-vc120-mt-1_55.dll" $Dir_Binaries
    Copy-Item "packages/boost_1_55_0/stage/lib/boost_system-vc120-mt-1_55.dll" $Dir_Binaries
    Copy-Item "packages/boost_1_55_0/stage/lib/boost_thread-vc120-mt-1_55.dll" $Dir_Binaries

    # Copy libtorrent DLL
    Copy-Item "packages/libtorrent-rasterbar-1.0.2/bin/msvc-12.0/$Configuration/boost-link-shared/boost-source/threading-multi/torrent.dll" $Dir_Binaries
}

Task NuGet-Pack -depends Output {
    Exec {
        & $Tools_NuGet pack Ragnar.nuspec -Version $Version -OutputDirectory $Dir_Artifacts
    }
}

Task NuGet-Push -depends NuGet-Pack {
    Exec {
        & $Tools_NuGet push $NuGetPkg $NuGet_API_Key
    }
}