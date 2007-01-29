import FarManager;
public class HelloJS extends BasePlugin{
	var menuItem:IPluginMenuItem;	
		function item_OnOpen(sender:Object, e:OpenPluginMenuItemEventArgs) {
			Far.Msg("Hello, world from js.net");
		}
		function sayHello(s:String){
			Far.Msg("(JS.Net)Hello, "+s);
		}
		function Connect(){			
			this.menuItem=Far.CreatePluginsMenuItem();
			this.menuItem.Name="Hello js.net";
			this.menuItem.add_OnOpen(this.item_OnOpen);
			Far.RegisterPluginsMenuItem(this.menuItem);
			Far.RegisterPrefix("hello", this.sayHello);
		}
		function Disconnect() {
			Far.UnregisterPluginsMenuItem(this.menuItem);
		}
}