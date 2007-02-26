import FarManager;
public class HelloJS extends BasePlugin{
	var menuItem;
	function item_OnOpen(sender:Object, e:OpenPluginMenuItemEventArgs) {
		Far.Msg("Hello, world from js.net");
	}
	function Connect(){			
		this.menuItem=Far.RegisterPluginsMenuItem("Hello js.net", this.item_OnOpen);
	}
	function Disconnect() {
		Far.UnregisterPluginsMenuItem(this.menuItem);
	}
}