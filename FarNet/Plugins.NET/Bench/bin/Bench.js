import FarManager;
public class Bench extends BasePlugin{
	var menuItem:IPluginMenuItem;
	function item_OnOpen(sender:Object, e:OpenPluginMenuItemEventArgs) {
		if(e.From==OpenFrom.Editor){
			this.measure(this.testLineAccess, "String access");
		}
	}
	function testLineAccess(){
		var t=Far.Editor.Lines.Item[0];
		var i;
		for(i=0;i<10000;i++){
			var s=t.Text;
		}
	}
	function measure(f, desc){
		var start=new Date().getTime()
		f();
		Far.Msg(desc+":"+new String((new Date().getTime())-start))
	}
	function Connect(){
		this.menuItem=Far.CreatePluginsMenuItem();
		this.menuItem.Name="Bench.net";
		this.menuItem.add_OnOpen(this.item_OnOpen);
		Far.RegisterPluginsMenuItem(this.menuItem);
	}
}
