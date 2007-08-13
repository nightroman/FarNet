import FarManager;
import System;
public class BaanWordDiv extends BasePlugin{
	function Connect(){			
		Far.AnyEditor.add_AfterOpen(AnyEditor_AfterOpen)
	}
	function AnyEditor_AfterOpen(sender:Object, e:EventArgs){
		if(isBaanFile())
			Far.Editor.WordDiv=Far.Editor.WordDiv.replace(/\./,"");
	}
	function isBaanFile(){
		var s//:String
			=Far.Editor.FileName.replace(/^.*\\([^\\]*)$/ig,"$1");
		return /(\.cln$)|(^(p|i|r)tfzz(y|z|t))/ig.test(s);
	}
}
