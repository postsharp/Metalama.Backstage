﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Collections.Generic;
using System.IO;

namespace PostSharp.Backstage.Extensibility
{
    public interface IFileSystemService
    {
        DateTime GetLastWriteTime( string path );

        bool FileExists( string path );

        bool DirectoryExists( string path );

        string[] GetFiles( string path, string? searchPattern = null, SearchOption? searchOption = null );

        IEnumerable<string> EnumerateFiles( string path, string? searchPattern = null, SearchOption? searchOption = null );

        string[] GetDirectories( string path, string? searchPattern = null, SearchOption? searchOption = null );

        IEnumerable<string> EnumerateDirectories( string path, string? searchPattern = null, SearchOption? searchOption = null );

        Stream OpenRead( string path );

        byte[] ReadAllBytes( string path );

        string ReadAllText( string path );

        string[] ReadAllLines( string path );
    }
}
