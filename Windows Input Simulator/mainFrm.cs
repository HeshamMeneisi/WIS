using System;
using System.Drawing;
using System.Windows.Forms;
using Simulator;
using System.Runtime.InteropServices;
using System.IO;
using Microsoft.Win32;

namespace Windows_Input_Simulator
{
    public partial class mainFrm : Form
    {
        string[] Arguments;
        public mainFrm(string[] args)
        {
            Arguments = args;
            InitializeComponent();
        }
        // Declarations
        RecorderSim recorder = new RecorderSim();
        ExecuterSim executer = new ExecuterSim();
        EventSet loadedset = null;
        Scheduler scheduler;
        RegistryKey GlobalRegistryKey = Registry.CurrentUser.CreateSubKey("Software\\Windows Event Simulator(.Net)");

        // Methods
        public Image NullImage()
        {
            Image img = new Bitmap(icon.Width, icon.Height);
            Graphics gpx = Graphics.FromImage(img);
            gpx.DrawString("X", new Font("Arial", 10, FontStyle.Bold), Brushes.Red, 2, 1);
            return img;
        }
        // Events
        private void mouse_det(MSMovedEventArgs e)
        {
            mousepos.Text = ((e.X > Screen.PrimaryScreen.Bounds.Width) ? Screen.PrimaryScreen.Bounds.Width.ToString() : (e.X > 0) ? e.X.ToString() : "0") + "," + ((e.Y > Screen.PrimaryScreen.Bounds.Height) ? Screen.PrimaryScreen.Bounds.Height.ToString() : (e.Y > 0) ? e.Y.ToString() : "0");
        }
        private void rcall_CheckedChanged(object sender, EventArgs e)
        {
            if (rcall.Checked)
            {
                rckeybd.Checked = rcmbs.Checked = rcmm.Checked = true;
                rckeybd.Enabled = rcmbs.Enabled = rcmm.Enabled = false;
            }
            else
            {
                rckeybd.Enabled = rcmbs.Enabled = rcmm.Enabled = true;
            }
        }

        private void startrcbtn_Click(object sender, EventArgs e)
        {
            if (recorder.Status == RecorderSim.SimStatus.Idle)
            {
                startrcbtn.Enabled = false;
                stoprcbtn.Enabled = true;
                rcpauseBtn.Enabled = true;
                rcoptions.Enabled = false;
                rcstatus.Text = "Recording!";
                if (rcdesktop.Checked)
                {
                    DisplayControl.MinimizeAll();
                }
                recorder.StartRecording(rckeybd.Checked, rcmbs.Checked, rcmm.Checked, isk.Checked, rcdesktop.Checked, (RecorderSim.DoubleClickDetectionMethod)Properties.Settings.Default.dcdm);
                AddLog(DateTime.Now, "Recording started.");
            }
            else
            {
                MessageBox.Show(this, "The local recorder is already running. There can be only one instance of the recorder running at a time.", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Stop, MessageBoxDefaultButton.Button1);
            }
        }

        private void stoprcbtn_Click(object sender, EventArgs e)
        {
            if (recorder.Status != RecorderSim.SimStatus.Idle)
            {
                recorder.StopRecording();
                stoprcbtn.Enabled = false;
                startrcbtn.Enabled = true;
                rcpauseBtn.Enabled = false;
                rcoptions.Enabled = true;
                rcstatus.Text = "Stopped";
                AddLog(DateTime.Now, "Recording session ended.");
                if (MessageBox.Show(this, "Recording session ended.\nDo you want to save the recorded file?", "Recorder", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1) == DialogResult.Yes)
                {
                    if (saveFileDialog1.ShowDialog(this) == DialogResult.OK)
                    {
                        FileManager.WriteToFile(recorder.GetEventSet(), saveFileDialog1.FileName);
                    }
                }
            }
        }
        Log logForm = new Log();
        private void Form1_Load(object sender, EventArgs e)
        {
            //
            logForm.relativePos = new Point(this.Width + 10, 0);
            logForm.Show(this);
            //
            Environment.CurrentDirectory = Path.GetDirectoryName(Application.ExecutablePath);
            tabControl1.SelectedIndex = 0;
            this.TopMost = Properties.Settings.Default.topmost;
            srtrchotkey.Items.AddRange(KeyManager.GetAllKeyNames());
            prchotkey.Items.AddRange(KeyManager.GetAllKeyNames());
            stprchotkey.Items.AddRange(KeyManager.GetAllKeyNames());
            srtexhotkey.Items.AddRange(KeyManager.GetAllKeyNames());
            pexhotkey.Items.AddRange(KeyManager.GetAllKeyNames());
            stpexhotkey.Items.AddRange(KeyManager.GetAllKeyNames());
            try { schedenabled.Checked = bool.Parse(GlobalRegistryKey.GetValue("SchedulerEnabled").ToString()); }
            catch { }
            try { srtrchotkey.Text = GlobalRegistryKey.GetValue("StartRecHotkey").ToString(); }
            catch { }
            try { prchotkey.Text = GlobalRegistryKey.GetValue("PauseRecHotkey").ToString(); }
            catch { }
            try { stprchotkey.Text = GlobalRegistryKey.GetValue("StopRecHotkey").ToString(); }
            catch { }
            try { srtexhotkey.Text = GlobalRegistryKey.GetValue("StartExeHotkey").ToString(); }
            catch { }
            try { pexhotkey.Text = GlobalRegistryKey.GetValue("PauseExeHotkey").ToString(); }
            catch { }
            try { stpexhotkey.Text = GlobalRegistryKey.GetValue("StopExeHotkey").ToString(); }
            catch { }
            executer.Finished += Executer_Finished;
            HotkeyManager.SetHotkeys((Keys)KeyManager.GetKey(srtrchotkey.Text), (Keys)KeyManager.GetKey(prchotkey.Text) ,(Keys)KeyManager.GetKey(stprchotkey.Text), (Keys)KeyManager.GetKey(srtexhotkey.Text), (Keys)KeyManager.GetKey(pexhotkey.Text), (Keys)KeyManager.GetKey(stpexhotkey.Text));
            HotkeyManager.HotkeyPressed += hotkey_pressed;
            HotkeyManager.Init();
            HookManager.MouseMove += mouse_det;
            scheduler = new Scheduler(schedenabled.Checked);
            scheduler.LoadEvents();
            scheduler.EventRaised += sched_eve;
            listView1.Items.AddRange(scheduler.GetEventsTable());
            if (Arguments.Length > 0)
            {
                tabControl1.SelectedTab = executerTab;
                FileManager.ReadFile(ref loadedset, Arguments[0]);
                filename.Text = Arguments[0];
                ac.Text = loadedset.EventsCount.ToString();
                tt.Text = ((double)loadedset.TotalTime / 1000).ToString("F2") + " Sec";
                sr.Text = loadedset.Resolution.Width + "x" + loadedset.Resolution.Height;
                csr.Text = Screen.PrimaryScreen.Bounds.Width + "x" + Screen.PrimaryScreen.Bounds.Height;
                icon.Image = IconReader.GetFileIcon(Environment.GetCommandLineArgs()[1], IconReader.EnumIconSize.Small, false).ToBitmap();
                if (Properties.Settings.Default.executeonload)
                {
                    startexbtn_Click(this, EventArgs.Empty);
                }
            }
            icon.Image = NullImage();
            iconb.Image = NullImage();

            HookManager.Init();
        }
        private void sched_eve(object sender, EventRaisedEventArgs e)
        {
        }
        private void hotkey_pressed(HotkeyPressedEventArgs e)
        {
            switch (e.KeyPressed)
            {
                case Hotkey.StartRecording:
                    startrcbtn_Click(this, EventArgs.Empty);
                    break;
                case Hotkey.PauseRecording:
                    rcpauseBtn_Click(this, EventArgs.Empty);
                    break;
                case Hotkey.StopRecording:
                    stoprcbtn_Click(this, EventArgs.Empty);
                    break;
                case Hotkey.StartExecuting:
                    startexbtn_Click(this, EventArgs.Empty);
                    break;
                case Hotkey.PauseExecuting:
                    expauseBtn_Click(this, EventArgs.Empty);
                    break;
                case Hotkey.StopExecuting:
                    stopexbtn_Click(this, EventArgs.Empty);
                    break;
            }
        }
        public bool loop = false;
        private void Executer_Finished(ExecuterSim sender, ExecuterFinishedEventArgs e)
        {
            if (loop && !e.Interrupted)
            {
                executer.LoadSet(loadedset);
                executer.StartExecuting(sopt);
            }
            else
            {
                stopexbtn.Invoke((MethodInvoker)(() => stopexbtn_Click(this, EventArgs.Empty)));
                AddLog(DateTime.Now, "Executed all events successfully.");
            }
        }
        private void stopexbtn_Click(object sender, EventArgs e)
        {
            if (scheduler.EventInProgress != null)
            {
                scheduler.EventInProgress = null;
            }
            exeoptions.Enabled = true;
            startexbtn.Enabled = true;
            stopexbtn.Enabled = false;
            expauseBtn.Enabled = false;
            exstatus.Text = "Stopped";
            if (executer.Status != ExecuterSim.SimStatus.Idle)
            {
                executer.StopExecuting();
                AddLog(DateTime.Now, "Execution session ended.");
            }
            if (resolutionChanged)
            {
                DisplayControl.SetResolution(dispResolution);
            }
        }

        public void AddLog(DateTime dateTime, string p)
        {
            logForm.AddLog(dateTime, p);
        }
        string executerLoadedFile = string.Empty;
        private void browse_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog(this) == DialogResult.OK)
            {
                FileManager.ReadFile(ref loadedset, openFileDialog1.FileName);
                exdesktop.Checked = loadedset.ShowDesktop;
                mpCb.SelectedIndex = loadedset.Multiplier - 1;
                rsk.Checked = !loadedset.IgnoreKeyStats;
                filename.Text = openFileDialog1.FileName;
                fileWatcher.Path = Path.GetDirectoryName(openFileDialog1.FileName);
                fileWatcher.Filter = Path.GetFileName(openFileDialog1.FileName);
                ac.Text = loadedset.EventsCount.ToString();
                tt.Text = ((double)loadedset.TotalTime / 1000).ToString("F2") + " Sec";
                sr.Text = loadedset.Resolution.Width + "x" + loadedset.Resolution.Height;
                csr.Text = Screen.PrimaryScreen.Bounds.Width + "x" + Screen.PrimaryScreen.Bounds.Height;
                icon.Image = IconReader.GetFileIcon(openFileDialog1.FileName, IconReader.EnumIconSize.Small, false).ToBitmap();
                executerLoadedFile = openFileDialog1.FileName;
            }
        }
        private void fileWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            if(MessageBox.Show(this, "The file currently loaded in the executer has been changed, would you like to reload it?","File Watcher",MessageBoxButtons.YesNo,MessageBoxIcon.Question) == DialogResult.Yes)
            {
                string p = Path.Combine(fileWatcher.Path, fileWatcher.Filter);
                FileManager.ReadFile(ref loadedset, p);
                filename.Text = p;
                ac.Text = loadedset.EventsCount.ToString();
                tt.Text = ((double)loadedset.TotalTime / 1000).ToString("F2") + " Sec";
                sr.Text = loadedset.Resolution.Width + "x" + loadedset.Resolution.Height;
                csr.Text = Screen.PrimaryScreen.Bounds.Width + "x" + Screen.PrimaryScreen.Bounds.Height;
                icon.Image = IconReader.GetFileIcon(p, IconReader.EnumIconSize.Small, false).ToBitmap();
            }
        }
        bool resolutionChanged = false;
        Size dispResolution = Screen.PrimaryScreen.Bounds.Size;
        SOptions sopt;
        private void startexbtn_Click(object sender, EventArgs e)
        {
            if(loadedset != null)
            {
                if (executer.Status == ExecuterSim.SimStatus.Idle)
                {
                    if (Screen.PrimaryScreen.Bounds.Size != loadedset.Resolution)
                    {
                        if (MessageBox.Show(this, "This file was recorded in a different screen resolution. ("+loadedset.Resolution.Width + "*" + loadedset.Resolution.Height +")\nHowever, the executer will try to resolve the difference in both X and Y axis but the clicks will remain inaccurate.\nDo you want to change the current screen resolution instead?\nNote: The current resolution will be restored after the execution is finished.", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                        {
                            dispResolution = Screen.PrimaryScreen.Bounds.Size;
                            try
                            {
                                DisplayControl.SetResolution(loadedset.Resolution);
                                resolutionChanged = true;
                                while (Screen.PrimaryScreen.Bounds.Size == dispResolution)
                                {
                                    // Wait for change
                                    Application.DoEvents(); // Prevent lag
                                }
                            }
                            catch (Exception ex)
                            {
                                AddLog(DateTime.Now, ex.Message + "\nProceeding with current resolution.");
                            }
                        }
                    }
                    executer.LoadSet(loadedset);
                    exeoptions.Enabled = false;
                    startexbtn.Enabled = false;
                    expauseBtn.Enabled = true;
                    stopexbtn.Enabled = true;
                    exstatus.Text = "Executing!";
                    executer.StartExecuting(sopt = new SOptions(exkebd.Checked, exmbs.Checked, exmm.Checked, rsk.Checked, mpCb.SelectedIndex + 1, exdesktop.Checked, loadedset.RestoreMouse));
                    AddLog(DateTime.Now, "Execution started.");
                }
                else
                {
                    AddLog(DateTime.Now, "The local executer is already running. There can be only one instance of the executer running at a time.");
                }
            }
            else
            {
                AddLog(DateTime.Now, "There's no valid event set to execute!");
            }
        }

        private void addeve_Click(object sender, EventArgs e)
        {
            AddEvent();
        }
        private void saveve_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                listView1.Items.Remove(listView1.SelectedItems[0]);
                LoadEventsToScheduler();
                AddEvent();
                LoadEventsToScheduler();
                scheduler.SaveEvents();
            }
        }
        private void AddEvent()
        {
            if (File.Exists(evefilename.Text))
            {
                if (evetype.SelectedIndex != -1)
                {
                    DateTime time = new DateTime((int)yy.Value, (int)mon.Value, (int)dd.Value, (int)hh.Value, (int)min.Value, 0);
                    if (GetSimEventType(evetype.Text) == SimEventType.Weekly)
                    {
                        if (dow.SelectedIndex != -1)
                        {
                            do
                            {
                                time = new DateTime(2010, 12, time.Day + 1, time.Hour, time.Minute, 0);
                            } while (time.DayOfWeek.ToString() != dow.Text);
                        }
                        else
                        {
                            MessageBox.Show(this, "Please Select Day Of The Week.", "Scheduler", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                    }
                    SimEvent eve = new SimEvent(time, evefilename.Text, GetSimEventType(evetype.Text), DateTime.Now);
                    scheduler.AddEvent(eve);
                    scheduler.SaveEvents();
                    listView1.Items.Clear();
                    listView1.Items.AddRange(scheduler.GetEventsTable());
                }
                else
                {
                    MessageBox.Show(this, "Please Select Event Type.", "Scheduler", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show(this, "File Doesn't Exist!", "Scheduler", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private SimEventType GetSimEventType(string text)
        {
            switch (text)
            {
                case "Once": return SimEventType.Once;
                case "Yearly": return SimEventType.Yearly;
                case "Monthly": return SimEventType.Monthly;
                case "Weekly": return SimEventType.Weekly;
                case "Daily": return SimEventType.Daily;
                default: return SimEventType.Hourly;
            }
        }

        private void schedenabled_CheckedChanged(object sender, EventArgs e)
        {
            scheduler.Enabled = schedenabled.Checked;
            GlobalRegistryKey.SetValue("SchedulerEnabled", schedenabled.Checked);
        }

        private void browseb_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog(this) == DialogResult.OK)
            {
                evefilename.Text = openFileDialog1.FileName;
                iconb.Image = IconReader.GetFileIcon(openFileDialog1.FileName, IconReader.EnumIconSize.Small, false).ToBitmap();
            }
        }

        private void evetype_TextChanged(object sender, EventArgs e)
        {
            if (!evetype.Items.Contains(evetype.Text))
            {
                evetype.Text = "<Select Event Type>";
            }
            else
            {
                switch (GetSimEventType(evetype.Text))
                {
                    case SimEventType.Once:
                        {
                            yy.Enabled = true;
                            mon.Enabled = true;
                            dd.Enabled = true;
                            hh.Enabled = true;
                            min.Enabled = true;
                            dow.Enabled = false;
                        } break;
                    case SimEventType.Yearly:
                        {
                            yy.Enabled = false;
                            mon.Enabled = true;
                            dd.Enabled = true;
                            hh.Enabled = true;
                            min.Enabled = true;
                            dow.Enabled = false;
                        } break;
                    case SimEventType.Monthly:
                        {
                            yy.Enabled = false;
                            mon.Enabled = false;
                            dd.Enabled = true;
                            hh.Enabled = true;
                            min.Enabled = true;
                            dow.Enabled = false;
                        } break;
                    case SimEventType.Weekly:
                        {
                            yy.Enabled = false;
                            mon.Enabled = false;
                            dd.Enabled = false;
                            hh.Enabled = true;
                            min.Enabled = true;
                            dow.Enabled = true;
                        } break;
                    case SimEventType.Daily:
                        {
                            yy.Enabled = false;
                            mon.Enabled = false;
                            dd.Enabled = false;
                            hh.Enabled = true;
                            min.Enabled = true;
                            dow.Enabled = false;
                        } break;
                    default:
                        {
                            yy.Enabled = false;
                            mon.Enabled = false;
                            dd.Enabled = false;
                            hh.Enabled = false;
                            min.Enabled = true;
                            dow.Enabled = false;
                        } break;

                }
            }
        }
        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                LoadEventOnBoard(listView1.SelectedItems[0]);
                saveve.Enabled = true;
            }
            else
            {
                saveve.Enabled = false;
            }

        }
        public void LoadEventOnBoard(ListViewItem eveitem)
        {
            SimEvent eve = (SimEvent)eveitem.Tag;
            evetype.Text = eve.Type.ToString();
            yy.Value = eve.Time.Year;
            mon.Value = eve.Time.Month;
            dd.Value = eve.Time.Day;
            dow.Text = eve.Time.DayOfWeek.ToString();
            hh.Value = eve.Time.Hour;
            min.Value = eve.Time.Minute;
            evefilename.Text = eve.FileName;
        }

        private void deleteEventToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                listView1.Items.Remove(listView1.SelectedItems[0]);
                LoadEventsToScheduler();
                scheduler.SaveEvents();
            }
        }
        public void LoadEventsToScheduler()
        {
            scheduler.ClearEvents();
            foreach (ListViewItem item in listView1.Items)
            {
                scheduler.AddEvent((SimEvent)item.Tag);
            }
            scheduler.SaveEvents();
        }

        private void copyFileNameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                Clipboard.SetText(((SimEvent)listView1.SelectedItems[0].Tag).FileName);
            }
        }
        private void relallbtn_Click(object sender, EventArgs e)
        {
            executer.ReleaseAllDownKeys();
            AddLog(DateTime.Now, "All keys released.");
            MessageBox.Show(this, "All keys released.\nIf you are still encountering problems with stuck keys, try going to the switch user menu (Window + L or Start > Switch user) and switch back in.", "Executer", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }

        private void srtrchotkey_TextChanged(object sender, EventArgs e)
        {
            if (!srtrchotkey.Items.Contains(srtrchotkey.Text))
            {
                srtrchotkey.Text = HotkeyManager.StartRecordingHotkey.ToString();
            }
            HotkeyManager.StartRecordingHotkey = (Keys)KeyManager.GetKey(srtrchotkey.Text);
            GlobalRegistryKey.SetValue("StartRecHotkey", srtrchotkey.Text);
        }

        private void prchotkey_TextChanged(object sender, EventArgs e)
        {
            if (!prchotkey.Items.Contains(prchotkey.Text))
            {
                prchotkey.Text = HotkeyManager.StartRecordingHotkey.ToString();
            }
            HotkeyManager.PauseRecordingHotkey = (Keys)KeyManager.GetKey(prchotkey.Text);
            GlobalRegistryKey.SetValue("StartRecHotkey", prchotkey.Text);
        }

        private void stprchotkey_TextChanged(object sender, EventArgs e)
        {
            if (!stprchotkey.Items.Contains(stprchotkey.Text))
            {
                stprchotkey.Text = HotkeyManager.StopRecordingHotkey.ToString();
            }
            HotkeyManager.StopRecordingHotkey = (Keys)KeyManager.GetKey(stprchotkey.Text);
            GlobalRegistryKey.SetValue("StopRecHotkey", stprchotkey.Text);
        }

        private void srtexhotkey_TextChanged(object sender, EventArgs e)
        {
            if (!srtexhotkey.Items.Contains(srtexhotkey.Text))
            {
                srtexhotkey.Text = HotkeyManager.StartExecutingHotkey.ToString();
            }
            HotkeyManager.StartExecutingHotkey = (Keys)KeyManager.GetKey(srtexhotkey.Text);
            GlobalRegistryKey.SetValue("StartExeHotkey", srtexhotkey.Text);
        }

        private void pexhotkey_TextChanged(object sender, EventArgs e)
        {
            if (!pexhotkey.Items.Contains(stpexhotkey.Text))
            {
                pexhotkey.Text = HotkeyManager.StopExecutingHotkey.ToString();
            }
            HotkeyManager.PauseExecutingHotkey = (Keys)KeyManager.GetKey(pexhotkey.Text);
            GlobalRegistryKey.SetValue("StopExeHotkey", stpexhotkey.Text);
        }

        private void stpexhotkey_TextChanged(object sender, EventArgs e)
        {
            if (!stpexhotkey.Items.Contains(stpexhotkey.Text))
            {
                stpexhotkey.Text = HotkeyManager.StopExecutingHotkey.ToString();
            }
            HotkeyManager.StopExecutingHotkey = (Keys)KeyManager.GetKey(stpexhotkey.Text);
            GlobalRegistryKey.SetValue("StopExeHotkey", stpexhotkey.Text);
        }

        private void dow_TextChanged(object sender, EventArgs e)
        {
            if (!dow.Items.Contains(dow.Text))
            {
                dow.Text = "<Select Day>";
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (exall.Checked)
            {
                exkebd.Checked = true;
                exkebd.Enabled = false;
                exmbs.Checked = true;
                exmbs.Enabled = false;
                exmm.Checked = true;
                exmm.Enabled = false;
            }
            else
            {
                exkebd.Enabled = true;
                exmbs.Enabled = true;
                exmm.Enabled = true;
            }
        }

        private void rcstatus_TextChanged(object sender, EventArgs e)
        {
            if (rcstatus.Text.StartsWith("S"))
            {
                rcstatus.ForeColor = Color.Red;
            }
            else
            {
                rcstatus.ForeColor = Color.Green;
            }
        }

        private void exstatus_Click(object sender, EventArgs e)
        {
            if (exstatus.Text.StartsWith("S"))
            {
                exstatus.ForeColor = Color.Red;
            }
            else
            {
                exstatus.ForeColor = Color.Green;
            }
        }
        #region Settings
        void LoadSettingsGUI()
        {
            intset.Checked = Properties.Settings.Default.intset;
            intwas.Checked = Properties.Settings.Default.intwis;
            load.Checked = !Properties.Settings.Default.executeonload;
            loadandexe.Checked = Properties.Settings.Default.executeonload;
            topm.Checked = Properties.Settings.Default.topmost;
            if (Properties.Settings.Default.dcdm == 1)
                realdcd.Checked = true;
            else if (Properties.Settings.Default.dcdm == 2)
                prdcd.Checked = true;
            else
                ndcd.Checked = true;
            maxexespeed.Value = Properties.Settings.Default.evespersecond;
        }
        private void button3_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.Reset();
            LoadSettingsGUI();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            SettingsManager.SaveSettings(intset.Checked, intwas.Checked, (int)maxexespeed.Value, loadandexe.Checked, (realdcd.Checked) ? 1 : (prdcd.Checked) ? 2 : 0, topm.Checked);
            this.TopMost = Properties.Settings.Default.topmost;
            SettingsManager.ApplySettings();
        }

        private void settings_Load(object sender, EventArgs e)
        {
            LoadSettingsGUI();
        }
        #endregion
        private void loopChk_CheckedChanged(object sender, EventArgs e)
        {
            loop = loopChk.Checked;
        }
        #region Coder
        string coderLoadedFile = null;
        EventSet coderLoadedSet = new EventSet();
        private void openBtn_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog(this) == DialogResult.OK)
            {
                if (coderLoadedFile == null)
                {
                    coderLoadedFile = openFileDialog1.FileName;
                    FileManager.ReadFile(ref coderLoadedSet, coderLoadedFile);
                    codeTextBox.Text = CodeManager.GenerateCode(coderLoadedSet);
                }
                else if (MessageBox.Show(this, "Any unsaved changes to the current file will be lost.\nAre you sure that you want to open this file?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    coderLoadedFile = openFileDialog1.FileName;
                    FileManager.ReadFile(ref coderLoadedSet, coderLoadedFile);
                    codeTextBox.Text = CodeManager.GenerateCode(coderLoadedSet);
                }
            }
        }
        private void saveBtn_Click(object sender, EventArgs e)
        {
            if (coderLoadedFile != null)
            {
                try
                {
                    coderLoadedSet = CodeManager.Interpret(codeTextBox.Text);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                if (coderLoadedFile == executerLoadedFile)
                {
                    fileWatcher.EndInit();
                    FileManager.WriteToFile(coderLoadedSet, coderLoadedFile);
                    fileWatcher.BeginInit();
                }
                else
                    FileManager.WriteToFile(coderLoadedSet, coderLoadedFile);
            }
            else
                saveasBtn_Click(this, EventArgs.Empty);
        }
        private void saveasBtn_Click(object sender, EventArgs e)
        {
            if (saveFileDialog1.ShowDialog(this) == DialogResult.OK)
            {
                try
                {
                    coderLoadedSet = CodeManager.Interpret(codeTextBox.Text);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                if (saveFileDialog1.FileName == executerLoadedFile)
                {
                    fileWatcher.EndInit();
                    FileManager.WriteToFile(coderLoadedSet, saveFileDialog1.FileName);
                    fileWatcher.BeginInit();
                }
                else
                    FileManager.WriteToFile(coderLoadedSet, saveFileDialog1.FileName);
            }
        }
        #endregion

        private void button5_Click(object sender, EventArgs e)
        {
            tabControl1.SelectedTab = scriptTab;
            if (coderLoadedFile == null)
            {
                coderLoadedFile = filename.Text;
                FileManager.ReadFile(ref coderLoadedSet, coderLoadedFile);
                codeTextBox.Text = CodeManager.GenerateCode(coderLoadedSet);
            }
            else if (MessageBox.Show(this, "Any unsaved changes to the current file will be lost.\nAre you sure that you want to open this file?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                coderLoadedFile = filename.Text;
                FileManager.ReadFile(ref coderLoadedSet, coderLoadedFile);
                codeTextBox.Text = CodeManager.GenerateCode(coderLoadedSet);
            }
        }

        private void exc_Click(object sender, EventArgs e)
        {
            try
            {
                coderLoadedSet = CodeManager.Interpret(codeTextBox.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (executer.Status == ExecuterSim.SimStatus.Idle)
            {
                executer.LoadSet(coderLoadedSet);
                executer.StartExecuting(true, true, true, !coderLoadedSet.IgnoreKeyStats, coderLoadedSet.Multiplier, coderLoadedSet.ShowDesktop, coderLoadedSet.RestoreMouse);
            }
            else
                MessageBox.Show(this, "Local executer is already running. There can be only one instance of the executer running at a time.", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Stop, MessageBoxDefaultButton.Button1);
        }

        private void codeTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F5)
                exc_Click(this, EventArgs.Empty);
        }

        #region Insert Feature
        [DllImport("user32.dll")]
        public static extern short GetKeyState(int keyCode);
        enum ExampleType
        {
            Explained = 0, Brief= 1
        }
        private string GetExample(EventType etype, ExampleType extype)
        {
            if (extype == ExampleType.Explained)
            {
                if (etype == EventType.Wheel)
                    return "\n@(Delay In Milliseconds)do{" + Enum.GetName(typeof(EventType), etype) + "}using<Mouse X,Mouse Y|Rotation Angle>end;";
                else if (etype == EventType.KeyDown || etype == EventType.KeyUp)
                    return "\n@(Delay In Milliseconds)do{" + Enum.GetName(typeof(EventType), etype) + "}using<KeyCode>end;";
                else if(etype == EventType.ExecuteFile)
                    return "\n@(Delay In Milliseconds)do{" + Enum.GetName(typeof(EventType), etype) + "}using<FilePath>end;";
                else
                    return "\n@(Delay In Milliseconds)do{" + Enum.GetName(typeof(EventType), etype) + "}using<Mouse X,Mouse Y>end;";
            }
            else
            {
                if (etype == EventType.Wheel)
                    return "\n@(0)do{Wheel}using<0,0|120>end;";
                else if (etype == EventType.KeyDown || etype == EventType.KeyUp)
                    return "\n@(0)do{" + Enum.GetName(typeof(EventType), etype) + "}using<0>end;";
                else if (etype == EventType.ExecuteFile)
                    return "\n@(0)do{" + Enum.GetName(typeof(EventType), etype) + "}using<C:\\Example.set>end;";
                else
                    return "\n@(0)do{" + Enum.GetName(typeof(EventType), etype) + "}using<0,0>end;";
            }
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            insertMenu.Tag = ExampleType.Explained;
            insertMenu.Show(codeTextBox, 0, 0);
        }

        private void linkLabel3_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            insertMenu.Tag = ExampleType.Brief;
            insertMenu.Show(codeTextBox, 0, 0);
        }

        private void insertMenu_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            if (insertMenu.Items.IndexOf(e.ClickedItem) > 3)
            {
                codeTextBox.Text = codeTextBox.Text.Insert(codeTextBox.SelectionStart, GetExample((EventType)Enum.Parse(typeof(EventType), e.ClickedItem.Text), (ExampleType)insertMenu.Tag));
            }
            else
            {
                string tx = string.Empty;
                if ((ExampleType)insertMenu.Tag == ExampleType.Explained)
                {
                    switch (insertMenu.Items.IndexOf(e.ClickedItem))
                    {
                        case 0:
                            tx = "EnsureResolution(Screen Width,Screen Height);\n";
                            break;
                        case 1:
                            tx = "SpecialKeys(NumLock State,CapsLock State,ScrollLock State);\n";
                            break;
                        case 2:
                            tx = "ShowDesktop();\n";
                            break;
                        case 3:
                            tx = "SetMultiplier(Multiplier Integer);";
                            break;
                    }
                }
                else
                {
                    switch (insertMenu.Items.IndexOf(e.ClickedItem))
                    {
                        case 0:
                            tx = "EnsureResolution(" + Screen.PrimaryScreen.Bounds.Width + "," + Screen.PrimaryScreen.Bounds.Height + ");\n";
                            break;
                        case 1:
                            tx = "SpecialKeys(" + (GetKeyState((int)Keys.NumLock) != 0).ToString() + "," + (GetKeyState((int)Keys.CapsLock) != 0).ToString() + "," + (GetKeyState((int)Keys.Scroll) != 0).ToString() + ");\n";
                            break;
                        case 2:
                            tx = "ShowDesktop();\n";
                            break;
                        case 3:
                            tx = "SetMultiplier(2);";
                            break;
                    }
                }
                codeTextBox.Text = codeTextBox.Text.Insert(codeTextBox.SelectionStart, tx);
            }
        }

        private void codeTextBox_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                insertMenu.Tag = ExampleType.Brief;
                insertMenu.Show(codeTextBox, 0, 0);
            }
        }
        #endregion

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            pmHelp form = new pmHelp();
            form.Show();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            dd.Value = DateTime.Now.Day;
            mon.Value = DateTime.Now.Month;
            yy.Value = DateTime.Now.Year;
            hh.Value = DateTime.Now.Hour;
            min.Value = DateTime.Now.Minute;
            dow.Text = DateTime.Now.DayOfWeek.ToString();
        }

        private void mpCb_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (mpCb.SelectedIndex == -1)
                mpCb.SelectedIndex = 0;
        }

        private void expauseBtn_Click(object sender, EventArgs e)
        {
            if (executer.Status == ExecuterSim.SimStatus.Running)
            {
                expauseBtn.Text = "Resume";
                executer.Pause();
                AddLog(DateTime.Now, "Executer paused.");
            }
            else if (executer.Status == ExecuterSim.SimStatus.Paused)
            {
                expauseBtn.Text = "Pause";
                executer.Resume();
                AddLog(DateTime.Now, "Executer resumed.");
            }
        }

        private void rcpauseBtn_Click(object sender, EventArgs e)
        {
            if (recorder.Status == RecorderSim.SimStatus.Running)
            {
                rcpauseBtn.Text = "Resume";
                recorder.Pause();
                AddLog(DateTime.Now, "Recorder paused.");
            }
            else if (recorder.Status == RecorderSim.SimStatus.Paused)
            {
                rcpauseBtn.Text = "Pause";
                recorder.Resume();
                AddLog(DateTime.Now, "Recorder resumed.");
            }
        }
    }

    public class SOptions
    {
        private bool kb;
        private bool mb;
        private bool mm;
        private bool spk;
        private bool sdt;
        private bool rms;
        private int mp;

        public bool KeyboardActive
        {
            get
            {
                return kb;
            }

            set
            {
                kb = value;
            }
        }

        public bool MouseButtonsAactive
        {
            get
            {
                return mb;
            }

            set
            {
                mb = value;
            }
        }

        public bool MouseMovementActive
        {
            get
            {
                return mm;
            }

            set
            {
                mm = value;
            }
        }

        public bool RestoreSpecial
        {
            get
            {
                return spk;
            }

            set
            {
                spk = value;
            }
        }

        public bool ShowDesktop
        {
            get
            {
                return sdt;
            }

            set
            {
                sdt = value;
            }
        }

        public bool RestoreMousePos
        {
            get
            {
                return rms;
            }

            set
            {
                rms = value;
            }
        }

        public int Multiplier
        {
            get
            {
                return mp;
            }

            set
            {
                mp = value;
            }
        }

        public SOptions(bool kbevent, bool mbtnevent, bool mmevent, bool restorespec, int multiplier, bool showdt, bool restoremouse)
        {
            kb = kbevent;
            mb = mbtnevent;
            mm = mmevent;
            spk = restorespec;
            mp = multiplier;
            sdt = showdt;
            rms = restoremouse;
        }
    }
}
