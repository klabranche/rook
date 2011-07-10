properties {
    $projectName = "Rook"
    $unitTestAssembly = "Rook.Test.dll"

    $projectConfig = "Release"
    $base_dir = resolve-path .\
    $source_dir = "$base_dir\src"
    $samples_dir = "$source_dir\Rook.Test\Samples"
    $nunit_dir = "$source_dir\packages\NUnit.2.5.9.10348\Tools"
	
    $build_dir = "$base_dir\build"
    $test_dir = "$build_dir\test"
    $package_dir = "$build_dir\package"	
    $package_file = "$build_dir\latestVersion\" + $projectName +".zip"
}

$framework = '4.0'

FormatTaskName "------------------------------
-- {0}
------------------------------"

task default -depends Init, CommonAssemblyInfo, Compile, Test
task package -depends Init, CommonAssemblyInfo, Compile, Test, ZipPackage

task Init {
    delete_file $package_file
    delete_directory $build_dir
    create_directory $test_dir
    create_directory $build_dir
}

task CommonAssemblyInfo {
    $version = "1.0.0.0"
    create-commonAssemblyInfo "$version" $projectName "$source_dir\CommonAssemblyInfo.cs"
}

task Compile -depends Init {
    msbuild /t:clean /v:q /nologo /p:Configuration=$projectConfig $source_dir\$projectName.sln
    delete_file $error_dir
    msbuild /t:build /v:q /nologo /p:Configuration=$projectConfig $source_dir\$projectName.sln
}

task Test {
    copy_all_assemblies_for_test $test_dir
    copy_samples_for_test $test_dir
    exec {
        & $nunit_dir\nunit-console.exe $test_dir\$unitTestAssembly /nologo /nodots /xml=$build_dir\TestResult.xml    
    }
}

task ZipPackage -depends Compile {
    delete_directory $package_dir

    copy_files "$source_dir\Parsley\bin\$projectConfig\" $package_dir
    copy_files "$source_dir\Parsley.Test\bin\$projectConfig\" $package_dir
    copy_files "$source_dir\Rook\bin\$projectConfig\" $package_dir
    copy_files "$source_dir\Rook.Compiling\bin\$projectConfig\" $package_dir
    copy_files "$source_dir\Rook.Core\bin\$projectConfig\" $package_dir

    zip_directory $package_dir $package_file 
}

function global:zip_directory($directory,$file) {
    delete_file $file
    cd $directory
    & "$base_dir\tools\7zip\7za.exe" a -mx=9 -r $file
    cd $base_dir
}

function global:copy_files($source,$destination,$exclude=@()){    
    create_directory $destination
    Get-ChildItem $source -Recurse -Exclude $exclude | Copy-Item -Destination {Join-Path $destination $_.FullName.Substring($source.length)} 
}

function global:copy_and_flatten ($source,$filter,$dest) {
    ls $source -filter $filter -r | cp -dest $dest
}

function global:copy_all_assemblies_for_test($destination){
    create_directory $destination
    copy_and_flatten $source_dir *.exe $destination
    copy_and_flatten $source_dir *.dll $destination
    copy_and_flatten $source_dir *.config $destination
    copy_and_flatten $source_dir *.xml $destination
    copy_and_flatten $source_dir *.pdb $destination
    copy-item $samples_dir $destination
}

function global:copy_samples_for_test($destination) {
    create_directory $destination
    copy-item "$samples_dir\*" "$destination\Samples"
}

function global:delete_file($file) {
    if($file) { remove-item $file -force -ErrorAction SilentlyContinue | out-null } 
}

function global:delete_directory($directory_name)
{
    rd $directory_name -recurse -force  -ErrorAction SilentlyContinue | out-null
}

function global:create_directory($directory_name)
{
    mkdir $directory_name  -ErrorAction SilentlyContinue  | out-null
}

function global:create-commonAssemblyInfo($version,$applicationName,$filename)
{
"using System;
using System.Reflection;
using System.Runtime.InteropServices;

//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:2.0.50727.4927
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

[assembly: ComVisible(false)]
[assembly: AssemblyVersion(""$version"")]
[assembly: AssemblyFileVersion(""$version"")]
[assembly: AssemblyCopyright(""Copyright Patrick Lioi 2011"")]
[assembly: AssemblyProduct(""$applicationName"")]
[assembly: AssemblyConfiguration(""release"")]
[assembly: AssemblyInformationalVersion(""$version"")]"  | out-file $filename -encoding "ASCII"    
}