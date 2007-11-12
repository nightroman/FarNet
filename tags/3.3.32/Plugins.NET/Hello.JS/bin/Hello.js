import FarManager;
public class HelloJS extends BasePlugin{
	function item_OnOpen(sender:Object, e:OpenPluginMenuItemEventArgs) {
		Far.Msg("Hello, world from js.net");
	}
	function sayHello(s:String){
		Far.Msg("(JS.Net)Hello, "+s);
	}
	function Connect(){
		Far.RegisterPluginsMenuItem("Hello js.net", this.item_OnOpen);
		Far.RegisterPrefix("hello", this.sayHello);
	}
}
