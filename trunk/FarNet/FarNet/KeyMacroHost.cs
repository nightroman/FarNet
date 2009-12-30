/*
FarNet plugin for Far Manager
Copyright (c) 2005 FarNet Team
*/

using System.Collections;

namespace FarNet
{
	/// <summary>
	/// Key macros host. Exposed as <see cref="IFar.KeyMacro"/>.
	/// </summary>
	/// <remarks>
	/// Important: your macro changes are all in the storage,
	/// not in memory where the current macros are loaded by Far.
	/// Thus, when you work on macros you should normally follow this scheme:
	/// 1) call <see cref="Save"/> (it saves memory macros to the storage);
	/// 2) do your work on macros (remember, you operate on data in the storage);
	/// 3) call <see cref="Load"/> after changes (it gets changes from the storage to memory).
	/// </remarks>
	public interface IKeyMacroHost
	{
		/// <summary>
		/// Posts a macro to Far.
		/// Processing is not displayed, and keys are sent to editor plugins.
		/// </summary>
		/// <param name="macro">Macro text.</param>
		void Post(string macro);
		/// <summary>
		/// Posts a macro to Far.
		/// </summary>
		/// <param name="macro">Macro text.</param>
		/// <param name="enableOutput">Enable screen output during macro playback.</param>
		/// <param name="disablePlugins">Don't send keystrokes to editor plugins.</param>
		void Post(string macro, bool enableOutput, bool disablePlugins);
		/// <summary>
		/// Save all macros from Far memory into the storage.
		/// </summary>
		/// <remarks>
		/// It is recommended to call this before your work on macros.
		/// Otherwise you cannot get not yet saved changes of macros in memory and they can be even lost on your changes.
		/// </remarks>
		void Save();
		/// <summary>
		/// Gets existing area names or macro names from the specified area.
		/// </summary>
		/// <param name="area">Macro area or empty string.</param>
		string[] GetNames(string area);
		/// <summary>
		/// Gets key macro data.
		/// </summary>
		/// <param name="area">Macro area.</param>
		/// <param name="name">Macro name.</param>
		/// <returns>Macro data or null.</returns>
		KeyMacroData GetData(string area, string name);
		/// <summary>
		/// Removes the specified macro.
		/// </summary>
		/// <param name="area">Macro area.</param>
		/// <param name="name">Macro name.</param>
		void Remove(string area, string name);
		/// <summary>
		/// Loads all macros from the storage into Far memory. Previous values are erased.
		/// </summary>
		/// <remarks>
		/// It is recommended to call this when you have finished macro changes.
		/// Otherwise your changes are not yet active and they can be even lost on future saving macros from memory.
		/// </remarks>
		void Load();
		/// <summary>
		/// Installs one macro.
		/// </summary>
		/// <param name="area">Macro area.</param>
		/// <param name="name">Macro name.</param>
		/// <param name="data">Macro data.</param>
		void Install(string area, string name, KeyMacroData data);
		/// <summary>
		/// Installs several macros in batch mode using a set of macro data dictionaries.
		/// </summary>
		/// <param name="dataSet">Set of dictionaries providing portions of macro data incrementally.</param>
		/// <remarks>
		/// Any dictionary provides a portion of macro data and triggers installation if data are ready.
		/// Data are ready when at least these three values are defined: 'Area', 'Name' and 'Sequence'.
		/// <para>
		/// If a dictionary in the sequence is null then all the current data are reset,
		/// so that the next dictionary starts a new portion of data for next macros.
		/// </para>
		/// <para>
		/// Dictionary keys and values: strings 'Area' and 'Name' define macro area and name,
		/// the others correspond to property names and values of <see cref="KeyMacroData"/>.
		/// You don't have to set default values. Values are active for all next entries
		/// until a null dictionary in the sequence.
		/// </para>
		/// <para>
		/// This method at first may look unusual, but it is suitable for scripting and it:
		/// *) allows to set many macros in one shot effectively;
		/// *) calls Save() and Load() internally, so that one this call is enough for installation;
		/// *) makes it easy to set the same macro in several areas or several similar macros in the same area.
		/// </para>
		/// <para>
		/// An exceptions is thrown if no macro is actually installed ('Area', 'Name' or 'Sequence' is never set)
		/// or the same macro is defined more than once (the same 'Area' and 'Name' are used second time).
		/// Note that macros installed before the exception remain installed.
		/// </para>
		/// </remarks>
		void Install(IDictionary[] dataSet);
	}
}
