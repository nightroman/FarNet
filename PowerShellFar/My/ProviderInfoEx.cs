
// PowerShellFar module for Far Manager
// Copyright (c) Roman Kuzmin

using System.Management.Automation;
using System.Management.Automation.Provider;

namespace My;

/// <summary>
/// My System.Management.Automation.ProviderInfo extensions.
/// </summary>
static class ProviderInfoEx
{
	public static bool HasContent(ProviderInfo provider)
	{
		return provider.ImplementingType.GetInterface("IContentCmdletProvider") != null;
	}

	public static bool HasDynamicProperty(ProviderInfo provider)
	{
		return provider.ImplementingType.GetInterface("IDynamicPropertyCmdletProvider") != null;
	}

	public static bool HasProperty(ProviderInfo provider)
	{
		return provider.ImplementingType.GetInterface("IPropertyCmdletProvider") != null;
	}

	public static bool IsNavigation(ProviderInfo provider)
	{
		//! 'is' does not work, because we work just on a type, not an instance
		return provider.ImplementingType.IsSubclassOf(typeof(NavigationCmdletProvider));
	}
}
