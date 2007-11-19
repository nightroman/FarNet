Imports FarManager
Public Class HelloVB
Inherits BasePlugin
	Sub item_OnOpen(ByVal sender As Object, ByVal e As PluginMenuEventArgs)
		Far.Msg("Hello, world from vb.net", "Far.NET")
	End Sub

	Sub sayHello(ByVal sender As Object, ByVal e As ExecutingEventArgs)
		Far.Msg("VB.NET: Hello, " + e.Command)
	End Sub

	Public Overloads Overrides Sub Connect()
		Far.RegisterPluginsMenuItem("Hello vbnet", AddressOf item_OnOpen)
		Far.RegisterPrefix("hellovb", AddressOf sayHello)
	End Sub
End Class
