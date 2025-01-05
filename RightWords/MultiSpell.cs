using System;
using System.Collections.Generic;
using System.IO;
using WeCantSpell.Hunspell;

namespace RightWords;

sealed class MultiSpell
{
	static MultiSpell? _instance;
	readonly List<DictionaryInfo> _dictionaries;

	public static MultiSpell Get()
	{
		return _instance ??= new MultiSpell(Actor.Dictionaries);
	}

	MultiSpell(List<DictionaryInfo> dictionaries)
	{
		// system dictionaries and spellers
		_dictionaries = dictionaries;

		// reset hit counters
		foreach (var dictionary in dictionaries)
			dictionary.HitCount = 0;

		// user dictionaries
		foreach (var dic in dictionaries)
		{
			var spell = WordList.CreateFromFiles(dic.HunspellDictFile, dic.HunspellAffFile);
			dic.WordList = spell;

			var userWordsPath = Actor.GetUserDictionaryPath(dic.Language, false);
			if (File.Exists(userWordsPath))
				dic.UserList = ReadUserWords(userWordsPath, dic.WordList, null);
		}
	}

	public static WordList ReadUserWords(string filePath, WordList wordList, Action<string>? writeWarning)
	{
		var builder = new WordList.Builder(wordList.Affix);
		using (var reader = File.OpenText(filePath))
		{
			while (reader.ReadLine() is { } line)
			{
				var words = line.Split(' ');
				if (words.Length == 1)
				{
					builder.Add(words[0]);
				}
				else if (words.Length == 2)
				{
					var details = wordList[words[1]];
					if (details.Length == 0)
					{
						builder.Add(words[0]);
						if (writeWarning is { })
							writeWarning($"No forms of {words[1]} in '{line}' at '{filePath}'.");
					}
					else
					{
						foreach (var detail in details)
							builder.Add(words[0], detail);
					}
				}
				else
				{
					throw new Exception($"Unexpected line '{line}' in '{filePath}'.");
				}
			}
		}
		return builder.ToImmutable();
	}

	public List<string> Suggest(string word)
	{
		var result = new List<string>();

		foreach (var dic in _dictionaries)
			foreach (var suggestion in dic.WordList!.Suggest(word))
				if (!result.Contains(suggestion))
					result.Add(suggestion);

		return result;
	}

	public List<string> Suggest(ReadOnlySpan<char> word)
	{
		var result = new List<string>();

		foreach (var dic in _dictionaries)
		{
			foreach (var suggestion in dic.WordList!.Suggest(word))
			{
				if (!result.Contains(suggestion))
					result.Add(suggestion);
			}
		}

		return result;
	}

	static void MoveItem<T>(List<T> list, int index)
	{
		(list[index - 1], list[index]) = (list[index], list[index - 1]);
	}

	public bool Check(ReadOnlySpan<char> word)
	{
		for (int i = 0; i < _dictionaries.Count; ++i)
		{
			var dic = _dictionaries[i];

			// wrong word or dictionary
			if (!dic.WordList!.Check(word) && (dic.UserList is null || !dic.UserList.Check(word)))
				continue;

			// update the hit count and move the winner
			++dic.HitCount;
			while (i > 0 && dic.HitCount > _dictionaries[i - 1].HitCount)
			{
				MoveItem(_dictionaries, i);
				--i;
			}

			// correct
			return true;
		}

		return false;
	}

	public DictionaryInfo GetSpell(string language)
	{
		for (int i = 0; i < _dictionaries.Count; ++i)
			if (_dictionaries[i].Language == language)
				return _dictionaries[i];

		throw new InvalidOperationException();
	}
}
