import FarManager;
public class ReplaceSlashes extends BasePlugin {
	function item_OnOpen(sender:Object, e:OpenPluginMenuItemEventArgs) {
		if (e.From==OpenFrom.Editor)
		Far.Editor.Selection.SetText(Far.Editor.Selection.GetText().replace(/\\/ig, "\\\\"));
		else
		Far.Msg("It works only under editor");
	}
	function Connect() {
		var menuItem:IPluginMenuItem=Far.CreatePluginsMenuItem();
		menuItem.Name="Replace slashes";
		menuItem.add_OnOpen(this.item_OnOpen);
		Far.RegisterPluginsMenuItem(menuItem);
	}
}
