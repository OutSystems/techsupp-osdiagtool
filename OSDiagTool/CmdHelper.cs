using System;
using System.Diagnostics;
using System.Text;

namespace OSDiagTool
{
    public class CmdHelper
    {

        public CmdHelper() { }


        public static void RunCommand(string command, bool isHidden = true)
        {

            try {

                Process process = new Process();
                ProcessStartInfo startInfo = new ProcessStartInfo();

                if (isHidden)
                    startInfo.WindowStyle = ProcessWindowStyle.Hidden;

                startInfo.FileName = "cmd.exe";
                startInfo.Arguments = "/C " + command;
                startInfo.UseShellExecute = false;
                startInfo.RedirectStandardOutput = true;
                startInfo.StandardOutputEncoding = Encoding.UTF8;

                startInfo.WindowStyle = ProcessWindowStyle.Minimized;
                startInfo.CreateNoWindow = true;

                //FileLogger.TraceLog("Cmd: " + command);

                process.StartInfo = startInfo;

                process.Start();
                //FileLogger.TraceLog(process.StandardOutput.ReadToEnd());
                process.WaitForExit();              
                process.Close();

            } catch (Exception e){

                FileLogger.LogError("Error retrieving memory dumps. Cmd: " + command, e.Message + e.StackTrace);
            }

            
        }

    }
}
