import System;
import FarNet;

public class JSTool extends ToolPlugin
{
	function Invoke(sender:Object, e:ToolEventArgs)
	{
		Far.Msg("Hello " + Name + " " + e.From);
	}
}
