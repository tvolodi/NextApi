# run from powershell on windows


$module_path_arr = @()
$module_path_arr += "src/base/NextApi.Testing"
$module_path_arr += "src/base/NextApi.Common"
$module_path_arr += "src/base/NextApi.Server.Common"
$module_path_arr += "src/base/NextApi.UploadQueue.Common"
$module_path_arr += "src/client/NextApi.Client"
$module_path_arr += "src/client/NextApi.Client.UploadQueue"
$module_path_arr += "src/client/NextApi.Client.Autofac"
$module_path_arr += "src/client/NextApi.Client.MicrosoftDI"
$module_path_arr += "src/server/NextApi.Server"
$module_path_arr += "src/server/NextApi.Server.EfCore"
$module_path_arr += "src/server/NextApi.Server.UploadQueue"

# msbuild -t:pack src/base/NextApi.Testing

ForEach ($modulePath in $module_path_arr){
	Write-Host $modulePath
	$module_name = $modulePath.Split("/")[-1]
	Write-Host $module_name
	
	$buildCmd = "msbuild -t:pack $modulePath"
	Invoke-Expression $buildCmd
	
	$copyCmd = "copy $modulePath/bin/debug/$module_name.2.0.0.71.nupkg c:\.nuget-local"
	Invoke-Expression $copyCmd
	
	
	
}
