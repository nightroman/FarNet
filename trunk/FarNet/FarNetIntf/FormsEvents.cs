using System;

namespace FarManager.Forms
{
	/// <summary>
	/// Arguments of any event in <see cref="IDialog"/>.
	/// </summary>
	public class AnyEventArgs : EventArgs
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="control">Control involved into this event or null.</param>
		public AnyEventArgs(IControl control)
		{
			_control = control;
		}
		/// <summary>
		/// Control involved into this event or null.
		/// </summary>
		public IControl Control
		{
			get { return _control; }
		}
		IControl _control;
	}

	/// <summary>
	/// Initialized event arguments: <see cref="IDialog"/>.
	/// </summary>
	public sealed class InitializedEventArgs : AnyEventArgs
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="focused">Focused control.</param>
		public InitializedEventArgs(IControl focused)
			: base(focused)
		{
		}
		/// <summary>
		/// Ingore changes.
		/// </summary>
		public bool Ignore
		{
			get { return _ignore; }
			set { _ignore = value; }
		}
		bool _ignore;
	}

	/// <summary>
	/// Closing event arguments: <see cref="IDialog"/>.
	/// </summary>
	public sealed class ClosingEventArgs : AnyEventArgs
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="selected">Selected control.</param>
		public ClosingEventArgs(IControl selected)
			: base(selected)
		{
		}
		/// <summary>
		/// Ingore and Continue the dialog.
		/// </summary>
		public bool Ignore
		{
			get { return _ignore; }
			set { _ignore = value; }
		}
		bool _ignore;
	}

	/// <summary>
	/// LosingFocus event arguments: <see cref="IControl"/>.
	/// </summary>
	public sealed class LosingFocusEventArgs : AnyEventArgs
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="losing">Control losing focus.</param>
		public LosingFocusEventArgs(IControl losing)
			: base(losing)
		{
		}
		/// <summary>
		/// null to allow to lose focus or control you want to pass the focus to.
		/// </summary>
		public IControl Focused
		{
			get { return _focused; }
			set { _focused = value; }
		}
		IControl _focused;
	}

	/// <summary>
	/// Clicked event arguments: <see cref="IButton"/>, <see cref="ICheckBox"/>, <see cref="IRadioButton"/>.
	/// </summary>
	public sealed class ButtonClickedEventArgs : AnyEventArgs
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="button">Button clicked.</param>
		/// <param name="selected">Selected state.</param>
		public ButtonClickedEventArgs(IControl button, int selected)
			: base(button)
		{
			_selected = selected;
		}
		/// <summary>
		/// Selected state:
		/// <see cref="IButton"/>: 0;
		/// <see cref="ICheckBox"/>: 0 (unchecked), 1 (checked) and 2 (undefined for ThreeState);
		/// <see cref="IRadioButton"/>: 0 - for the previous element in the group, 1 - for the active element in the group.
		/// </summary>
		public int Selected
		{
			get { return _selected; }
		}
		int _selected;
		/// <summary>
		/// The message has been handled and it should not be processed by the kernel.
		/// </summary>
		public bool Ignore
		{
			get { return _ignore; }
			set { _ignore = value; }
		}
		bool _ignore;
	}

	/// <summary>
	/// TextChanged event arguments: <see cref="IEdit"/>, <see cref="IComboBox"/>.
	/// </summary>
	public sealed class TextChangedEventArgs : AnyEventArgs
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="edit">Edit control.</param>
		/// <param name="text">New text.</param>
		public TextChangedEventArgs(IControl edit, string text)
			: base(edit)
		{
			_text = text;
		}
		/// <summary>
		/// New text.
		/// </summary>
		public string Text
		{
			get { return _text; }
		}
		string _text;
		/// <summary>
		/// Ignore changes. [NOT USED in Dialog API 1.0]
		/// </summary>
		public bool Ignore
		{
			get { return _ignore; }
			set { _ignore = value; }
		}
		bool _ignore;
	}

	/// <summary>
	/// Internal key event.
	/// </summary>
	public sealed class KeyPressedEventArgs : AnyEventArgs
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="control">Current control.</param>
		/// <param name="code">Internal key code.</param>
		public KeyPressedEventArgs(IControl control, int code)
			: base(control)
		{
			_code = code;
		}
		/// <summary>
		/// Key code.
		/// </summary>
		public int Code
		{
			get { return _code; }
		}
		int _code;
		/// <summary>
		/// Ignore further processing.
		/// </summary>
		public bool Ignore
		{
			get { return _ignore; }
			set { _ignore = value; }
		}
		bool _ignore;
	}

	/// <summary>
	/// Mouse clicked event.
	/// </summary>
	public sealed class MouseClickedEventArgs : AnyEventArgs
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="control">Current control.</param>
		/// <param name="mouse">Mouse info.</param>
		public MouseClickedEventArgs(IControl control, MouseInfo mouse)
			: base(control)
		{
			_mouse = mouse;
		}
		/// <summary>
		/// Mouse info.
		/// </summary>
		public MouseInfo Mouse
		{
			get { return _mouse; }
		}
		MouseInfo _mouse;
		/// <summary>
		/// Ignore further processing.
		/// </summary>
		public bool Ignore
		{
			get { return _ignore; }
			set { _ignore = value; }
		}
		bool _ignore;
	}
}
