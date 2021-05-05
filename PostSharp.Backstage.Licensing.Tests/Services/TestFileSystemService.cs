// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions.TestingHelpers;
using PostSharp.Backstage.Extensibility;

namespace PostSharp.Backstage.Licensing.Tests.Services
{
    internal class TestFileSystemService : IFileSystemService
    {
        public MockFileSystem Mock { get; } = new();

        public DateTime GetLastWriteTime( string file )
        {
            return this.Mock.File.GetLastWriteTime( file );
        }

        public bool FileExists( string path )
        {
            return this.Mock.File.Exists( path );
        }

        public bool DirectoryExists( string path )
        {
            return this.Mock.Directory.Exists( path );
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

                return this.Mock.Directory.GetFiles( path, searchPattern, searchOption.Value );
            }
            else if ( searchPattern != null )
            {
                return this.Mock.Directory.GetFiles( path, searchPattern );
            }
            else
            {
                return this.Mock.Directory.GetFiles( path );
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

                directories = this.Mock.Directory.GetDirectories( path, searchPattern, searchOption.Value );
            }
            else if ( searchPattern != null )
            {
                directories = this.Mock.Directory.GetDirectories( path, searchPattern );
            }
            else
            {
                directories = this.Mock.Directory.GetDirectories( path );
            }

            // The mock returns trailing separator, but BCL does not.

            for ( var i = 0; i < directories.Length; i++ )
            {
                directories[i] = directories[i].TrimEnd( Path.DirectorySeparatorChar );
            }

            return directories;
        }

        public void CreateDirectory( string path ) => this.Mock.Directory.CreateDirectory( path );

        public Stream OpenRead( string path ) => this.Mock.File.OpenRead( path );

        public Stream OpenWrite( string path ) => this.Mock.File.OpenWrite( path );

        public byte[] ReadAllBytes( string path ) => this.Mock.File.ReadAllBytes( path );

        public void WriteAllBytes( string path, byte[] bytes ) => this.Mock.File.WriteAllBytes( path, bytes );

        public string ReadAllText( string path ) => this.Mock.File.ReadAllText( path );

        public void WriteAllText( string path, string content ) => this.Mock.File.WriteAllText( path, content );

        public string[] ReadAllLines( string path ) => this.Mock.File.ReadAllLines( path );

        public void WriteAllLines( string path, string[] content ) => this.Mock.File.WriteAllLines( path, content );

        public void WriteAllLines( string path, IEnumerable<string> content ) => this.Mock.File.WriteAllLines( path, content );
    }
}
