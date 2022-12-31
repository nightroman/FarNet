# FolderChart

FarNet module FolderChart for Far Manager

## Synopsis

For the current panel directory this tool calculates file and directory sizes
and shows the results as a chart in a separate window with some interaction.

**Project**

 * Source: <https://github.com/nightroman/FarNet/tree/main/FolderChart>
 * Author: Roman Kuzmin

***
## Installation

**Requirements**

 * Far Manager
 * Package FarNet
 * Package FarNet.FolderChart

**Instructions**

How to install and update FarNet and modules:\
<https://github.com/nightroman/FarNet#readme>

***
## Description

Chart windows do not block, you may keep charts open and work in Far Manager.
Charts are shown on top of other windows but you may minimize them when they
are not needed. You may open several charts at the same time. When Far Manager
exits then opened charts are closed as well.

**Chart interaction:**

* Click on a file shown in the panel sets it current, double click also closes the chart.
* Click on a folder opens it in the current panel, double click also closes the chart.
* Click in other areas switches the chart modes.
* Right click hides an item in the chart.
* Chart tool-tips show approximate sizes.

***
## Credits

The junction code is borrowed from [Manipulating NTFS Junction Points in .NET](https://www.codeproject.com/Articles/15633/Manipulating-NTFS-Junction-Points-in-NET).
