// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;

namespace PostSharp.Backstage.Licensing
{
    public abstract class LicenseSourceMonitor : IDisposable
    {
        private bool _disposedValue;

        public event EventHandler Changed;

        public abstract void Start();

        public abstract void Stop();

        protected virtual void Dispose( bool disposing )
        {
            if ( !_disposedValue )
            {
                if ( disposing )
                {
                    this.Stop();
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            this.Dispose( disposing: true );
            GC.SuppressFinalize( this );
        }
    }
}
