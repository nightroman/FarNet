
/*
PowerShellFar module for Far Manager
Copyright (c) 2006-2015 Roman Kuzmin
*/

using System.Management.Automation;
using System.Diagnostics.CodeAnalysis;
using FarNet;

namespace PowerShellFar.Commands
{
	/// <summary>
	/// Common features of cmdlets opening text files.
	/// </summary>
	class BaseTextCmdlet : BaseCmdlet
	{
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		[Parameter]
		public string Title { get; set; }
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		[Parameter]
		public DeleteSource DeleteSource { get; set; }
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		[Parameter]
		public SwitchParameter DisableHistory { get; set; }
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		[Parameter]
		public Switching Switching { get; set; }
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		[Parameter]
		public int CodePage { get { return _CodePage; } set { _CodePage = value; } }
		int _CodePage = -1;
	}
}
