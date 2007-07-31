Imports FarManager
Public Class HelloVB
Inherits BasePlugin
	Private menuItem As IPluginMenuItem

	Sub item_OnOpen(ByVal sender As Object, ByVal e As OpenPluginMenuItemEventArgs)
		Far.Msg("Hello, world from vb.net", "Far.NET")
	End Sub

	Sub sayHello(ByVal s As String)
		Far.Msg("VB.NET: Hello, " + s)
	End Sub

	Public Overloads Overrides Sub Connect()
		Me.menuItem = Far.CreatePluginsMenuItem
		Me.menuItem.Name = "Hello vbnet"
		AddHandler Me.menuItem.OnOpen, AddressOf item_OnOpen
		Far.RegisterPluginsMenuItem(Me.menuItem)
		Far.RegisterPrefix("hellovb", AddressOf sayHello)
	End Sub

	Public Overloads Overrides Sub Disconnect()
		Far.UnregisterPluginsMenuItem(Me.menuItem)
	End Sub
End Class 
