
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

namespace FarNet
{
	/// <summary>
	/// Implements the validation method.
	/// </summary>
	public interface IValidate
	{
		/// <summary>
		/// Validates and completes data and throws errors on issues.
		/// </summary>
		void Validate();
	}
}
