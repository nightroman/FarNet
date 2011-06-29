﻿
/*
FarNet module RightWords
Copyright (c) 2011 Roman Kuzmin
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
		readonly List<Hunspell> _spells;
		readonly List<DictionaryInfo> _dictionaries;
		public static MultiSpell GetWeakInstance(List<DictionaryInfo> dictionaries)
		{
			var spell = (MultiSpell)_instance.Target;
			if (spell != null)
				return spell;

			spell = new MultiSpell(dictionaries);
			_instance.Target = spell;
			return spell;
		}
		MultiSpell(List<DictionaryInfo> dictionaries)
		{
			Log.Source.TraceInformation("Loading RightWords data");
			
			_dictionaries = dictionaries;
			_spells = new List<Hunspell>(dictionaries.Count);

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
		public Hunspell GetSpell(string language)
		{
			for (int i = 0; i < _spells.Count; ++i)
				if (_dictionaries[i].Language == language)
					return _spells[i];
			
			throw new InvalidOperationException();
		}
	}
}