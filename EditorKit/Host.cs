using FarNet;

namespace EditorKit;

public sealed class Host : ModuleHost
{
	private static Point _LCPos;
	private static bool _MouseMenu;
	private static bool _MouseSelection;

	public override bool ToUseEditors => true;
	public override void UseEditors()
	{
		var settings = Settings.Default.GetData();
		_MouseMenu = settings.MouseMenu;
		_MouseSelection = settings.MouseSelection;

		if (_MouseSelection)
			Far.Api.AnyEditor.GotFocus += AnyEditor_GotFocus;

		if (_MouseSelection)
			Far.Api.AnyEditor.MouseMove += AnyEditor_MouseMove;

		if (_MouseSelection || _MouseMenu)
			Far.Api.AnyEditor.MouseClick += AnyEditor_MouseClick;
	}

	private void AnyEditor_GotFocus(object? sender, EventArgs e)
	{
		// reset the last position
		_LCPos = new Point(-1);
	}

	private void AnyEditor_MouseClick(object? sender, MouseEventArgs e)
	{
		var m = e.Mouse;
		if (m.Buttons == MouseButtons.Left)
		{
			if (!_MouseSelection)
				return;

			if (m.Is())
			{
				// just keep the position
				var editor = (IEditor)sender!;
				_LCPos = editor.ConvertPointScreenToEditor(m.Where);
				return;
			}

			if (m.IsShift())
			{
				// select text from the last to current position
				e.Ignore = true;
				var editor = (IEditor)sender!;
				var p1 = _LCPos;
				if (p1.X < 0)
					p1 = editor.Caret;
				var p2 = editor.ConvertPointScreenToEditor(m.Where);
				editor.SelectText(p1.X, p1.Y, p2.X, p2.Y);
				editor.Redraw();
			}
		}
		else if (m.Buttons == MouseButtons.Right)
		{
			if (!_MouseMenu)
				return;

			// show the menu

			e.Ignore = true;
			var editor = (IEditor)sender!;
			var selectionExists = editor.SelectionExists;

			var menu = Far.Api.CreateMenu();
			menu.AutoAssignHotkeys = true;
			menu.NoMargin = true;
			menu.X = m.Where.X;
			menu.Y = m.Where.Y;

			FarItem it;

			it = menu.Add("Cut");
			it.Disabled = !selectionExists;
			it.Click = delegate
			{
				Far.Api.CopyToClipboard(editor.GetSelectedText());
				editor.DeleteText();
			};

			it = menu.Add("Copy");
			it.Disabled = !selectionExists;
			it.Click = delegate
			{
				Far.Api.CopyToClipboard(editor.GetSelectedText());
			};

			it = menu.Add("Paste");
			it.Click = delegate
			{
				if (selectionExists)
					editor.DeleteText();

				editor.InsertText(Far.Api.PasteFromClipboard());
			};

			it = menu.Add("Copy base");
			it.Click = delegate
			{
				Far.Api.CopyToClipboard(Path.GetFileNameWithoutExtension(editor.FileName));
			};

			it = menu.Add("Copy name");
			it.Click = delegate
			{
				Far.Api.CopyToClipboard(Path.GetFileName(editor.FileName));
			};

			it = menu.Add("Copy path");
			it.Click = delegate
			{
				Far.Api.CopyToClipboard(editor.FileName);
			};

			menu.Show();
		}
	}

	private void AnyEditor_MouseMove(object? sender, MouseEventArgs e)
	{
		var m = e.Mouse;
		if (m.Buttons != MouseButtons.Left)
			return;

		if (m.Is())
		{
			// drag, select text from the last to current position
			var p1 = _LCPos;
			if (p1.X >= 0)
			{
				e.Ignore = true;
				var editor = (IEditor)sender!;
				var p2 = editor.ConvertPointScreenToEditor(m.Where);
				editor.SelectText(p1.X, p1.Y, p2.X, p2.Y);
				editor.Redraw();
			}
		}
	}
}
