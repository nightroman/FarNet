using System;

namespace FarManager.Impl
{
	public class MenuItem : IMenuItem
	{
		string _text;
		bool _selected;
		bool _checked;
		bool _isSeparator;
		object _data;

		public event EventHandler OnClick;

		public string Text
		{
			get { return _text; }
			set { _text = value; }
		}

		public bool Selected
		{
			get { return _selected; }
			set { _selected = value; }
		}

		public bool Checked
		{
			get { return _checked; }
			set { _checked = value; }
		}

		public bool IsSeparator
		{
			get { return _isSeparator; }
			set { _isSeparator = value; }
		}

		public object Data
		{
			get { return _data; }
			set { _data = value; }
		}

		public void FireOnClick()
		{
			if (OnClick != null)
				OnClick(this, new EventArgs());
		}

		public void Add(EventHandler handler)
		{
			OnClick += handler;
		}

		public override string ToString()
		{
			return Text;
		}
	}
}
