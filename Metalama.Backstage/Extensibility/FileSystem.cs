﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace Metalama.Backstage.Extensibility
{
    /// <summary>
    /// Provides access to file system using API in <see cref="System.IO" /> namespace.
    /// </summary>
    internal class FileSystem : IFileSystem
    {
        /// <inheritdoc />
        public DateTime GetFileLastWriteTime( string path )
        {
            return File.GetLastWriteTime( path );
        }

        /// <inheritdoc />
        public void SetFileLastWriteTime( string path, DateTime lastWriteTime )
        {
            File.SetLastWriteTime( path, lastWriteTime );
        }

        /// <inheritdoc />
        public DateTime GetDirectoryLastWriteTime( string path )
        {
            return Directory.GetLastWriteTime( path );
        }

        /// <inheritdoc />
        public void SetDirectoryLastWriteTime( string path, DateTime lastWriteTime )
        {
            Directory.SetLastWriteTime( path, lastWriteTime );
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
        public string[] GetFiles( string path )
        {
            return Directory.GetFiles( path );
        }

        /// <inheritdoc />
        public string[] GetFiles( string path, string searchPattern )
        {
            return Directory.GetFiles( path, searchPattern );
        }

        /// <inheritdoc />
        public string[] GetFiles( string path, string searchPattern, SearchOption searchOption )
        {
            return Directory.GetFiles( path, searchPattern, searchOption );
        }

        /// <inheritdoc />
        public IEnumerable<string> EnumerateFiles( string path )
        {
            return Directory.EnumerateFiles( path );
        }

        /// <inheritdoc />
        public IEnumerable<string> EnumerateFiles( string path, string searchPattern )
        {
            return Directory.EnumerateFiles( path, searchPattern );
        }

        /// <inheritdoc />
        public IEnumerable<string> EnumerateFiles( string path, string searchPattern, SearchOption searchOption )
        {
            return Directory.EnumerateFiles( path, searchPattern, searchOption );
        }

        /// <inheritdoc />
        public string[] GetDirectories( string path )
        {
            return Directory.GetDirectories( path );
        }

        /// <inheritdoc />
        public string[] GetDirectories( string path, string searchPattern )
        {
            return Directory.GetDirectories( path, searchPattern );
        }

        /// <inheritdoc />
        public string[] GetDirectories( string path, string searchPattern, SearchOption searchOption )
        {
            return Directory.GetDirectories( path, searchPattern, searchOption );
        }

        /// <inheritdoc />
        public IEnumerable<string> EnumerateDirectories( string path )
        {
            return Directory.EnumerateDirectories( path );
        }

        /// <inheritdoc />
        public IEnumerable<string> EnumerateDirectories( string path, string searchPattern )
        {
            return Directory.EnumerateDirectories( path, searchPattern );
        }

        /// <inheritdoc />
        public IEnumerable<string> EnumerateDirectories( string path, string searchPattern, SearchOption searchOption )
        {
            return Directory.EnumerateDirectories( path, searchPattern, searchOption );
        }

        /// <inheritdoc />
        public Stream CreateFile( string path )
        {
            return File.Create( path );
        }

        /// <inheritdoc />
        public Stream CreateFile( string path, int bufferSize )
        {
            return File.Create( path, bufferSize );
        }

        /// <inheritdoc />
        public Stream CreateFile( string path, int bufferSize, FileOptions options )
        {
            return File.Create( path, bufferSize, options );
        }

        /// <inheritdoc />
        public string GetTempFileName()
        {
            return Path.GetTempFileName();
        }

        /// <inheritdoc />
        public void CreateDirectory( string path )
        {
            Directory.CreateDirectory( path );
        }

        /// <inheritdoc />
        public Stream Open( string path, FileMode mode )
        {
            return File.Open( path, mode );
        }

        /// <inheritdoc />
        public Stream Open( string path, FileMode mode, FileAccess access )
        {
            return File.Open( path, mode, access );
        }

        /// <inheritdoc />
        public Stream Open( string path, FileMode mode, FileAccess access, FileShare share )
        {
            return File.Open( path, mode, access, share );
        }

        /// <inheritdoc />
        public Stream Open( string path, FileMode mode, FileAccess access, FileShare share, int bufferSize, FileOptions options )
        {
            return new FileStream( path, mode, access, share, bufferSize, options );
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
        public void MoveFile( string sourceFileName, string destFileName )
        {
            File.Move( sourceFileName, destFileName );
        }

        /// <inheritdoc />
        public void DeleteFile( string path )
        {
            File.Delete( path );
        }

        /// <inheritdoc />
        public void MoveDirectory( string sourceDirName, string destDirName )
        {
            Directory.Move( sourceDirName, destDirName );
        }

        /// <inheritdoc />
        public void DeleteDirectory( string path, bool recursive )
        {
            Directory.Delete( path, recursive );
        }

        /// <inheritdoc />
        public bool IsDirectoryEmpty( string path ) => !Directory.EnumerateFileSystemEntries( path ).Any();

        /// <inheritdoc />
        public void ExtractZipArchiveToDirectory( ZipArchive sourceZipArchive, string destinationDirectoryPath )
            => sourceZipArchive.ExtractToDirectory( destinationDirectoryPath );
    }
}