
Module   : FarNet.Vessel
Release  : 2010-12-23
Category : File history
Author   : Roman Kuzmin
E-mail   : nightroman@gmail.com
Source   : http://code.google.com/p/farnet/

	= PREREQUISITES =

 * Far Manager 2.0.1767
 * Plugin FarNet 4.3.33
 * .NET Framework 3.5+

	= DESCRIPTION =

Vessel (short for View/Edit/Save/SELect) records and maintains history of file
view, edit, and save operations and related statistics. The history file is:
%USERPROFILE%\VesselHistory.log

Features

 * smart and plain history lists
 * no history conflicts between different Far sessions (Mantis issue #1169)
 * incremental filter in history lists works at once (no need for CtrlAltF)
 * advanced filter with reusable regular expressions (CtrlDown, AltDown)
 * for other features see history lists help (F1)

If the log file is missing then it is generated from the existing Far history.
This history is not yet effective for training. The smart history list will be
the same as the plain list for some time even after training.

The smart history list shows files in heuristically improved order. Recently
used files are sorted by last times, as usual. Files not used for a while are
sorted by ranks. Rank is based on last time, frequency, activity, and factors
calculated by training.

How training works. For every file record it builds the plain history list and
several ranked lists with different factors. The list with the file nearest to
the top wins. Finally the factor that maximizes the total difference between
the plain and ranked lists for all records is taken. The plain list may win as
well (factor 0) if there are no better ranked lists.

	= HISTORY =

1.0.1
* Key counts are used in ranking.
* Improved ranking model and training.
* Expected extra gain is more than 20%.
