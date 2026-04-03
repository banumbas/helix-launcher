using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using Robust.LoaderApi;

namespace SS14.Loader;

internal sealed class FilteredFileApi : IFileApi
{
    private readonly IFileApi _inner;
    private readonly Func<string, bool> _allowPath;

    public FilteredFileApi(IFileApi inner, Func<string, bool> allowPath)
    {
        _inner = inner;
        _allowPath = allowPath;
    }

    public bool TryOpen(string path, [NotNullWhen(true)] out Stream? stream)
    {
        if (!_allowPath(path))
        {
            stream = null;
            return false;
        }

        return _inner.TryOpen(path, out stream);
    }

    public IEnumerable<string> AllFiles => _inner.AllFiles.Where(_allowPath);
}
