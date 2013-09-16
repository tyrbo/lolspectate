﻿using System;
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

        [STAThread]
        static void Main(string[] args)
        {
            if (Settings.Default.LeaguePath == null || Settings.Default.LeaguePath == "")
            {
                MessageBox.Show("We can't find your League installation.\nYou'll need to show us where it is.", "Initial Setup", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                RequestPath();
            }
            else
            {
                if (!File.Exists(Settings.Default.LeaguePath + @"\lol.launcher.exe"))
                {
                    MessageBox.Show("We can't find your League installation.\nYou'll need to show us where it is.", "Initial Setup", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                    RequestPath();
                }
            }

            if (args.Length == 0)
                Environment.Exit(0);

            argCopy = args;

            checkForUpdates();

            string leaguePath = Settings.Default.LeaguePath;
            string spectatorUrl = args[0].Replace("lsp://", "").Replace("%20", " ").TrimEnd('/');
            string arguments = "\"8391\" \"\" \"\" \"" + spectatorUrl + "\"";

            //MessageBox.Show(leaguePath + " " + spectatorUrl + " " + arguments);

            DirectoryInfo di = new DirectoryInfo(leaguePath + @"\RADS\solutions\lol_game_client_sln\releases");
            DirectoryInfo[] dirs = di.GetDirectories();

            string folderName = "";
            DateTime modifiedTime = new DateTime();
            
            foreach (DirectoryInfo dir in dirs)
            {
                try
                {
                    if (dir.LastWriteTime > modifiedTime)
                    {
                        modifiedTime = dir.LastWriteTime;
                        folderName = dir.Name;
                    }
                }
                catch { }
            }

            ProcessStartInfo start = new ProcessStartInfo();
            start.Arguments = arguments;
            start.WorkingDirectory = leaguePath + @"\RADS\solutions\lol_game_client_sln\releases\" + folderName + @"\deploy\";
            start.FileName = start.WorkingDirectory + @"League of Legends.exe";
            Process p = new Process();
            p = Process.Start(start);
        }

        static void checkForUpdates()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            string version = fvi.FileVersion;
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
                if (openFileDialog1.ShowDialog(new Form() { TopMost = true, TopLevel = true }) == DialogResult.OK)
                {
                    if (File.Exists(openFileDialog1.SelectedPath + @"\lol.launcher.exe"))
                    {
                        Settings.Default.LeaguePath = openFileDialog1.SelectedPath;
                        Settings.Default.Save();
                        loopIt = false;
                    }
                    else
                    {
                        MessageBox.Show("Unable to locate \"lol.launcher.exe\". Please try again.", "Can't Find League", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                    }
                }
                else
                {
                    loopIt = false;
                }
            }
        }
    }
}