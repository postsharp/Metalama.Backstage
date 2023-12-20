using Metalama.Backstage.Commands;

namespace Metalama.Backstage.Tool;

internal class ThrowCommand : BaseCommand<BaseCommandSettings>
{
    protected override void Execute( ExtendedCommandContext context, BaseCommandSettings settings )
        => throw new InvalidOperationException( "This exception is intentional." );
}