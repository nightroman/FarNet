using FarManager;
public class HelloCS : BasePlugin
{
	void item_OnOpen(object sender, PluginMenuEventArgs e)
	{
		Far.Msg("Hello, world from c#", "Far.NET");
	}
	void sayHello(object sender, ExecutingEventArgs e)
	{
		Far.Msg("C#: Hello, " + e.Command);
	}
	override public void Connect()
	{
		Far.RegisterPluginsMenuItem("Hello c#", item_OnOpen);
		Far.RegisterPrefix("hellocs", sayHello);
	}
}
