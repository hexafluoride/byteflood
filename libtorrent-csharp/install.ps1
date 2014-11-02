param($installPath, $toolsPath, $package, $project)

$fileNames = @(
    "boost_chrono-vc120-mt-1_55.dll",
    "boost_date_time-vc120-mt-1_55.dll",
    "boost_system-vc120-mt-1_55.dll",
    "boost_thread-vc120-mt-1_55.dll",
    "torrent.dll"
)

$propertyName = "CopyToOutputDirectory"

foreach($fileName in $fileNames) {
  $item = $project.ProjectItems.Item($fileName)

  if ($item -eq $null) {
    continue
  }

  $property = $item.Properties.Item($propertyName)

  if ($property -eq $null) {
    continue
  }

  $property.Value = 1
}