using System.Runtime.Versioning;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FarGit.Commands;

/// <summary>
/// Persists per-host git credentials (username + token) encrypted with Windows DPAPI.
/// Stored in %APPDATA%\FarGit\credentials.json — readable only by the current Windows user.
/// </summary>
[SupportedOSPlatform("windows")]
static class Credentials
{
	static string StorePath => Path.Combine(
		Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
		"FarGit", "credentials.json");

	record Entry(
		[property: JsonPropertyName("u")] string Username,
		[property: JsonPropertyName("p")] string Protected);

	static readonly JsonSerializerOptions JsonOpts = new() { WriteIndented = true };

	static Dictionary<string, Entry> Load()
	{
		try
		{
			if (!File.Exists(StorePath)) return [];
			var json = File.ReadAllText(StorePath, Encoding.UTF8);
			return JsonSerializer.Deserialize<Dictionary<string, Entry>>(json, JsonOpts) ?? [];
		}
		catch { return []; }
	}

	static void Save(Dictionary<string, Entry> data)
	{
		Directory.CreateDirectory(Path.GetDirectoryName(StorePath)!);
		File.WriteAllText(StorePath, JsonSerializer.Serialize(data, JsonOpts), Encoding.UTF8);
	}

	static string Protect(string plaintext)
	{
		var bytes = ProtectedData.Protect(Encoding.UTF8.GetBytes(plaintext), null, DataProtectionScope.CurrentUser);
		return Convert.ToBase64String(bytes);
	}

	static string Unprotect(string base64)
	{
		var bytes = ProtectedData.Unprotect(Convert.FromBase64String(base64), null, DataProtectionScope.CurrentUser);
		return Encoding.UTF8.GetString(bytes);
	}

	/// <summary>Returns stored (username, token) for the given host, or null if not saved.</summary>
	public static (string Username, string Token)? Get(string host)
	{
		var data = Load();
		if (!data.TryGetValue(host.ToLowerInvariant(), out var entry)) return null;
		try   { return (entry.Username, Unprotect(entry.Protected)); }
		catch { return null; }
	}

	/// <summary>Saves or replaces credentials for the given host (encrypted with DPAPI).</summary>
	public static void Set(string host, string username, string token)
	{
		var data = Load();
		data[host.ToLowerInvariant()] = new Entry(username, Protect(token));
		Save(data);
	}

	/// <summary>Removes stored credentials. Returns true if an entry existed.</summary>
	public static bool Remove(string host)
	{
		var data = Load();
		if (!data.Remove(host.ToLowerInvariant())) return false;
		Save(data);
		return true;
	}

	/// <summary>Extracts the hostname from a URL, or returns the raw URL on failure.</summary>
	public static string HostOf(string url)
	{
		try { return new Uri(url).Host.ToLowerInvariant(); }
		catch { return url.ToLowerInvariant(); }
	}
}
