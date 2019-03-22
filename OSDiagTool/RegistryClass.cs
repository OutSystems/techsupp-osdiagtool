using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;
using System.IO;

namespace OSDiagTool
{
    class RegistryClass
    {
        public RegistryClass() {}


        public static Microsoft.Win32.RegistryKey GetRegistryKey(string sKey)
        {
           return Microsoft.Win32.Registry.LocalMachine.OpenSubKey(sKey);
        }

        public static Object GetRegistryValue(string sKey, string sValue)
        {
            Object obj = null;

            using (Microsoft.Win32.RegistryKey key = GetRegistryKey(sKey))
            {
                if (key != null)
                {
                    obj = key.GetValue(sValue);
                }
            }
            
            return obj;
        }

        // Use this function to copy the values of a registry path. Set the getSubKeys to true if you want all the values of subpaths 
        public static void RegistryCopy(string registryPath, string destPath, bool getSubKeys, bool isRootCall = true)
        {
            if (isRootCall)
            {
                using (var txtFile = File.Create(destPath));
            }

            RegistryKey key = Registry.LocalMachine.OpenSubKey(registryPath);

            string[] keyValueNames = key.GetValueNames();

            // If the array is not null, keyValueNames and valueNames are written to a text file
            if (keyValueNames.Length != 0)
            {
                File.AppendAllText(destPath, Environment.NewLine + Environment.NewLine + registryPath);

                foreach (string keyValueName in keyValueNames)
                {
                    File.AppendAllText(destPath, Environment.NewLine + "\t" + keyValueName + "\t" + key.GetValue(keyValueName).ToString());
                }
            }

            if (getSubKeys)
            {
                string[] subkeys = key.GetSubKeyNames();

                // If the array is not null, the full paths of the subkeys are appended to a list and
                // the function RegistryCopy calls itself recursively until it has reached the deepest subkey
                if (subkeys.Length != 0)
                {
                    List<string> subkeysFullPaths = new List<string>();

                    // Add the full registry path of the subkey to the list
                    foreach (string subkey in subkeys)
                    {
                        subkeysFullPaths.Add(Path.Combine(registryPath, subkey));
                    }

                    // Cycles each subkey registry path and calls itself recursively
                    foreach (string subkeyFullPath in subkeysFullPaths)
                    {
                        RegistryCopy(subkeyFullPath, destPath, true, false);
                    }
                }
            }
        }


    }
}
