// Copyright (c) SharpCrafters s.r.o. This file is not open source. It is released under a commercial
// source-available license. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace PostSharp.Backstage.Licensing
{
    internal class RegistryChangeMonitor : IDisposable
    {
        private IntPtr hKey;
        private readonly AutoResetEvent changedEvent = new AutoResetEvent(false);
        public RegistryChangeMonitor( UIntPtr parent, string keyName, bool watchSubtree )
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                throw new PlatformNotSupportedException();

            if ( RegOpenKeyEx( parent, keyName, 0, KEY_QUERY_VALUE | KEY_NOTIFY, out this.hKey ) != 0 )
            {
                this.hKey = IntPtr.Zero;
            }
            else
            {
                _ = RegNotifyChangeKeyValue( this.hKey, watchSubtree, REG_NOTIFY_CHANGE.LAST_SET | REG_NOTIFY_CHANGE.NAME,
                                         this.changedEvent.SafeWaitHandle.DangerousGetHandle(), true );
            }

            this.registeredWaitHandle = ThreadPool.RegisterWaitForSingleObject( this.changedEvent, this.CallBack, null, -1, false );

        }

        private void CallBack( object state, bool timedOut )
        {
            
            if ( this.Changed != null )
            {
                this.Changed(this, EventArgs.Empty);
            }
        }

        private void DisposeImpl()
        {
            if ( this.registeredWaitHandle != null )
            {
                this.registeredWaitHandle.Unregister( this.changedEvent );
                this.registeredWaitHandle = null;
            }

            if (this.hKey != IntPtr.Zero)
            {
                _ = RegCloseKey(this.hKey);
                this.hKey = IntPtr.Zero;
            }

            this.changedEvent.Close();
        }

        public event EventHandler Changed;

        public void Dispose()
        {
            this.DisposeImpl();
            GC.SuppressFinalize( this );
        }

        ~RegistryChangeMonitor()
        {
            this.DisposeImpl();
        }

        [DllImport("Advapi32.dll")]
        private static extern int RegNotifyChangeKeyValue(
            IntPtr hKey,
            bool watchSubtree,
            REG_NOTIFY_CHANGE notifyFilter,
            IntPtr hEvent,
            bool asynchronous
            );

        [Flags]
        private enum REG_NOTIFY_CHANGE : uint
        {
            /// <summary>
            /// Notify the caller if a subkey is added or deleted
            /// </summary>
            NAME = 0x1,
            /// <summary>
            /// Notify the caller of changes to the attributes of the key,
            /// such as the security descriptor information
            /// </summary>
            ATTRIBUTES = 0x2,
            /// <summary>
            /// Notify the caller of changes to a value of the key. This can
            /// include adding or deleting a value, or changing an existing value
            /// </summary>
            LAST_SET = 0x4,
            /// <summary>
            /// Notify the caller of changes to the security descriptor of the key
            /// </summary>
            SECURITY = 0x8
        }

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode)]
        private static extern int RegOpenKeyEx(
            UIntPtr hKey,
            [MarshalAs(UnmanagedType.LPWStr)] string subKey,
            int ulOptions,
            int samDesired,
            out IntPtr hkResult);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto)]
        private static extern int RegCloseKey(IntPtr hKey);


        public static UIntPtr HKEY_LOCAL_MACHINE = new UIntPtr(0x80000002u);
        public static UIntPtr HKEY_CURRENT_USER = new UIntPtr(0x80000001u);
        private RegisteredWaitHandle registeredWaitHandle;

        public const int KEY_QUERY_VALUE = 0x1;
        public const int KEY_SET_VALUE = 0x2;
        public const int KEY_CREATE_SUB_KEY = 0x4;
        public const int KEY_ENUMERATE_SUB_KEYS = 0x8;
        public const int KEY_NOTIFY = 0x10;
        public const int KEY_CREATE_LINK = 0x20;
        public const int KEY_WOW64_32KEY = 0x200;
        public const int KEY_WOW64_64KEY = 0x100;
        public const int KEY_WOW64_RES = 0x300;

    }
}
