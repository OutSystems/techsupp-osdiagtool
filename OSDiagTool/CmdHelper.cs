using System.Diagnostics;
using System.Text;

namespace OSDiagTool
{
    public class CmdHelper
    {

        public CmdHelper() { }


        public static void RunCommand(string command, bool isHidden = true)
        {
            Process process = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo();

            if(isHidden)
                startInfo.WindowStyle = ProcessWindowStyle.Hidden;

            startInfo.FileName = "cmd.exe";
            startInfo.Arguments = "/C " + command;
            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardOutput = true;
            startInfo.StandardOutputEncoding = Encoding.UTF8;

            startInfo.WindowStyle = ProcessWindowStyle.Minimized;
            startInfo.CreateNoWindow = true;

            process.StartInfo = startInfo;

            //using (Process.Start(startInfo)) ;
            process.Start();   
            process.WaitForExit();
            process.Close();
        }

    }
}
