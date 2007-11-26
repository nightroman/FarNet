Imports FarManager
Public Class VBTool
Inherits ToolPlugin
	Public Overrides Sub Invoke (ByVal sender As Object, ByVal e As ToolEventArgs)
		Far.Msg("Hello from " + Name)
	End Sub
End Class
