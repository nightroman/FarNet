
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

using System;
using System.Threading.Tasks;

namespace FarNet.Works;

/// <summary>
/// INTERNAL
/// </summary>
public static class Tasks2
{
	///
	public static async Task<object> Wait(string message, Func<bool> job)
	{
		if (await Tasks.Wait(50, 5000, job))
			return null;
		else
			throw new Exception(message);
	}
}
