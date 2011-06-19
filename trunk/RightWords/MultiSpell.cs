
/*
FarNet module RightWords
Copyright (c) 2011 Roman Kuzmin
*/

using System;
using System.Collections.Generic;
using NHunspell;

namespace FarNet.RightWords
{
	sealed class MultiSpell : IDisposable
	{
		readonly List<Hunspell> _spells;
		public MultiSpell(IList<LanguageConfig> dictionaries)
		{
			_spells = new List<Hunspell>(dictionaries.Count);
			foreach (var dic in dictionaries)
				_spells.Add(new Hunspell(dic.HunspellAffFile, dic.HunspellDictFile));
		}
		public void Dispose()
		{
			foreach (var spell in _spells)
				spell.Dispose();
		}
		public bool Spell(string word)
		{
			foreach (var spell in _spells)
				if (spell.Spell(word))
					return true;

			return false;
		}
		public List<string> Suggest(string word)
		{
			var words = new List<string>();
			foreach (var spell in _spells)
				words.AddRange(spell.Suggest(word));

			return words;
		}
	}
}
