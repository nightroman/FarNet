
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

using System;

namespace FarNet.Tools;

class ValueGetter(string name, Func<object, object?> value)
{
	public string Name { get; } = name;

	public Func<object, object?> Value { get; } = value;
}
