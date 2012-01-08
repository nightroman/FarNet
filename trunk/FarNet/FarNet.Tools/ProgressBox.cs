
/*
FarNet.Tools library for FarNet
Copyright (c) 2010 Roman Kuzmin
*/

using System;

namespace FarNet.Tools
{
	/// <summary>
	/// Not modal progress message box.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Always call <see cref="Dispose"/> in order to hide the box, restore the original screen, and release internal data.
	/// If restoring of the original screen is not enough, e.g. some changes are expected, then consider to redraw the
	/// context. <see cref="IUserInterface.Redraw()"/> will do but this is not always the most effective way.
	/// </para>
	/// <para>
	/// Create the box, set its <see cref="Activity"/>, call <see cref="SetProgressValue"/> and <see cref="ShowProgress"/>.
	/// Use the <see cref="IUserInterface.ReadKeys"/> method to check for some keys that should break the job.
	/// </para>
	/// </remarks>
	public sealed class ProgressBox : IProgress, IDisposable
	{
		int _savedScreen;
		readonly Progress _progress = new Progress();
		readonly string _title = Far.Net.UI.WindowTitle;
		/// <summary>
		/// Gets or sets the progress box title.
		/// </summary>
		public string Title { get; set; }
		/// <summary>
		/// New progress box.
		/// </summary>
		public ProgressBox() { }
		/// <summary>
		/// New progress box with the title.
		/// </summary>
		/// <param name="title">The progress box title.</param>
		public ProgressBox(string title)
		{
			Title = title;
		}
		/// <summary>
		/// Gets or sets text line count.
		/// </summary>
		/// <remarks>
		/// It should be set before the show.
		/// The default is 0 (automatic line count).
		/// </remarks>
		public int LineCount { get; set; }
		/// <summary>
		/// Disposes the resources and hides the message box after use.
		/// </summary>
		public void Dispose()
		{
			Hide();
			Far.Net.UI.WindowTitle = _title;
		}
		void Hide()
		{
			if (_savedScreen != 0)
			{
				Far.Net.UI.RestoreScreen(_savedScreen);
				_savedScreen = 0;
			}
		}
		#region IProgress
		/// <summary>
		/// Gets or sets the current activity description.
		/// </summary>
		/// <remarks>
		/// The first line is also used to update the window title.
		/// </remarks>
		public string Activity
		{
			get { return _progress.Activity; }
			set { _progress.Activity = value; }
		}
		/// <summary>
		/// Sets the current progress information.
		/// </summary>
		/// <param name="currentValue">Progress current value, from 0 to the maximum.</param>
		/// <param name="maximumValue">Progress maximum value, positive or 0.</param>
		public void SetProgressValue(double currentValue, double maximumValue)
		{
			_progress.SetProgressValue(currentValue, maximumValue);
		}
		/// <summary>
		/// Tells to update the progress.
		/// </summary>
		public void ShowProgress()
		{
			Hide();
			_savedScreen = Far.Net.UI.SaveScreen(0, 0, -1, -1);

			string progress;
			var lines = _progress.Build(out progress, LineCount);
			string text;
			if (lines.Length == 1)
				text = lines[0] + "\r" + progress;
			else
				text = string.Join("\r", lines) + "\r" + progress;

			Far.Net.Message(text, Title, MessageOptions.Draw | MessageOptions.LeftAligned);

			_lastShow = _progress.Elapsed;
		}
		/// <summary>
		/// Gets the time elapsed from the last show.
		/// </summary>
		public TimeSpan ElapsedFromShow
		{
			get { return _progress.Elapsed - _lastShow; }
		}
		/// <summary>
		/// Gets the time elapsed from the start.
		/// </summary>
		public TimeSpan ElapsedFromStart
		{
			get { return _progress.Elapsed; }
		}
		TimeSpan _lastShow;
		#endregion
	}
}
