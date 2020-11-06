az login 

# deploy resorces
$resourceGroupName = "naftaschedulerapi"
$templateFile = "NaftaSchedulerAPI.template.json"
$parametersFile = "NaftaSchedulerAPI.parameters.json"

az deployment group create --resource-group $resourceGroupName `
    --template-file $templateFile --parameters $parametersFile

# deploy cosmos db stored procedure
$cosmosdbAccountName = "naftaschedulercosmos"
$cosmosdbDatabaseName = "Events"
$cosmosdbContainerName = "users"
$cosmosdbProcedureFile = "..\cosmosdb\users\procedures\addUser.js"
$cosmosdbProcedureName = "addUser"

az cosmosdb sql stored-procedure update -g $resourceGroupName -a $cosmosdbAccountName `
    -d $cosmosdbDatabaseName -c $cosmosdbContainerName `
    -n $cosmosdbProcedureName -b @$cosmosdbProcedureFile

# deploy function app
$buildPath = "..\api\NaftaScheduler.csproj"
$publishZip = "publish\publish.zip"
$publishFolder = "..\api\bin\Debug\netcoreapp3.1"

dotnet build $buildPath

if (Test-path $publishZip) { Remove-item $publishZip }
Add-Type -assembly "system.io.compression.filesystem"
[io.compression.zipfile]::CreateFromDirectory($publishFolder, $publishZip)
Write-Warning "publish file has been created.";

az functionapp deployment source config-zip `
    -g $resourceGroupName -n "naftaschedulerfa" --src $publishZip

