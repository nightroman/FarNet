namespace PowerShellFar;

class RunArgs(string code)
{
	public string Code { get; } = code;

	public OutputWriter? Writer { get; set; }

	public bool NoOutReason { get; set; }

	public bool UseLocalScope { get; set; }

	public object[]? Arguments { get; set; }

	public Exception? Reason { get; set; }
}
