/*
Far.NET plugin for Far Manager
Copyright (c) 2005-2007 Far.NET Team
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
		/// Returns control to the calling function immediately after <see cref="Open"/>.
		/// If false continues when a user has closed the viewer.
		/// It is read only when the viewer is opened.
		/// </summary>
		bool Async { get; set; }
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
		/// Disable switching to other windows.
		/// It is read only when a viewer is opened.
		/// </summary>
		bool IsModal { get; set; }
		/// <summary>
		/// Viewer window title. Set it before opening.
		/// </summary>
		string Title { get; set; }
		/// <summary>
		/// Open a viewer using properties:
		/// <see cref="FileName"/>,
		/// <see cref="Title"/>,
		/// <see cref="Async"/>,
		/// <see cref="DeleteOnClose"/>,
		/// <see cref="DeleteOnlyFileOnClose"/>,
		/// <see cref="DisableHistory"/>,
		/// <see cref="EnableSwitch"/>,
		/// <see cref="IsModal"/>,
		/// </summary>
		void Open();
	}
}
