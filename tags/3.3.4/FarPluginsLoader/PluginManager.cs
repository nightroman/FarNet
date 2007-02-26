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
					foreach(string s in classes)
						AddPlugin(plugAsm.GetType(s, true));
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
				Far.ShowError("Error in plugin: " + dir.Name, e);
			}
			Trace.Unindent();
		}
	}
}
