
/*
FarNet module RightWords
Copyright (c) 2011-2014 Roman Kuzmin
*/

using System;
using System.Collections.Generic;
using NHunspell;
namespace FarNet.RightWords
{
	sealed class MultiThesaurus : IDisposable
	{
		readonly List<Hunspell> _spells;
		readonly List<MyThes> _thesaurus;
		public MultiThesaurus(IList<DictionaryInfo> dictionaries)
		{
			_spells = new List<Hunspell>(dictionaries.Count);
			_thesaurus = new List<MyThes>(dictionaries.Count);
			foreach (var dic in dictionaries)
			{
				if (!string.IsNullOrEmpty(dic.MyThesDatFile))
				{
					_spells.Add(new Hunspell(dic.HunspellAffFile, dic.HunspellDictFile));
					_thesaurus.Add(new MyThes(dic.MyThesDatFile));
				}
			}
		}
		public void Dispose()
		{
			foreach (var spell in _spells)
				spell.Dispose();
		}
		public List<ThesResult> Lookup(string word)
		{
			var results = new List<ThesResult>();
			for (int i = 0; i < _spells.Count; ++i)
			{
				var result = _thesaurus[i].Lookup(word, _spells[i]);
				if (result != null)
					results.Add(result);
			}

			return results;
		}
	}
}
