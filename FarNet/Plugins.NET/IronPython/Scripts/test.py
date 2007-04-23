def hello(sender, e):
	name = ""
	if e.From == FarManager.OpenFrom.Editor:
		name = "(Editor)"		
	far.Msg("Hello, IronPython "+name)

far.RegisterPluginsMenuItem("Hello, IronPython", hello)