using FarManager;
public class HelloCS : BasePlugin
{
	IPluginMenuItem menuItem;
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
		this.menuItem = Far.CreatePluginsMenuItem();
		this.menuItem.Name = "Hello c#";
		this.menuItem.OnOpen += item_OnOpen;
		Far.RegisterPluginsMenuItem(this.menuItem);

		Far.RegisterPrefix("hellocs", new StringDelegate(sayHello));
	}
	override public void Disconnect()
	{
		Far.UnregisterPluginsMenuItem(this.menuItem);
	}
}
