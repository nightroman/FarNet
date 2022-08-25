
using System;
using System.IO;
using FarNet;

namespace IronPythonFar;

[ModuleCommand(Name = "Execute file", Prefix = Prefix, Id = "3b8af2c8-91ac-4e4a-a89e-6dfad756397c")]
public class ExecuteFile : ModuleCommand
{
    const string Prefix = "ip";

    public override void Invoke(object sender, ModuleCommandEventArgs e)
    {
        var filePath = Environment.ExpandEnvironmentVariables(e.Command.Trim());

        if (!Path.IsPathRooted(filePath))
            filePath = Path.GetFullPath(Path.Combine(Far.Api.CurrentDirectory, filePath));

        string print = string.Empty;
        try
        {
            Actor.ExecuteFile(filePath, s => print = s);
        }
        finally
        {
            if (print.Length > 0)
            {
                Far.Api.UI.ShowUserScreen();
                Far.Api.UI.WriteLine($"{Prefix}: {filePath}", ConsoleColor.DarkGray);

                if (print.EndsWith("\n"))
                    Far.Api.UI.Write(print);
                else
                    Far.Api.UI.WriteLine(print);

                Far.Api.UI.SaveUserScreen();
            }
        }
    }
}
