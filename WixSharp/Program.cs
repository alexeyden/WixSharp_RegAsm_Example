using System;
using System.Windows.Forms;
using System.Linq;
using WixSharp;
using System.Reflection;
using System.Diagnostics;
using WixSharp.Forms;

namespace WixSharpSetup
{
    class Program
    {
        static void Main()
        {
            var project = new ManagedProject("Wix# Managed assembly registration example",
                             new Dir(@"%LocalAppDataFolder%\WixSharpRegAsmExample",
                                 new File(new Id("MainAssembly"), "YourAssembly.dll")),
                             // embed regasm binaries for both architectures
                             new Binary(new Id("regasm32.exe"), RegAsmPath + @"x86\Debug\Regasm.exe"),
                             new Binary(new Id("regasm64.exe"), RegAsmPath + @"x64\Debug\Regasm.exe"));

            project.InstallScope = InstallScope.perUser;
            project.GUID = new Guid("6fe30b47-2577-43ad-9095-1865ba25889b");

            var current = System.IO.Directory.GetCurrentDirectory();

            project.SourceBaseDir = System.IO.Path.Combine(current, SourcePath);
            project.OutDir = System.IO.Path.Combine(current, OutputDir);

            var mainAssemblyPath = "[INSTALLDIR]" + project.FindFile(f => f.Id == "MainAssembly").FirstOrDefault()?.Name;

            // create file actions that are going to call enbedded 32 and 64 bit regasm binaries
            // when the installation is finalized

            var regasmInstallAction32 = new BinaryFileAction("regasm32.exe", $"\"{mainAssemblyPath}\"", Return.check, When.After, Step.InstallFinalize, Condition.NOT_BeingRemoved);
            var regasmInstallAction64 = new BinaryFileAction("regasm64.exe", $"\"{mainAssemblyPath}\"", Return.check, When.After, Step.InstallFinalize, Condition.NOT_BeingRemoved);

            // create corresponding custom uninstall actions

            var regasmUninstallAction32 = new BinaryFileAction("regasm32.exe", "/u " + $"\"{mainAssemblyPath}\"", Return.check, When.Before, Step.RemoveFiles, Condition.BeingRemoved);
            var regasmUninstallAction64 = new BinaryFileAction("regasm64.exe", "/u " + $"\"{mainAssemblyPath}\"", Return.check, When.Before, Step.RemoveFiles, Condition.BeingRemoved);

            project.Actions = new[]
            {
                regasmInstallAction32, regasmInstallAction64,
                regasmUninstallAction32, regasmUninstallAction64
            };

            project.BuildMsi();
        }

        /// <summary>
        /// Regasm output directory (relative to source path)
        /// </summary>
        static string RegAsmPath = @"..\..\Regasm\bin";

        /// <summary>
        /// Source directory
        /// </summary>
        static string SourcePath = @"Build\bin\Debug\";

        /// <summary>
        /// Output directory
        /// </summary>
        static string OutputDir = @"Build\msi\";
    }
}
