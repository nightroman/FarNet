using FarNet;
using FarNetTest.About;
using System.Reflection;

namespace FarNetTest;

public abstract class AbcTest
{
	public static string TestRoot { get; }

	static AbcTest()
	{
		Far.Api = new TestFar();

		Environment.SetEnvironmentVariable("FarNetTest", "1");

		var appRoot = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
		TestRoot = Path.GetFullPath($"{appRoot}/../../..")!;
	}
}
