
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

using System;
using System.Collections.Generic;

namespace FarNet.Works;
#pragma warning disable 1591

public abstract class AnyMenu : IAnyMenu
{
	protected readonly List<FarItem> myItems = [];

	protected readonly List<KeyData> myKeys = [];

	protected readonly List<EventHandler<MenuEventArgs>?> myHandlers = [];

	protected int myKeyIndex = -1;

	public bool AutoAssignHotkeys { get; set; }

	public bool NoShadow { get; set; }

	public bool SelectLast { get; set; }

	public bool ShowAmpersands { get; set; }

	public bool WrapCursor { get; set; }

	public int MaxHeight { get; set; }

	public int Selected { get; set; }

	public int X { get; set; }

	public int Y { get; set; }

	public IList<FarItem> Items { get { return myItems; } }

	public object? SelectedData { get { return Selected < 0 || Selected >= myItems.Count ? null : myItems[Selected].Data; } }

	public object? Sender { get; set; }

	public string? Bottom { get; set; }

	public string? HelpTopic { get; set; }

	public string? Title { get; set; }

	public abstract bool Show();

	public AnyMenu()
	{
		X = -1;
		Y = -1;
		Selected = -1;
		WrapCursor = true; //! default is true, as recommended by Far
	}

	public FarItem Add(string text)
	{
		return Add(text, null);
	}

	public FarItem Add(string text, EventHandler<MenuEventArgs>? click)
	{
		var r = new SetItem()
		{
			Text = text,
			Click = click
		};
		myItems.Add(r);
		return r;
	}

	public KeyData Key
	{
		get
		{
			return myKeyIndex < 0 ? KeyData.Empty : myKeys[myKeyIndex];
		}
	}

	public void AddKey(int virtualKeyCode)
	{
		AddKey(virtualKeyCode, ControlKeyStates.None, null);
	}

	public void AddKey(int virtualKeyCode, ControlKeyStates controlKeyState)
	{
		AddKey(virtualKeyCode, controlKeyState, null);
	}

	public void AddKey(int virtualKeyCode, ControlKeyStates controlKeyState, EventHandler<MenuEventArgs>? handler)
	{
		myKeys.Add(new KeyData(virtualKeyCode, controlKeyState));
		myHandlers.Add(handler);
	}
}
