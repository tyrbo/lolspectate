using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using System.Reflection;
using System.Net;
using System.Text;

namespace Update
{
    class Program
    {
        static void Main(string[] args)
        {
            string pathToExecutable = Application.StartupPath.ToString();
            string upgradeFileLocal = String.Format("{0}\\{1}", pathToExecutable, "LOLSpectate.1.exe");
            string oldExecutable = String.Format("{0}\\{1}", pathToExecutable, "LOLSpectate.exe");
            try
            {
                System.Net.WebClient client = new System.Net.WebClient();
                client.DownloadFile("http://static.lolsummoners.com/static/LOLSpectate.exe", "LOLSpectate.1.exe");
                File.Delete(oldExecutable);
                File.Move(upgradeFileLocal, oldExecutable);
            }
            catch { }

            if (args.Length > 0)
            {
                ProcessStartInfo upgradeProcess = new ProcessStartInfo(oldExecutable);
                upgradeProcess.WorkingDirectory = pathToExecutable;
                upgradeProcess.Arguments = String.Format("\"{0}\"", args[0]);
                Process.Start(upgradeProcess);
            }
            Environment.Exit(0);
        }
    }
}
