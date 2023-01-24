// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Extensibility;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using IFileSystem = Metalama.Backstage.Extensibility.IFileSystem;

namespace Metalama.Backstage.Testing
{
    // Resharper disable UnusedMember.Global

    public class TestFileSystem : IFileSystem
    {
        private enum ExecutionKind
        {
            Manage,
            Read,
            Write
        }

        private abstract class FileSystemWrapper
        {
            protected TestFileSystem Parent { get; }

            public FileSystemWrapper( TestFileSystem parent )
            {
                this.Parent = parent;
            }

            public abstract bool Exists( string path );

            public abstract void SetCreationTime( string path, DateTime creationTime );

            public abstract void SetLastAccessTime( string path, DateTime lastAccessTime );

            public abstract void SetLastWriteTime( string path, DateTime lastWriteTime );

            protected TResult Execute<TResult>( ExecutionKind executionKind, string path, Func<TResult> action )
            {
                lock ( this.Parent.Mock )
                {
                    if ( executionKind == ExecutionKind.Read || executionKind == ExecutionKind.Write )
                    {
                        this.Parent.WaitAndThrowIfBlocked( path, true );
                    }

                    var accessTime = this.Parent._time.Now;

                    var isCreated = executionKind == ExecutionKind.Write && !this.Exists( path );
                    var result = action();

                    if ( isCreated )
                    {
                        this.SetCreationTime( path, accessTime );
                    }

                    if ( executionKind == ExecutionKind.Read || executionKind == ExecutionKind.Write )
                    {
                        this.SetLastAccessTime( path, accessTime );
                    }

                    if ( executionKind == ExecutionKind.Write )
                    {
                        this.SetLastWriteTime( path, accessTime );
                    }

                    return result;
                }
            }

            protected void Execute( ExecutionKind executionKind, string path, Action action )
                => _ = this.Execute<object?>(
                    executionKind,
                    path,
                    () =>
                    {
                        action();

                        return null;
                    } );
        }

        private class DirectoryWrapper : FileSystemWrapper
        {
            public DirectoryWrapper( TestFileSystem parent ) : base( parent ) { }

            public override bool Exists( string path ) => this.Parent.Mock.Directory.Exists( path );

            public override void SetCreationTime( string path, DateTime creationTime ) => this.Parent.Mock.Directory.SetCreationTime( path, creationTime );

            public override void SetLastAccessTime( string path, DateTime lastAccessTime )
                => this.Parent.Mock.Directory.SetLastAccessTime( path, lastAccessTime );

            public override void SetLastWriteTime( string path, DateTime lastWriteTime ) => this.Parent.Mock.Directory.SetLastWriteTime( path, lastWriteTime );

            public TResult Execute<TResult>( ExecutionKind executionKind, string path, Func<IDirectory, TResult> action )
                => this.Execute( executionKind, path, () => action( this.Parent.Mock.Directory ) );

            public void Execute( ExecutionKind executionKind, string path, Action<IDirectory> action )
                => this.Execute( executionKind, path, () => action( this.Parent.Mock.Directory ) );
        }

        private class FileWrapper : FileSystemWrapper
        {
            public FileWrapper( TestFileSystem parent ) : base( parent ) { }

            public override bool Exists( string path ) => this.Parent.Mock.File.Exists( path );

            public override void SetCreationTime( string path, DateTime creationTime ) => this.Parent.Mock.File.SetCreationTime( path, creationTime );

            public override void SetLastAccessTime( string path, DateTime lastAccessTime ) => this.Parent.Mock.File.SetLastAccessTime( path, lastAccessTime );

            public override void SetLastWriteTime( string path, DateTime lastWriteTime ) => this.Parent.Mock.File.SetLastWriteTime( path, lastWriteTime );

            public TResult Execute<TResult>( ExecutionKind executionKind, string path, Func<IFile, TResult> action )
                => this.Execute( executionKind, path, () => action( this.Parent.Mock.File ) );

            public void Execute( ExecutionKind executionKind, string path, Action<IFile> action )
                => this.Execute( executionKind, path, () => action( this.Parent.Mock.File ) );
        }

        private readonly ConcurrentDictionary<string, (ManualResetEventSlim Callee, ManualResetEventSlim Caller)> _blockedReads =
            new();

        private readonly ConcurrentDictionary<string, (ManualResetEventSlim Callee, ManualResetEventSlim Caller)> _blockedWrites =
            new();

        private readonly List<string> _failedAccesses = new();

        private readonly IDateTimeProvider _time;

        private readonly DirectoryWrapper _directory;

        private readonly FileWrapper _file;

        public MockFileSystem Mock { get; } = new();

        public IReadOnlyList<string> FailedFileAccesses => this._failedAccesses;

        public TestFileSystem( IServiceProvider serviceProvider )
        {
            this._time = serviceProvider.GetRequiredBackstageService<IDateTimeProvider>();
            this._directory = new DirectoryWrapper( this );
            this._file = new FileWrapper( this );
        }

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

        public DateTime GetFileLastWriteTime( string path ) => this._file.Execute( ExecutionKind.Manage, path, f => f.GetLastWriteTime( path ) );

        public void SetFileLastWriteTime( string path, DateTime lastWriteTime )
            => this._file.Execute(
                ExecutionKind.Manage,
                path,
                f =>
                {
                    f.SetLastAccessTime( path, lastWriteTime );
                    f.SetLastWriteTime( path, lastWriteTime );
                } );

        public DateTime GetDirectoryLastWriteTime( string path ) => this._directory.Execute( ExecutionKind.Manage, path, d => d.GetLastWriteTime( path ) );

        public void SetDirectoryLastWriteTime( string path, DateTime lastWriteTime )
            => this._directory.Execute(
                ExecutionKind.Manage,
                path,
                d =>
                {
                    d.SetLastAccessTime( path, lastWriteTime );
                    d.SetLastWriteTime( path, lastWriteTime );
                } );

        public bool FileExists( string path ) => this._file.Execute( ExecutionKind.Manage, path, f => f.Exists( path ) );

        public bool DirectoryExists( string path ) => this._directory.Execute( ExecutionKind.Manage, path, d => d.Exists( path ) );

        // We use GetFiles instead of EnumerateFiles because the EnumerateFiles method doesn't behave as expected.
        public IEnumerable<string> EnumerateFiles( string path ) => this._directory.Execute( ExecutionKind.Manage, path, d => d.GetFiles( path ) );

        // We use GetFiles instead of EnumerateFiles because the EnumerateFiles method doesn't behave as expected.
        public IEnumerable<string> EnumerateFiles( string path, string searchPattern )
            => this._directory.Execute( ExecutionKind.Manage, path, d => d.GetFiles( path, searchPattern ) );

        // We use GetFiles instead of EnumerateFiles because the EnumerateFiles method doesn't behave as expected.
        public IEnumerable<string> EnumerateFiles( string path, string searchPattern, SearchOption searchOption )
            => this._directory.Execute( ExecutionKind.Manage, path, d => d.GetFiles( path, searchPattern, searchOption ) );

        public string[] GetFiles( string path ) => this._directory.Execute( ExecutionKind.Manage, path, d => d.GetFiles( path ) );

        public string[] GetFiles( string path, string searchPattern )
            => this._directory.Execute( ExecutionKind.Manage, path, d => d.GetFiles( path, searchPattern ) );

        public string[] GetFiles( string path, string searchPattern, SearchOption searchOption )
            => this._directory.Execute( ExecutionKind.Manage, path, d => d.GetFiles( path, searchPattern, searchOption ) );

        // We use GetDirectories instead of EnumerateDirectories because the EnumerateDirectories method doesn't behave as expected.
        public IEnumerable<string> EnumerateDirectories( string path ) => this._directory.Execute( ExecutionKind.Manage, path, d => d.GetDirectories( path ) );

        // We use GetDirectories instead of EnumerateDirectories because the EnumerateDirectories method doesn't behave as expected.
        public IEnumerable<string> EnumerateDirectories( string path, string searchPattern )
            => this._directory.Execute( ExecutionKind.Manage, path, d => d.GetDirectories( path, searchPattern ) );

        // We use GetDirectories instead of EnumerateDirectories because the EnumerateDirectories method doesn't behave as expected.
        public IEnumerable<string> EnumerateDirectories( string path, string searchPattern, SearchOption searchOption )
            => this._directory.Execute( ExecutionKind.Manage, path, d => d.GetDirectories( path, searchPattern, searchOption ) );

        // This method helps to handle cases where the mock returns trailing separator, but BCL does not.
        private static string[] RemoveTrailingSeparators( string[] paths ) => paths.Select( p => p.TrimEnd( Path.DirectorySeparatorChar ) ).ToArray();

        public string[] GetDirectories( string path )
            => this._directory.Execute( ExecutionKind.Manage, path, d => RemoveTrailingSeparators( d.GetDirectories( path ) ) );

        public string[] GetDirectories( string path, string searchPattern )
            => this._directory.Execute( ExecutionKind.Manage, path, d => RemoveTrailingSeparators( d.GetDirectories( path, searchPattern ) ) );

        public string[] GetDirectories( string path, string searchPattern, SearchOption searchOption )
            => this._directory.Execute( ExecutionKind.Manage, path, d => RemoveTrailingSeparators( d.GetDirectories( path, searchPattern, searchOption ) ) );

        public void CreateDirectory( string path ) => this._directory.Execute( ExecutionKind.Write, path, d => d.CreateDirectory( path ) );

        public Stream Open( string path, FileMode mode ) => this._file.Execute( ExecutionKind.Write, path, f => f.Open( path, mode ) );

        public Stream Open( string path, FileMode mode, FileAccess access )
            => this._file.Execute( access == FileAccess.Read ? ExecutionKind.Read : ExecutionKind.Write, path, f => f.Open( path, mode, access ) );

        public Stream Open( string path, FileMode mode, FileAccess access, FileShare share )
            => this._file.Execute( access == FileAccess.Read ? ExecutionKind.Read : ExecutionKind.Write, path, f => f.Open( path, mode, access, share ) );

        public Stream OpenRead( string path ) => this._file.Execute( ExecutionKind.Read, path, f => f.OpenRead( path ) );

        public Stream OpenWrite( string path ) => this._file.Execute( ExecutionKind.Write, path, f => f.OpenWrite( path ) );

        public byte[] ReadAllBytes( string path ) => this._file.Execute( ExecutionKind.Read, path, f => f.ReadAllBytes( path ) );

        public void WriteAllBytes( string path, byte[] bytes ) => this._file.Execute( ExecutionKind.Write, path, f => f.WriteAllBytes( path, bytes ) );

        public string ReadAllText( string path ) => this._file.Execute( ExecutionKind.Read, path, f => f.ReadAllText( path ) );

        public void WriteAllText( string path, string content ) => this._file.Execute( ExecutionKind.Write, path, f => f.WriteAllText( path, content ) );

        public string[] ReadAllLines( string path ) => this._file.Execute( ExecutionKind.Read, path, f => f.ReadAllLines( path ) );

        public void WriteAllLines( string path, string[] content ) => this._file.Execute( ExecutionKind.Write, path, f => f.WriteAllLines( path, content ) );

        public void WriteAllLines( string path, IEnumerable<string> content )
            => this._file.Execute( ExecutionKind.Write, path, f => f.WriteAllLines( path, content ) );

        public void MoveFile( string sourceFileName, string destFileName )
            => this._file.Execute( ExecutionKind.Manage, destFileName, f => f.Move( sourceFileName, destFileName ) );

        public void DeleteFile( string path ) => this._file.Execute( ExecutionKind.Manage, path, f => f.Delete( path ) );

        public void MoveDirectory( string sourceDirName, string destDirName )
            => this._directory.Execute( ExecutionKind.Manage, destDirName, d => d.Move( sourceDirName, destDirName ) );

        public void DeleteDirectory( string path, bool recursive ) => this._directory.Execute( ExecutionKind.Manage, path, d => d.Delete( path, recursive ) );

        public bool IsDirectoryEmpty( string path ) => this._directory.Execute( ExecutionKind.Read, path, d => !d.EnumerateFileSystemEntries( path ).Any() );
    }
}