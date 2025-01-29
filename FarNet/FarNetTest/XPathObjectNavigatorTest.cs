using FarNet.Tools;
using System;
using System.Collections;
using System.Globalization;

namespace FarNetTest;

public class XPathObjectNavigatorTest
{
	static readonly Any1 RootObject = new();

	[Fact]
	public void TreeXml()
	{
		var nav = new XPathObjectNavigator(RootObject, -1);
		var res = nav.InnerXml;
		Assert.Equal(ExpectedXml, res);
	}

	[Fact]
	public void ValuePredicate()
	{
		var nav = new XPathObjectNavigator(RootObject, -1);
		var res = nav.Select("//data[.=1]").Cast<XPathObjectNavigator>().First();
		Assert.Equal("""<data name="BoolTrue" type="Boolean">1</data>""", res.OuterXml);
	}

	// how to use variable and underlying object
	[Fact]
	public void Variable()
	{
		var nav = new XPathObjectNavigator(RootObject, -1);
		var xp = nav.Compile("//data[.=$version]", new Hashtable() { { "version", "1.2.3" } });
		var res = nav.SelectSingleNode(xp);
		Assert.Equal(Version.Parse("1.2.3"), res?.UnderlyingObject);
	}

	// function `equals`, helper with ignore case
	[Fact]
	public void Function_equals()
	{
		var nav = new XPathObjectNavigator(RootObject, -1);
		var xp = nav.Compile("//data[equals(., \"BAR\")]");
		var res = nav.Select(xp).Cast<XPathObjectNavigator>().ToList();
		Assert.Equal(3, res.Count);
	}

	// function `regex`, with inline options
	[Fact]
	public void Function_regex()
	{
		var nav = new XPathObjectNavigator(RootObject, -1);
		var xp = nav.Compile("//data[is-match(., \"(?i)^BA\")]");
		var res = nav.Select(xp).Cast<XPathObjectNavigator>().ToList();
		Assert.Equal(3, res.Count);
	}

	// function `compare` strings, less < 0, equal = 0, greater > 0
	[Fact]
	public void Function_compare()
	{
		var nav = new XPathObjectNavigator(RootObject, -1);
		var xp = nav.Compile("//data[@type=\"DateTime\" and compare(., \"2024-01-01\") < 0]");
		var res = nav.Select(xp).Cast<XPathObjectNavigator>().ToList();
		Assert.Single(res);
	}

	// with set depth, values are returned but deeper elements are not
	[Fact]
	public void Depth1()
	{
		var nav = new XPathObjectNavigator(RootObject, 1);
		var res = nav.InnerXml;
		Assert.Contains("""<data name="String" type="String">bar</data>""", res);
		Assert.Contains("""<item name="Any" type="Any2" />""", res);
		Assert.Contains("""<list name="Collection" type="Object[]" />""", res);
		Assert.Contains("""<list name="Dictionary" type="OrderedDictionary" />""", res);
	}

	// with unlimited depth, values are returned but deeper elements are not
	[Fact]
	public void AvoidLoops()
	{
		var root = new Hashtable();
		root.Add("loop", root);

		// unlimited depth
		{
			var nav = new XPathObjectNavigator(root, -1);
			var res = nav.InnerXml;
			Assert.Contains("""  <list name="loop" type="Hashtable" />""", res);
		}

		// limited depth
		{
			var nav = new XPathObjectNavigator(root, 2);
			var res = nav.InnerXml;
			Assert.Contains("""  <list name="loop" type="Hashtable">""", res);
			Assert.Contains("""    <list name="loop" type="Hashtable" />""", res);
		}
	}

	public class Any1
	{
		public string String = "bar";
		public bool BoolTrue = true;
		public bool BoolFalse = false;
		public DateTime Old = DateTime.Parse("2000-01-01");
		public DateTime New = DateTime.Parse("2024-01-04 01:01:01.12345");
		public int Int = 42;
		public Version Version = Version.Parse("1.2.3");
		public CultureInfo Culture = new("en-GB");
		public Version? NullProperty = null;
		public Any2 Any = new();
		public object ValueTuple = System.ValueTuple.Create("p1", 42);
		public object?[] Collection = [42, "bar", null];
		public object Dictionary = new System.Collections.Specialized.OrderedDictionary() { { "Int", 42 }, { "String", "bar" }, { "NullValue", null } };
	}

	public class Any2
	{
		public string Name = "May";
		public int Age = 33;
	}

	const string ExpectedXml = """
		<item type="Any1">
		  <data name="String" type="String">bar</data>
		  <data name="BoolTrue" type="Boolean">1</data>
		  <data name="BoolFalse" type="Boolean">0</data>
		  <data name="Old" type="DateTime">2000-01-01</data>
		  <data name="New" type="DateTime">2024-01-04 01:01:01</data>
		  <data name="Int" type="Int32">42</data>
		  <data name="Version" type="Version">1.2.3</data>
		  <data name="Culture" type="CultureInfo">en-GB</data>
		  <data name="NullProperty" type="Version"></data>
		  <item name="Any" type="Any2">
		    <data name="Name" type="String">May</data>
		    <data name="Age" type="Int32">33</data>
		  </item>
		  <item name="ValueTuple" type="ValueTuple`2">
		    <data name="Item1" type="String">p1</data>
		    <data name="Item2" type="Int32">42</data>
		  </item>
		  <list name="Collection" type="Object[]">
		    <data type="Int32">42</data>
		    <data type="String">bar</data>
		    <data type="Object"></data>
		  </list>
		  <list name="Dictionary" type="OrderedDictionary">
		    <data name="Int" type="Int32">42</data>
		    <data name="String" type="String">bar</data>
		    <data name="NullValue" type="Object"></data>
		  </list>
		</item>
		""";
}
