// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Metalama.Backstage.Extensibility
{
    // TODO: Add missing methods.

    /// <summary>
    /// Provides access to file system.
    /// </summary>
    public interface IFileSystem
    {
        /// <summary>
        /// Returns the date and time the specified file or directory was last written to.
        /// </summary>
        /// <param name="path">The file or directory for which to obtain write date and time information.</param>
        /// <returns>
        /// A <see cref="DateTime" /> structure set to the date and time that the specified file
        /// or directory was last written to. This value is expressed in local time.
        /// </returns>
        DateTime GetLastWriteTime( string path );

        /// <summary>
        /// Determines whether the specified file exists.
        /// </summary>
        /// <param name="path">The file to check.</param>
        /// <returns>
        /// <c>true</c> if <paramref name="path"/> refers to an existing file;
        /// <c>false</c> if the file does not exist
        /// or an error occurs when trying to determine if the specified file exists.
        /// </returns>
        bool FileExists( string path );

        /// <summary>
        /// Determines whether the given path refers to an existing directory on disk.
        /// </summary>
        /// <param name="path">The directory to check.</param>
        /// <returns>
        /// <c>true</c> if <paramref name="path"/> refers to an existing directory;
        /// <c>false</c> if the directory does not exist
        /// or an error occurs when trying to determine if the specified directory exists.
        /// </returns>
        bool DirectoryExists( string path );

        /// <summary>
        /// Returns an array of file names that match a search pattern
        /// in a specified path, and optionally searches subdirectories.
        /// </summary>
        /// <param name="path">
        /// The relative or absolute path to the directory to search.
        /// This string is not case-sensitive.
        /// </param>
        /// <param name="searchPattern">
        /// The search string to match against the names of files in path.
        /// This parameter can contain a combination of valid literal path
        /// and wild-card (* and ?) characters.
        /// It doesn't support regular expressions.
        /// </param>
        /// <param name="searchOption">
        /// One of the enumeration values that specify whether the search operation
        /// should include only the current directory or all subdirectories.
        /// The default value is <see cref="SearchOption.TopDirectoryOnly" />.
        /// </param>
        /// <returns>
        /// An array of the full names (including paths)
        /// of the files in the directory specified by <paramref name="path"/>
        /// and that match the specified <paramref name="searchPattern"/>
        /// and <paramref name="searchOption"/>.
        /// </returns>
        string[] GetFiles( string path, string? searchPattern = null, SearchOption? searchOption = null );

        /// <summary>
        /// Returns an enumerable collection of file names that match a search pattern
        /// in a specified path, and optionally searches subdirectories.
        /// </summary>
        /// <param name="path">
        /// The relative or absolute path to the directory to search.
        /// This string is not case-sensitive.
        /// </param>
        /// <param name="searchPattern">
        /// The search string to match against the names of files in path.
        /// This parameter can contain a combination of valid literal path
        /// and wild-card (* and ?) characters.
        /// It doesn't support regular expressions.
        /// </param>
        /// <param name="searchOption">
        /// One of the enumeration values that specify whether the search operation
        /// should include only the current directory or all subdirectories.
        /// The default value is <see cref="SearchOption.TopDirectoryOnly" />.
        /// </param>
        /// <returns>
        /// An enumerable collection of the full names (including paths)
        /// of the files in the directory specified by <paramref name="path"/>
        /// and that match the specified <paramref name="searchPattern"/>
        /// and <paramref name="searchOption"/>.
        /// </returns>
        IEnumerable<string> EnumerateFiles(
            string path,
            string? searchPattern = null,
            SearchOption? searchOption = null );

        /// <summary>
        /// Returns an array of directory names that match a search pattern
        /// in a specified path, and optionally searches subdirectories.
        /// </summary>
        /// <param name="path">
        /// The relative or absolute path to the directory to search.
        /// This string is not case-sensitive.
        /// </param>
        /// <param name="searchPattern">
        /// The search string to match against the names of directories in path.
        /// This parameter can contain a combination of valid literal path
        /// and wild-card (* and ?) characters.
        /// It doesn't support regular expressions.
        /// </param>
        /// <param name="searchOption">
        /// One of the enumeration values that specify whether the search operation
        /// should include only the current directory or all subdirectories.
        /// The default value is <see cref="SearchOption.TopDirectoryOnly" />.
        /// </param>
        /// <returns>
        /// An array of the full names (including paths)
        /// of the directories in the directory specified by <paramref name="path"/>
        /// and that match the specified <paramref name="searchPattern"/>
        /// and <paramref name="searchOption"/>.
        /// </returns>
        string[] GetDirectories( string path, string? searchPattern = null, SearchOption? searchOption = null );

        /// <summary>
        /// Returns an enumerable collection of directory names that match a search pattern
        /// in a specified path, and optionally searches subdirectories.
        /// </summary>
        /// <param name="path">
        /// The relative or absolute path to the directory to search.
        /// This string is not case-sensitive.
        /// </param>
        /// <param name="searchPattern">
        /// The search string to match against the names of directories in path.
        /// This parameter can contain a combination of valid literal path
        /// and wild-card (* and ?) characters.
        /// It doesn't support regular expressions.
        /// </param>
        /// <param name="searchOption">
        /// One of the enumeration values that specify whether the search operation
        /// should include only the current directory or all subdirectories.
        /// The default value is <see cref="SearchOption.TopDirectoryOnly" />.
        /// </param>
        /// <returns>
        /// An enumerable collection of the full names (including paths)
        /// of the directories in the directory specified by <paramref name="path"/>
        /// and that match the specified <paramref name="searchPattern"/>
        /// and <paramref name="searchOption"/>.
        /// </returns>
        IEnumerable<string> EnumerateDirectories(
            string path,
            string? searchPattern = null,
            SearchOption? searchOption = null );

        /// <summary>
        /// Creates all directories and subdirectories in the specified path unless they already exist.
        /// </summary>
        /// <param name="path">The directory to create.</param>
        void CreateDirectory( string path );

        /// <summary>
        /// Creates or overwrites a file in the specified path.
        /// </summary>
        /// <param name="path">The path and name of the file to create.</param>
        Stream CreateFile( string path );

        /// <summary>
        /// Creates a uniquely named, zero-byte temporary file on disk
        /// and returns the full path of that file.
        /// </summary>
        /// <returns>The full path of the temporary file.</returns>
        string GetTempFileName();

        /// <summary>
        /// Opens a file on the specified path with read/write access.
        /// </summary>
        /// <param name="path">The file to open.</param>
        /// <param name="mode">
        /// A <see cref="FileMode" /> value that specifies whether a file is created
        /// if one does not exist, and determines whether the contents of existing files
        /// are retained or overwritten.
        /// </param>
        Stream Open( string path, FileMode mode );

        /// <summary>
        /// Opens a file on the specified path with read/write access.
        /// </summary>
        /// <param name="path">The file to open.</param>
        /// <param name="mode">
        /// A <see cref="FileMode" /> value that specifies whether a file is created
        /// if one does not exist, and determines whether the contents of existing files
        /// are retained or overwritten.
        /// </param>
        /// <param name="access">
        /// A <see cref="FileAccess" /> value that specifies the operations that
        /// can be performed on the file.
        /// </param>
        Stream Open( string path, FileMode mode, FileAccess access );

        /// <summary>
        /// Opens a file on the specified path with read/write access.
        /// </summary>
        /// <param name="path">The file to open.</param>
        /// <param name="mode">
        /// A <see cref="FileMode" /> value that specifies whether a file is created
        /// if one does not exist, and determines whether the contents of existing files
        /// are retained or overwritten.
        /// </param>
        /// <param name="access">
        /// A <see cref="FileAccess" /> value that specifies the operations
        /// that can be performed on the file.
        /// </param>
        /// <param name="share">
        /// A <see cref="FileShare" /> value specifying the type
        /// of access other threads have to the file.
        /// </param>
        Stream Open( string path, FileMode mode, FileAccess access, FileShare share );

        /// <summary>
        /// Opens an existing file for reading.
        /// </summary>
        /// <param name="path">The file to be opened for reading.</param>
        /// <returns>A read-only <see cref="Stream" /> on the specified path.</returns>
        Stream OpenRead( string path );

        /// <summary>
        /// Opens an existing file or creates a new file for writing.
        /// </summary>
        /// <param name="path">The file to be opened for writing.</param>
        /// <returns>An unshared writable <see cref="Stream" /> object on the specified path.</returns>
        Stream OpenWrite( string path );

        /// <summary>
        /// Opens a binary file, reads the contents of the file into a byte array, and then closes the file.
        /// </summary>
        /// <param name="path">The file to read from.</param>
        /// <returns>A <see cref="byte" /> array containing the contents of the file.</returns>
        byte[] ReadAllBytes( string path );

        /// <summary>
        /// Creates a new file, writes the specified byte array to the file, and then closes the file.
        /// If the target file already exists, it is overwritten.
        /// </summary>
        /// <param name="path">The file to write to.</param>
        /// <param name="bytes">The bytes to write to the file.</param>
        void WriteAllBytes( string path, byte[] bytes );

        /// <summary>
        /// Opens a text file, reads all text of the file, and then closes the file.
        /// </summary>
        /// <param name="path">The file to read from.</param>
        /// <returns>A string containing all lines of the file.</returns>
        string ReadAllText( string path );

        /// <summary>
        /// Creates a new file, writes the specified string to the file, and then closes the file.
        /// If the target file already exists, it is overwritten.
        /// </summary>
        /// <param name="path">The file to write to.</param>
        /// <param name="content">The string to write to the file.</param>
        void WriteAllText( string path, string content );

        /// <summary>
        /// Creates a new file, writes the specified string to the file using the specified
        /// encoding, and then closes the file. If the target file already exists, it is overwritten.
        /// </summary>
        /// <param name="path">The file to write to.</param>
        /// <param name="content">The string to write to the file.</param>
        /// <param name="encoding">The encoding to apply to the string.</param>
        void WriteAllText( string path, string content, Encoding encoding );

        /// <summary>
        /// Opens a text file, reads all lines of the file, and then closes the file.
        /// </summary>
        /// <param name="path">The file to read from.</param>
        /// <returns>A <see cref="string" /> array containing all lines of the file.</returns>
        string[] ReadAllLines( string path );

        /// <summary>
        /// Creates a new file, writes the specified string to the file, and then closes the file.
        /// If the target file already exists, it is overwritten.
        /// </summary>
        /// <param name="path">The file to write to.</param>
        /// <param name="content">The lines to write to the file.</param>
        void WriteAllLines( string path, string[] content );

        /// <summary>
        /// Creates a new file, writes the specified string to the file, and then closes the file.
        /// If the target file already exists, it is overwritten.
        /// </summary>
        /// <param name="path">The file to write to.</param>
        /// <param name="content">The lines to write to the file.</param>
        void WriteAllLines( string path, IEnumerable<string> content );

        /// <summary>
        /// Moves a specified file to a new location, providing the option
        /// to specify a new file name.
        /// </summary>
        /// <param name="sourceFileName">The name of the file to move. Can include a relative or absolute path.</param>
        /// <param name="destinationFileName">The new path and name for the file.</param>
        void MoveFile( string sourceFileName, string destinationFileName );

        /// <summary>
        /// Deletes the specified file.
        /// </summary>
        /// <param name="path">The name of the file to be deleted. Wildcard characters are not supported.</param>
        void DeleteFile( string path );
    }
}