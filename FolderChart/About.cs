
// FarNet module FolderChart
// Copyright (c) Roman Kuzmin

using System;
using System.Globalization;
using System.Windows.Forms;

namespace FolderChart;

class FolderItem
{
	public string Name;
	public long Size;
}

class WindowWrapper(IntPtr hwnd) : IWin32Window
{
	public IntPtr Handle { get; } = hwnd;
}

static class Kit
{
	public static string FormatSize(double size, string path)
	{
		return FormatSize(size) + " ~ " + path;
	}
	static string FormatSize(double size)
	{
		if (size < 1024)
			return size.ToString(CultureInfo.InvariantCulture);

		if (size < 1024 * 1024)
			return string.Format(CultureInfo.InvariantCulture, "{0:n2} K", size / 1024);

		if (size < 1024 * 1024 * 1024)
			return string.Format(CultureInfo.InvariantCulture, "{0:n2} M", size / (1024 * 1024));

		return string.Format(CultureInfo.InvariantCulture, "{0:n2} G", size / (1024 * 1024 * 1024));
	}
}
