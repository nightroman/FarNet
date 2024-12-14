namespace JsonKit.Commands;

abstract class AbcCommand
{
	protected static class Param
	{
		public const string File = "File";
		public const string Select = "Select";
	}

	public abstract void Invoke();
}
