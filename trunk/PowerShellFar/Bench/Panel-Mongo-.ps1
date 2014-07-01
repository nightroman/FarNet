
<#
.Synopsis
	MongoDB data browser.
	Author: Roman Kuzmin

.Description
	DISCLAIMER: Use this tool on your own risk. You can delete or corrupt
	databases, collections, documents, and data - the choice is all yours.

	Requires:
	- MongoDB server: http://www.mongodb.org/
	- Mdbc module v2.0.0: https://github.com/nightroman/Mdbc

	The script connects to the specified server and shows available databases,
	collections, documents, and contents including nested documents and arrays.

	Paging. Large collections is not a problem. Documents are shown 1000/page.
	Press [PgDn]/[PgUp] at last/first panel items to show next/previous pages.

	KEYS AND ACTIONS

	[Del]
		Deletes selected documents and empty databases and collections.

	[ShiftDel]
		Also deletes not empty databases and collections.

	[ShiftF6]
		Prompts for a new name and renames the current collection.

.Parameter ConnectionString
		MongoDB server connection string. The default is ".", the default local
		server and port. If DatabaseName and CollectionName are not defined
		then a server panel with databases is opened.

.Parameter DatabaseName
		Specifies the database name. If CollectionName is not defined then a
		database panel with collections is opened.

.Parameter CollectionName
		Tells to open a panel with documents of the specified collection. This
		parameter is used together with DatabaseName.

.Parameter File
		Specifies the bson data file path and tells to open it in the panel.
		Documents in the file must have unique _id's.
#>

param
(
	[Parameter()]
	$ConnectionString = '.',
	$DatabaseName,
	$CollectionName,
	$File
)

Import-Module Mdbc

function global:New-MdbcServerExplorer($ConnectionString) {
	Connect-Mdbc $ConnectionString
	New-Object PowerShellFar.PowerExplorer 35495dbe-e693-45c6-ab0d-30f921b9c46f -Property @{
		Data = @{Server = $Server}
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
			foreach($databaseName in $1.Data.Server.GetDatabaseNames()) {
				New-FarFile -Name $databaseName -Attributes 'Directory'
			}
		}
		AsExploreDirectory = {
			param($1, $2)
			New-MdbcDatabaseExplorer $1.Data.Server $2.File.Name
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
					$database = $1.Data.Server.GetDatabase($file.Name)
					if (!$2.Force) {
						$names = $database.GetCollectionNames()
						if ($names.Count -ge 2 -or ($names.Count -eq 1 -and $names[0] -cne 'system.indexes')) {
							throw "Database '$($file.Name)' is not empty."
						}
					}
					$database.Drop()
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

function global:New-MdbcDatabaseExplorer($Server, $DatabaseName) {
	New-Object PowerShellFar.PowerExplorer f0dbf3cf-d45a-40fd-aa6f-7d8ccf5e3bf5 -Property @{
		Data = @{Database = $Server.GetDatabase($DatabaseName)}
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
			foreach($collectionName in $1.Data.Database.GetCollectionNames()) {
				New-FarFile -Name $collectionName -Attributes 'Directory'
			}
		}
		AsExploreDirectory = {
			param($1, $2)
			New-MdbcCollectionExplorer $1.Data.Database $2.File.Name
		}
		AsRenameFile = {
			param($1, $2)
			$newName = ([string]$Far.Input('New name', $null, 'Rename', $2.File.Name)).Trim()
			if (!$newName) {return}
			if ($1.Data.Database.CollectionExists($newName)) {return $Far.Message('Collection exists')}
			$1.Data.Database.RenameCollection($2.File.Name, $newName)
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
					$collection = $1.Data.Database.GetCollection($file.Name)
					if (!$2.Force -and $collection.Count()) {
						throw "Collection '$($file.Name)' is not empty."
					}
					$collection.Drop()
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

function global:New-MdbcCollectionExplorer($Database, $CollectionName, $File) {
	if ($File) {
		Open-MdbcFile $File
	}
	else {
		$Collection = $Database.GetCollection($CollectionName)
	}
	New-Object PowerShellFar.ObjectExplorer -Property @{
		Data = @{ Collection = $Collection }
		FileComparer = [PowerShellFar.FileMetaComparer]'_id'
		AsCreatePanel = {
			param($1)
			$panel = [PowerShellFar.ObjectPanel]$1
			$panel.Title = 'Documents'
			$panel.PageLimit = 1000
			$1.Data.Panel = $panel
			$panel
		}
		AsGetData = {
			param($1, $2)
			if ($2.NewFiles -or !$1.Cache) {
				Get-MdbcData -Collection $1.Data.Collection -As PS -First $2.Limit -Skip $2.Offset
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
				$2.FilesData | Remove-MdbcData -Collection $1.Data.Collection -ErrorAction 1
			}
			catch {
				$2.Result = 'Incomplete'
				if ($2.UI) {Show-FarMessage "$_"}
			}
			Save-MdbcFile -Collection $1.Data.Collection
			$1.Data.Panel.NeedsNewFiles = $1.Data.Collection -is [MongoDB.Driver.MongoCollection]
		}
		AsGetContent = {
			param($1, $2)

			$id = $2.File.Data._id
			if ($null -eq $id) {
				$2.UseText = $2.File.Data | Format-List | Out-String
				return
			}

			$Collection = $1.Data.Collection
			$doc = Get-MdbcData $id

			$writer = New-Object System.IO.StringWriter
			$settings = New-Object MongoDB.Bson.IO.JsonWriterSettings -Property @{Indent = $true}
			[MongoDB.Bson.Serialization.BsonSerializer]::Serialize(
				(New-Object MongoDB.Bson.IO.JsonWriter $writer, $settings),
				$doc.ToBsonDocument()
			)

			$2.UseText = $writer.ToString()
			$2.UseFileExtension = '.js'
			$2.CanSet = $true
		}
		AsSetText = {
			param($1, $2)

			$id = $2.File.Data._id
			if ($null -eq $id) {
				return
			}

			$Collection = $1.Data.Collection

			$reader = [MongoDB.Bson.IO.BsonReader]::Create($2.Text)
			$new = [MongoDB.Bson.Serialization.BsonSerializer]::Deserialize($reader, [Mdbc.Dictionary])

			if ($id -cne $new._id) {
				Show-FarMessage "Cannot change _id."
				return
			}

			$new | Add-MdbcData -Update
			Save-MdbcFile

			$1.Cache.Clear()
			$Far.Panel.Update($true)
		}
	}
}

if ($CollectionName) {
	Connect-Mdbc $ConnectionString $DataBaseName
	(New-MdbcCollectionExplorer $Database $CollectionName).OpenPanel()
}
elseif ($DatabaseName) {
	Connect-Mdbc $ConnectionString
	(New-MdbcDatabaseExplorer $Server $DatabaseName).OpenPanel()
}
elseif ($File) {
	(New-MdbcCollectionExplorer -File $File).OpenPanel()
}
else {
	(New-MdbcServerExplorer $ConnectionString).OpenPanel()
}
