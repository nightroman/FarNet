// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

namespace FarNet;

/// <summary>
/// Panel sort mode.
/// </summary>
public enum PanelSortMode
{
	/// <summary>
	/// Default mode.
	/// </summary>
	Default,

	/// <summary>
	/// Unsorted mode.
	/// </summary>
	Unsorted,

	/// <summary>
	/// Sorted by name.
	/// </summary>
	Name,

	/// <summary>
	/// Sorted by extension.
	/// </summary>
	Extension,

	/// <summary>
	/// Sorted by modification time.
	/// </summary>
	LastWriteTime,

	/// <summary>
	/// Sorted by creation time.
	/// </summary>
	CreationTime,

	/// <summary>
	/// Sorted by access time.
	/// </summary>
	LastAccessTime,

	/// <summary>
	/// Sorted by length.
	/// </summary>
	Length,

	/// <summary>
	/// Sorted by description.
	/// </summary>
	Description,

	/// <summary>
	/// Sorted by owner.
	/// </summary>
	Owner,

	/// <summary>
	/// Sorted by compressed size.
	/// </summary>
	CompressedSize,

	/// <summary>
	/// Sorted by hard link count.
	/// </summary>
	LinkCount,

	/// <summary>
	/// Sorted by NTFS stream count.
	/// </summary>
	StreamCount,

	/// <summary>
	/// Sorted by NTFS stream data size.
	/// </summary>
	StreamSize,

	/// <summary>
	/// Sorted by full name.
	/// </summary>
	FullName,

	/// <summary>
	/// Sorted by time of any change.
	/// </summary>
	ChangeTime,

	/// <summary>
	/// Unsorted mode.
	/// </summary>
	UnsortedReversed = -Unsorted,

	/// <summary>
	/// Sorted by name.
	/// </summary>
	NameReversed = -Name,

	/// <summary>
	/// Sorted by extension.
	/// </summary>
	ExtensionReversed = -Extension,

	/// <summary>
	/// Sorted by modification time.
	/// </summary>
	LastWriteTimeReversed = -LastWriteTime,

	/// <summary>
	/// Sorted by creation time.
	/// </summary>
	CreationTimeReversed = -CreationTime,

	/// <summary>
	/// Sorted by access time.
	/// </summary>
	LastAccessTimeReversed = -LastAccessTime,

	/// <summary>
	/// Sorted by length.
	/// </summary>
	LengthReversed = -Length,

	/// <summary>
	/// Sorted by description.
	/// </summary>
	DescriptionReversed = -Description,

	/// <summary>
	/// Sorted by owner.
	/// </summary>
	OwnerReversed = -Owner,

	/// <summary>
	/// Sorted by compressed size.
	/// </summary>
	CompressedSizeReversed = -CompressedSize,

	/// <summary>
	/// Sorted by hard link count.
	/// </summary>
	LinkCountReversed = -LinkCount,

	/// <summary>
	/// Sorted by NTFS stream count.
	/// </summary>
	StreamCountReversed = -StreamCount,

	/// <summary>
	/// Sorted by NTFS stream data size.
	/// </summary>
	StreamSizeReversed = -StreamSize,

	/// <summary>
	/// Sorted by full name.
	/// </summary>
	FullNameReversed = -FullName,

	/// <summary>
	/// Sorted by time of any change.
	/// </summary>
	ChangeTimeReversed = -ChangeTime
}
