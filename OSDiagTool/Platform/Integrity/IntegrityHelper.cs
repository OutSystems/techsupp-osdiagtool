using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSDiagTool.Platform.Integrity
{
    class IntegrityHelper
    {
        public static Dictionary<string, Dictionary<string, DateTime>> GetLastModulesPublished(string platformRunningPath)
        {
            Dictionary<string, Dictionary<string, DateTime>> allRunningFolders = new Dictionary<string, Dictionary<string, DateTime>>(); // <path folder>, [<module name>, <creation date>] - check for duplicates and use latest created only

            // Add all modules running paths and respective creation date time to a dict
            foreach (string moduleRunning in Directory.GetDirectories(platformRunningPath))
            {
                allRunningFolders.Add(moduleRunning, new Dictionary<string, DateTime> { { moduleRunning.Substring(moduleRunning.LastIndexOf('\\') + 1, moduleRunning.LastIndexOf('.') - (moduleRunning.LastIndexOf('\\') + 1)), Directory.GetCreationTime(moduleRunning) } });
            }

            // Check for duplicates and output a filtered dict
            List<string> duplicateKeystoRemove = new List<string>();
            foreach (KeyValuePair<string, Dictionary<string, DateTime>> element in allRunningFolders)
            {
                string elementNameTrimmed = element.Key.Substring(element.Key.LastIndexOf('\\') + 1, element.Key.LastIndexOf('.') - (element.Key.LastIndexOf('\\') + 1));
                foreach (KeyValuePair<string, Dictionary<string, DateTime>> elementToCheck in allRunningFolders)
                {
                    if (!String.Equals(element.Key.ToString(), elementToCheck.Key.ToString())) // skip check for same key - if the paths are exactly the same, it's the same module folder
                    {
                        string elementToCheckNameTrimmed = elementToCheck.Key.Substring(elementToCheck.Key.LastIndexOf('\\') + 1, elementToCheck.Key.LastIndexOf('.') - (elementToCheck.Key.LastIndexOf('\\') + 1));
                        if (String.Equals(elementNameTrimmed, elementToCheckNameTrimmed)) // check if the trimmed name is the same - if true, it's a duplicate
                        {

                            allRunningFolders.TryGetValue(element.Key, out var elementInnerDictionary);
                            allRunningFolders.TryGetValue(elementToCheck.Key, out var elementToCheckInnerDictionary);

                            elementInnerDictionary.TryGetValue(elementNameTrimmed, out DateTime elementDateTime);
                            elementToCheckInnerDictionary.TryGetValue(elementNameTrimmed, out DateTime elementToCheckDateTime);

                            if (elementDateTime > elementToCheckDateTime)
                            {
                                if (!duplicateKeystoRemove.Contains(elementToCheck.Key))
                                {
                                    //FileLogger.TraceLog("Found duplicate module in the running folder. Removing connection string check for the oldest module and checking only " + elementNameTrimmed + " with creation date time : " + elementDateTime);
                                    duplicateKeystoRemove.Add(elementToCheck.Key);
                                }
                            }
                        }
                    }
                }
            }

            //Remove duplicates from the dictionary
            if (!duplicateKeystoRemove.Count.Equals(0))
            {
                foreach (string key in duplicateKeystoRemove)
                {
                    allRunningFolders.Remove(key);
                }
            }

            return allRunningFolders;
        }
    }
}
