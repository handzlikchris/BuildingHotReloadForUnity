using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;

public class HotReloadCompilation
{
    public static Assembly Compile(string sourceCodeFilePath)
    {
        var asmName = Guid.NewGuid().ToString().Replace("-", "") + HotReloadDynamicAssemblyDetourManager.HotReloadAssemblyNamePostfix;
        var tempFolder = Path.GetTempPath();
        var rspFilePath = tempFolder + $"{asmName}.rsp";
        var outLibraryPath = $"{tempFolder}{asmName}.dll";
        
        var rspFileContent = GenerateCompilerArgsRspFileContents(outLibraryPath, tempFolder, asmName, sourceCodeFilePath);
        File.WriteAllText(rspFilePath, rspFileContent);
        
        ExecuteDotnetExeCompilation(rspFilePath);
        
        return Assembly.LoadFrom(outLibraryPath);
    }
    
    private static string GenerateCompilerArgsRspFileContents(string outLibraryPath, string tempFolder, string asmName, string sourceCodeFilePath)
    {
        var rspContents = new StringBuilder();
        rspContents.AppendLine("-target:library");
        rspContents.AppendLine($"-out:\"{outLibraryPath}\"");
        rspContents.AppendLine($"-refout:\"{tempFolder}{asmName}.ref.dll\"");

        foreach (var referenceToAdd in ResolveReferencesToAdd())
        {
            rspContents.AppendLine($"-r:\"{referenceToAdd}\"");
        }

        rspContents.AppendLine($"\"{sourceCodeFilePath}\"");

        rspContents.AppendLine($"-langversion:latest");

        rspContents.AppendLine("/deterministic");
        rspContents.AppendLine("/optimize-");
        rspContents.AppendLine("/debug:portable");
        rspContents.AppendLine("/nologo");
        rspContents.AppendLine("/RuntimeMetadataVersion:v4.0.30319");

        rspContents.AppendLine("/nowarn:0169");
        rspContents.AppendLine("/nowarn:0649");
        rspContents.AppendLine("/nowarn:1701");
        rspContents.AppendLine("/nowarn:1702");
        rspContents.AppendLine("/utf8output");
        rspContents.AppendLine("/preferreduilang:en-US");

        return rspContents.ToString();
    }
    
    private static List<string> ResolveReferencesToAdd()
    {
        var referencesToAdd = new List<string>();
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            if (string.IsNullOrEmpty(assembly.Location))
            {
                continue;
            }

            referencesToAdd.Add(assembly.Location);
        }
        
        return referencesToAdd;
    }
    
    private static void ExecuteDotnetExeCompilation(string rspFile)
    {
        var process = new Process();
        process.StartInfo.FileName = FindFile("dotnet.exe");
        process.StartInfo.Arguments = $"exec \"{FindFile("csc.dll")}\" /nostdlib /noconfig /shared \"@{rspFile}\"";
        
        process.StartInfo.CreateNoWindow = true;
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        
        process.Start();
        process.WaitForExit();
        process.Close();
    }
    
    private static string FindFile(string fileName)
    {
        return Directory
            .GetFiles(EditorApplication.applicationContentsPath, fileName, SearchOption.AllDirectories)
            .FirstOrDefault();
    }
}