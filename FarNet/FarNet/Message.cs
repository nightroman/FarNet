// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

namespace FarNet;

/// <summary>
/// Parameters of <see cref="IFar.Message(MessageArgs)"/>.
/// </summary>
public class MessageArgs
{
	/// <summary>
	/// Message text.
	/// </summary>
	public string Text { get; set; }

	/// <summary>
	/// Message caption.
	/// </summary>
	public string Caption { get; set; }

	/// <summary>
	/// Message options.
	/// </summary>
	public MessageOptions Options { get; set; }

	/// <summary>
	/// Message buttons. Not supported with <c>Gui*</c> options.
	/// </summary>
	public string[] Buttons { get; set; }

	/// <summary>
	/// <include file='doc.xml' path='doc/HelpTopic/*'/>
	/// It is ignored in GUI and drawn messages.
	/// </summary>
	public string HelpTopic { get; set; }

	/// <summary>
	/// Message position.
	/// </summary>
	public Point? Position { get; set; }
}
