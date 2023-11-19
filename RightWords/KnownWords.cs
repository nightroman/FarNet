
// FarNet module RightWords
// Copyright (c) Roman Kuzmin

using System.Collections.Generic;
using System.IO;

namespace FarNet.RightWords;

static class KnownWords
{
	/// <summary>
	/// User selected words to ignore in this session.
	/// </summary>
	static readonly HashSet<string> s_IgnoreWords = [];

	/// <summary>
	/// Cache of the common dictionary words.
	/// </summary>
	static HashSet<string> s_CommonWords_;

	/// <summary>
	/// Get the current version of known words.
	/// </summary>
	public static int Version { get; private set; }

	/// <summary>
	/// Bumps the version, e.g. on adding words to language dictionaries.
	/// </summary>
	public static void BumpVersion()
	{
		++Version;
	}

	/// <summary>
	/// Gets true if the word is known and should be ignored.
	/// </summary>
	public static bool Contains(string word)
	{
		return s_IgnoreWords.Contains(word) || GetCommonWords(false).Contains(word);
	}

	/// <summary>
	/// Adds the word to the session ignore set.
	/// </summary>
	public static void AddIgnoreWord(string word)
	{
		++Version;
		s_IgnoreWords.Add(word);
	}

	/// <summary>
	/// Adds the words to the common word dictionary.
	/// </summary>
	public static void AddCommonWords(string[] newWords)
	{
		// read cache from file, it may be changed externally
		var cache = GetCommonWords(true);

		// add new words to the cache and append to the file
		var path = Path.Combine(Actor.GetUserDictionaryDirectory(true), Settings.UserFile);
		using var writer = File.AppendText(path);
		foreach (var newWord in newWords)
		{
			if (cache.Add(newWord))
			{
				++Version;
				writer.WriteLine(newWord);
			}
		}
	}

	/// <summary>
	/// Ensures the cache and gets the common words from cache/file.
	/// </summary>
	/// <param name="force">Tells to read from the file and refresh the cache.</param>
	static HashSet<string> GetCommonWords(bool force)
	{
		if (s_CommonWords_ != null && !force)
			return s_CommonWords_;

		s_CommonWords_ = [];
		var path = Path.Combine(Actor.GetUserDictionaryDirectory(false), Settings.UserFile);
		if (File.Exists(path))
		{
			foreach (string line in File.ReadAllLines(path))
				s_CommonWords_.Add(line);
		}

		return s_CommonWords_;
	}
}
