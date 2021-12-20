
// FarNet module Drawer
// Copyright (c) Roman Kuzmin

using System;

namespace FarNet.Drawer
{
	public sealed class Settings : ModuleSettings<Settings.Data>
	{
		public const string CurrentWordGuid = "a9a6f877-e049-4438-a315-d5914b200988";
		public const string CurrentWordName = "Current word";
		public const string FixedColumnGuid = "efe9454e-0284-4047-ba74-a00685fe40a6";
		public const string FixedColumnName = "Fixed column";

		public static Settings Default { get; } = new Settings();

		[Serializable]
		public class Data
		{
			public CurrentWord CurrentWord { get; set; } = new CurrentWord();
			public FixedColumn FixedColumn { get; set; } = new FixedColumn();
		}

		[Serializable]
		public class CurrentWord
		{
			public XmlCData WordRegex { get; set; } = @"\w[-\w]*";

			public bool ExcludeCurrent { get; set; }

			public ConsoleColor ColorForeground { get; set; } = ConsoleColor.Black;

			public ConsoleColor ColorBackground { get; set; } = ConsoleColor.Gray;
		}

		[Serializable]
		public class FixedColumn
		{
			public int[] ColumnNumbers { get; set; } = new int[] { 80, 120 };

			public ConsoleColor ColorForeground { get; set; } = ConsoleColor.Black;

			public ConsoleColor ColorBackground { get; set; } = ConsoleColor.Gray;
		}
	}
}
