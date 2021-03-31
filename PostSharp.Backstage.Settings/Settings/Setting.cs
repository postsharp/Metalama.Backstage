// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace PostSharp.Backstage.Settings
{
    public abstract class Setting
    {
        private readonly SettingsReader _reader;

        protected Setting( SettingsReader reader )
        {
            this._reader = reader;
        }

        public void Refresh( SettingsReader? reader = null )
        {
            if ( reader != null )
            {
                try
                {
                    this.RefreshImpl( reader );
                }
                catch
                {
                }
            }
            else
            {
                this._reader.Open();

                try
                {
                    this.RefreshImpl( this._reader );
                }
                catch
                {
                }
                finally
                {
                    this._reader.Close();
                }
            }
        }

        protected abstract void RefreshImpl( SettingsReader reader );
    }

    public abstract class Setting<T> : Setting
    {
        private readonly SettingsWriter _writer;

        public string Name { get; }

        public T? DefaultValue { get; }

        public T? Value { get; protected set; }

        public bool HasValue => this.Value == null;

        protected Setting( string name, SettingsReader reader, SettingsWriter writer, T? defaultValue = default )
            : base( reader )
        {
            this.Name = name;
            this.DefaultValue = defaultValue;
            this.DefaultValue = defaultValue;
            this._writer = writer;
        }

        public void Set( T value, SettingsWriter? writer = null )
        {
            if ( (value == null && this.Value == null) || (this.Value?.Equals( value ) ?? false) )
            {
                return;
            }

            this.Value = value;

            if ( writer != null )
            {
                try
                {
                    this.SetImpl( value, writer );
                }
                catch
                {
                }
            }
            else
            {
                this._writer.Open();

                try
                {
                    this.SetImpl( value, this._writer );
                }
                catch
                {
                }
                finally
                {
                    this._writer.Close();
                }
            }
        }

        protected abstract void SetImpl( T value, SettingsWriter writer );

        public static implicit operator T( Setting<T> setting ) => setting.Value;
    }
}
