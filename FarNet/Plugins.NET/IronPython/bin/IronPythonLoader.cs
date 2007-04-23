using FarManager;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using IronPython.Hosting;
using IronPython.Modules;
public class IronPythonLoader : BasePlugin
{
	protected string PluginFile(string name)
	{
       	FileInfo fi=new FileInfo(Assembly.GetExecutingAssembly().Location);
       	return fi.Directory.Parent.FullName+"\\"+name;
    }
    protected void LoadScript(string fileName)
    {
		PythonEngine engine = new PythonEngine();
		engine.AddToPath(Path.GetDirectoryName(fileName));
		engine.Import("site");		
		ClrModule clr =  (ClrModule)engine.Import("clr");

		
		EngineModule engineModule = engine.CreateModule("__main__", false);
        engine.DefaultModule = engineModule;

		clr.AddReferenceByPartialName("FarNetIntf");
		engine.Import("FarManager");
		engine.Globals["far"] = Far;
		engine.ExecuteFile(fileName);
    }
	override public void Connect()
	{
		string scriptsFolder = PluginFile("Scripts");
		foreach(FileInfo file in new DirectoryInfo(scriptsFolder).GetFiles())
			LoadScript(file.FullName);
	}
}
