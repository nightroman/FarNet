import FarManager
import System.Reflection
public class JSCalc extends BasePlugin{
	var menuItem:IPluginMenuItem;	
		function item_OnOpen(sender:Object, e:OpenPluginMenuItemEventArgs) {
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
			this.menuItem=Far.CreatePluginsMenuItem();
			this.menuItem.Name="Hello js.net";
			this.menuItem.add_OnOpen(this.item_OnOpen);
			Far.RegisterPluginsMenuItem(this.menuItem);
		}
		function Disconnect() {
			Far.UnregisterPluginsMenuItem(this.menuItem);
		}
}