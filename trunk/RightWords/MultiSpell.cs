
/*
FarNet module RightWords
Copyright (c) 2011-2012 Roman Kuzmin
*/

using System;
using System.Collections.Generic;
using System.IO;
using NHunspell;
namespace FarNet.RightWords
{
	sealed class MultiSpell
	{
		static readonly WeakReference _instance = new WeakReference(null);
		readonly List<DictionaryInfo> _dictionaries;
		readonly List<Hunspell> _spells;
		public static MultiSpell Get()
		{
			var spell = (MultiSpell)_instance.Target;
			if (spell == null)
			{
				spell = new MultiSpell(Actor.Dictionaries);
				_instance.Target = spell;
			}
			return spell;
		}
		MultiSpell(List<DictionaryInfo> dictionaries)
		{
			Log.Source.TraceInformation("Loading RightWords data");

			// system dictionaries and spellers
			_dictionaries = dictionaries;
			_spells = new List<Hunspell>(dictionaries.Count);

			// reset hit counters
			foreach (var dictionary in dictionaries)
				dictionary.HitCount = 0;

			// user dictionaries
			foreach (var dic in dictionaries)
			{
				var spell = new Hunspell(dic.HunspellAffFile, dic.HunspellDictFile);
				_spells.Add(spell);

				var userWordsPath = Actor.GetUserDictionaryPath(dic.Language, false);
				if (File.Exists(userWordsPath))
				{
					using (var reader = File.OpenText(userWordsPath))
					{
						string line;
						while((line = reader.ReadLine()) != null)
						{
							var words = line.Split(' ');
							if (words.Length == 1)
								spell.Add(words[0]);
							else if (words.Length == 2)
								spell.AddWithAffix(words[0], words[1]);
						}
					}
				}
			}
		}
		~MultiSpell()
		{
			Log.Source.TraceInformation("Disposing RightWords data");

			foreach (var spell in _spells)
				spell.Dispose();
		}
		public List<string> Suggest(string word)
		{
			var result = new List<string>();

			foreach (var spell in _spells)
				foreach (var suggestion in spell.Suggest(word))
					if (!result.Contains(suggestion))
						result.Add(suggestion);

			return result;
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
		public Hunspell GetSpell(string language)
		{
			for (int i = 0; i < _spells.Count; ++i)
				if (_dictionaries[i].Language == language)
					return _spells[i];

			throw new InvalidOperationException();
		}
	}
}
