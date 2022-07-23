
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

namespace FarNet.Tools;

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
