# progress-hierarchy

[![NuGet](https://img.shields.io/nuget/v/ConsoleProgressBar.svg)](https://www.nuget.org/packages/ConsoleProgressBar/)

![preview animation](.img/progressbar.gif)

Provides a .NET console progress bar that supports absolute reporting. Comes coupled with support for hierarchical progress reporting.

The `Progress` class can be used without the progress bar. It’s thread-safe and lock-free. It is, however, not asynchronous. The `ProgressChanged` event handler will be called synchronously by the thread currently reporting its progress.

The `Progress` class implements `System.IProgress<double>`.

# Usage

## `Progress`

```C#
using (var p = new Progress())
{
    p.ProgressChanged += OnProgressChanged;
    
    using (var p1 = p.Fork(0.5, "Long-running task A"))
    {
        for (var i = 0; i < 10; i++)
        {
            using (p1.Fork(0.1, $"Item {i}"))
            {
                // do stuff
            }
        }
    }
    
    using (var p2 = p.Fork(0.5, "Long-running task B"))
    {
        for (var i = 0; i < 10; i++)
        {
            p2.Report(i/10, $"Item {i}");
            
            // do stuff
        }
    }
}
```

## `ProgressBar`

```C#
using (var pb = new ProgressBar())
{
    using (var p1 = pb.Progress.Fork(0.5))
    {
        // do stuff
    }
    
    using (var p2 = pb.Progress.Fork(0.5))
    {
        // do stuff
    }
}
```
# Compatibility

The library compiles to .NET Standard 1.3 and .NET 4.5. It is not compatible with .NET 4 because it depends on the `Interlocked` class.

# License

This project is licensed under the [MIT License](LICENSE).