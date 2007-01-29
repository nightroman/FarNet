using FarManager;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System;

namespace FarNetPlugMan
{
	/// <summary>
	/// This is simple plugin manager which loads
	/// </summary>
	public class PluginManager : BasePlugin
	{
		IList<IPlugin> _plugins = new List<IPlugin>();
		public override void Connect()
		{
			Trace.WriteLine("Loading plugins");
			Trace.Indent();
			foreach (DirectoryInfo dir in PluginsDirectory().GetDirectories())
				LoadPlugin(dir);
			Trace.Unindent();
		}
		public DirectoryInfo PluginsDirectory()
		{
			return new DirectoryInfo(Environment.ExpandEnvironmentVariables(System.Configuration.ConfigurationManager.AppSettings["FarManager.Plugins"]));
		}
		public override void Disconnect()
		{
			foreach (IPlugin plug in _plugins)
				plug.Far = null;
			_plugins.Clear();
		}
		private void AddPlugin(Type type)
		{
			Trace.WriteLine("Class:" + type.Name);
			IPlugin plugin = (IPlugin)Activator.CreateInstance(type);
			_plugins.Add(plugin);
			plugin.Far = Far;
			Trace.WriteLine("Attached:" + type.Name);
		}
		private void LoadConfig(StreamReader text, DirectoryInfo dir)
		{
			try
			{
				DirectoryInfo bin = dir.GetDirectories("bin")[0];
				string line;
				while ((line = text.ReadLine()) != null)
				{
					Trace.WriteLine("Loaded Line:" + line);
					string[] classes = line.Split(' ');
					string assembly = classes[0];
					Trace.WriteLine("Assembly:" + assembly);
					Assembly plugAsm = Assembly.LoadFrom(bin.GetFiles(assembly)[0].FullName);
					for (int i = 1; i < classes.Length; i++)
					{
						AddPlugin(plugAsm.GetType(classes[i], true));
					}
				}
			}
			finally
			{
				text.Close();
			}
		}
		private bool IsPlugin(Type type)
		{
			return !type.IsAbstract && typeof(IPlugin).IsAssignableFrom(type);
		}
		private void LoadAllFrom(DirectoryInfo dir)
		{
			FileSystemInfo[] dlls = dir.GetFiles("*.dll");
			foreach (FileInfo dll in dlls)
			{
				Assembly plugAsm = Assembly.LoadFrom(dll.FullName);
				foreach (Type type in plugAsm.GetExportedTypes())
				{
					if (IsPlugin(type))
						AddPlugin(type);
				}
			}
		}
		private void LoadPlugin(DirectoryInfo dir)
		{
			Trace.WriteLine("Plugin:" + dir.FullName);
			Trace.Indent();
			try
			{
				DirectoryInfo[] cfgs = dir.GetDirectories("cfg");
				bool cfgIsFound = cfgs.Length > 0;
				if (cfgIsFound)
				{
					FileInfo[] cfgFiles = cfgs[0].GetFiles("plugin.cfg");
					cfgIsFound = cfgFiles.Length > 0;
					if (cfgIsFound)
						LoadConfig(cfgFiles[0].OpenText(), dir);
				}
				if (!cfgIsFound)
				{
					DirectoryInfo[] bins = dir.GetDirectories("bin");
					LoadAllFrom((bins.Length > 0) ? bins[0] : dir);
				}
			}
			catch (Exception e)
			{
				Trace.WriteLine("Exception: " + e.Message);
				IMessage m = Far.CreateMessage();
				m.Body.Add(e.Message);
				m.Body.Add("Plugin:" + dir.Name);
				m.Buttons.Add("Ok");
				m.Buttons.Add("Stack trace");
				m.Header = "Error while loading plugin";
				m.IsWarning = true;
				m.Show();
				if (m.Selected == 1)
				{
					String traceFile = Path.GetTempFileName();
					StreamWriter f = File.CreateText(traceFile);
					f.WriteLine(e.Message);
					f.Write(e.StackTrace);
					f.Close();
					IEditor editor = Far.CreateEditor();
					editor.FileName = traceFile;
					editor.Open();
				}
			}
			Trace.Unindent();
		}
	}
}
