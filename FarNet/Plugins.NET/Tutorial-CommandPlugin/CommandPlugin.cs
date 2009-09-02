using System;
using FarNet;

// A simplest command: it uses the default Prefix and Name (the class name,
// "cmd1") and it does not care that actually used prefix can be different.
public class Cmd1 : CommandPlugin
{
	// Command handler: this one just shows the prefix and the command.
	public override void Invoke(object sender, CommandEventArgs e)
	{
		Far.Msg(Prefix + ":" + e.Command, Name);
	}
}

// This command provides another default Prefix, watches its changes and also
// overrides the default Name. The class is derived from Cmd1 so that Invoke()
// is the same but it reflects changed Prefix and Name.
public class Cmd2 : Cmd1
{
	string prefix = "test";

	// Gets the prefix; 'set' is used by FarNet only to change the prefix.
	public override string Prefix
	{
		get { return prefix; }
		set { prefix = value; }
	}

	// The name is used in the config menu where a user can change the prefix
	// and in the registry where this prefix is stored.
	public override string Name
	{
		get { return "Test command"; }
	}
}
