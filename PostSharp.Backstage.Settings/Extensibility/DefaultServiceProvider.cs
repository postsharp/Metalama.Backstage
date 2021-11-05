// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using PostSharp.Backstage.Licensing.Registration;
using PostSharp.Backstage.Licensing.Registration.Evaluation;

namespace PostSharp.Backstage.Extensibility
{
    public class DefaultServiceProvider : DictionaryServiceProvider
    {
        public DefaultServiceProvider()
        {
            this.AddService<IDateTimeProvider>( new CurrentDateTimeProvider() );
            this.AddService<IFileSystem>( new FileSystem() );
            this.AddService<IStandardDirectories>( new StandardDirectories() );
            
            this.AddService<IStandardLicenseFileLocations>( new StandardLicenseFilesLocations( this ) );
            this.AddService<IEvaluationLicenseFilesLocations>( new EvaluationLicenseFilesLocations( this ) );
        }
    }
}