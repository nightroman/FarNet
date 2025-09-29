using FarNet;
using System.Collections.ObjectModel;
using System.Management.Automation;

namespace PowerShellFar;

/// <summary>
/// Folder tree explorer.
/// </summary>
public sealed class FolderExplorer : TreeExplorer
{
	const string TypeIdString = "c9b2ddc2-80d8-4074-9cf7-d009b4ef7ffb";
	static readonly string[] s_paramPath = ["."];

/// <summary>
/// New folder explorer.
/// </summary>
/// <param name="path">The provider path to a root container.</param>
public FolderExplorer(string? path) : base(new Guid(TypeIdString))
	{
		Reset(path);
	}

	/// <inheritdoc/>
	public override Panel CreatePanel() => new FolderTree(this);

	static readonly ScriptAction<TreeFile> TheFill = new(Fill);

	static void Fill(TreeFile node)
	{
		// get
		Collection<PSObject> items = A.GetChildItems(node.Path);

		foreach (PSObject item in items)
		{
			if (!(bool)item.Properties["PSIsContainer"].Value)
				continue;

			TreeFile t = node.ChildFiles.Add();

			// name
			t.Data = item;
			t.Name = (string)item.Properties["PSChildName"].Value;
			t.Fill = TheFill;

			// description
			PSPropertyInfo pi = item.Properties["FarDescription"];
			if (pi != null && pi.Value != null)
				t.Description = pi.Value.ToString();

			// attributes _090810_180151
			if (item.BaseObject is FileSystemInfo fsi)
				t.Attributes = fsi.Attributes & ~FileAttributes.Directory;
		}
	}

	void Reset(string? path)
	{
		// set location
		if (!string.IsNullOrEmpty(path) && path != ".")
			A.Psf.Engine.SessionState.Path.SetLocation(WildcardPattern.Escape(path));

		// get location
		var location = new PathInfoEx(A.Psf.Engine.SessionState.Path.CurrentLocation);
		if (!My.ProviderInfoEx.IsNavigation(location.Provider))
			throw new RuntimeException("Provider '" + location.Provider + "' does not support navigation.");

		// get root item
		Collection<PSObject> items = A.Psf.Engine.SessionState.InvokeProvider.Item.Get(s_paramPath, true, true);
		//! trap Get-Item at Cert:
		if (items.Count == 0)
			throw new RuntimeException(string.Format(null, "Provider '{0}' cannot get '{1}'.", location.Provider, location.Path));
		PSObject data = items[0];

		// reset roots
		RootFiles.Clear();
		var ti = new TreeFile
		{
			Name = location.Path, // special case name for the root
			Fill = TheFill,
			Data = data
		};
		RootFiles.Add(ti);
		ti.Expand();

		// panel info
		Location = ti.Path;
	}

	/// <inheritdoc/>
	public override Explorer? ExploreParent(ExploreParentEventArgs args)
	{
		ArgumentNullException.ThrowIfNull(args);

		string newLocation = My.PathEx.GetDirectoryName(Location); //???? GetDirectoryName to add '\' for the root like IO.Path does
		if (newLocation.Length == 0)
			return null;

		if (newLocation.EndsWith(':'))
			newLocation += "\\";
		if (newLocation.Equals(Location, StringComparison.OrdinalIgnoreCase))
			return null;

		args.PostName = My.PathEx.GetFileName(Location);

		return new FolderExplorer(newLocation);
	}
}
