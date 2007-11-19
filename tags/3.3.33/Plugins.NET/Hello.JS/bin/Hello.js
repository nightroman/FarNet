import FarManager;
public class HelloJS extends BasePlugin{
	function item_OnOpen(sender:Object, e:PluginMenuEventArgs) {
		Far.Msg("Hello, world from js.net");
	}
	function sayHello(sender:Object, e:ExecutingEventArgs){
		Far.Msg("(JS.Net)Hello, "+e.Command);
	}
	function Connect(){
		Far.RegisterPluginsMenuItem("Hello js.net", this.item_OnOpen);
		Far.RegisterPrefix("hello", this.sayHello);
	}
}
