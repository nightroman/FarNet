
/*
FarNet.Tools library for FarNet
Copyright (c) 2010 Roman Kuzmin
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace FarNet.Tools
{
	/// <summary>
	/// Used to update the progress information.
	/// </summary>
	public interface IProgress
	{
		/// <summary>
		/// Gets or sets the current activity description.
		/// </summary>
		string Activity { get; set; }

		/// <summary>
		/// Sets the current progress information.
		/// </summary>
		/// <param name="currentValue">Progress current value, from 0 to the maximum.</param>
		/// <param name="maximumValue">Progress maximum value, positive or 0.</param>
		void SetProgressValue(double currentValue, double maximumValue);

		/// <summary>
		/// Shows the current progress information.
		/// </summary>
		void ShowProgress();
	}

	class Progress
	{
		const char EMPTY_BLOCK = '\x2591';
		const char SOLID_BLOCK = '\x2588';

		internal const int FORM_WIDTH = 76;
		internal const int TEXT_HEIGHT = 10;
		internal const int TEXT_WIDTH = FORM_WIDTH - 10;
		const int PROGRESS_WIDTH = TEXT_WIDTH - 4;

		string _Activity = string.Empty;
		readonly List<string> _Lines = new List<string>(TEXT_HEIGHT);

		int _percentage = -1;
		Stopwatch _stopwatch = Stopwatch.StartNew();

		public string Activity
		{
			get { return _Activity; }
			set
			{
				// only new value drops old data
				if (_Activity != value)
				{
					_Lines.Clear();
					_Activity = value ?? string.Empty;
				}
			}
		}

		public void SetProgressValue(double currentValue, double maximumValue)
		{
			if (currentValue <= 0)
				_percentage = 0;
			if (maximumValue <= 0 || currentValue >= maximumValue)
				_percentage = 100;
			_percentage = (int)(currentValue * 100 / maximumValue);
		}

		internal List<string> Build(out string progress, int height, bool title)
		{
			// format activity if not yet
			if (_Lines.Count == 0)
			{
				// format with limited height
				bool useHeight = height > 0 && height <= TEXT_HEIGHT;
				FarNet.Works.Kit.FormatMessage(
					_Lines,
					_Activity,
					TEXT_WIDTH,
					useHeight ? height : TEXT_HEIGHT,
					FarNet.Works.FormatMessageMode.Cut);
				
				// pad by empty lines
				if (useHeight)
				{
					while (_Lines.Count < height)
						_Lines.Add(string.Empty);
				}
			}

			if (_percentage < 0)
			{
				// time span, mind unwanted milliseconds
				progress = (new TimeSpan(0, 0, (int)_stopwatch.Elapsed.TotalSeconds)).ToString();
				
				// 1) window title
				if (title)
					Far.Net.UI.WindowTitle = progress + " " + _Lines[0];
				
				// 2) ensure fixed box width
				progress = progress.PadRight(TEXT_WIDTH);
			}
			else
			{
				// thermometer
				progress = FormatProgress(_percentage);

				// icon progress
				Far.Net.UI.SetProgressValue(_percentage, 100);

				// window title
				if (title)
					Far.Net.UI.WindowTitle = string.Format(null, "{0}% {1}", _percentage, _Lines[0]);
			}

			return _Lines;
		}

		static string FormatProgress(int percentage)
		{
			// number of chars to fill
			int n = PROGRESS_WIDTH * percentage / 100;

			// do not fill too much
			if (n > PROGRESS_WIDTH)
			{
				n = PROGRESS_WIDTH;
			}
			// leave 1 not filled
			else if (n == PROGRESS_WIDTH)
			{
				if (percentage < 100)
					--n;
			}
			// fill at least 1
			else if (n == 0)
			{
				if (percentage > 0)
					n = 1;
			}

			return new string(SOLID_BLOCK, n) + new string(EMPTY_BLOCK, PROGRESS_WIDTH - n) + string.Format(null, "{0,3}%", percentage);
		}

	}

}
