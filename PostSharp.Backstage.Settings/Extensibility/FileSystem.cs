// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PostSharp.Backstage.Extensibility
{
    /// <summary>
    /// Provides access to file system using API in <see cref="System.IO" /> namespace.
    /// </summary>
    internal class FileSystem : IFileSystem
    {
        /// <inheritdoc />
        public DateTime GetLastWriteTime( string path )
        {
            return File.GetLastWriteTime( path );
        }

        /// <inheritdoc />
        public bool FileExists( string path )
        {
            return File.Exists( path );
        }

        /// <inheritdoc />
        public bool DirectoryExists( string path )
        {
            return Directory.Exists( path );
        }

        /// <inheritdoc />
        public string[] GetFiles( string path, string? searchPattern = null, SearchOption? searchOption = null )
        {
            return this.EnumerateFiles( path, searchPattern, searchOption ).ToArray();
        }

        /// <inheritdoc />
        public IEnumerable<string> EnumerateFiles( string path, string? searchPattern = null, SearchOption? searchOption = null )
        {
            if ( searchOption.HasValue )
            {
                if ( searchPattern == null )
                {
                    throw new ArgumentNullException( nameof(searchPattern) );
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

        /// <inheritdoc />
        public string[] GetDirectories( string path, string? searchPattern = null, SearchOption? searchOption = null )
        {
            return this.EnumerateDirectories( path, searchPattern, searchOption ).ToArray();
        }

        /// <inheritdoc />
        public IEnumerable<string> EnumerateDirectories( string path, string? searchPattern = null, SearchOption? searchOption = null )
        {
            if ( searchOption.HasValue )
            {
                if ( searchPattern == null )
                {
                    throw new ArgumentNullException( nameof(searchPattern) );
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

        /// <inheritdoc />
        public void CreateDirectory( string path )
        {
            Directory.CreateDirectory( path );
        }

        /// <inheritdoc />
        public Stream OpenRead( string path )
        {
            return File.OpenRead( path );
        }

        /// <inheritdoc />
        public Stream OpenWrite( string path )
        {
            return File.OpenWrite( path );
        }

        /// <inheritdoc />
        public byte[] ReadAllBytes( string path )
        {
            return File.ReadAllBytes( path );
        }

        /// <inheritdoc />
        public void WriteAllBytes( string path, byte[] bytes )
        {
            File.WriteAllBytes( path, bytes );
        }

        /// <inheritdoc />
        public string ReadAllText( string path )
        {
            return File.ReadAllText( path );
        }

        /// <inheritdoc />
        public void WriteAllText( string path, string content )
        {
            File.WriteAllText( path, content );
        }

        /// <inheritdoc />
        public string[] ReadAllLines( string path )
        {
            return File.ReadAllLines( path );
        }

        /// <inheritdoc />
        public void WriteAllLines( string path, string[] content )
        {
            File.WriteAllLines( path, content );
        }

        /// <inheritdoc />
        public void WriteAllLines( string path, IEnumerable<string> content )
        {
            File.WriteAllLines( path, content );
        }
    }
}