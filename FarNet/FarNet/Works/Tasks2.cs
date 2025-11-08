namespace FarNet.Works;
#pragma warning disable CS1591

public static class Tasks2
{
	public static async Task<object?> Wait(string message, Func<bool> job)
	{
		if (await Tasks.Wait(50, 5000, job))
			return null;
		else
			throw new Exception(message);
	}
}
