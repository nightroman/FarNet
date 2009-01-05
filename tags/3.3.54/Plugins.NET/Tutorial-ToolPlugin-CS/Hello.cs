using System;
using FarManager;

public class CSTool : ToolPlugin
{
	public override void Invoke(object sender, ToolEventArgs e)
	{
		Far.Msg("Hello " + Name + " " + e.From);
	}
}
