using FarManager;
public class HelloCS : BasePlugin
{
	void item_OnOpen(object sender, OpenPluginMenuItemEventArgs e)
	{
		Far.Msg("Hello, world from c#", "Far.NET");
	}
	void sayHello(string s)
	{
		Far.Msg("C#: Hello, " + s);
	}
	override public void Connect()
	{
		Far.RegisterPluginsMenuItem("Hello c#", item_OnOpen);
		Far.RegisterPrefix("hellocs", sayHello);
	}
}
