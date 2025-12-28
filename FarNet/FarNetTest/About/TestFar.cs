using FarNet;
using FarNet.Forms;
using System.Globalization;

namespace FarNetTest.About;
#pragma warning disable CS0067

public class TestFar : IFar
{
	public override Version FarVersion => throw new NotImplementedException();

	public override Version FarNetVersion => throw new NotImplementedException();

	public override IAnyEditor AnyEditor => throw new NotImplementedException();

	public override IAnyViewer AnyViewer => throw new NotImplementedException();

	public override IEditor? Editor => throw new NotImplementedException();

	public override IViewer? Viewer => throw new NotImplementedException();

	public override IPanel? Panel => throw new NotImplementedException();

	public override IPanel? Panel2 => throw new NotImplementedException();

	public override ILine CommandLine => throw new NotImplementedException();

	public override MacroArea MacroArea => throw new NotImplementedException();

	public override MacroState MacroState => throw new NotImplementedException();

	public override IDialog? Dialog => throw new NotImplementedException();

	public override ILine? Line => throw new NotImplementedException();

	public override string CurrentDirectory => AbcTest.TestRoot;

	readonly IWindow _Window = new TestWindow();

	public override event EventHandler<DirectoryChangedEventArgs>? DirectoryChanged;

	public override IWindow Window => _Window;

	public override IUserInterface UI => throw new NotImplementedException();

	public override IHistory History => throw new NotImplementedException();

	public override void CopyToClipboard(string text) => throw new NotImplementedException();

	public override IDialog CreateDialog(int left, int top, int right, int bottom) => throw new NotImplementedException();

	public override IEditor CreateEditor() => throw new NotImplementedException();

	public override IInputBox CreateInputBox() => throw new NotImplementedException();

	public override IListMenu CreateListMenu() => throw new NotImplementedException();

	public override IMenu CreateMenu() => throw new NotImplementedException();

	public override IViewer CreateViewer() => throw new NotImplementedException();

	public override IEditor[] Editors() => throw new NotImplementedException();

	public override CultureInfo GetCurrentUICulture(bool update) => throw new NotImplementedException();

	public override string GetFolderPath(SpecialFolder folder) => throw new NotImplementedException();

	public override IModuleAction GetModuleAction(Guid id) => throw new NotImplementedException();

	public override IModuleManager GetModuleManager(string name) => throw new NotImplementedException();

	public override object GetSetting(FarSetting settingSet, string settingName) => throw new NotImplementedException();

	public override string? Input(string? prompt, string? history, string? title, string? text) => throw new NotImplementedException();

	public override void InvokeCommand(string command) => throw new NotImplementedException();

	public override bool IsMaskMatch(string path, string mask, bool full) => throw new NotImplementedException();

	public override bool IsMaskValid(string mask) => throw new NotImplementedException();

	public override string KeyInfoToName(KeyInfo key) => throw new NotImplementedException();

	public override int Message(MessageArgs args) => throw new NotImplementedException();

	public override KeyInfo NameToKeyInfo(string key) => throw new NotImplementedException();

	public override Panel[] Panels(Type type) => throw new NotImplementedException();

	public override Panel[] Panels(Guid typeId) => throw new NotImplementedException();

	public override string PasteFromClipboard() => throw new NotImplementedException();

	public override void PostJob(Action job) => throw new NotImplementedException();

	public override void PostMacro(string macro, bool enableOutput, bool disablePlugins) => throw new NotImplementedException();

	public override void PostStep(Action step) => throw new NotImplementedException();

	public override void Quit() => throw new NotImplementedException();

	public override void ShowError(string? title, Exception exception) => throw new NotImplementedException();

	public override void ShowHelp(string path, string topic, HelpOptions options) => throw new NotImplementedException();

	public override string TempName(string? prefix) => throw new NotImplementedException();

	public override IViewer[] Viewers() => throw new NotImplementedException();
}
