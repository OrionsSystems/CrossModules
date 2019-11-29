﻿using System;
using System.IO;

namespace Orions.Systems.CrossModules.Components
{
    public interface IFileListEntry
    {
        DateTime LastModified { get; }

        string Name { get; }

        long Size { get; }

        string Type { get; }

        Stream Data { get; }

        event EventHandler OnDataRead;
    }
}
