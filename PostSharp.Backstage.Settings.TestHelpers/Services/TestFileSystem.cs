// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using PostSharp.Backstage.Extensibility;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions.TestingHelpers;
using System.Runtime.CompilerServices;
using System.Threading;

namespace PostSharp.Backstage.Testing.Services
{
    public class TestFileSystem : IFileSystem
    {
        private readonly Dictionary<string, (ManualResetEventSlim Callee, ManualResetEventSlim Caller)> _blockedReads =
            new();

        private readonly Dictionary<string, (ManualResetEventSlim Callee, ManualResetEventSlim Caller)> _blockedWrites =
            new();

        private readonly List<string> _failedAccesses = new();

        public MockFileSystem Mock { get; } = new();

        public IReadOnlyList<string> FailedFileAccesses => this._failedAccesses;

        public ManualResetEventSlim BlockRead( string path )
        {
            ManualResetEventSlim readEvent = new();
            this._blockedReads.Add( path, (new ManualResetEventSlim(), readEvent) );

            return readEvent;
        }

        public ManualResetEventSlim BlockWrite( string path )
        {
            ManualResetEventSlim writeEvent = new();
            this._blockedWrites.Add( path, (new ManualResetEventSlim(), writeEvent) );

            return writeEvent;
        }

        public void Unblock( string path )
        {
            void UnblockEvents(
                Dictionary<string, (ManualResetEventSlim Callee, ManualResetEventSlim Caller)> blockingEvents )
            {
                if ( blockingEvents.TryGetValue( path, out var events ) )
                {
                    events.Callee.Set();
                    blockingEvents.Remove( path );
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
            return this.Mock.File.GetLastWriteTime( path );
        }

        public bool FileExists( string path )
        {
            return this.Mock.File.Exists( path );
        }

        public bool DirectoryExists( string path )
        {
            return this.Mock.Directory.Exists( path );
        }

        public IEnumerable<string> EnumerateFiles( string path, string? searchPattern = null,
            SearchOption? searchOption = null )
        {
            return this.GetFiles( path, searchPattern, searchOption );
        }

        public string[] GetFiles( string path, string? searchPattern = null, SearchOption? searchOption = null )
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

        public IEnumerable<string> EnumerateDirectories( string path, string? searchPattern = null,
            SearchOption? searchOption = null )
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

        public void CreateDirectory( string path )
        {
            this.WaitAndThrowIfBlocked( path, true );
            this.Mock.Directory.CreateDirectory( path );
        }

        public Stream OpenRead( string path )
        {
            this.WaitAndThrowIfBlocked( path, false );

            return this.Mock.File.OpenRead( path );
        }

        public Stream OpenWrite( string path )
        {
            this.WaitAndThrowIfBlocked( path, true );

            return this.Mock.File.OpenWrite( path );
        }

        public byte[] ReadAllBytes( string path )
        {
            this.WaitAndThrowIfBlocked( path, false );

            return this.Mock.File.ReadAllBytes( path );
        }

        public void WriteAllBytes( string path, byte[] bytes )
        {
            this.WaitAndThrowIfBlocked( path, true );
            this.Mock.File.WriteAllBytes( path, bytes );
        }

        public string ReadAllText( string path )
        {
            this.WaitAndThrowIfBlocked( path, false );

            return this.Mock.File.ReadAllText( path );
        }

        public void WriteAllText( string path, string content )
        {
            this.WaitAndThrowIfBlocked( path, true );
            this.Mock.File.WriteAllText( path, content );
        }

        public string[] ReadAllLines( string path )
        {
            this.WaitAndThrowIfBlocked( path, false );

            return this.Mock.File.ReadAllLines( path );
        }

        public void WriteAllLines( string path, string[] content )
        {
            this.WaitAndThrowIfBlocked( path, true );
            this.Mock.File.WriteAllLines( path, content );
        }

        public void WriteAllLines( string path, IEnumerable<string> content )
        {
            this.WaitAndThrowIfBlocked( path, true );
            this.Mock.File.WriteAllLines( path, content );
        }
    }
}