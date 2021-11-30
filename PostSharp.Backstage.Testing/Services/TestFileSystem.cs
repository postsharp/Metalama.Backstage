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
        private readonly Dictionary<string, (ManualResetEventSlim Callee, ManualResetEventSlim Caller)> _blockedReads = new();
        private readonly Dictionary<string, (ManualResetEventSlim Callee, ManualResetEventSlim Caller)> _blockedWrites = new();

        private readonly List<string> _failedAccesses = new();

        public MockFileSystem Mock { get; } = new();

        public IReadOnlyList<string> FailedFileAccesses => _failedAccesses;

        public ManualResetEventSlim BlockRead( string path )
        {
            ManualResetEventSlim readEvent = new();
            _blockedReads.Add( path, ( new ManualResetEventSlim(), readEvent ) );

            return readEvent;
        }

        public ManualResetEventSlim BlockWrite( string path )
        {
            ManualResetEventSlim writeEvent = new();
            _blockedWrites.Add( path, ( new ManualResetEventSlim(), writeEvent ) );

            return writeEvent;
        }

        public void Unblock( string path )
        {
            void UnblockEvents( Dictionary<string, (ManualResetEventSlim Callee, ManualResetEventSlim Caller)> blockingEvents )
            {
                if (blockingEvents.TryGetValue( path, out var events ))
                {
                    events.Callee.Set();
                    blockingEvents.Remove( path );
                }
            }

            UnblockEvents( _blockedReads );
            UnblockEvents( _blockedWrites );
        }

        private void WaitAndThrowIfBlocked( string path, bool write, [CallerMemberName] string callerName = "" )
        {
            var blockedFiles = write ? _blockedWrites : _blockedReads;

            if (blockedFiles.TryGetValue( path, out var events ))
            {
                events.Caller.Set();
                events.Callee.Wait();
                _failedAccesses.Add( $"{callerName}({path})" );

                throw new IOException( $"{callerName} failed. File '{path}' in use." );
            }
        }

        public DateTime GetLastWriteTime( string path )
        {
            return Mock.File.GetLastWriteTime( path );
        }

        public bool FileExists( string path )
        {
            return Mock.File.Exists( path );
        }

        public bool DirectoryExists( string path )
        {
            return Mock.Directory.Exists( path );
        }

        public IEnumerable<string> EnumerateFiles( string path, string? searchPattern = null, SearchOption? searchOption = null )
        {
            return GetFiles( path, searchPattern, searchOption );
        }

        public string[] GetFiles( string path, string? searchPattern = null, SearchOption? searchOption = null )
        {
            if (searchOption.HasValue)
            {
                if (searchPattern == null)
                {
                    throw new ArgumentNullException( nameof(searchPattern) );
                }

                return Mock.Directory.GetFiles( path, searchPattern, searchOption.Value );
            }
            else if (searchPattern != null)
            {
                return Mock.Directory.GetFiles( path, searchPattern );
            }
            else
            {
                return Mock.Directory.GetFiles( path );
            }
        }

        public IEnumerable<string> EnumerateDirectories( string path, string? searchPattern = null, SearchOption? searchOption = null )
        {
            return GetDirectories( path, searchPattern, searchOption );
        }

        public string[] GetDirectories( string path, string? searchPattern = null, SearchOption? searchOption = null )
        {
            string[] directories;

            if (searchOption.HasValue)
            {
                if (searchPattern == null)
                {
                    throw new ArgumentNullException( nameof(searchPattern) );
                }

                directories = Mock.Directory.GetDirectories( path, searchPattern, searchOption.Value );
            }
            else if (searchPattern != null)
            {
                directories = Mock.Directory.GetDirectories( path, searchPattern );
            }
            else
            {
                directories = Mock.Directory.GetDirectories( path );
            }

            // The mock returns trailing separator, but BCL does not.

            for (var i = 0; i < directories.Length; i++)
            {
                directories[i] = directories[i].TrimEnd( Path.DirectorySeparatorChar );
            }

            return directories;
        }

        public void CreateDirectory( string path )
        {
            WaitAndThrowIfBlocked( path, true );
            Mock.Directory.CreateDirectory( path );
        }

        public Stream OpenRead( string path )
        {
            WaitAndThrowIfBlocked( path, false );

            return Mock.File.OpenRead( path );
        }

        public Stream OpenWrite( string path )
        {
            WaitAndThrowIfBlocked( path, true );

            return Mock.File.OpenWrite( path );
        }

        public byte[] ReadAllBytes( string path )
        {
            WaitAndThrowIfBlocked( path, false );

            return Mock.File.ReadAllBytes( path );
        }

        public void WriteAllBytes( string path, byte[] bytes )
        {
            WaitAndThrowIfBlocked( path, true );
            Mock.File.WriteAllBytes( path, bytes );
        }

        public string ReadAllText( string path )
        {
            WaitAndThrowIfBlocked( path, false );

            return Mock.File.ReadAllText( path );
        }

        public void WriteAllText( string path, string content )
        {
            WaitAndThrowIfBlocked( path, true );
            Mock.File.WriteAllText( path, content );
        }

        public string[] ReadAllLines( string path )
        {
            WaitAndThrowIfBlocked( path, false );

            return Mock.File.ReadAllLines( path );
        }

        public void WriteAllLines( string path, string[] content )
        {
            WaitAndThrowIfBlocked( path, true );
            Mock.File.WriteAllLines( path, content );
        }

        public void WriteAllLines( string path, IEnumerable<string> content )
        {
            WaitAndThrowIfBlocked( path, true );
            Mock.File.WriteAllLines( path, content );
        }
    }
}