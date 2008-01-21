/*
FAR.NET plugin for Far Manager
Copyright (c) 2005-2007 FAR.NET Team
*/

using System;

namespace FarManager
{
	/// <summary>
	/// FAR viewer interface. It is created by <see cref="IFar.CreateViewer"/>.
	/// </summary>
	public interface IViewer
	{
		/// <summary>
		/// Delete a directory with a file when it is closed and it is the only file there.
		/// It is read only when a viewer is opened.
		/// </summary>
		/// <seealso cref="DeleteOnlyFileOnClose"/>
		bool DeleteOnClose { get; set; }
		/// <summary>
		/// Delete a file when it is closed.
		/// It is read only when a viewer is opened.
		/// </summary>
		/// <seealso cref="DeleteOnClose"/>
		bool DeleteOnlyFileOnClose { get; set; }
		/// <summary>
		/// Enable switching to viewer.
		/// It is read only when a viewer is opened.
		/// </summary>
		bool EnableSwitch { get; set; }
		/// <summary>
		/// Do not use viewer history.
		/// It is read only when a viewer is opened.
		/// </summary>
		bool DisableHistory { get; set; }
		/// <summary>
		/// Name of a file being edited.
		/// It is read only when a viewer is opened.
		/// </summary>
		string FileName { get; set; }
		/// <summary>
		/// Viewer window position.
		/// </summary>
		Place Window { get; set; }
		/// <summary>
		/// Viewer window title. Set it before opening.
		/// </summary>
		string Title { get; set; }
		/// <summary>
		/// Opens the viewer using properties:
		/// <see cref="FileName"/>,
		/// <see cref="Title"/>,
		/// <see cref="DeleteOnClose"/>,
		/// <see cref="DeleteOnlyFileOnClose"/>,
		/// <see cref="DisableHistory"/>,
		/// <see cref="EnableSwitch"/>,
		/// </summary>
		void Open(OpenMode mode);
		/// <summary>
		/// Obsolete. Use <see cref="Open(OpenMode)"/>.
		/// </summary>
		[Obsolete("Use Open(OpenMode).")]
		void Open();
		/// <summary>
		/// Obsolete. Use <see cref="Open(OpenMode)"/>.
		/// </summary>
		[Obsolete("Use Open(OpenMode).")]
		bool Async { get; set; }
		/// <summary>
		/// Obsolete. Use <see cref="Open(OpenMode)"/>.
		/// </summary>
		[Obsolete("Use Open(OpenMode).")]
		bool IsModal { get; set; }
	}

	/// <summary>
	/// Open modes of editor and viewer.
	/// </summary>
	public enum OpenMode
	{
		/// <summary>
		/// Tells to open not modal editor or viewer and return immediately.
		/// </summary>
		None,
		/// <summary>
		/// Tells to open not modal editor or viewer and wait for exit.
		/// </summary>
		Wait,
		/// <summary>
		/// Tells to open modal editor or viewer.
		/// </summary>
		Modal
	}
}
