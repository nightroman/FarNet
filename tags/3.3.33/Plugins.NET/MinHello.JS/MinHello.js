import FarManager;
public class HelloJS extends BasePlugin {
	function item_OnOpen(sender:Object, e:PluginMenuEventArgs) {
		Far.Msg("Hello world!");
	}
	function Connect() {
		Far.RegisterPluginsMenuItem("Hello js.net", this.item_OnOpen);
	}
}
