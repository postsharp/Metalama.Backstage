﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PostSharp.Backstage.Extensibility
{
    public class FileSystemService : IFileSystemService
    {
        public DateTime GetLastWriteTime( string path )
        {
            return File.GetLastWriteTime( path );
        }

        public bool FileExists( string path )
        {
            return File.Exists( path );
        }

        public bool DirectoryExists( string path )
        {
            return Directory.Exists( path );
        }

        public string[] GetFiles( string path, string? searchPattern = null, SearchOption? searchOption = null )
        {
            return this.EnumerateFiles( path, searchPattern, searchOption ).ToArray();
        }

        public IEnumerable<string> EnumerateFiles( string path, string? searchPattern = null, SearchOption? searchOption = null )
        {
            if ( searchOption.HasValue )
            {
                if ( searchPattern == null )
                {
                    throw new ArgumentNullException( nameof( searchPattern ) );
                }

                return Directory.EnumerateFiles( path, searchPattern, searchOption.Value );
            }
            else if ( searchPattern != null )
            {
                return Directory.EnumerateFiles( path, searchPattern );
            }
            else
            {
                return Directory.EnumerateFiles( path );
            }
        }

        public string[] GetDirectories( string path, string? searchPattern = null, SearchOption? searchOption = null )
        {
            return this.EnumerateDirectories( path, searchPattern, searchOption ).ToArray();
        }

        public IEnumerable<string> EnumerateDirectories( string path, string? searchPattern = null, SearchOption? searchOption = null )
        {
            if ( searchOption.HasValue )
            {
                if ( searchPattern == null )
                {
                    throw new ArgumentNullException( nameof( searchPattern ) );
                }

                return Directory.EnumerateDirectories( path, searchPattern, searchOption.Value );
            }
            else if ( searchPattern != null )
            {
                return Directory.EnumerateDirectories( path, searchPattern );
            }
            else
            {
                return Directory.EnumerateDirectories( path );
            }
        }

        public void CreateDirectory( string path ) => Directory.CreateDirectory( path );

        public Stream OpenRead( string path ) => File.OpenRead( path );

        public Stream OpenWrite( string path ) => File.OpenWrite( path );

        public byte[] ReadAllBytes( string path ) => File.ReadAllBytes( path );

        public void WriteAllBytes( string path, byte[] bytes ) => File.WriteAllBytes( path, bytes );

        public string ReadAllText( string path ) => File.ReadAllText( path );

        public void WriteAllText( string path, string content ) => File.WriteAllText( path, content );

        public string[] ReadAllLines( string path ) => File.ReadAllLines( path );

        public void WriteAllLines( string path, string[] content ) => File.WriteAllLines( path, content );

        public void WriteAllLines( string path, IEnumerable<string> content ) => File.WriteAllLines( path, content );
    }
}