
/*
FarNet plugin for Far Manager
Copyright (c) 2006-2013 Roman Kuzmin
*/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace FarNet.Works
{
	public abstract class ShelveInfo
	{
		//! PSF test.
		public static Collection<ShelveInfo> Stack { get { return new Collection<ShelveInfo>(_Stack); } }
		static readonly List<ShelveInfo> _Stack = new List<ShelveInfo>();
		//! PSF test.
		public string[] GetSelectedNames() { return _SelectedNames; }
		string[] _SelectedNames;
		//! PSF test.
		public int[] GetSelectedIndexes() { return _SelectedIndexes; }
		int[] _SelectedIndexes;
		public abstract bool CanRemove { get; }
		public abstract string Title { get; }
		public abstract void Pop(bool active);
		public void Pop() { Pop(true); }
		protected void InitSelectedNames(IPanel panel)
		{
			if (panel == null) throw new ArgumentNullException("panel");

			// nothing
			if (!panel.SelectionExists)
				return;

			// copy selected names
			var files = panel.SelectedList;
			_SelectedNames = new string[files.Count];
			for (int i = files.Count; --i >= 0; )
				_SelectedNames[i] = files[i].Name;
		}
		protected void InitSelectedIndexes(IPanel panel)
		{
			if (panel == null) throw new ArgumentNullException("panel");

			// nothing
			if (!panel.SelectionExists)
				return;

			// keep selected indexes
			_SelectedIndexes = panel.SelectedIndexes();
		}
	}
}
