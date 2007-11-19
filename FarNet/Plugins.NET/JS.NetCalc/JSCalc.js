import FarManager
import System.Reflection
public class JSCalc extends BasePlugin{
	function item_OnOpen(sender:Object, e:PluginMenuEventArgs) {
		var InpBox = Far.CreateInputBox();
		InpBox.Prompt = "Enter expression";
		InpBox.History = Assembly.GetExecutingAssembly().Location;
		if(InpBox.Show()){
			var Result = 0;
			eval("Result=" + InpBox.Text);
			Far.Msg( "Result=" + Result );
		};
	}
	function Connect(){
		Far.RegisterPluginsMenuItem("Hello js.net", this.item_OnOpen);
	}
}
