Import-Module FarNet.Redis
$db = Open-Redis

function make_test_tree {
	Set-RedisString :test-tree-file-in-empty-folder-name 1
	$null = Remove-RedisKey (Search-RedisKey test-tree:*)

	# root files
	Set-RedisString test-tree:file-in-root-1 1
	Set-RedisString test-tree:file-in-root-2 2

	# dupe key, delete 1/2 message
	Set-RedisString test-tree:delete-me:dupe 1
	Set-RedisList test-tree:delete-me:dupe 1

	# empty folder name, empty file name, 2 normal files
	Set-RedisString test-tree:: empty-file-name
	Set-RedisString test-tree::file-in-empty-1 1
	Set-RedisString test-tree::file-in-empty-2 2

	# normal folder and sub-folder with files
	Set-RedisString test-tree:folder1:file-in-folder1-1 1
	Set-RedisString test-tree:folder1:file-in-folder1-2 2
	Set-RedisString test-tree:folder1:folder2:file-in-folder2-1 1
	Set-RedisString test-tree:folder1:folder2:file-in-folder2-2 2
}
