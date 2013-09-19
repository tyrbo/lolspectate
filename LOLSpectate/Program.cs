using System;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Reflection;
using System.Net;
using System.Text;
using System.Threading;

namespace LOLSpectate
{
    class Program
    {
        static string[] argCopy;
        static bool Garena = false;
        static string version;
        static bool didCancel = false;

        [STAThread]
        static void Main(string[] args)
        {
            try
            {
                Assembly assembly = Assembly.GetExecutingAssembly();
                FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
                version = fvi.FileVersion;

                checkForUpdates();

                if (Settings.Default.ResetPaths && Settings.Default.HasReset)
                {
                    Settings.Default.LeaguePath = null;
                    Settings.Default.HasReset = false;
                    Settings.Default.Save();
                }

                if (Settings.Default.LeaguePath == null || Settings.Default.LeaguePath == "")
                {
                    MessageBox.Show("We can't find your League installation.\nYou'll need to show us where it is.", "Initial Setup", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                    RequestPath();
                }
                else if (File.Exists(Settings.Default.LeaguePath + @"\lol.launcher.exe"))
                {
                    Garena = false;
                }
                else if (File.Exists(Settings.Default.LeaguePath + @"\lol.exe"))
                {
                    Garena = true;
                }
                else
                {
                    MessageBox.Show("We can't find your League installation.\nYou'll need to show us where it is.", "Initial Setup", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                    RequestPath();
                }

                if (args.Length == 0 || didCancel)
                    Environment.Exit(0);

                if (Settings.Default.LastVersion != version)
                {
                    Settings.Default.LastVersion = version;
                    Settings.Default.Save();
                    if (MessageBox.Show("LOL Spectate was recently updated. Would you like to view the release notes?", "LOL Spectate", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly) == DialogResult.Yes)
                    {
                        Process.Start("https://github.com/tyrbo/lolspectate/wiki/Release-Notes");
                    }
                }

                argCopy = args;

                string leaguePath = Settings.Default.LeaguePath;
                string spectatorUrl = args[0].Replace("lsp://", "").Replace("%20", " ").TrimEnd('/');
                string arguments = "\"8391\" \"\" \"\" \"" + spectatorUrl + "\"";

                //MessageBox.Show(leaguePath + " " + spectatorUrl + " " + arguments);

                ProcessStartInfo start = new ProcessStartInfo();

                if (!Garena)
                {
                    DirectoryInfo di = new DirectoryInfo(leaguePath + @"\RADS\solutions\lol_game_client_sln\releases");
                    DirectoryInfo[] dirs = di.GetDirectories();

                    string folderName = "";
                    DateTime modifiedTime = new DateTime();

                    foreach (DirectoryInfo dir in dirs)
                    {
                        if (dir.LastWriteTime > modifiedTime)
                        {
                            modifiedTime = dir.LastWriteTime;
                            folderName = dir.Name;
                        }
                    }
                    start.Arguments = arguments;
                    start.WorkingDirectory = leaguePath + @"\RADS\solutions\lol_game_client_sln\releases\" + folderName + @"\deploy\";
                    start.FileName = start.WorkingDirectory + @"League of Legends.exe";
                }
                else
                {
                    start.Arguments = arguments;
                    start.WorkingDirectory = leaguePath + @"\Game\";
                    start.FileName = start.WorkingDirectory + @"League of Legends.exe";
                }
                Process p = new Process();
                p = Process.Start(start);
            }
            catch (Exception e)
            {
                string msg = "Something went wrong!\nScreenshot this message box, and email the screenshot to jon@lolsummoners.com.\n\n" + e.ToString();
                MessageBox.Show(msg, "We Crashed!", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
            }
        }

        static void checkForUpdates()
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://static.lolsummoners.com/static/version.txt");
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                Stream receiveStream = response.GetResponseStream();
 
                StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8);
                string latestVersion = readStream.ReadToEnd().TrimEnd('\r', '\n');

                response.Close();
                readStream.Close();

                if (version != latestVersion)
                {
                    if (MessageBox.Show("A new version is available. Would you like to update now?", "LOL Spectate", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly) == DialogResult.Yes)
                    {
                        startUpdate(argCopy);
                    }
                }
            }
            catch
            {
            }
        }

        static void startUpdate(string[] args)
        {
            string pathToExecutable = Application.StartupPath.ToString();
            string upgradeExecutable = "Update.exe";
            string fullUpgradeExecutable = String.Format("{0}\\{1}", pathToExecutable, upgradeExecutable);
            if (File.Exists(fullUpgradeExecutable))
            {
                ProcessStartInfo upgradeProcess = new ProcessStartInfo(fullUpgradeExecutable);
                upgradeProcess.WorkingDirectory = pathToExecutable;
                upgradeProcess.Arguments = String.Format("\"{0}\"", argCopy[0]);
                Process.Start(upgradeProcess);
                Environment.Exit(0);
            }
        }

        static void RequestPath()
        {
            bool loopIt = true;

            // This seems to fix message boxes not showing up on top of things.
            Form form1 = new Form();
            form1.Show();
            form1.Hide();

            while (loopIt)
            {
                FolderBrowserDialog openFileDialog1 = new FolderBrowserDialog();
                openFileDialog1.ShowNewFolderButton = false;
                openFileDialog1.Description = "Please select the path to your League installation. e.g.\n" +
                    @"C:\Riot Games\League of Legends" + "\n" +
                    @"C:\Program Files (x86)\GarenaLoL\GameData\Apps\LoL";
                if (openFileDialog1.ShowDialog(new Form() { TopMost = true, TopLevel = true }) == DialogResult.OK)
                {
                    if (File.Exists(openFileDialog1.SelectedPath + @"\lol.launcher.exe") || File.Exists(openFileDialog1.SelectedPath + @"\lol.exe"))
                    {
                        Settings.Default.LeaguePath = openFileDialog1.SelectedPath;
                        Settings.Default.Save();
                        loopIt = false;
                    }
                    else
                    {
                        MessageBox.Show("Unable to locate League of Legends. Please try again.", "Can't Find League", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                    }
                }
                else
                {
                    // User did cancel the window.
                    didCancel = true;
                    loopIt = false;
                }
            }
        }
    }
}
