namespace FarNet.Works;
#pragma warning disable 1591

public static class Abc
{
	// Gets property value if any or null.
	public static object? TryProperty(this object obj, string name)
	{
		try
		{
			var meta = obj.GetType().GetProperty(name);
			return meta?.GetValue(obj);
		}
		catch (Exception ex)
		{
			Log.TraceException(ex);
			return null;
		}
	}
}
