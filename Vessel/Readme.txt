
Module   : FarNet.Vessel
Release  : 2010-12-30
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
the top wins. Finally the factors that maximize the total difference between
the plain and ranked lists for all records are taken.

Automatic fast training starts after updating the history file from the menu
and after opening not recent files from the smart history. It works in the
background and it is very fast (~50 times faster than full training).

	= OPTIONS =

Registry key:
HKEY_CURRENT_USER\Software\Far2\Plugins\FarNet.Modules\Vessel.dll

String value: Limits="Limit0/Limit1/Limit2"
Default, slower training, best results: "2/200/30"
Faster training and still good results: "2/100/15"

Limit0
Span 0 in hours. It defines the most recent files to be sorted by times.

Limit1
Maximum span 1 in hours. Training finds the best value (factor 1).

Limit2
Maximum span 2 in days. Training finds the best value (factor 2).

	= HISTORY =

1.0.1

* Key counts are used in ranking.
* Improved ranking model and training.
* Expected extra gain is more than 20%.

1.0.2

* Training is 2-4 times faster.
* Fixed recent time is 2 hours.
* Expected extra gain is about 15%.

1.0.3

* Use of closing time instead of opening time is better for a few reasons.
* Smart history list shows separators of the groups defined by factors.
* Trainig shows the progress form.
* Option Limits (see Readme.txt).
* Ranking model is based on two factors instead of one. As a result:
- Training is slow but factors can live longer without re-training.
- Expected about 10% more total gain.

1.0.4

Fixed the plain history list and minor defects in 1.0.3.

1.0.5

Fix: plain history should be used on negative training results. Negative
results are often possible when the collected history is not long enough.

1.0.6

Training is now done in the background. When training has completed the menu
shows the "Training results" item until it is not visited once. This item is
also shown after automatic fast training.

Automatic fast training starts after updating the history file from the menu
and after opening not recent files from the smart history. It works in the
background and it is very fast (~50 times faster than full training).

Training result numbers do not include openings below Limit0: they are not
really important because they are the same as in the classic plain history.

1.0.7

Minor improvement of training performance.
