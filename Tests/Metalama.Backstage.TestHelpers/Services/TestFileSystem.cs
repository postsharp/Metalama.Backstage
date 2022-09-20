// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Extensibility;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Metalama.Backstage.Testing.Services
{
    public class TestFileSystem : IFileSystem
    {
        private readonly ConcurrentDictionary<string, (ManualResetEventSlim Callee, ManualResetEventSlim Caller)> _blockedReads =
            new();

        private readonly ConcurrentDictionary<string, (ManualResetEventSlim Callee, ManualResetEventSlim Caller)> _blockedWrites =
            new();

        private readonly List<string> _failedAccesses = new();

        public MockFileSystem Mock { get; } = new();

        public IReadOnlyList<string> FailedFileAccesses => this._failedAccesses;

        public ManualResetEventSlim BlockRead( string path )
        {
            ManualResetEventSlim readEvent = new();

            if ( !this._blockedReads.TryAdd( path, (new ManualResetEventSlim(), readEvent) ) )
            {
                throw new InvalidOperationException();
            }

            return readEvent;
        }

        public ManualResetEventSlim BlockWrite( string path )
        {
            ManualResetEventSlim writeEvent = new();

            if ( !this._blockedWrites.TryAdd( path, (new ManualResetEventSlim(), writeEvent) ) )
            {
                throw new InvalidOperationException();
            }

            return writeEvent;
        }

        public void Unblock( string path )
        {
            void UnblockEvents( ConcurrentDictionary<string, (ManualResetEventSlim Callee, ManualResetEventSlim Caller)> blockingEvents )
            {
                if ( blockingEvents.TryGetValue( path, out var events ) )
                {
                    events.Callee.Set();

                    if ( !blockingEvents.TryRemove( path, out _ ) )
                    {
                        throw new InvalidOperationException();
                    }
                }
            }

            UnblockEvents( this._blockedReads );
            UnblockEvents( this._blockedWrites );
        }

        private void WaitAndThrowIfBlocked( string path, bool write, [CallerMemberName] string callerName = "" )
        {
            var blockedFiles = write ? this._blockedWrites : this._blockedReads;

            if ( blockedFiles.TryGetValue( path, out var events ) )
            {
                events.Caller.Set();
                events.Callee.Wait();
                this._failedAccesses.Add( $"{callerName}({path})" );

                throw new IOException( $"{callerName} failed. File '{path}' in use." );
            }
        }

        public DateTime GetLastWriteTime( string path )
        {
            lock ( this.Mock )
            {
                return this.Mock.File.GetLastWriteTime( path );
            }
        }

        public void SetLastWriteTime( string path, DateTime lastWriteTime )
        {
            lock ( this.Mock )
            {
                this.Mock.File.SetLastWriteTime( path, lastWriteTime );
            }
        }

        public bool FileExists( string path )
        {
            lock ( this.Mock )
            {
                return this.Mock.File.Exists( path );
            }
        }

        public bool DirectoryExists( string path )
        {
            lock ( this.Mock )
            {
                return this.Mock.Directory.Exists( path );
            }
        }

        public IEnumerable<string> EnumerateFiles(
            string path,
            string? searchPattern = null,
            SearchOption? searchOption = null )
        {
            lock ( this.Mock )
            {
                return this.GetFiles( path, searchPattern, searchOption ).ToList();
            }
        }

        public string[] GetFiles( string path, string? searchPattern = null, SearchOption? searchOption = null )
        {
            lock ( this.Mock )
            {
                if ( searchOption.HasValue )
                {
                    if ( searchPattern == null )
                    {
                        throw new ArgumentNullException( nameof(searchPattern) );
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
        }

        public IEnumerable<string> EnumerateDirectories(
            string path,
            string? searchPattern = null,
            SearchOption? searchOption = null )
        {
            lock ( this.Mock )
            {
                return this.GetDirectories( path, searchPattern, searchOption );
            }
        }

        public string[] GetDirectories( string path, string? searchPattern = null, SearchOption? searchOption = null )
        {
            lock ( this.Mock )
            {
                string[] directories;

                if ( searchOption.HasValue )
                {
                    if ( searchPattern == null )
                    {
                        throw new ArgumentNullException( nameof(searchPattern) );
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
        }

        public void CreateDirectory( string path )
        {
            lock ( this.Mock )
            {
                this.WaitAndThrowIfBlocked( path, true );
                this.Mock.Directory.CreateDirectory( path );
            }
        }

        public Stream Open( string path, FileMode mode )
        {
            lock ( this.Mock )
            {
                this.WaitAndThrowIfBlocked( path, false );

                return this.Mock.File.Open( path, mode );
            }
        }

        public Stream Open( string path, FileMode mode, FileAccess access )
        {
            lock ( this.Mock )
            {
                this.WaitAndThrowIfBlocked( path, false );

                return this.Mock.File.Open( path, mode, access );
            }
        }

        public Stream Open( string path, FileMode mode, FileAccess access, FileShare share )
        {
            lock ( this.Mock )
            {
                this.WaitAndThrowIfBlocked( path, false );

                return this.Mock.File.Open( path, mode, access, share );
            }
        }

        public Stream OpenRead( string path )
        {
            lock ( this.Mock )
            {
                this.WaitAndThrowIfBlocked( path, false );

                return this.Mock.File.OpenRead( path );
            }
        }

        public Stream OpenWrite( string path )
        {
            lock ( this.Mock )
            {
                this.WaitAndThrowIfBlocked( path, true );

                return this.Mock.File.OpenWrite( path );
            }
        }

        public byte[] ReadAllBytes( string path )
        {
            lock ( this.Mock )
            {
                this.WaitAndThrowIfBlocked( path, false );

                return this.Mock.File.ReadAllBytes( path );
            }
        }

        public void WriteAllBytes( string path, byte[] bytes )
        {
            lock ( this.Mock )
            {
                this.WaitAndThrowIfBlocked( path, true );
                this.Mock.File.WriteAllBytes( path, bytes );
            }
        }

        public string ReadAllText( string path )
        {
            lock ( this.Mock )
            {
                this.WaitAndThrowIfBlocked( path, false );

                return this.Mock.File.ReadAllText( path );
            }
        }

        public void WriteAllText( string path, string content )
        {
            lock ( this.Mock )
            {
                this.WaitAndThrowIfBlocked( path, true );
                this.Mock.File.WriteAllText( path, content );
                this.Mock.File.SetLastWriteTime( path, DateTime.Now );
            }
        }

        public string[] ReadAllLines( string path )
        {
            lock ( this.Mock )
            {
                this.WaitAndThrowIfBlocked( path, false );

                return this.Mock.File.ReadAllLines( path );
            }
        }

        public void WriteAllLines( string path, string[] content )
        {
            lock ( this.Mock )
            {
                this.WaitAndThrowIfBlocked( path, true );
                this.Mock.File.WriteAllLines( path, content );
                this.Mock.File.SetLastWriteTime( path, DateTime.Now );
            }
        }

        public void WriteAllLines( string path, IEnumerable<string> content )
        {
            lock ( this.Mock )
            {
                this.WaitAndThrowIfBlocked( path, true );
                this.Mock.File.WriteAllLines( path, content );
                this.Mock.File.SetLastWriteTime( path, DateTime.Now );
            }
        }

        public void MoveDirectory( string sourceDirName, string destDirName )
        {
            lock ( this.Mock )
            {
                this.Mock.Directory.Move( sourceDirName, destDirName );
            }
        }

        public void DeleteDirectory( string path, bool recursive )
        {
            lock ( this.Mock )
            {
                this.Mock.Directory.Delete( path, recursive );
            }
        }

        public bool IsDirectoryEmpty( string path ) => !this.Mock.Directory.EnumerateFileSystemEntries( path ).Any();
    }
}