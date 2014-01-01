
/*
FarNet module FolderChart
Copyright (c) 2010-2014 Roman Kuzmin
*/

using System;
using System.Globalization;
using System.Windows.Forms;

class FolderItem
{
	public string Name { get; set; }
	public long Size { get; set; }
}

class WindowWrapper : IWin32Window
{
	public IntPtr Handle { get; private set; }
	public WindowWrapper(IntPtr hwnd) { Handle = hwnd; }
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
			return String.Format(CultureInfo.InvariantCulture, "{0:n2} K", size / 1024);

		if (size < 1024 * 1024 * 1024)
			return String.Format(CultureInfo.InvariantCulture, "{0:n2} M", size / (1024 * 1024));

		return String.Format(CultureInfo.InvariantCulture, "{0:n2} G", size / (1024 * 1024 * 1024));
	}
}
