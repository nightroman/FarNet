///
		// how to find if Colorer works without Far settings
		// not cached, the public method is cached in C++
		public static bool HasColorer(IEditor editor)
		{
			var guidColorer = new Guid("d2f36b62-a470-418d-83a3-ed7a3710e5b5");
			var lineStart = editor.Frame.VisibleLine;
			var lineEnd = lineStart + editor.WindowSize.Y;
			var colors = new List<EditorColorInfo>();
			for (int i = lineStart; i < lineEnd; ++i)
			{
				editor.GetColors(i, colors);
				if (colors.Count > 0)
					return colors.FirstOrDefault(x => x.Owner == guidColorer) != null;
			}
			return false;
		}
///
