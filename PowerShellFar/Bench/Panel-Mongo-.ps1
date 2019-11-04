<#
.Synopsis
	MongoDB data browser.
	Author: Roman Kuzmin

.Description
	DISCLAIMER: USE THIS TOOL ON YOUR OWN RISK. YOU CAN DELETE, CHANGE, CORRUPT
	DATABASES, COLLECTIONS, DOCUMENTS, AND DATA - THE CHOICE AND RISK IS YOURS.

	Requires:
	- MongoDB server: http://www.mongodb.org/
	- Mdbc module v6.0.0: https://github.com/nightroman/Mdbc

	The script connects the specified server and shows databases, collections,
	views, documents including nested documents and arrays. Root documents may
	be viewed and edited as JSON. Nested documents may not be edited directly.

	Paging. Large collections is not a problem. Documents are shown 1000/page.
	Press [PgDn]/[PgUp] at last/first panel items to show next/previous pages.

	Aggregation pipelines may be defined for custom panel views of collections.
	If result documents have the same _id as the source collection then they
	are edited and deleted in the source collection from this custom view.

	KEYS AND ACTIONS

	[Del]
		Deletes selected documents and empty databases, collections, views.
		For deleting not empty containers use [ShiftDel].

	[ShiftDel]
		Deletes selected databases, collections, views, documents.

	[ShiftF6]
		Prompts for a new name and renames the current collection.

.Parameter ConnectionString
		Specifies the connection string. Use "." for the default local server.
		If DatabaseName and CollectionName are omitted then the panel shows
		databases.

.Parameter DatabaseName
		Specifies the database name. If CollectionName is not defined then the
		panel shows this database collections.

.Parameter CollectionName
		Specifies the collection name and tells to show collection documents.
		This parameter must be used together with DatabaseName. Use Pipeline
		in order to customise the view of this collection for the panel.

.Parameter Pipeline
		Aggregation pipeline for the custom view of the specified collection.
#>

[CmdletBinding()]
param(
	[string]$ConnectionString = '.',
	[string]$DatabaseName,
	[string]$CollectionName,
	$Pipeline
)

Import-Module Mdbc

function global:Get-PMSourceCollection($Collection) {
	$Database = $Collection.Database
	$views = Get-MdbcCollection system.views

	$r = Get-MdbcData @{_id = $Collection.CollectionNamespace.FullName} -Collection $views
	if ($r) {
		Get-MdbcCollection $r.viewOn
	}
	else {
		$Collection
	}
}

function global:New-PMServerExplorer($ConnectionString) {
	Connect-Mdbc $ConnectionString
	New-Object PowerShellFar.PowerExplorer 35495dbe-e693-45c6-ab0d-30f921b9c46f -Property @{
		Data = @{Client = $Client}
		Functions = 'DeleteFiles'
		AsCreatePanel = {
			param($1)
			$panel = [FarNet.Panel]$1
			$panel.Title = 'Databases'
			$panel.ViewMode = 0
			$panel.SetPlan(0, (New-Object FarNet.PanelPlan))
			$panel
		}
		AsGetFiles = {
			param($1)
			foreach($database in Get-MdbcDatabase -Client $1.Data.Client) {
				New-FarFile -Name $database.DatabaseNamespace.DatabaseName -Attributes Directory -Data $database
			}
		}
		AsExploreDirectory = {
			param($1, $2)
			New-PMDatabaseExplorer $2.File.Data
		}
		AsDeleteFiles = {
			param($1, $2)
			# ask
			if ($2.UI) {
				$text = @"
$($2.Files.Count) database(s):
$($2.Files[0..9] -join "`n")
"@
				if (Show-FarMessage $text Delete YesNo -LeftAligned) {return}
			}
			# drop
			foreach($file in $2.Files) {
				try {
					$database = $file.Data
					if (!$2.Force) {
						$collections = @(Get-MdbcCollection -Database $database)
						if ($collections) {
							throw "Database '$($file.Name)' is not empty, $($collections.Count) collections."
						}
					}
					Remove-MdbcDatabase $database.DatabaseNamespace.DatabaseName -Client $1.Data.Client
				}
				catch {
					$2.Result = 'Incomplete'
					$2.FilesToStay.Add($file)
					if ($2.UI) {Show-FarMessage "$_"}
				}
			}
		}
	}
}

function global:New-PMDatabaseExplorer($Database) {
	New-Object PowerShellFar.PowerExplorer f0dbf3cf-d45a-40fd-aa6f-7d8ccf5e3bf5 -Property @{
		Data = @{Database = $Database}
		Functions = 'DeleteFiles, RenameFile'
		AsCreatePanel = {
			param($1)
			$panel = [FarNet.Panel]$1
			$panel.Title = 'Collections'
			$panel.ViewMode = 0
			$panel.SetPlan(0, (New-Object FarNet.PanelPlan))
			$panel
		}
		AsGetFiles = {
			param($1)
			foreach($collection in Get-MdbcCollection -Database $1.Data.Database) {
				New-FarFile -Name $collection.CollectionNamespace.CollectionName -Attributes 'Directory' -Data $collection
			}
		}
		AsExploreDirectory = {
			param($1, $2)
			New-PMCollectionExplorer $2.File.Data
		}
		AsRenameFile = {
			param($1, $2)
			$newName = ([string]$Far.Input('New name', $null, 'Rename', $2.File.Name)).Trim()
			if (!$newName) {return}
			Rename-MdbcCollection $2.File.Name $newName -Database $1.Data.Database
			$2.PostName = $newName
		}
		AsDeleteFiles = {
			param($1, $2)
			# ask
			if ($2.UI) {
				$text = @"
$($2.Files.Count) collection(s):
$($2.Files[0..9] -join "`n")
"@
				if (Show-FarMessage $text Delete YesNo -LeftAligned) {return}
			}
			# drop
			foreach($file in $2.Files) {
				try {
					$collection = $file.Data
					if (!$2.Force -and (Get-MdbcData -Collection $collection -Count -First 1)) {
						throw "Collection '$($file.Name)' is not empty."
					}
					Remove-MdbcCollection $collection.CollectionNamespace.CollectionName -Database $1.Data.Database
				}
				catch {
					$2.Result = 'Incomplete'
					$2.FilesToStay.Add($file)
					if ($2.UI) {Show-FarMessage "$_"}
				}
			}
		}
	}
}

function global:New-PMCollectionExplorer($Collection, $Pipeline) {
	New-Object PowerShellFar.ObjectExplorer -Property @{
		Data = @{
			Collection = $Collection
			Pipeline = $Pipeline
			Source = Get-PMSourceCollection $Collection
		}
		FileComparer = [PowerShellFar.FileMetaComparer]'_id'
		AsCreatePanel = {
			param($1)
			$panel = [PowerShellFar.ObjectPanel]$1
			$title = $1.Data.Collection.CollectionNamespace.CollectionName
			if ($1.Data.Collection -ne $1.Data.Source) {
				$title = "$title ($($1.Data.Source.CollectionNamespace.CollectionName))"
			}
			if ($1.Data.Pipeline) {
				$panel.Title = $title + ' (aggregate)'
			}
			else {
				$panel.Title = $title
				$panel.PageLimit = 1000
			}
			$1.Data.Panel = $panel
			$panel
		}
		AsGetData = {
			param($1, $2)
			if ($2.NewFiles -or !$1.Cache) {
				if ($1.Data.Pipeline) {
					Invoke-MdbcAggregate $1.Data.Pipeline -Collection $1.Data.Collection -As PS
				}
				else {
					Get-MdbcData -Collection $1.Data.Collection -As PS -First $2.Limit -Skip $2.Offset
				}
			}
			else {
				, $1.Cache
			}
		}
		AsDeleteFiles = {
			param($1, $2)
			# ask
			if ($2.UI) {
				$text = "$($2.Files.Count) documents(s)"
				if (Show-FarMessage $text Delete YesNo) {return}
			}
			# remove
			try {
				foreach($doc in $2.FilesData) {
					Remove-MdbcData @{_id=$doc._id} -Collection $1.Data.Source -ErrorAction 1
				}
			}
			catch {
				$2.Result = 'Incomplete'
				if ($2.UI) {Show-FarMessage "$_"}
			}
			$1.Data.Panel.NeedsNewFiles = $true
		}
		AsGetContent = {
			param($1, $2)

			$id = $2.File.Data._id
			if ($null -eq $id) {
				$doc = New-MdbcData $2.File.Data
			}
			else {
				$doc = Get-MdbcData @{_id = $id} -Collection $1.Data.Source
			}

			$2.UseText = $doc.Print()
			$2.UseFileExtension = '.js'
			$2.CanSet = $true
		}
		AsSetText = {
			param($1, $2)

			$id = $2.File.Data._id
			if ($null -eq $id) {
				Show-FarMessage "Document must have _id."
				return
			}

			$new = [MongoDB.Bson.BsonDocument]::Parse($2.Text)
			if ($id -cne $new['_id']) {
				Show-FarMessage "Cannot change _id."
				return
			}

			try {
				Set-MdbcData @{_id = $id} $new -Collection $1.Data.Source -ErrorAction 1
			}
			catch {
				Show-FarMessage $_
			}

			$1.Cache.Clear()
			$Far.Panel.Update($true)
		}
	}
}

if ($CollectionName) {
	Connect-Mdbc $ConnectionString $DataBaseName $CollectionName
	(New-PMCollectionExplorer $Collection $Pipeline).OpenPanel()
}
elseif ($DatabaseName) {
	Connect-Mdbc $ConnectionString $DatabaseName
	(New-PMDatabaseExplorer $Database).OpenPanel()
}
else {
	(New-PMServerExplorer $ConnectionString).OpenPanel()
}
