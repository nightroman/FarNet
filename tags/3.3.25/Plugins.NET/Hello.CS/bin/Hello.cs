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
		IPluginMenuItem menuItem = Far.CreatePluginsMenuItem();
		menuItem.Name = "Hello c#";
		menuItem.OnOpen += item_OnOpen;
		Far.RegisterPluginsMenuItem(menuItem);
		Far.RegisterPrefix("hellocs", new StringDelegate(sayHello));
	}
}
