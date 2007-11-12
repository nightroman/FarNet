Imports FarManager
Public Class HelloVB
Inherits BasePlugin
	Sub item_OnOpen(ByVal sender As Object, ByVal e As OpenPluginMenuItemEventArgs)
		Far.Msg("Hello, world from vb.net", "Far.NET")
	End Sub

	Sub sayHello(ByVal s As String)
		Far.Msg("VB.NET: Hello, " + s)
	End Sub

	Public Overloads Overrides Sub Connect()
		Far.RegisterPluginsMenuItem("Hello vbnet", AddressOf item_OnOpen)
		Far.RegisterPrefix("hellovb", AddressOf sayHello)
	End Sub
End Class
