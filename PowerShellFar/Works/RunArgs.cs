namespace PowerShellFar;

class RunArgs(string code)
{
	public string Code { get; } = code;

	public AbcOutputWriter? Writer { get; set; }

	public bool NoOutReason { get; set; }

	public bool UseLocalScope { get; set; }

	public bool UseTeeResult { get; set; }

	public object[]? Arguments { get; set; }

	public Exception? Reason { get; set; }
}
