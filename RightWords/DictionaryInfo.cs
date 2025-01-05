using WeCantSpell.Hunspell;

namespace RightWords;

class DictionaryInfo
{
	public required string Language { get; init; }
	public required string HunspellAffFile { get; init; }
	public required string HunspellDictFile { get; init; }
	public int HitCount { get; set; }
	public WordList? WordList { get; set; }
	public WordList? UserList { get; set; }
}
