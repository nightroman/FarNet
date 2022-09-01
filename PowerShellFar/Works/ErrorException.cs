
// PowerShellFar module for Far Manager
// Copyright (c) Roman Kuzmin

using FarNet;
using System;
using System.Runtime.Serialization;

namespace PowerShellFar;

[Serializable]
class ErrorException : ModuleException
{
	public ErrorException()
	{
	}

	public ErrorException(string message) : base(message)
	{
	}

	public ErrorException(string message, Exception innerException) : base(message, innerException)
	{
	}

	protected ErrorException(SerializationInfo info, StreamingContext context) : base(info, context)
	{
	}
}
