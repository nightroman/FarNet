using System.Management.Automation;
using System.Management.Automation.Internal;

namespace PowerShellFar;

abstract class AbcOutputWriter
{
	public static string RemoveOutputRendering(string s)
	{
		var str = new StringDecorated(s);
		return str.ToString(OutputRendering.PlainText);
	}

	public AbcOutputWriter? Next { get; set; }
	public virtual void Flush() { }
	public abstract void Write(string value);
	public abstract void WriteLine();
	public abstract void WriteLine(string value);
	public abstract void Write(ConsoleColor foregroundColor, ConsoleColor backgroundColor, string value);
	public abstract void WriteLine(ConsoleColor foregroundColor, ConsoleColor backgroundColor, string value);
	public abstract void WriteDebugLine(string message);
	public abstract void WriteErrorLine(string value);
	public abstract void WriteVerboseLine(string message);
	public abstract void WriteWarningLine(string message);
}
