# Ragnar
Ragnar is a C++/CLI wrapper for Rasterbar's *libtorrent*. It aims to provide a (mostly) complete interface to the underlying C++ library.

## Usage

### 1. Install the NuGet package.

```posh
PM> Install-Package Ragnar
```

### 2. Create a session

```csharp
using(var session = new Session())
{
    // Make the session listen on a port in the range
    // 6881-6889
    session.ListenOn(6881, 6889);

    // Create the AddTorrentParams with info about the torrent
    // we'd like to add.
    var addParams = new AddTorrentParams
    {
        SavePath = "C:\\Downloads",
        Url = "<url to a torrent file>"
    };

    // Add a torrent to the session and get a `TorrentHandle`
    // in return.
    var handle = session.AddTorrent(addParams);

    while(true)
    {
        // Get a `TorrentStatus` instance from the handle.
        var status = handle.QueryStatus();

        // If we are seeding, our job here is done.
        if(status.IsSeeding)
        {
            break;
        }

        // Print our progress and sleep for a bit.
        Console.WriteLine("{0}% downloaded", status.Progress * 100);
        Thread.Sleep(1000);
    }
}
```

## How to build from source
The repository includes a `bootstrap.bat` file which runs the `run-bootstrap.ps1` script. This will download and compile both Boost and libtorrent automatically.

You need to update powershell to the lastest version.

### 1. Run the `bootstrap.bat` file

### 2. Building Ragnar
Open `Ragnar.sln` in Visual Studio 2013. Press `F6`. Wait a while. Success. If not - report an issue.

## Contributing
* Fork the repository.
* Make your feature addition or bug fix.
* Send a pull request. *Bonus for topic branches. Funny .gif will be your reward.*

## License
Copyright (c) 2014, Viktor Elofsson and contributors

Ragnar is provided as-is under the **MIT** license. For more information see `LICENSE`.

*For Boost, see http://svn.boost.org/svn/boost/trunk/LICENSE_1_0.txt*

*For libtorrent, see https://libtorrent.googlecode.com/svn/trunk/LICENSE*
