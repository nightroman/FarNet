import FarManager;
import System;
public class Megatest extends BasePlugin{
	function item_OnOpen(sender:Object, e:PluginMenuEventArgs) {
		var menu=Far.CreateMenu();
		menu.Title="Select a test";
		menu.Items.Add("Message", testMessage);
		menu.Items.Add("Menu", testMenu);
		menu.Items.Add("InputBox", testInputBox);
		menu.Items.Add("KeySequence", testKeySequence);
		menu.Items.Add("Panel", testPanel);
		menu.Items.Add("Exception", testException);
		if(e.From==OpenFrom.Editor)
		menu.Items.Add("Editor", testEditor);
		menu.Show();
	}
	function Connect(){
		Far.RegisterPluginsMenuItem("Run Far.NET tests", this.item_OnOpen);
	}
	function testInputBox(sender:Object, eventArgs:EventArgs){
		var ib=Far.CreateInputBox();
		ib.Title="Test input box";
		ib.Prompt="Enter text";
		Far.Msg(ib.Show()?("Entered:"+ib.Text):"Canceled");
	}
	function testKeySequence(sender:Object, eventArgs:EventArgs){
		Far.PostKeySequence(Far.CreateKeySequence('t e s t'), false);
	}
	function testPanel(sender:Object, eventArgs:EventArgs){
		for(var file in Far.Panel.Contents){
			Far.Msg(file.Name);
		}
	}
	function testEditor(sender:Object, eventArgs:EventArgs){
		Far.Msg("Test Editor");
		Far.Msg("FileName="+Far.Editor.FileName);
		var line:ILine=Far.Editor.Lines.Item[0];
		var text=line.Text;
		Far.Msg("Line[0].Text="+text);
	}
	function testException(sender:Object, eventArgs:EventArgs){
		throw new Exception("Test exception");
	}
	function testMenu(sender:Object, eventArgs:EventArgs){
		var m=Far.CreateMenu();
		m.Items.Add("Test Checked").Checked=true;
		m.Items.Add("Test IsSeparator").IsSeparator=true;
		m.Items.Add("Test Selected");
		m.Selected=2
		m.Show();
		Far.Msg(m.Selected);
	}
	function testMessage(sender:Object, eventArgs:EventArgs){
		var m=Far.CreateMessage();
		m.Body.Add("Test")
		m.Body.Add("Warning Message")
		m.Buttons.Add("Ok")
		m.Buttons.Add("Retry")
		m.Buttons.Add("Cancel")
		m.IsWarning=true;
		m.Show();
	}
}
