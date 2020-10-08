using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace OSDiagTool.Utils {
    class WinUtils {

        public static void WriteToFile(string filePath, string message) {

            if (!File.Exists(filePath)) { 
                using (File.Create(filePath));
            }

            File.AppendAllText(filePath, message + Environment.NewLine);

        }

    }
}
