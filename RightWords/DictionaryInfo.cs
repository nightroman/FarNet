
// FarNet module RightWords
// Copyright (c) Roman Kuzmin

using WeCantSpell.Hunspell;

namespace FarNet.RightWords;

class DictionaryInfo
{
	public string Language;
	public string HunspellAffFile;
	public string HunspellDictFile;
	public int HitCount;
	public WordList WordList;
	public WordList UserList;
}
