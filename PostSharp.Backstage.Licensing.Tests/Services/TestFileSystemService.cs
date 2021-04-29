// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using PostSharp.Backstage.Extensibility;

namespace PostSharp.Backstage.Licensing.Tests.Services
{
    internal class TestFileSystemService : IFileSystemService
    {
        private readonly IFileSystem _fileSystemMock;

        public TestFileSystemService( IFileSystem fileSystemMock )
        {
            this._fileSystemMock = fileSystemMock;
        }

        public DateTime GetLastWriteTime( string file )
        {
            return this._fileSystemMock.File.GetLastWriteTime( file );
        }

        public bool FileExists( string path )
        {
            return this._fileSystemMock.File.Exists( path );
        }

        public bool DirectoryExists( string path )
        {
            return this._fileSystemMock.Directory.Exists( path );
        }

        public IEnumerable<string> EnumerateFiles( string path, string? searchPattern = null, SearchOption? searchOption = null )
        {
            return this.GetFiles( path, searchPattern, searchOption );
        }

        public string[] GetFiles( string path, string? searchPattern = null, SearchOption? searchOption = null )
        {
            if ( searchOption.HasValue )
            {
                if ( searchPattern == null )
                {
                    throw new ArgumentNullException( "searchPattern" );
                }

                return this._fileSystemMock.Directory.GetFiles( path, searchPattern, searchOption.Value );
            }
            else if ( searchPattern != null )
            {
                return this._fileSystemMock.Directory.GetFiles( path, searchPattern );
            }
            else
            {
                return this._fileSystemMock.Directory.GetFiles( path );
            }
        }

        public IEnumerable<string> EnumerateDirectories( string path, string? searchPattern = null, SearchOption? searchOption = null )
        {
            return this.GetDirectories( path, searchPattern, searchOption );
        }

        public string[] GetDirectories( string path, string? searchPattern = null, SearchOption? searchOption = null )
        {
            string[] directories;

            if ( searchOption.HasValue )
            {
                if ( searchPattern == null )
                {
                    throw new ArgumentNullException( "searchPattern" );
                }

                directories = this._fileSystemMock.Directory.GetDirectories( path, searchPattern, searchOption.Value );
            }
            else if ( searchPattern != null )
            {
                directories = this._fileSystemMock.Directory.GetDirectories( path, searchPattern );
            }
            else
            {
                directories = this._fileSystemMock.Directory.GetDirectories( path );
            }

            // The mock returns trailing separator, but BCL does not.

            for ( var i = 0; i < directories.Length; i++ )
            {
                directories[i] = directories[i].TrimEnd( Path.DirectorySeparatorChar );
            }

            return directories;
        }

        public Stream OpenRead( string path )
        {
            return this._fileSystemMock.File.OpenRead( path );
        }

        public byte[] ReadAllBytes( string path )
        {
            return this._fileSystemMock.File.ReadAllBytes( path );
        }

        public string ReadAllText( string path )
        {
            return this._fileSystemMock.File.ReadAllText( path );
        }

        public string[] ReadAllLines( string path )
        {
            return this._fileSystemMock.File.ReadAllLines( path );
        }
    }
}
