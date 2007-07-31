import FarManager;
public class ReplaceSlashes extends BasePlugin{
	var menuItem:IPluginMenuItem;
		function item_OnOpen(sender:Object, e:OpenPluginMenuItemEventArgs) {
			if(e.From==OpenFrom.Editor)
				Far.Editor.Selection.SetText(
					Far.Editor.Selection.GetText().replace(/\\/ig, "\\\\"));
			else
				Far.Msg("It works only under editor");
		}
		function Connect(){
			this.menuItem=Far.CreatePluginsMenuItem();
			this.menuItem.Name="Replace slashes";
			this.menuItem.add_OnOpen(this.item_OnOpen);
			Far.RegisterPluginsMenuItem(this.menuItem);
		}
		function Disconnect() {
			Far.UnregisterPluginsMenuItem(this.menuItem);
		}
}