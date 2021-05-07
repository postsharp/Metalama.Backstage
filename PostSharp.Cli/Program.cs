using System.CommandLine;
using System.Threading.Tasks;
using PostSharp.Cli.Commands.Licensing;

namespace PostSharp.Cli
{
    internal class Program
    {
        private static Task Main( string[] args )
        {
            // TODO: Description?
            RootCommand rootCommand = new();

            rootCommand.Add( new LicenseCommand(null, null) );

            return rootCommand.InvokeAsync( args );
        }
    }
}
