/*
PowerShellFar plugin for Far Manager
Copyright (c) 2006 Roman Kuzmin
*/

using System.Management.Automation;
using FarNet;

namespace FarMacro
{
	/// <summary>
	/// Base cmdlet.
	/// </summary>
	public class BaseCmdlet : PSCmdlet
	{
		internal const string Noun = "FarMacro";
		
		protected IFar Far
		{
			get { return (IFar)Host.PrivateData.BaseObject; }
		}
	}
}
