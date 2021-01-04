# progress-hierarchy

[![CI Build](https://github.com/fuzzykiller/progress-hierarchy/workflows/CI%20Build/badge.svg)](https://github.com/fuzzykiller/progress-hierarchy/actions)

## ConsoleProgressBar

[![NuGet](https://img.shields.io/nuget/v/ConsoleProgressBar.svg)](https://www.nuget.org/packages/ConsoleProgressBar/)

![preview animation](.img/progressbar.gif)

Provides a .NET console progress bar that supports absolute reporting and a separate library for hierarchical progress reporting.

### Usage

```cs
using (var pb = new ProgressBar())
{
    using (var p1 = pb.HierarchicalProgress.Fork(0.5))
    {
        // do stuff
    }
    
    using (var p2 = pb.HierarchicalProgress.Fork(0.5))
    {
        // do stuff
    }
}
```

## ProgressHierarchy

[![NuGet](https://img.shields.io/nuget/v/ProgressHierarchy.svg)](https://www.nuget.org/packages/ProgressHierarchy/)

The `ProgressHierarchy` package can be used without the progress bar.

The `HierarchicalProgress` class implements `System.IProgress<double>`. It’s thread-safe and lock-free. It is, however, not asynchronous. The `ProgressChanged`
event handlers will be called synchronously by the thread currently reporting its progress. Event handlers must be fast. They must take care of continuing on
the correct thread if required.

The `HierarchicalProgress` class was renamed to not conflict with the `System.Progress<T>` class.

### Usage

```cs
using (var p = new HierarchicalProgress())
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

# Compatibility

The library compiles to .NET Standard 1.3 and .NET 4.5. It is not compatible with .NET 4 because it depends on the `Interlocked` class.

# License

This project is licensed under the [MIT License](LICENSE).