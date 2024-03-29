﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Bhaptics.Tact;
using CustomWebSocketSharp;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace TactsuitAlyx
{
    public partial class Form1 : Form
    {
        bool parsingMode = false;

        public TactsuitVR tactsuitVr;
        public Engine engine;
        public string alyxDirectory = "C:\\Steam\\steamapps\\common\\Half-Life Alyx";


        private delegate void SafeCallDelegate(string text);

        public Form1()
        {
            InitializeComponent();
        }
        public static IEnumerable<string> ReadLines(string path)
        {
            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 0x1000, FileOptions.SequentialScan))
            using (var sr = new StreamReader(fs, Encoding.UTF8))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    yield return line;
                }
            }
        }

        private void WriteTextSafe(string text)
        {
            if (lblInfo.InvokeRequired)
            {
                var d = new SafeCallDelegate(WriteTextSafe);
                lblInfo.Invoke(d, new object[] { text });
            }
            else
            {
                lblInfo.Text = text;
            }
        }

        void ParseLine(string line)
        {
            string newLine = line.Replace('{', ' ');
            newLine = newLine.Replace('}', ' ');
            newLine = newLine.Trim();

            string[] splitted = newLine.Split(new char[] {'|'});

            if (splitted.Length > 0)
            {
                string command = splitted[0].Trim();
                if (command == "PlayerHealth")
                {
                    if (splitted.Length > 1)
                    {
                        int health = int.Parse(splitted[1].Trim());
                        if (health >= 0)
                        {
                            engine.HealthRemaining(health);
                        }
                    }
                }
                else if (command == "PlayerHurt")
                {
                    if (splitted.Length > 1)
                    {
                        int healthRemaining = int.Parse(splitted[1].Trim());
                        string enemy = "";
                        float playerAngle = 0;
                        string enemyName = "";
                        string enemyDebugName = "";

                        if (splitted.Length > 2)
                        {
                            enemy = splitted[2].Trim();
                        }

                        if (splitted.Length > 3)
                        {
                            playerAngle = float.Parse(splitted[3].Trim());
                        }
                        if (splitted.Length > 4)
                        {
                            enemyName = (splitted[4].Trim());
                        }
                        if (splitted.Length > 5)
                        {
                            enemyDebugName = (splitted[5].Trim());
                        }

                        engine.PlayerHurt(healthRemaining, enemy, playerAngle, enemyName, enemyDebugName);
                    }
                }
                else if (command == "PlayerShootWeapon")
                {
                    if (splitted.Length > 1)
                    {
                        engine.PlayerShoot(splitted[1].Trim());
                    }
                }
                else if (command == "TwoHandStart")
                {
                    engine.twoHandedMode = true;
                }
                else if (command == "TwoHandEnd")
                {
                    engine.twoHandedMode = false;
                }
                else if (command == "PlayerOpenedGameMenu")
                {
                    engine.menuOpen = true;
                }
                else if (command == "PlayerClosedGameMenu")
                {
                    engine.menuOpen = false;
                }
                else if (command == "PlayerShotgunUpgradeGrenadeLauncherState")
                {
                    if (splitted.Length > 1)
                    {
                        int state = int.Parse(splitted[1].Trim());

                        engine.GrenadeLauncherStateChange(state);
                    }
                }
                else if (command == "PlayerGrabbityLockStart")
                {
                    if (splitted.Length > 1)
                    {
                        int primary = int.Parse(splitted[1].Trim());
                        
                        engine.GrabbityLockStart(primary == 1);
                    }
                }
                else if (command == "PlayerGrabbityLockStop")
                {
                    if (splitted.Length > 1)
                    {
                        int primary = int.Parse(splitted[1].Trim());

                        engine.GrabbityLockStop(primary == 1);
                    }
                }
                else if (command == "PlayerGrabbityPull")
                {
                    if (splitted.Length > 1)
                    {
                        int primary = int.Parse(splitted[1].Trim());

                        engine.GrabbityGlovePull(primary == 1);
                    }
                }
                else if (command == "PlayerGrabbedByBarnacle")
                {
                    engine.BarnacleGrabStart();
                }
                else if (command == "PlayerReleasedByBarnacle")
                {
                    engine.barnacleGrab = false;
                }
                else if (command == "PlayerDeath")
                {
                    if (splitted.Length > 1)
                    {
                        int damagebits = int.Parse(splitted[1].Trim());

                        engine.PlayerDeath(damagebits);
                    }
                }
                else if (command == "Reset")
                {
                    engine.Reset();
                }
                else if (command == "PlayerCoughStart")
                {
                    engine.Cough();
                }
                else if (command == "PlayerCoughEnd")
                {
                    engine.coughing = false;
                }
                else if (command == "PlayerCoveredMouth")
                {
                    engine.coughing = false;
                }
                else if (command == "GrabbityGloveCatch")
                {
                    if (splitted.Length > 1)
                    {
                        int primary = int.Parse(splitted[1].Trim());

                        engine.GrabbityGloveCatch(primary == 1);
                    }
                }
                else if (command == "PlayerDropAmmoInBackpack")
                {
                    if (splitted.Length > 1)
                    {
                        int leftShoulder = int.Parse(splitted[1].Trim());
                        engine.DropAmmoInBackpack(leftShoulder == 1);
                    }
                }
                else if (command == "PlayerDropResinInBackpack")
                {
                    if (splitted.Length > 1)
                    {
                        int leftShoulder = int.Parse(splitted[1].Trim());
                        engine.DropResinInBackpack(leftShoulder == 1);
                    }
                }
                else if (command == "PlayerRetrievedBackpackClip")
                {
                    if (splitted.Length > 1)
                    {
                        int leftShoulder = int.Parse(splitted[1].Trim());
                        engine.RetrievedBackpackClip(leftShoulder == 1);
                    }
                }
                else if (command == "PlayerStoredItemInItemholder"
                         || command == "HealthPenTeachStorage"
                         //|| command == "HealthVialTeachStorage"
                         )
                {
                    if (splitted.Length > 1)
                    {
                        int leftHolder = int.Parse(splitted[1].Trim());
                        engine.StoredItemInItemholder(leftHolder == 1);
                    }
                }
                else if (command == "PlayerRemovedItemFromItemholder")
                {
                    if (splitted.Length > 1)
                    {
                        int leftHolder = int.Parse(splitted[1].Trim());
                        engine.RemovedItemFromItemholder(leftHolder == 1);
                    }
                }
                else if (command == "PrimaryHandChanged" || command == "SingleControllerModeChanged")
                {
                    if (splitted.Length > 1)
                    {
                        int leftHanded = int.Parse(splitted[1].Trim());

                        engine.leftHandedMode = leftHanded == 1;
                    }
                }
                else if (command == "PlayerHeal")
                {
                    float angle = 0;
                    if (splitted.Length > 1)
                    {
                        angle = float.Parse(splitted[1].Trim());
                    }
                    engine.HealthPenUse(angle);
                }
                else if (command == "PlayerUsingHealthstation")
                {
                    if (splitted.Length > 1)
                    {
                        int leftArm = int.Parse(splitted[1].Trim());
                        engine.HealthStationUse(leftArm == 1);
                    }
                }
                else if (command == "ItemPickup")
                {
                    if (splitted.Length > 1)
                    {
                        string item = splitted[1].Trim();

                        if (item == "item_hlvr_crafting_currency_large" || item == "item_hlvr_crafting_currency_small")
                        {
                            if (splitted.Length > 2)
                            {
                                int leftShoulder = int.Parse(splitted[2].Trim());
                                engine.RetrievedBackpackResin(leftShoulder == 1);
                            }
                        }
                    }
                }
                else if (command == "ItemReleased")
                {
                    if (splitted.Length > 1)
                    {
                        string item = splitted[1].Trim();

                        if (item == "item_hlvr_prop_battery")
                        {
                            if (splitted.Length > 2)
                            {
                                int leftArm = int.Parse(splitted[2].Trim());
                                engine.ShockOnArm(leftArm == 1);
                            }
                        }
                    }
                }
                else if (command == "PlayerPistolClipInserted" || command == "PlayerShotgunShellLoaded" || command == "PlayerRapidfireInsertedCapsuleInChamber" || command == "PlayerRapidfireInsertedCapsuleInMagazine")
                {
                    engine.ClipInserted();
                }
                else if (command == "PlayerPistolChamberedRound" || command == "PlayerShotgunLoadedShells" || command == "PlayerRapidfireClosedCasing" || command == "PlayerRapidfireOpenedCasing")
                {
                    engine.ChamberedRound();
                }
            }
            WriteTextSafe(line);

            GC.Collect();
        }

        void ParseConsole()
        {
            string filePath = txtAlyxDirectory.Text + "\\game\\hlvr\\console.log";

            bool first = true;
            int counter = 0;

            while (parsingMode)
            {
                if (File.Exists(filePath))
                {
                    if (first)
                    {
                        first = false;
                        WriteTextSafe("Interface active");
                        counter = ReadLines(filePath).Count();
                    }
                    int lineCount = ReadLines(filePath).Count();//read text file line count to establish length for array
                    
                    if (counter < lineCount && lineCount > 0)//if counter is less than lineCount keep reading lines
                    {
                        var lines = Enumerable.ToList(ReadLines(filePath).Skip(counter).Take(lineCount - counter));
                        for (int i = 0; i < lines.Count; i++)
                        {
                            if (lines[i].Contains("[Tactsuit]"))
                            {
                                //Do haptic feedback
                                string line = lines[i].Substring(lines[i].LastIndexOf(']') + 1);
                                Thread thread = new Thread(() => ParseLine(line));
                                thread.Start();
                            }
                            else if (lines[i].Contains("unpaused the game"))
                            {
                                engine.menuOpen = false;
                            }
                            else if (lines[i].Contains("paused the game"))
                            {
                                engine.menuOpen = true;
                            }
                            else if (lines[i].Contains("Quitting"))
                            {
                                engine.Reset();
                            }
                        }

                        counter += lines.Count;
                    }
                    else if (counter == lineCount && lineCount > 0)
                    {
                        Thread.Sleep(50);
                    }
                    else
                    {
                        counter = 0;
                    }
                }
                else
                {
                    WriteTextSafe("Cannot file console.log. Waiting.");

                    Thread.Sleep(2000);
                }
            }
            WriteTextSafe("Waiting...");
        }

        private void Connect(string myIP)
        {
            string exePath = txtAlyxDirectory.Text + "\\game\\bin\\win64\\hlvr.exe";
            if (!File.Exists(exePath))
            {
                MessageBox.Show("Please select your Half-Life Alyx installation folder correctly first: " + exePath, "Error Starting", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            string scriptPath = txtAlyxDirectory.Text + "\\game\\hlvr\\scripts\\vscripts\\tactsuit.lua";
            if (!File.Exists(scriptPath))
            {
                MessageBox.Show("Script file installation is not correct. Please read the instructions on the mod page and reinstall.", "Script Installation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            string scriptLoaderPath = txtAlyxDirectory.Text + "\\game\\hlvr\\cfg\\skill_manifest.cfg";
            string configText = File.ReadAllText(scriptLoaderPath);
            if (!configText.Contains("script_reload_code tactsuit.lua"))
            {
                MessageBox.Show("skill_manifest.cfg file installation is not correct. Please read the instructions on the mod page and reinstall.", "Script Installation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            WriteTextSafe("Starting...");
            btnStart.Enabled = false;
            btnStop.Enabled = true;
            btnBrowse.Enabled = false;

            tactsuitVr = new TactsuitVR();
            tactsuitVr.CreateSystem(myIP);
            Thread.Sleep(5000);
            if (tactsuitVr.systemInitialized)
            {
                engine = new Engine(tactsuitVr);
                parsingMode = true;
                Thread thread = new Thread(ParseConsole);
                thread.Start();
            }
            else
            {
                tactsuitVr.DestroySystem();
                tactsuitVr = null;
                btnStart.Enabled = true;
                btnStop.Enabled = false;
                btnBrowse.Enabled = true;
                string errorMessage = "Unable to connect";
                if (myIP != "") errorMessage += " to IP " + myIP;
                errorMessage += "? Please make sure the OWO app is running in the same network and with mobile data turned off.";
                MessageBox.Show(errorMessage, "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            Connect("");
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            btnStart.Enabled = true;
            btnStop.Enabled = false;
            btnBrowse.Enabled = true;

            parsingMode = false;

            if (tactsuitVr.systemInitialized)
            {
                tactsuitVr.DestroySystem();
                tactsuitVr = null;
                //tactsuitVr.hapticPlayer.Disable();
                //tactsuitVr.hapticPlayer.Dispose();
            }

            WriteTextSafe("Stopping...");
        }

        public string readAlyxDir()
        {
            string mydir = alyxDirectory;
            if (File.Exists(".\\AlyxDir.settings"))
            {
                StreamReader sr = new StreamReader(".\\AlyxDir.settings");
                mydir = sr.ReadLine();
                alyxDirectory = mydir;
                sr.Close();
            }

            return mydir;
        }

        public void writeAlyxDir(string directory)
        {
            StreamWriter sw = new StreamWriter(".\\AlyxDir.settings", true, Encoding.ASCII);
            sw.WriteLine(directory);
            sw.Close();
            alyxDirectory = directory;
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            dialog.InitialDirectory = "C:\\";
            dialog.RestoreDirectory = true;
            dialog.IsFolderPicker = true;
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                txtAlyxDirectory.Text = dialog.FileName;
                writeAlyxDir(txtAlyxDirectory.Text);
                //Properties.Settings.Default.AlyxDirectory = txtAlyxDirectory.Text;
                //Properties.Settings.Default.Save();
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            btnStart.Enabled = true;
            btnStop.Enabled = false;
            btnBrowse.Enabled = true;

            parsingMode = false;

            if (tactsuitVr != null && tactsuitVr.systemInitialized)
            {
                WriteTextSafe("Stopping...");
                //tactsuitVr.hapticPlayer.Disable();
                //tactsuitVr.hapticPlayer.Dispose();
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            txtAlyxDirectory.Text = readAlyxDir();
        }

        private static void CopyFilesRecursively(string sourcePath, string targetPath)
        {
            //Now Create all of the directories
            foreach (string dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
            {
                if (!Directory.Exists(dirPath.Replace(sourcePath, targetPath))) Directory.CreateDirectory(dirPath.Replace(sourcePath, targetPath));
            }

            //Copy all the files & Replaces any files with the same name
            foreach (string newPath in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories))
            {
                File.Copy(newPath, newPath.Replace(sourcePath, targetPath), true);
            }
        }

        private void InstallMod_Click(object sender, EventArgs e)
        {
            string exePath = txtAlyxDirectory.Text + "\\game\\bin\\win64\\hlvr.exe";
            if (!File.Exists(exePath))
            {
                MessageBox.Show("Please select your Half-Life Alyx installation folder correctly first: " + exePath, "Error Starting", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            string scriptPath = txtAlyxDirectory.Text + "\\game\\hlvr\\scripts\\vscripts\\tactsuit.lua";
            if (!File.Exists(scriptPath))
            {
                WriteTextSafe("Installing...");
                CopyFilesRecursively(".\\scripts", txtAlyxDirectory.Text);
                return;
            }
            else
            {
                WriteTextSafe("Already installed");
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string myIP = IP.Text;
            IPAddress myAddress;
            if (myIP == "")
            {
                MessageBox.Show("Please enter an IP address in the field behind the button.", "IP Address Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if ((!myIP.Contains("."))||(!IPAddress.TryParse(myIP, out myAddress)))
            {
                MessageBox.Show("Please enter a valid IP address (e.g. 192.168.1.15) in the field.", "IP Address Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            Connect(myIP);
        }
    }
}
