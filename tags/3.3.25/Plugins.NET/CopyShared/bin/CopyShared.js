import FarManager
import System.Reflection
import System.IO
public class CopyShared extends BasePlugin{
	var menuItem:IPluginMenuItem;
	var shares;
	function item_OnOpen(sender:Object, e:OpenPluginMenuItemEventArgs) {
		this.copySelected();
	}
	function Connect(){
		this.readCfg()
		this.menuItem=Far.CreatePluginsMenuItem();
		this.menuItem.Name="Copy Sha&red";
		this.menuItem.add_OnOpen(this.item_OnOpen);
		Far.RegisterPluginsMenuItem(this.menuItem);
	}
	function GetExternalPath(CmdLine){
		for(var folder in shares)
		if(startsWith(CmdLine.toLowerCase(), folder))
		return (shares[folder]+"\\"+CmdLine.substr(folder.length+1)).replace(/ /ig, "%20");
	}
	function startsWith(s, s1){
		// var s // : String
		// var s1 // : String
		return s.substr(0, s1.length).toLowerCase()==s1.toLowerCase();
	}
	function copySelected(){
		if(Far.Panel.Selected.Count==0){
			Far.Clipboard=(GetExternalPath(Far.Panel.Path+"\\"+Far.Panel.Current.Name));
		}else{
			var s="";
			for(var item in Far.Panel.Selected)
			s+="\r\n"+(this.GetExternalPath(item.Path));

			Far.Clipboard=s;
		}
	}
	function PluginFile(s){
		var fi=new FileInfo(Assembly.GetExecutingAssembly().Location);
		return fi.Directory.Parent.FullName+"\\"+s;
	}
	function readCfg(){
		this.shares=[];
		var cfg=File.OpenText(this.PluginFile("cfg\\shares.cfg"));
		var name
		while((name=cfg.ReadLine())!=null){
			var value=cfg.ReadLine();
			shares[name]=value;
		}
		cfg.Close();
	}
}
