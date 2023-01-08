using Metalama.Backstage.Extensibility;

internal class TestToolApplicationInfo : ApplicationInfoBase
{
    public TestToolApplicationInfo() : base( typeof(TestToolApplicationInfo).Assembly ) { }

    public override string Name => typeof(TestToolApplicationInfo).Assembly.GetName().Name!;
}