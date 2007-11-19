import FarManager;
public class ReplaceSlashes extends BasePlugin {
	function item_OnOpen(sender:Object, e:PluginMenuEventArgs) {
		if (e.From==OpenFrom.Editor)
		Far.Editor.Selection.SetText(Far.Editor.Selection.GetText().replace(/\\/ig, "\\\\"));
		else
		Far.Msg("It works only under editor");
	}
	function Connect() {
		Far.RegisterPluginsMenuItem("Replace slashes", this.item_OnOpen);
	}
}
