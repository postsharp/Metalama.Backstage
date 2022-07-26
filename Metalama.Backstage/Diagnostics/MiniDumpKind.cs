// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;

namespace Metalama.Backstage.Diagnostics;

[Flags]
public enum MiniDumpKind : uint
{
    // From dbghelp.h.
    // https://docs.microsoft.com/en-us/windows/win32/api/minidumpapiset/ne-minidumpapiset-minidump_type

    Normal = 0x00000000,
    WithDataSegments = 0x00000001,
    WithFullMemory = 0x00000002,
    WithHandleData = 0x00000004,
    FilterMemory = 0x00000008,
    ScanMemory = 0x00000010,
    WithUnloadedModules = 0x00000020,
    WithIndirectlyReferencedMemory = 0x00000040,
    FilterModulePaths = 0x00000080,
    WithProcessThreadData = 0x00000100,
    WithPrivateReadWriteMemory = 0x00000200,
    WithoutOptionalData = 0x00000400,
    WithFullMemoryInfo = 0x00000800,
    WithThreadInfo = 0x00001000,
    WithCodeSegments = 0x00002000,
    WithoutAuxiliaryState = 0x00004000,
    WithFullAuxiliaryState = 0x00008000,
    
    // The following flags have been introduced in  DbgHelp 6.1
    
    // WithPrivateWriteCopyMemory = 0x00010000,
    // IgnoreInaccessibleMemory = 0x00020000,
    // ValidTypeFlags = 0x0003ffff
}