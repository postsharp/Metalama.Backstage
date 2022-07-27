// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this rep root for details.

using Metalama.Backstage.Configuration;
using Metalama.Backstage.Testing;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Metalama.Backstage.Licensing.Tests.ConfigurationManager;

public class ConfigurationManagerTests : TestsBase
{
    public ConfigurationManagerTests( ITestOutputHelper logger ) : base( logger ) { }

    [Fact]
    public async Task ConcurrentUpdate()
    {
        for ( var i = 0; i < 50; i++ )
        {
            this.Logger.WriteLine( $"------------ {i + 1} ------------- " );
            var configurationManager = new Configuration.ConfigurationManager( this.ServiceProvider );
            configurationManager.Update<TestConfigurationFile>( f => f.IsModified = false );

            bool UpdateImpl()
            {
                var myConfigurationManager = new Configuration.ConfigurationManager( this.ServiceProvider );

                return myConfigurationManager.UpdateIf<TestConfigurationFile>( f => !f.IsModified, f => f.IsModified = true );
            }

            var tasks = new[] { Task.Run( UpdateImpl ), Task.Run( UpdateImpl ) };

            await Task.WhenAll( tasks );

            var countTrue = tasks.Count( t => t.Result );
            Assert.Equal( 1, countTrue );
        }
    }
}