Imports System
Imports FarNet

Public Class VBTool
Inherits ToolPlugin
	Public Overrides Sub Invoke (ByVal sender As Object, ByVal e As ToolEventArgs)
		Far.Msg("Hello " + Name + " " + e.From.ToString())
	End Sub
End Class
