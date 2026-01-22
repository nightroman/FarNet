namespace FarNet.Works;

// Reads and writes, wraps the dictionary.
class ModuleCache
{
	const int CurrentVersion = 1;
	readonly string _FileName;
	readonly int _CountCached;
	internal int CountFound;
	bool _ToUpdate;
	readonly Dictionary<string, ModuleManager> _Cache = [];

	public ModuleCache()
	{
		//! read the cache, let it fail if missing
		_FileName = Far.Api.GetFolderPath(SpecialFolder.LocalData) + (IntPtr.Size == 4 ? @"\FarNet\Cache32.bin" : @"\FarNet\Cache64.bin");
		try
		{
			using var stream = new FileStream(_FileName, FileMode.Open, FileAccess.Read, FileShare.Read);
			using var reader = new BinaryReader(stream);

			// [1]
			var savedVersion = reader.ReadUInt32();
			if (savedVersion != CurrentVersion)
				throw new IOException("Version");

			// [2]
			var moduleCount = reader.ReadUInt32();
			for (int i = 0; i < moduleCount; i++)
			{
				// [3]
				var assemblyPath = reader.ReadString();

				// [4]
				var moduleManager = new ModuleManager(assemblyPath);
				moduleManager.ReadCache(reader);

				_Cache.Add(assemblyPath, moduleManager);
			}
		}
		catch (IOException) //! FileNotFoundException, DirectoryNotFoundException
		{
			_Cache.Clear();
			_ToUpdate = true;
		}
		catch (Exception ex)
		{
			_Cache.Clear();
			_ToUpdate = true;
			Far.Api.ShowError("Reading cache", ex);
		}

		// count to load
		_CountCached = _Cache.Count;
	}

	void Write()
	{
		// ensure the directory
		Directory.CreateDirectory(Path.GetDirectoryName(_FileName)!);

		// write the cache
		using var stream = new FileStream(_FileName, FileMode.Create, FileAccess.Write, FileShare.None);
		using var writer = new BinaryWriter(stream);

		// [1]
		writer.Write(CurrentVersion);

		// [2]
		writer.Write(_Cache.Count);
		foreach (var kv in _Cache)
		{
			// [3]
			writer.Write(kv.Key);

			// [4]
			kv.Value.WriteCache(writer);
		}
	}

	public void Update()
	{
		// obsolete records?
		if (_CountCached != CountFound)
		{
			var missingAssemblyPaths = new List<string>();
			foreach (var assemblyPath in _Cache.Keys)
			{
				if (!File.Exists(assemblyPath))
					missingAssemblyPaths.Add(assemblyPath);
			}

			if (missingAssemblyPaths.Count > 0)
			{
				_ToUpdate = true;
				foreach (var assemblyPath in missingAssemblyPaths)
					_Cache.Remove(assemblyPath);
			}
		}

		// write changed
		if (_ToUpdate)
			Write();
	}

	public ModuleManager? Find(string assemblyPath)
	{
		return _Cache.TryGetValue(assemblyPath, out var manager) ? manager : null;
	}

	public void Set(string assemblyPath, ModuleManager manager)
	{
		_Cache[assemblyPath] = manager;
		_ToUpdate = true;
	}

	public void Remove(string assemblyPath)
	{
		_Cache.Remove(assemblyPath);
		_ToUpdate = true;
	}
}
