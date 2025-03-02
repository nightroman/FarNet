using FarNet;

namespace FarNetTest;

public class CommandParametersTest : AbcTest
{
	[Theory]
	[InlineData("  bar p1 = v1  ")]
	public void BasicSpace(string commandLine)
	{
		var parameters = CommandParameters.Parse(commandLine);
		Assert.Equal("", parameters.Command);
		Assert.Equal("bar p1 = v1", parameters.Text);
		try
		{
			parameters.ThrowUnknownParameters();
			Assert.Fail();
		}
		catch (NullReferenceException)
		{
		}
	}

	[Theory]
	[InlineData("bar p1=v1;p2=v2;p3=v3")]
	[InlineData("bar p1 = v1 ; p2 = v2 ; p3 = v3  ")]
	public void BasicNormal(string commandLine)
	{
		var parameters = CommandParameters.Parse(commandLine);
		Assert.Equal("bar", parameters.Command);
		Assert.Equal("", parameters.Text);
		Assert.Equal("v1", parameters.GetRequiredString("p1"));
		try
		{
			parameters.ThrowUnknownParameters();
			Assert.Fail();
		}
		catch (ModuleException ex)
		{
			Assert.Equal("Command: bar\r\nUnknown parameters: p2, p3", ex.Message);
		}
	}

	[Theory]
	[InlineData("p1=v1")]
	[InlineData("  p1 = v1  ")]
	public void NoCommandNoText(string commandLine)
	{
		var parameters = CommandParameters.Parse(commandLine, false);
		Assert.Equal("", parameters.Command);
		Assert.Equal("", parameters.Text);
		Assert.Equal("v1", parameters.GetRequiredString("p1"));
		parameters.ThrowUnknownParameters();
	}

	[Theory]
	[InlineData("p1=v1;;text")]
	[InlineData("  p1 = v1  ;;  text  ")]
	public void NoCommandWithText(string commandLine)
	{
		var parameters = CommandParameters.Parse(commandLine, false);
		Assert.Equal("", parameters.Command);
		Assert.Equal("text", parameters.Text);
		Assert.Equal("v1", parameters.GetRequiredString("p1"));
		parameters.ThrowUnknownParameters();
	}

	[Theory]
	[InlineData("bar p1=v1;;text")]
	[InlineData("bar p1 = v1  ;;  text  ")]
	public void WithCommandWithText(string commandLine)
	{
		var parameters = CommandParameters.Parse(commandLine, true);
		Assert.Equal("bar", parameters.Command);
		Assert.Equal("text", parameters.Text);
		Assert.Equal("v1", parameters.GetRequiredString("p1"));
		parameters.ThrowUnknownParameters();
	}

	[Fact]
	public void GetPath()
	{
		var parameters = CommandParameters.Parse("bar p1=%FarNetTest%/test.txt; p2=%FarNetTest%/test.txt");
		Assert.Equal(@$"{TestRoot}\1\test.txt", parameters.GetRequiredPath("p1"));
		Assert.Equal(@$"{TestRoot}\1\test.txt", parameters.GetPath("p2"));
		Assert.Null(parameters.GetPath("missing"));
		Assert.Equal(TestRoot, parameters.GetPathOrCurrentDirectory("missing"));
	}

	[Fact]
	public void GetBool()
	{
		var parameters = CommandParameters.Parse("bar p1=true; p2=1; p3=false; p4=0; p5=oops");
		Assert.True(parameters.GetBool("p1"));
		Assert.True(parameters.GetBool("p2"));
		Assert.False(parameters.GetBool("p3"));
		Assert.False(parameters.GetBool("p4"));
		Assert.False(parameters.GetBool("missing"));
		try
		{
			parameters.GetBool("p5");
			Assert.Fail();
		}
		catch (ModuleException ex)
		{
			Assert.Equal("Command: bar\r\nParameter 'p5': Invalid value 'oops': valid values: true, false, 1, 0.", ex.Message);
		}
	}

	[Fact]
	public void GetInt()
	{
		var parameters = CommandParameters.Parse("bar p1=42; p2=bad");
		Assert.Equal(42, parameters.GetValue<int>("p1"));
		try
		{
			parameters.GetValue<int>("p2");
			Assert.Fail();
		}
		catch (ModuleException ex)
		{
			Assert.Equal("Command: bar\r\nParameter 'p2': Invalid value 'bad': The input string 'bad' was not in a correct format.", ex.Message);
		}
	}

	[Fact]
	public void GetEnum()
	{
		var parameters = CommandParameters.Parse("bar p1=Red; p2=bad");
		Assert.Equal(ConsoleColor.Red, parameters.GetValue<ConsoleColor>("p1"));
		try
		{
			parameters.GetValue<ConsoleColor>("p2");
			Assert.Fail();
		}
		catch (ModuleException ex)
		{
			Assert.Equal("Command: bar\r\nParameter 'p2': Invalid value 'bad': Requested value 'bad' was not found.", ex.Message);
		}
	}

	[Fact]
	public void AtNotation()
	{
		var file = RepoRoot + @"\Modules\ScriptPS\Script.fn.dbcs ? age=33";

		var parameters = CommandParameters.Parse($"@{file}", false);
		Assert.Equal("ScriptPS", parameters.GetRequiredString("script"));
		Assert.Equal("Message", parameters.GetRequiredString("method"));
		Assert.True(parameters.GetValue<bool>("unload"));
		parameters.ThrowUnknownParameters();

		Assert.EndsWith("age=42", parameters.Text);
		Assert.Equal("age=33", parameters.Text2);
	}
}
