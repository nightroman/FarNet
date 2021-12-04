
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

namespace FarNet
{
	/// <summary>
	/// Validates and completes data.
	/// </summary>
	/// <seealso cref="ModuleSettings{T}"/>
	public interface IValidate
	{
		/// <summary>
		/// Validates and completes data.
		/// </summary>
		void Validate();
	}
}
