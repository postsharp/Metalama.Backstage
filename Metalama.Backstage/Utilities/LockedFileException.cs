// Copyright (c) SharpCrafters s.r.o. All rights reserved. This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.IO;

namespace Metalama.Backstage.Utilities;

/// <summary>
/// An exception thrown by <see cref="RetryHelper"/> when an operation failed and a process is detected that locks a file.
/// </summary>
public class LockedFileException : IOException
{
    public LockedFileException( string message, Exception innerException ) : base( message, innerException ) { }
}