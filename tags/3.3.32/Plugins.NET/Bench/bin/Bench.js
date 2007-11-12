import FarManager;
public class Bench extends BasePlugin{
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
		Far.RegisterPluginsMenuItem("Bench.net", this.item_OnOpen);
	}
}
