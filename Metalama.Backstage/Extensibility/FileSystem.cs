// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this rep root for details.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Metalama.Backstage.Extensibility
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

        public void SetLastWriteTime( string path, DateTime lastWriteTime )
        {
            File.SetLastWriteTime( path, lastWriteTime );
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
        public IEnumerable<string> EnumerateFiles(
            string path,
            string? searchPattern = null,
            SearchOption? searchOption = null )
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
        public IEnumerable<string> EnumerateDirectories(
            string path,
            string? searchPattern = null,
            SearchOption? searchOption = null )
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

        public string[] GetFileSystemEntries(
            string path,
            string? searchPattern = null,
            SearchOption? searchOption = null )
        {
            return this.EnumerateFileSystemEntries( path, searchPattern, searchOption ).ToArray();
        }

        public IEnumerable<string> EnumerateFileSystemEntries(
            string path,
            string? searchPattern = null,
            SearchOption? searchOption = null )
        {
            string[] directories;
            string[] files;

            if ( searchOption.HasValue )
            {
                if ( searchPattern == null )
                {
                    throw new ArgumentNullException( nameof(searchPattern) );
                }

                directories = Directory.GetDirectories( path, searchPattern, searchOption.Value );
                files = Directory.GetFiles( path, searchPattern, searchOption.Value );
            }
            else if ( searchPattern != null )
            {
                directories = Directory.GetDirectories( path, searchPattern );
                files = Directory.GetFiles( path, searchPattern );
            }
            else
            {
                directories = Directory.GetDirectories( path );
                files = Directory.GetFiles( path );
            }

            return directories.Concat( files ).ToArray();
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

        /// <inheritdoc />
        public void DirectoryMove( string sourceDirName, string destDirName )
        {
            Directory.Move( sourceDirName, destDirName );
        }

        /// <inheritdoc />
        public void DirectoryDelete( string path, bool recursive )
        {
            Directory.Delete( path, recursive );
        }
    }
}