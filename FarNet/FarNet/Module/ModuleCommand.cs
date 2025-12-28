namespace FarNet;

/// <summary>
/// A command called by its prefix from command lines and macros.
/// </summary>
/// <remarks>
/// The <see cref="Invoke"/> method has to be implemented.
/// <para>
/// Commands are called by their prefixes from command lines: the panel
/// command line and user menu and file association commands. Macros call
/// commands by <c>Plugin.Call()</c> (see the FarNet manual).
/// </para>
/// <para>
/// Use <see cref="ModuleCommandAttribute"/> and specify <see cref="ModuleActionAttribute.Id"/>,
/// <see cref="ModuleActionAttribute.Name"/>, and the default command prefix <see cref="ModuleCommandAttribute.Prefix"/>.
/// </para>
/// </remarks>
public abstract class ModuleCommand : ModuleAction
{
	/// <summary>
	/// Command handler called from the command line with a prefix.
	/// </summary>
	/// <param name="sender">The sender.</param>
	/// <param name="e">The arguments.</param>
	public abstract void Invoke(object sender, ModuleCommandEventArgs e);

	/// <summary>
	/// Invokes a subcommand returned by <paramref name="factory"/>.
	/// </summary>
	/// <param name="command">The command text.</param>
	/// <param name="factory">Gets the subcommand to be invoked.</param>
	/// <remarks>
	/// <para>
	/// Used by <c>FarNet.GitKit</c>, <c>FarNet.JsonKit</c>, <c>FarNet.RedisKit</c>.
	/// See the repository for examples.
	/// </para>
	/// <para>
	/// The <paramref name="factory"/> gets the subcommand by the name and parameters
	/// or returns null if the name is unknown.
	/// </para>
	/// <para>
	/// The subcommand should use <see cref="CommandParameters"/> <c>Get*</c> for all parameters.
	/// Unused parameters trigger <see cref="CommandParameters.ThrowUnknownParameters"/>.
	/// </para>
	/// </remarks>
	protected void InvokeSubcommand(string command, Func<ReadOnlySpan<char>, CommandParameters, Subcommand?> factory)
	{
		try
		{
			var parameters = CommandParameters.Parse(command);
			var subcommand = factory(parameters.Command, parameters)
				?? throw new ModuleException($"Unknown subcommand '{parameters.Command}'.");

			parameters.ThrowUnknownParameters();
			subcommand.Invoke();
		}
		catch (Exception ex)
		{
			throw new ModuleException($"{Manager.ModuleName}: {ex.Message}", ex);
		}
	}
}
