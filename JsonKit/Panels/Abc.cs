using FarNet;

namespace JsonKit.Panels;

class Parent(AbcExplorer explorer, SetFile file)
{
	public AbcExplorer Explorer => explorer;
	public SetFile File => file;
}

static class Errors
{
	public static ModuleException CannotFindSource() => new("Cannot find the source.");
}
