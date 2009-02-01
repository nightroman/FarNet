using System;
using FarNet;

public class CSTool : ToolPlugin
{
	public override void Invoke(object sender, ToolEventArgs e)
	{
		Far.Msg("Hello " + Name + " " + e.From);
	}
}
