namespace FarNet;

/// <summary>
/// Validates and completes data.
/// </summary>
[Obsolete("Use IValidatableObject.")]
public interface IValidate
{
	/// <summary>
	/// Validates and completes data.
	/// </summary>
	void Validate();
}
