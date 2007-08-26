import FarManager;
public class HelloJS extends BasePlugin{
	function item_OnOpen(sender:Object, e:OpenPluginMenuItemEventArgs) {
		Far.Msg("Hello, world from js.net");
	}
	function sayHello(s:String){
		Far.Msg("(JS.Net)Hello, "+s);
	}
	function Connect(){
		var menuItem:IPluginMenuItem=Far.CreatePluginsMenuItem();
		menuItem.Name="Hello js.net";
		menuItem.add_OnOpen(this.item_OnOpen);
		Far.RegisterPluginsMenuItem(menuItem);
		Far.RegisterPrefix("hello", this.sayHello);
	}
}
