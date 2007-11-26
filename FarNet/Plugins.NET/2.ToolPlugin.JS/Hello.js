import FarManager;
public class JSTool extends ToolPlugin
{
	function Invoke(sender:Object, e:ToolEventArgs)
	{
		Far.Msg("Hello from " + Name);
	}
}
