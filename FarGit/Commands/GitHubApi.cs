using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace FarGit.Commands;

/// <summary>
/// Minimal GitHub REST API v3 client.
/// Uses synchronous HttpClient.Send so it fits naturally into FAR's synchronous plugin callbacks.
/// </summary>
static class GitHubApi
{
	static readonly HttpClient Http = new();

	// ── Public API ────────────────────────────────────────────────────────────

	/// <summary>
	/// Creates a new repository under the authenticated user's account.
	/// Returns the HTTPS clone URL.
	/// </summary>
	public static string CreateRepo(string pat, string name, bool isPrivate)
	{
		var body = JsonSerializer.Serialize(new { name, description = "", @private = isPrivate });
		using var req = BuildRequest(HttpMethod.Post, "https://api.github.com/user/repos", pat, body);
		using var resp = Http.Send(req);
		var json = ReadBody(resp);
		EnsureSuccess(resp.StatusCode, json);

		using var doc = JsonDocument.Parse(json);
		return doc.RootElement.GetProperty("clone_url").GetString()
			?? throw new Exception("GitHub did not return a clone URL.");
	}

	/// <summary>Validates a PAT and returns the GitHub login name.</summary>
	public static string GetUsername(string pat)
	{
		using var req = BuildRequest(HttpMethod.Get, "https://api.github.com/user", pat);
		using var resp = Http.Send(req);
		var json = ReadBody(resp);
		EnsureSuccess(resp.StatusCode, json);

		using var doc = JsonDocument.Parse(json);
		return doc.RootElement.GetProperty("login").GetString()
			?? throw new Exception("Could not read GitHub username from API response.");
	}

	// ── Helpers ───────────────────────────────────────────────────────────────

	static HttpRequestMessage BuildRequest(HttpMethod method, string url, string pat, string? jsonBody = null)
	{
		var req = new HttpRequestMessage(method, url);
		req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", pat);
		req.Headers.UserAgent.Add(new ProductInfoHeaderValue("FarGit", "1.0"));
		req.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
		if (jsonBody is not null)
			req.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
		return req;
	}

	static string ReadBody(HttpResponseMessage resp)
	{
		using var reader = new StreamReader(resp.Content.ReadAsStream());
		return reader.ReadToEnd();
	}

	static void EnsureSuccess(System.Net.HttpStatusCode status, string json)
	{
		if ((int)status is >= 200 and < 300) return;

		try
		{
			using var doc = JsonDocument.Parse(json);
			var msg = doc.RootElement.TryGetProperty("message", out var m) ? m.GetString() : null;
			var errors = doc.RootElement.TryGetProperty("errors", out var e)
				? string.Join(", ", e.EnumerateArray()
					.Select(x => x.TryGetProperty("message", out var em) ? em.GetString() : null)
					.Where(s => s is not null))
				: null;
			var detail = string.IsNullOrEmpty(errors) ? msg : $"{msg} ({errors})";
			throw new Exception($"GitHub error {(int)status}: {detail ?? json}");
		}
		catch (JsonException)
		{
			throw new Exception($"GitHub error {(int)status}: {json}");
		}
	}
}
