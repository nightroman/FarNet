using FarNet.Works;

namespace FarNetTest;

public class KitTest
{
	static bool IsPsfPrefix(ReadOnlySpan<char> prefix)
	{
		return
			prefix.Equals("PS", StringComparison.OrdinalIgnoreCase) ||
			prefix.Equals("VPS", StringComparison.OrdinalIgnoreCase);
	}

	[Theory]
	[InlineData("", "", "")]
	[InlineData(" ", " ", "")]
	[InlineData("text", "", "text")]
	[InlineData(" text ", " ", "text ")]
	[InlineData("ps:text", "ps:", "text")]
	[InlineData("vps:text", "vps:", "text")]
	[InlineData(" ps: text ", " ps: ", "text ")]
	[InlineData(" vps: text ", " vps: ", "text ")]
	[InlineData("bar:text", "", "bar:text")]
	[InlineData(" bar: text ", " ", "bar: text ")]
	public void SplitCommandWithPrefix(string commandLine, string expectedPrefix, string expectedCommand)
	{
		Kit.SplitCommandWithPrefix(commandLine, out var prefix, out var command, IsPsfPrefix);
		Assert.Equal(expectedPrefix, prefix);
		Assert.Equal(expectedCommand, command);
	}

	[Fact]
	public void IsInvalidFileName()
	{
		// names with invalid chars
		var invalidChars = Path.GetInvalidFileNameChars();
		Assert.Equal(41, invalidChars.Length);
		foreach (var invalidChar in invalidChars)
			Assert.True(Kit.IsInvalidFileName("a" + invalidChar + "z"));

		// other invalid names
		string?[] names =
		[
			// null or empty
			null, string.Empty,
			// ending with dot or space
			"bar.", "bar ",
			// special names
			"con", "prn", "aux", "nul",
			"com1", "com2", "com3", "com4", "com5", "com6", "com7", "com8", "com9",
			"lpt1", "lpt2", "lpt3", "lpt4", "lpt5", "lpt6", "lpt7", "lpt8", "lpt9"
		];

		foreach (var name in names)
			Assert.True(Kit.IsInvalidFileName(name));
	}
}
