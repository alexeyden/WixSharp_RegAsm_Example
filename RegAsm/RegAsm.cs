using System.IO;
using System.Runtime.InteropServices;

using Microsoft.Win32;

using System;
using System.Reflection;

namespace RegAsm
{
    public class Regasm
    {
        /// <summary>
        /// Assembly registration class ctor
        /// </summary>
        /// <param name="dll">full path of a dll to register</param>
        public Regasm(string dll)
        {
            this.dll = dll;
        }

        /// <summary>
        /// Register assembly
        /// </summary>
        public void Install()
        {
            if (!File.Exists(dll))
            {
                throw new FileNotFoundException($"Target assembly is not found: \"{dll}\"");
            }

            var key = Registry.CurrentUser.OpenSubKey(@"Software\Classes", true);

            try
            {
                // temporarily override HKCR to HKCU\Software\Classes
                var ret = RegOverridePredefKey(HKEY_CLASSES_ROOT, key.Handle.DangerousGetHandle());

                if (ret != ERROR_SUCCESS)
                {
                    throw new Exception($"Cannot override HKEY_CLASSES_ROOT (error code {ret})");
                }

                // register assembly using RegistrationServices API

                var rs = new RegistrationServices();
                var asm = Assembly.LoadFrom(dll);
                rs.RegisterAssembly(asm, AssemblyRegistrationFlags.SetCodeBase);
            }
            finally
            {
                // restore HKCR back
                RegOverridePredefKey(HKEY_CLASSES_ROOT, IntPtr.Zero);
                key.Close();
            }
        }

        /// <summary>
        /// Unregister assembly
        /// </summary>
        public void Uninstall()
        {
            if (!File.Exists(dll))
            {
                throw new FileNotFoundException("Target assembly is not found");
            }

            var key = Registry.CurrentUser.OpenSubKey(@"Software\Classes", true);

            try
            {
                var ret = RegOverridePredefKey(HKEY_CLASSES_ROOT, key.Handle.DangerousGetHandle());

                if (ret != ERROR_SUCCESS)
                {
                    throw new Exception($"Cannot override HKEY_CLASSES_ROOT (error code {ret})");
                }
                
                var rs = new RegistrationServices();
                var asm = Assembly.LoadFrom(dll);
                rs.UnregisterAssembly(asm);
            }
            finally
            {
                RegOverridePredefKey(HKEY_CLASSES_ROOT, IntPtr.Zero);
                key.Close();
            }
        }

        private string dll;

        static readonly IntPtr HKEY_CURRENT_USER = new IntPtr(unchecked((int)0x80000001));
        static readonly IntPtr HKEY_LOCAL_MACHINE = new IntPtr(unchecked((int)0x80000002));
        static readonly IntPtr HKEY_USERS = new IntPtr(unchecked((int)0x80000003));
        static readonly IntPtr HKEY_CLASSES_ROOT = new IntPtr(unchecked((int)0x80000000));
        static readonly int ERROR_SUCCESS = 0;

        [DllImport("advapi32.dll")]
        internal static extern int RegOverridePredefKey(IntPtr hKey, IntPtr hNewHKey);
    }
}
