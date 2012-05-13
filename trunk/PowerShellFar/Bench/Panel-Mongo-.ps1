
<#
.Synopsis
	MongoDB data browser.
	Author: Roman Kuzmin

.Description
	Requires:
	- MongoDB server: http://www.mongodb.org/
	- Mdbc module: https://github.com/nightroman/Mdbc

	The script connects to the specified server and shows available databases,
	collections, and data. Viewing only even if data can be modified in panels.

	Large collections is not a problem. Their documents are shown 1000/page.
	Press [PgDn]/[PgUp] at last/first panel items to show next/previous pages.

.Parameter ConnectionString
		MongoDB server connection string.
#>

param
(
	[Parameter()]
	$ConnectionString = '.'
)

Import-Module Mdbc

function global:New-MdbcServerExplorer($ConnectionString) {
	New-Object PowerShellFar.PowerExplorer 35495dbe-e693-45c6-ab0d-30f921b9c46f -Property @{
		Data = @{Server = Connect-Mdbc $ConnectionString}
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
		AsCreatePanel = {
			param($1)
			$panel = [FarNet.Panel]$1
			$panel.Title = 'Databases'
			$panel.ViewMode = 0
			$panel.SetPlan(0, (New-Object FarNet.PanelPlan))
			$panel
		}
	}
}

function global:New-MdbcDatabaseExplorer($Server, $DatabaseName) {
	New-Object PowerShellFar.PowerExplorer f0dbf3cf-d45a-40fd-aa6f-7d8ccf5e3bf5 -Property @{
		Data = @{Database = $Server.GetDatabase($DatabaseName)}
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
		AsCreatePanel = {
			param($1)
			$panel = [FarNet.Panel]$1
			$panel.Title = 'Collections'
			$panel.ViewMode = 0
			$panel.SetPlan(0, (New-Object FarNet.PanelPlan))
			$panel
		}
	}
}

function global:New-MdbcCollectionExplorer($Database, $CollectionName) {
	New-Object PowerShellFar.ObjectExplorer -Property @{
		Data = @{ Collection = $Database.GetCollection($CollectionName) }
		AsGetData = {
			param($1, $2)
			if ($2.NewFiles) {
				Get-MdbcData $1.Data.Collection -AsCustomObject -First $2.Limit -Skip $2.Offset
			}
			else {
				, $1.Cache
			}
		}
		AsCreatePanel = {
			param($1)
			$panel = [PowerShellFar.ObjectPanel]$1
			$panel.Title = 'Documents'
			$panel.PageLimit = 1000
			$panel
		}
	}
}

(New-MdbcServerExplorer $ConnectionString).OpenPanel()
