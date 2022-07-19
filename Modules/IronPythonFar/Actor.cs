using FarNet;
using IronPython.Hosting;
using System;
using System.IO;
using System.Text;

namespace IronPythonFar;

internal static class Actor
{
    internal static void ExecuteFile(string filePath, Action<string> print)
    {
        var farHome = Environment.GetEnvironmentVariable("FARHOME");

        var engine = Python.CreateEngine();
        engine.SetSearchPaths(new string[]
            {
                Path.GetDirectoryName(filePath),
                Path.Combine(farHome, "FarNet"),
                Path.Combine(farHome, "FarNet\\Modules\\IronPythonFar\\Lib"),
            });

        var scope = engine.CreateScope();
        scope.SetVariable("far", Far.Api);

        var encoding = Encoding.UTF8;
        var output = new MemoryStream();
        engine.Runtime.IO.SetOutput(output, encoding);
        engine.Runtime.IO.SetErrorOutput(output, encoding);

        try
        {
            engine.ExecuteFile(filePath, scope);
        }
        finally
        {
            engine.Runtime.IO.RedirectToConsole();

            output.Flush();
            print(encoding.GetString(output.GetBuffer(), 0, (int)output.Length));
        }
    }
}
