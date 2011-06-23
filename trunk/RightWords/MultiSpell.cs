
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
		readonly List<DictionaryInfo> _dictionaries;
		public MultiSpell(List<DictionaryInfo> dictionaries)
		{
			_dictionaries = dictionaries;
			_spells = new List<Hunspell>(dictionaries.Count);

			foreach (var dic in dictionaries)
				_spells.Add(new Hunspell(dic.HunspellAffFile, dic.HunspellDictFile));
		}
		public void Dispose()
		{
			foreach (var spell in _spells)
				spell.Dispose();
		}
		public List<string> Suggest(string word)
		{
			var words = new List<string>();
			foreach (var spell in _spells)
				words.AddRange(spell.Suggest(word));

			return words;
		}
		static void MoveItem<T>(List<T> list, int index)
		{
			var tmp = list[index];
			list[index] = list[index - 1];
			list[index - 1] = tmp;
		}
		public bool Spell(string word)
		{
			for (int i = 0; i < _spells.Count; ++i)
			{
				// wrong word or dictionary
				if (!_spells[i].Spell(word))
					continue;

				// update the hit count and move the winner
				++_dictionaries[i].HitCount;
				while (i > 0 && _dictionaries[i].HitCount > _dictionaries[i - 1].HitCount)
				{
					MoveItem(_dictionaries, i);
					MoveItem(_spells, i);
					--i;
				}

				// correct
				return true;
			}

			return false;
		}
	}
}
