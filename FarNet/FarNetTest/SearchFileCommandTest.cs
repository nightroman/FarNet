using FarNet.Tools;

namespace FarNetTest;

public class SearchFileCommandTest
{
	//! use a folder with 2+ subfolders, fixed issue
	static readonly string root = $@"{Environment.GetEnvironmentVariable("FarNetCode")}\JavaScriptFar";
	static readonly string[] all;
	static readonly string[] level0;
	static readonly string[] level1;
	static readonly string[] level2;

	static SearchFileCommandTest()
	{
		all = Directory.GetFileSystemEntries(root, "*", SearchOption.AllDirectories)
			.Select(x => x[(root.Length + 1)..])
			.ToArray();

		level0 = all.Where(x => !x.Contains('\\')).ToArray();
		level1 = all.Where(x => x.Split('\\').Length == 2).ToArray();
		level2 = all.Where(x => x.Split('\\').Length == 3).ToArray();
	}

	static SearchFileCommand CreateSearch()
	{
		return new SearchFileCommand(new FileSystemExplorer(root));
	}

	[Fact]
	public void Defaults()
	{
		var search = CreateSearch();

		Assert.False(search.Bfs);
		Assert.Equal(-1, search.Depth);
	}

	[Theory]
	[InlineData(false, "test-MyLibForJS.fas.ps1")]
	[InlineData(true, "_session.2.mjs")]
	public void All(bool bfs, string last)
	{
		var search = CreateSearch();
		search.Bfs = bfs;
		var res = search.Invoke().ToList();

		Assert.Equal(all.Length, res.Count);
		Assert.Equal(last, res[^1].Name);
	}

	[Theory]
	[InlineData(false, "SessionConfiguration.cs")]
	[InlineData(true, "SessionConfiguration.cs")]
	public void Depth0(bool bfs, string last)
	{
		var search = CreateSearch();
		search.Bfs = bfs;
		search.Depth = 0;
		var res = search.Invoke().ToList();

		Assert.Equal(level0.Length, res.Count);
		Assert.Equal(last, res[^1].Name);
	}

	[Theory]
	[InlineData(false, "test-MyLibForJS.fas.ps1")]
	[InlineData(true, "test-MyLibForJS.fas.ps1")]
	public void Depth1(bool bfs, string last)
	{
		var search = CreateSearch();
		search.Bfs = bfs;
		search.Depth = 1;
		var res = search.Invoke().ToList();

		Assert.Equal(level0.Length + level1.Length, res.Count);
		Assert.Equal(last, res[^1].Name);
	}

	[Theory]
	[InlineData(false, "test-MyLibForJS.fas.ps1")]
	[InlineData(true, "try.mjs")]
	public void Depth2(bool bfs, string last)
	{
		var search = CreateSearch();
		search.Bfs = bfs;
		search.Depth = 2;
		var res = search.Invoke().ToList();

		Assert.Equal(level0.Length + level1.Length + level2.Length, res.Count);
		Assert.Equal(last, res[^1].Name);
	}

	[Fact]
	public void DepthXPath()
	{
		var search = CreateSearch();
		search.XPath = "//*";
		{
			var res = search.Invoke().ToList();
			Assert.Equal(all.Length, res.Count);
		}
		{
			search.Depth = 0;
			var res = search.Invoke().ToList();
			Assert.Equal(level0.Length, res.Count);
		}
		{
			search.Depth = 1;
			var res = search.Invoke().ToList();
			Assert.Equal(level0.Length + level1.Length, res.Count);
		}
		{
			search.Depth = 2;
			var res = search.Invoke().ToList();
			Assert.Equal(level0.Length + level1.Length + level2.Length, res.Count);
		}
	}
}
