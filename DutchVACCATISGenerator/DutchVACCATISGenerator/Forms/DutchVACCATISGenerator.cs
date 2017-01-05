using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Media;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using DutchVACCATISGenerator.Extensions;
using DutchVACCATISGenerator.Logic.Metar;
using DutchVACCATISGenerator.Properties;
using DutchVACCATISGenerator.Workers;

namespace DutchVACCATISGenerator.Forms
{
    /// <summary>
    ///     DutchVACCATISGenerator class.
    /// </summary>
    public partial class DutchVACCATISGenerator : Form
    {
        private readonly MetarLogic _metarLogic;
        private readonly IMetarWorker _metarWorker;
        private readonly IRealRunwayWorker _realRunwayWorker;
        private readonly VersionWorker _versionWorker;

        /// <summary>
        ///     Constructor of DutchVACCATISGenerator.
        /// </summary>
        public DutchVACCATISGenerator()
        {
            //Initialize workers.
            _metarLogic = new MetarLogic();
            _metarWorker = new MetarWorker();
            _realRunwayWorker = new RealRunwayWorker();
            _versionWorker = new VersionWorker();

            InitializeComponent();

            //Load settings.
            LoadSettings();

            //Set initial states of boolean.
            SoundState = RunwayInfoState = ICAOTabSwitched = UserLetterSelection = RandomLetter = false;

            //Set phonetic alphabet.
            SetPhoneticAlphabet();

            //Set ATIS index and label.
            RandomizeATISLetter();

            //Start version background worker.
            versionBackgroundWorker.RunWorkerAsync();

            //Load EHAM METAR.
            metarBackgroundWorker.RunWorkerAsync(icaoTextBox.Text);

            //Delete temporary folder if it exists.
            DeleteTemporaryFolder();

            //If auto load EHAM runways is selected.
            if (autoLoadEHAMRunwayToolStripMenuItem.Checked)
                realRunwayBackgroundWorker.RunWorkerAsync();

            //If auto generate ATIS is selected.
            if (autoGenerateATISToolStripMenuItem.Checked)
                autoGenerateATISBackgroundWorker.RunWorkerAsync();

            //Initialize sound form.
            Sound = new Sound(this);
        }

        private int ATISIndex { get; set; }
        private bool ICAOTabSwitched { get; set; }
        private List<string> PhoneticAlphabet { get; set; }
        private bool RandomLetter { get; set; }
        private RunwayInfo RunwayInfo { get; set; }
        private bool RunwayInfoState { get; set; }
        private Sound Sound { get; }
        private bool SoundState { get; set; }
        private TAF TAF { get; set; }
        private DateTime TimerEnabled { get; set; }
        private bool UserLetterSelection { get; set; }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Initialize new About form.
            Form aboutForm = new About();

            //Show about form.
            aboutForm.ShowDialog();
        }

        private void amsterdamInfoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Open Amsterdam Info page of the Dutch VACC site.
            Process.Start("http://www.dutchvacc.nl/index.php?option=com_content&view=article&id=127&Itemid=70");
        }

        private void appArrOnlyCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (appArrOnlyCheckBox.Checked)
                appOnlyCheckBox.Enabled =
                    arrOnlyCheckBox.Enabled =
                        appOnlyCheckBox.Checked =
                            arrOnlyCheckBox.Checked = false;
            else
                appOnlyCheckBox.Enabled = arrOnlyCheckBox.Enabled = true;
        }

        private void appOnlyCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (!arrOnlyCheckBox.Checked)
                return;

            appArrOnlyCheckBox.Checked = true;
            appOnlyCheckBox.Checked = arrOnlyCheckBox.Checked = false;
        }

        private void arrOnlyCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (!appOnlyCheckBox.Checked)
                return;

            appArrOnlyCheckBox.Checked = true;
            appOnlyCheckBox.Checked = arrOnlyCheckBox.Checked = false;
        }

        private void atcOperationalInformationManualToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start(Application.StartupPath + "\\manuals\\Handleiding ATC Operational Information.pdf");
            }
            catch (Exception)
            {
                MessageBox.Show("Unable to open ATC Operational Information manual.", "Error");
            }
        }

        private void autoFetchMETARToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            //Write value to settings.
            Settings.Default.autofetch = autoFetchMETARToolStripMenuItem.Checked;

            //Save setting.
            Settings.Default.Save();

            if (autoFetchMETARToolStripMenuItem.Checked)
            {
                //Set new time to check to
                TimerEnabled = DateTime.UtcNow;

                //Set the fetch METAR label to visible.
                fetchMetarLabel.Visible = true;

                //Start the METAR fetch timer.
                metarFetchTimer.Start();
                metarFetchTimer_Tick(null, null);
            }
            else
            {
                //Set the fetch METAR label to hide.
                fetchMetarLabel.Visible = false;

                //Stop the METAR fetch timer.
                metarFetchTimer.Stop();
            }
        }

        private void autoGenerateATISBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            while (metarBackgroundWorker.IsBusy)
            {
                /* WAIT */
            }

            while (realRunwayBackgroundWorker.IsBusy)
            {
                /* WAIT */
            }
        }

        private void autoGenerateATISBackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                generateATISButton_Click(null, null);
            }
            catch (Exception)
            {
                MessageBox.Show("Unable to auto generate the ATIS.\nGenerate the ATIS manually.", "Error");
            }
        }

        private void autoGenerateATISToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            //Write value to settings.
            Settings.Default.autogenerateatis = autoGenerateATISToolStripMenuItem.Checked;

            //Save setting.
            Settings.Default.Save();
        }

        private void autoLoadEHAMRunwayToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            //Write value to settings.
            Settings.Default.autoloadrunways = autoLoadEHAMRunwayToolStripMenuItem.Checked;

            //Save setting.
            Settings.Default.Save();

            if (autoLoadEHAMRunwayToolStripMenuItem.Checked && autoProcessMETARToolStripMenuItem.Checked)
                autoGenerateATISToolStripMenuItem.Enabled = true;

            else
                autoGenerateATISToolStripMenuItem.Enabled = autoGenerateATISToolStripMenuItem.Checked = false;
        }

        private void autoProcessMETARToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            //Write value to settings.
            Settings.Default.autoprocess = autoProcessMETARToolStripMenuItem.Checked;

            //Save setting.
            Settings.Default.Save();

            if (autoProcessMETARToolStripMenuItem.Checked && autoLoadEHAMRunwayToolStripMenuItem.Checked)
                autoGenerateATISToolStripMenuItem.Enabled = true;

            else
                autoGenerateATISToolStripMenuItem.Enabled = autoGenerateATISToolStripMenuItem.Checked = false;
        }

        /// <summary>
        ///     Checks if correct runways are selected for a regional airport.
        /// </summary>
        /// <param name="checkbox">Checkbox boolean</param>
        /// <param name="selectedIndex">Checkbox selected index</param>
        /// <returns></returns>
        private static bool CheckRegionalRunways(bool checkbox, int selectedIndex)
        {
            //If the runway check box is checked OR the main runway combo box selection is NULL.
            if (checkbox && selectedIndex != -1)
                return true;

            //Show warning message.
            MessageBox.Show("No main runway selected.", "Warning");
            return false;
        }

        /// <summary>
        ///     Checks if the required runway selections are made based on the selected tab.
        /// </summary>
        /// <returns>Boolean indicating if all required check boxes are checked for generating a runway output</returns>
        private bool CheckRunwaySelected()
        {
            switch (ICAOTabControl.SelectedTab.Name)
            {
                //If selected ICAO tab is EHAM.
                case "EHAM":
                    //If EHAM main landing runway check box is checked OR the EHAM main landing runway combo box selection is NULL.
                    if (!EHAMmainLandingRunwayCheckBox.Checked || EHAMmainLandingRunwayComboBox.SelectedIndex == -1)
                    {
                        //Show warning message.
                        MessageBox.Show("No main landing runway selected.", "Warning");
                        return false;
                    }

                    //If EHAM main departure runway check box is checked OR the EHAM main departure runway combo box selection is NULL.
                    if (!EHAMmainDepartureRunwayCheckBox.Checked || EHAMmainDepartureRunwayComboBox.SelectedIndex == -1)
                    {
                        //Show warning message.
                        MessageBox.Show("No main departure runway selected.", "Warning");
                        return false;
                    }

                    //If EHAM secondary landing runway check box is checked OR the EHAM secondary landing runway combo box selection is NULL.
                    if (EHAMsecondaryLandingRunwayCheckBox.Checked &&
                        EHAMsecondaryLandingRunwayComboBox.SelectedIndex == -1)
                    {
                        //Show warning message.
                        MessageBox.Show("Secondary landing runway is enabled but no runway is selected.", "Warning");
                        return false;
                    }

                    //If EHAM secondary departure runway check box is checked OR the EHAM secondary departure runway combo box selection is NULL.
                    if (EHAMsecondaryDepartureRunwayCheckBox.Checked &&
                        EHAMsecondaryDepartureRunwayComboBox.SelectedIndex == -1)
                    {
                        //Show warning message.
                        MessageBox.Show("Secondary departure runway is enabled but no runway is selected.", "Warning");
                        return false;
                    }

                    return true;

                case "EHBK":
                    return CheckRegionalRunways(EHBKmainRunwayCheckBox.Checked, EHBKmainRunwayComboBox.SelectedIndex);

                case "EHEH":
                    return CheckRegionalRunways(EHEHmainRunwayCheckBox.Checked, EHEHmainRunwayComboBox.SelectedIndex);

                case "EHGG":
                    return CheckRegionalRunways(EHGGmainRunwayCheckBox.Checked, EHGGmainRunwayComboBox.SelectedIndex);

                case "EHRD":
                    return CheckRegionalRunways(EHRDmainRunwayCheckBox.Checked, EHRDmainRunwayComboBox.SelectedIndex);
            }

            return false;
        }

        /// <summary>
        ///     Deletes the temporary update folder.
        /// </summary>
        private static void DeleteTemporaryFolder()
        {
            //Check if temp directory exists, if so, delete it.
            if (!Directory.Exists(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\temp"))
                return;

            //Check if setup.exe is still being used.
            if (
                new FileInfo(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\temp" +
                             "\\Dutch VACC ATIS Generator - Setup.exe").IsFileLocked())
                return;

            //Remove read only attribute.
            var directoryInfo =
                Directory.CreateDirectory(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\temp");
            directoryInfo.Attributes &= ~FileAttributes.ReadOnly;

            //Delete temp folder.
            Directory.Delete(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\temp", true);
        }

        private void DutchVACCATISGenerator_Resize(object sender, EventArgs e)
        {
            //If form is restored to normal window state.
            if (WindowState == FormWindowState.Normal)
            {
                //Show runwayInfo form.
                if (RunwayInfo != null && !RunwayInfo.IsDisposed && RunwayInfoState)
                {
                    RunwayInfo.Visible = true;
                    RunwayInfo.BringToFront();
                }
                //Show sound form.
                if (Sound != null && !Sound.IsDisposed && SoundState)
                {
                    Sound.Visible = true;
                    Sound.BringToFront();
                }

                BringToFront();
            }

            //If form is not minimized.
            if (WindowState != FormWindowState.Minimized)
                return;

            //Hide runwayInfo form.
            if (RunwayInfo != null && !RunwayInfo.IsDisposed)
                RunwayInfo.Visible = false;

            //Hide sound form.
            if (Sound != null && !Sound.IsDisposed)
                Sound.Visible = false;
        }

        private void dutchVACCATISGeneratorV2ManualToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start(Application.StartupPath + "\\manuals\\Handleiding Dutch VACC ATIS Generator v2.pdf");
            }
            catch (Exception)
            {
                MessageBox.Show("Unable to open ATC Operational Information manual.", "Error");
            }
        }

        private void dutchVACCToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Open the Dutch VACC site.
            Process.Start("http://www.dutchvacc.nl/");
        }

        private void ehamToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            //Write value to settings.
            Settings.Default.eham = ehamToolStripMenuItem.Checked;

            //Save setting.
            Settings.Default.Save();

            //Set phonetic alphabet.
            SetPhoneticAlphabet();
        }

        private void EHBKmainRunwayCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            EHBKmainRunwayComboBox.Enabled = EHBKmainRunwayCheckBox.Checked;
        }

        private void EHEHmainRunwayCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            EHEHmainRunwayComboBox.Enabled = EHEHmainRunwayCheckBox.Checked;
        }

        private void EHGGmainRunwayCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            EHGGmainRunwayComboBox.Enabled = EHGGmainRunwayCheckBox.Checked;
        }

        private void EHRDmainRunwayCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            EHRDmainRunwayComboBox.Enabled = EHRDmainRunwayCheckBox.Checked;
        }

        private void ehrdToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            //Write value to settings.
            Settings.Default.ehrd = ehrdToolStripMenuItem.Checked;

            //Save setting.
            Settings.Default.Save();

            //Set phonetic alphabet.
            SetPhoneticAlphabet();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Exit application.
            Application.Exit();
        }

        private void generateATISButton_Click(object sender, EventArgs e)
        {
            //If the ICAO code of the processed METAR doesn't equals the ICAO of the selected ICAO tab.
            if (!_metarLogic._metarProcessor.metar.ICAO.Equals(ICAOTabControl.SelectedTab.Name))
            {
                //Show warning message.
                MessageBox.Show("Selected ICAO tab does not match the ICAO of the processed METAR.", "Error");
                return;
            }

            //Check runways selected.
            if (!CheckRunwaySelected())
                return;

            _metarLogic.GenerateAtis();
        }

        /// <summary>
        ///     Retrieves a handle to the foreground window (the window with which the user is currently working).
        /// </summary>
        /// <returns>IntPtr - Handle of foreground window</returns>
        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        private void getMetarButton_Click(object sender, EventArgs e)
        {
            //Set new time to check to.
            TimerEnabled = DateTime.UtcNow;

            //Get ICAO entered.
            var icao = icaoTextBox.Text;

            //If no ICAO has been entered.
            if (icao == string.Empty)
            {
                MessageBox.Show("Enter an ICAO code.", "Warning");
                return;
            }

            //Disable the get METAR button so the user can't overload it.
            getMetarButton.Enabled = false;

            //Start METAR background worker to start pulling the METAR.
            if (!metarBackgroundWorker.IsBusy)
                metarBackgroundWorker.RunWorkerAsync(icao);
        }

        private void getSelectBestRunwayButton_Click(object sender, EventArgs e)
        {
            if (ICAOTabControl.SelectedTab.Name.Equals("EHAM"))
            {
                //Disable get select best runway button;
                getSelectBestRunwayButton.Enabled = false;

                //Start real runway background worker.
                realRunwayBackgroundWorker.RunWorkerAsync();
            }
            else
            {
                //Get best runway for selected airport.
                RunwayInfo.ICAOBestRunway(ICAOTabControl.SelectedTab.Name);

                MessageBox.Show("Controller notice! Verify auto selected runway(s).", "Warning");
            }
        }

        private void ICAOTabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            //Set ICAO tab switched boolean.
            ICAOTabSwitched = true;

            //Set selected ICAO tab as ICAO.
            icaoTextBox.Text = ICAOTabControl.SelectedTab.Name;

            if (ICAOTabControl.SelectedTab.Name.Equals("EHAM"))
            {
                //Set text of get select best runway button.
                getSelectBestRunwayButton.Text = "Get EHAM runway(s)";

                //Enable get select best runway button.
                getSelectBestRunwayButton.Enabled = true;
            }
            else
            {
                //Set text of get select best runway button.
                getSelectBestRunwayButton.Text = "Select best runway";

                //If selected ICAO equals the ICAO of the last processed METAR, enable the get select best runway button.
                if (RunwayInfo != null && ICAOTabControl.SelectedTab.Name.Equals(RunwayInfo.metar.ICAO))
                    getSelectBestRunwayButton.Enabled = true;
                //Else keep disable it.
                else
                    getSelectBestRunwayButton.Enabled = false;
            }

            //Update TAF in TAF form.
            if (TAF != null && !TAF.IsDisposed)
            {
                if (TAF.tafBackgroundWorker.IsBusy)
                    TAF.tafBackgroundWorker.CancelAsync();

                TAF.tafBackgroundWorker.RunWorkerAsync();
            }

            metarBackgroundWorker.RunWorkerAsync(icaoTextBox.Text);

            //Set phonetic alphabet.
            SetPhoneticAlphabet();

            //Set ATIS index and label.
            RandomizeATISLetter();
        }

        private void ICAOTabControl_Selecting(object sender, TabControlCancelEventArgs e)
        {
            if (metarBackgroundWorker.IsBusy)
                e.Cancel = true;
        }

        /// <summary>
        ///     Load application settings from INI file.
        /// </summary>
        private void LoadSettings()
        {
            //Load settings.
            autoFetchMETARToolStripMenuItem.Checked = Settings.Default.autofetch;
            autoProcessMETARToolStripMenuItem.Checked = Settings.Default.autoprocess;
            autoLoadEHAMRunwayToolStripMenuItem.Checked = Settings.Default.autoloadrunways;
            autoGenerateATISToolStripMenuItem.Checked = Settings.Default.autogenerateatis;
            ehamToolStripMenuItem.Checked = Settings.Default.eham;
            ehrdToolStripMenuItem.Checked = Settings.Default.ehrd;
            playSoundWhenMETARIsFetchedToolStripMenuItem.Checked = Settings.Default.playsound;
            randomLetterToolStripMenuItem.Checked = Settings.Default.randomletter;
        }

        private void mainDepartureRunwayCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            EHAMmainDepartureRunwayComboBox.Enabled = EHAMmainDepartureRunwayCheckBox.Checked;
        }

        private void mainLandingRunwayCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            EHAMmainLandingRunwayComboBox.Enabled = EHAMmainLandingRunwayCheckBox.Checked;
        }

        private void metarBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            _metarWorker.MetarBackgroundWorker_DoWork(sender, e);
        }

        private void metarBackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            //Get result.
            var metar = (string) e.Result;

            //Set pulled METAR in the METAR text box.
            metarTextBox.Text = metar;

            //If auto process METAR check box is checked, automatically process the METAR.
            if (autoProcessMETARToolStripMenuItem.Checked && metarTextBox.Text != null)
                processMetarButton_Click(null, null);

            //Re-enable the METAR button.
            getMetarButton.Enabled = true;
        }

        private void metarFetchTimer_Tick(object sender, EventArgs e)
        {
            //Update fetch METAR label.
            fetchMetarLabel.Text = "Fetching METAR in: " + (30 - (DateTime.UtcNow - TimerEnabled).Minutes) + " minutes.";

            //If 30 minutes have passed, update the METAR.
            if ((DateTime.UtcNow - TimerEnabled).Minutes <= 29)
                return;

            //Update METAR.
            getMetarButton_Click(null, null);

            //Flash task bar.
            if (Handle != GetForegroundWindow())
                try
                {
                    FlashingWindow.FlashWindowEx(this);
                }
                catch (Exception)
                {
                    //Do nothing... Can't flash the window. To bad... =(
                }

            //Play notification sound.
            if (!playSoundWhenMETARIsFetchedToolStripMenuItem.Checked)
                return;

            try
            {
                using (
                    var player =
                        new SoundPlayer(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) +
                                        "\\sounds\\alert.wav"))
                {
                    player.Play();
                }
            }
            catch (Exception)
            {
                //Meh... Can't play sound for some reason...
            }
        }

        private void metarTextBox_TextChanged(object sender, EventArgs e)
        {
            //If METAR text box contains text, enable process METAR button.
            processMetarButton.Enabled = !metarTextBox.Text.Trim().Equals(string.Empty);
        }

        private void nextATISLetterButton_Click(object sender, EventArgs e)
        {
            //Set user letter selection boolean true.
            UserLetterSelection = true;

            if (ATISIndex == PhoneticAlphabet.Count - 1)
                ATISIndex = 0;
            else
                ATISIndex++;

            //Set ATIS letter in ATIS letter label.
            atisLetterLabel.Text = PhoneticAlphabet[ATISIndex];
        }

        private void playSoundWhenMETARIsFetchedToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            //Write value to settings.
            Settings.Default.playsound = playSoundWhenMETARIsFetchedToolStripMenuItem.Checked;

            //Save setting.
            Settings.Default.Save();
        }

        private void previousATISLetterButton_Click(object sender, EventArgs e)
        {
            //Set user letter selection boolean true.
            UserLetterSelection = true;

            if (ATISIndex == 0)
                ATISIndex = PhoneticAlphabet.Count - 1;
            else
                ATISIndex--;

            //Set ATIS letter in ATIS letter label.
            atisLetterLabel.Text = PhoneticAlphabet[ATISIndex];
        }

        private void processMetarButton_Click(object sender, EventArgs e)
        {
            //Check if a METAR has been entered.
            if (metarTextBox.Text.Trim().Equals(string.Empty))
            {
                MessageBox.Show("No METAR fetched or entered.", "Error");
                return;
            }

            //Check if entered METAR ICAO matches the selected ICAO tab.
            if (!metarTextBox.Text.Trim().StartsWith(ICAOTabControl.SelectedTab.Name))
            {
                MessageBox.Show("Selected ICAO tab does not match the ICAO of the entered METAR.", "Warning");
                return;
            }

            //Process METAR.
            _metarLogic.ProcessMetar(metarTextBox.Text.Trim());

            //Calculate the transition level.
            try
            {
                tlOutLabel.Text = _metarLogic.CalculateTransitionLevel().ToString();
            }
            catch (Exception)
            {
                MessageBox.Show("Error parsing the METAR, check if METAR is in correct format.", "Error");
                return;
            }

            //Clear output and METAR text box.
            outputTextBox.Clear();
            metarTextBox.Clear();

            //Checks if ATIS index has to be increased.
            if (!(UserLetterSelection | RandomLetter | ICAOTabSwitched | (lastLabel.Text == string.Empty)))
                if (ATISIndex == PhoneticAlphabet.Count - 1)
                    ATISIndex = 0;
                else
                    ATISIndex++;

            //Set all letter booleans to false for next generation.
            RandomLetter = UserLetterSelection = ICAOTabSwitched = false;

            //Set processed METAR in last processed METAR label.
            if (metarTextBox.Text.Trim().Length > 140)
                lastLabel.Text = "Last successful processed METAR:\n" + metarTextBox.Text.Trim().Substring(0, 69).Trim() +
                                 "\n" + metarTextBox.Text.Trim().Substring(69, 69).Trim() + "...";
            else if (metarTextBox.Text.Trim().Length > 69)
                lastLabel.Text = "Last successful processed METAR:\n" + metarTextBox.Text.Trim().Substring(0, 69).Trim() +
                                 "\n" + metarTextBox.Text.Trim().Substring(69).Trim();
            else
                lastLabel.Text = "Last successful processed METAR:\n" + metarTextBox.Text.Trim();

            //Set ATIS letter in ATIS letter label.
            atisLetterLabel.Text = PhoneticAlphabet[ATISIndex];

            //Enable generate ATIS and runway info button.
            generateATISButton.Enabled = true;
            runwayInfoButton.Enabled = runwayInfoToolStripMenuItem.Enabled = true;

            //If runwayInfo is null, create RunwayInfo form.
            if (RunwayInfo != null && RunwayInfo.IsDisposed || RunwayInfo == null)
            {
                RunwayInfo = new RunwayInfo(this, _metarLogic._metarProcessor.metar);
            }
            else
            {
                //Update runway info form.
                RunwayInfo.metar = _metarLogic._metarProcessor.metar;
                RunwayInfo.setVisibleRunwayInfoDataGrid(ICAOTabControl.SelectedTab.Text);
                RunwayInfo.checkICAOTabSelected();
            }

            //If processed METAR equals the selected ICAO.
            if (RunwayInfo.metar.ICAO.Equals(ICAOTabControl.SelectedTab.Name))
                getSelectBestRunwayButton.Enabled = true;
        }

        /// <summary>
        ///     Process if multiple runways we're found and set the runway selection boxes with the founded values.
        /// </summary>
        private void ProcessMultipleRunways(IReadOnlyCollection<string> deparuteRunways, IReadOnlyCollection<string> landingRunways)
        {
            #region LANDING RUNWAYS

            //If there are more than two landing runways.
            if (landingRunways.Count > 1)
            {
                var firstRunway = landingRunways.First();
                var secondRunway = landingRunways.Last();

                //Generate landing runway combinations list.
                var landingRunwayCombinations = new List<Tuple<string, string>>
                {
                    /* 06 combinations */
                    new Tuple<string, string>("06", "18R"),
                    new Tuple<string, string>("06", "36R"),
                    new Tuple<string, string>("06", "18C"),
                    new Tuple<string, string>("06", "36C"),
                    new Tuple<string, string>("06", "27"),
                    new Tuple<string, string>("06", "22"),
                    new Tuple<string, string>("06", "09"),
                    new Tuple<string, string>("06", "04"),
                    /* 18R combinations */
                    new Tuple<string, string>("18R", "36R"),
                    new Tuple<string, string>("18R", "18C"),
                    new Tuple<string, string>("18R", "36C"),
                    new Tuple<string, string>("18R", "27"),
                    new Tuple<string, string>("18R", "22"),
                    new Tuple<string, string>("18R", "24"),
                    new Tuple<string, string>("18R", "09"),
                    new Tuple<string, string>("18R", "04"),
                    /* 36R combinations */
                    new Tuple<string, string>("36R", "18C"),
                    new Tuple<string, string>("36R", "36C"),
                    new Tuple<string, string>("36R", "27"),
                    new Tuple<string, string>("36R", "22"),
                    new Tuple<string, string>("36R", "24"),
                    new Tuple<string, string>("36R", "09"),
                    new Tuple<string, string>("36R", "04"),
                    /* 18C combinations */
                    new Tuple<string, string>("18C", "27"),
                    new Tuple<string, string>("18C", "22"),
                    new Tuple<string, string>("18C", "24"),
                    new Tuple<string, string>("18C", "09"),
                    new Tuple<string, string>("18C", "04"),
                    /* 36C combinations */
                    new Tuple<string, string>("36C", "27"),
                    new Tuple<string, string>("36C", "22"),
                    new Tuple<string, string>("36C", "24"),
                    new Tuple<string, string>("36C", "09"),
                    new Tuple<string, string>("36C", "04"),
                    /* 27 combinations */
                    new Tuple<string, string>("27", "22"),
                    new Tuple<string, string>("27", "24"),
                    new Tuple<string, string>("27", "09"),
                    new Tuple<string, string>("27", "04"),
                    /* 22 combinations */
                    new Tuple<string, string>("22", "24"),
                    new Tuple<string, string>("22", "09"),
                    /* 24 combinations */
                    new Tuple<string, string>("24", "09"),
                    new Tuple<string, string>("24", "04"),
                    /* 09 combinations */
                    new Tuple<string, string>("09", "04")
                };

                //Check which runways are found and set the correct main and secondary landing runway.
                foreach (var runwayCombination in landingRunwayCombinations)
                {
                    if ((!firstRunway.Equals(runwayCombination.Item1) || !secondRunway.Equals(runwayCombination.Item2)) &&
                        (!firstRunway.Equals(runwayCombination.Item2) || !secondRunway.Equals(runwayCombination.Item1)))
                        continue;

                    EHAMmainLandingRunwayComboBox.Text = runwayCombination.Item1;
                    EHAMsecondaryLandingRunwayComboBox.Text = runwayCombination.Item2;
                }
            }

            #endregion

            #region DEPARTURE RUNWAYS

            //If there are more than two departure runways found.
            if (deparuteRunways.Count > 1)
            {
                var firstRunway = deparuteRunways.First();
                var secondRunway = deparuteRunways.Last();

                //Generate departure runway combinations list.
                var departureRunwayCombinations = new List<Tuple<string, string>>
                {
                    /* 36L combinations */
                    new Tuple<string, string>("36L", "24"),
                    new Tuple<string, string>("36L", "36C"),
                    new Tuple<string, string>("36L", "18L"),
                    new Tuple<string, string>("36L", "18C"),
                    new Tuple<string, string>("36L", "09"),
                    new Tuple<string, string>("36L", "27"),
                    new Tuple<string, string>("36L", "06"),
                    new Tuple<string, string>("36L", "22"),
                    new Tuple<string, string>("36L", "04"),
                    /* 24 combinations */
                    new Tuple<string, string>("24", "36C"),
                    new Tuple<string, string>("24", "18L"),
                    new Tuple<string, string>("24", "18C"),
                    new Tuple<string, string>("24", "09"),
                    new Tuple<string, string>("24", "27"),
                    new Tuple<string, string>("24", "22"),
                    new Tuple<string, string>("24", "04"),
                    /* 36C combinations */
                    new Tuple<string, string>("36C", "18L"),
                    new Tuple<string, string>("36C", "09"),
                    new Tuple<string, string>("36C", "27"),
                    new Tuple<string, string>("36C", "06"),
                    new Tuple<string, string>("36C", "22"),
                    new Tuple<string, string>("36C", "04"),
                    /* 18L combinations */
                    new Tuple<string, string>("18L", "18C"),
                    new Tuple<string, string>("18L", "09"),
                    new Tuple<string, string>("18L", "27"),
                    new Tuple<string, string>("18L", "06"),
                    new Tuple<string, string>("18L", "22"),
                    new Tuple<string, string>("18L", "04"),
                    /* 18C combinations */
                    new Tuple<string, string>("18C", "09"),
                    new Tuple<string, string>("18C", "27"),
                    new Tuple<string, string>("18C", "06"),
                    new Tuple<string, string>("18C", "22"),
                    new Tuple<string, string>("18C", "04"),
                    /* 09 combinations */
                    new Tuple<string, string>("09", "27"),
                    new Tuple<string, string>("09", "06"),
                    new Tuple<string, string>("09", "22"),
                    new Tuple<string, string>("09", "04"),
                    /* 27 combinations */
                    new Tuple<string, string>("27", "06"),
                    new Tuple<string, string>("27", "22"),
                    new Tuple<string, string>("27", "04"),
                    /* 06 combinations */
                    new Tuple<string, string>("06", "22"),
                    new Tuple<string, string>("06", "04"),
                    /* 22 combinations */
                    new Tuple<string, string>("22", "04")
                };

                //Check which runways are found and set the correct main and secondary departure runway.
                foreach (var runwayCombination in departureRunwayCombinations)
                    if (firstRunway.Equals(runwayCombination.Item1) && secondRunway.Equals(runwayCombination.Item2) ||
                        firstRunway.Equals(runwayCombination.Item2) && secondRunway.Equals(runwayCombination.Item1))
                    {
                        EHAMmainDepartureRunwayComboBox.Text = runwayCombination.Item1;
                        EHAMsecondaryDepartureRunwayComboBox.Text = runwayCombination.Item2;
                    }
            }

            #endregion
        }

        /// <summary>
        ///     Randomizes ATIS letter.
        /// </summary>
        private void RandomizeATISLetter()
        {
            //Random ATIS letter.
            if (randomLetterToolStripMenuItem.Checked)
            {
                RandomLetter = true;

                var random = new Random();
                ATISIndex = random.Next(0, PhoneticAlphabet.Count - 1);
            }
            //Set ATIS index to Z for first generation.
            else
            {
                ATISIndex = 0;
            }

            //Set ATIS label.
            atisLetterLabel.Text = PhoneticAlphabet[ATISIndex];
        }

        private void randomLetterToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            //Write value to settings.
            Settings.Default.randomletter = randomLetterToolStripMenuItem.Checked;

            //Save setting.
            Settings.Default.Save();
        }

        private void realRunwayBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            _realRunwayWorker.RealRunwayBackgroundWorker_DoWork(sender, e);
        }

        private void realRunwayBackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            var result = (Tuple<List<string>, List<string>>) e.Result;

            var deparuteRunways = result.Item1;
            var landingRunways = result.Item1;

            //Clear runway combo boxes.
            EHAMmainDepartureRunwayComboBox.SelectedIndex =
                EHAMmainLandingRunwayComboBox.SelectedIndex =
                    EHAMsecondaryDepartureRunwayComboBox.SelectedIndex =
                        EHAMsecondaryLandingRunwayComboBox.SelectedIndex = -1;

            //Only one departure runway found.
            if (deparuteRunways.Count == 1)
                EHAMmainDepartureRunwayComboBox.Text = deparuteRunways.First();

            //Only one landing runway found.
            if (landingRunways.Count == 1)
                EHAMmainLandingRunwayComboBox.Text = landingRunways.First();

            //Two or more landing or departure runways found.
            if (landingRunways.Count > 1 || deparuteRunways.Count > 1)
                ProcessMultipleRunways(deparuteRunways, landingRunways);

            //Re-enable get select best runway button.
            getSelectBestRunwayButton.Enabled = true;

            if (landingRunways.Any() || deparuteRunways.Count > 0)
                MessageBox.Show("Controller notice! Verify auto selected runway(s).", "Warning");
        }

        private void runwayInfoButton_Click(object sender, EventArgs e)
        {
            SetRunwayInfoForm();
        }

        private void runwayInfoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetRunwayInfoForm();
        }

        private void secondaryDepartureRunwayCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            EHAMsecondaryDepartureRunwayComboBox.Enabled = EHAMsecondaryDepartureRunwayCheckBox.Checked;
        }

        private void secondaryLandingRunway_CheckedChanged(object sender, EventArgs e)
        {
            EHAMsecondaryLandingRunwayComboBox.Enabled = EHAMsecondaryLandingRunwayCheckBox.Checked;
        }

        /// <summary>
        ///     Checks what phonetic alphabet to be set for ATIS generation.
        /// </summary>
        private void SetPhoneticAlphabet()
        {
            //If selected tab is EHAM and EHAM (A - M) tool strip menu item is checked.
            if (ICAOTabControl.SelectedTab.Name.Equals("EHAM") && ehamToolStripMenuItem.Checked)
                PhoneticAlphabet = new List<string> {"A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M"};

            //If selected tab is EHRD and EHRD (N - Z) tool strip menu item is checked.
            else if (ICAOTabControl.SelectedTab.Name.Equals("EHRD") && ehrdToolStripMenuItem.Checked)
                PhoneticAlphabet = new List<string> {"N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z"};

            //Else set full phonetic alpha bet.
            else
                PhoneticAlphabet = new List<string>
                {
                    "A",
                    "B",
                    "C",
                    "D",
                    "E",
                    "F",
                    "G",
                    "H",
                    "I",
                    "J",
                    "K",
                    "L",
                    "M",
                    "N",
                    "O",
                    "P",
                    "Q",
                    "R",
                    "S",
                    "T",
                    "U",
                    "V",
                    "W",
                    "X",
                    "Y",
                    "Z"
                };

            //If the current index is higher than the phonetic alphabet count (will cause exception!).
            if (ATISIndex > PhoneticAlphabet.Count)
                ATISIndex = PhoneticAlphabet.Count - 1;
        }

        /// <summary>
        ///     Sets all controls for opening and closing the runway info form.
        /// </summary>
        private void SetRunwayInfoForm()
        {
            #region OPENING

            //If runway info form doesn't exists OR isn't visible.
            if (RunwayInfo == null || !RunwayInfo.Visible)
            {
                RunwayInfo = new RunwayInfo(this, _metarLogic.Metar);

                //Initialize new RunwayInfo form.
                runwayInfoButton.Text = "<";

                //Show runway info form.
                RunwayInfo.Show();

                //Set runway info position relative to this.
                RunwayInfo.showRelativeToDutchVACCATISGenerator(this);

                //Inverse runway info state boolean.
                RunwayInfoState = !RunwayInfoState;

                //Set runway info tool strip menu item back color to gradient active caption.
                runwayInfoToolStripMenuItem.BackColor = SystemColors.GradientActiveCaption;
            }
            #endregion

            #region CLOSING

            //If runway info is opened.
            else
            {
                runwayInfoButton.Text = ">";

                //Hide runway info form.
                RunwayInfo.Visible = false;

                //Inverse runway info state boolean.
                RunwayInfoState = !RunwayInfoState;

                //Set runway info tool strip menu item back color to control.
                runwayInfoToolStripMenuItem.BackColor = SystemColors.Control;
            }

            #endregion
        }

        /// <summary>
        ///     Sets all controls for opening and closing the sound form.
        /// </summary>
        private void SetSoundForm()
        {
            //If sound form doesn't exists OR isn't visible.
            if (Sound == null || !Sound.Visible)
            {
                //Create new Sound form.
                soundButton.Text = "▲";
                Sound?.Show();
                Sound?.showRelativeToDutchVACCATISGenerator(this);

                //Inverse sound state boolean.
                SoundState = !SoundState;

                //Set runway sound tool strip menu item back color to gradient active caption.
                soundToolStripMenuItem.BackColor = SystemColors.GradientActiveCaption;
            }
            else
            {
                soundButton.Text = "▼";

                //Hide the sound form.
                Sound.Visible = false;

                //Stop the wavePlayer.
                Sound.wavePlayer?.Stop();

                //Inverse sound state boolean.
                SoundState = !SoundState;

                //Set sound tool strip menu item back color to control.
                soundToolStripMenuItem.BackColor = SystemColors.Control;
            }
        }

        /// <summary>
        ///     Sets all controls for opening and closing the TAF form.
        /// </summary>
        private void SetTAFForm()
        {
            //If TAF form doesn't exists OR isn't visible.
            if (TAF == null || !TAF.Visible)
            {
                //Create new Sound form.
                TAF = new TAF(this);
                TAF.Show();

                //Set TAF tool strip menu item back color to gradient active caption.
                tAFToolStripMenuItem.BackColor = SystemColors.GradientActiveCaption;
            }
            else
            {
                //Hide the TAF form.
                TAF.Visible = false;

                //Set TAF strip menu item back color to control.
                tAFToolStripMenuItem.BackColor = SystemColors.Control;
            }
        }

        private void soundButton_Click(object sender, EventArgs e)
        {
            SetSoundForm();
        }

        private void soundToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetSoundForm();
        }

        private void tAFToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetTAFForm();
        }

        private void versionBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            _versionWorker.VersionBackgroundWorker_DoWork(sender, e);
        }

        private void versionBackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            _versionWorker.VersionBackgroundWorker_RunWorkerCompleted(sender, e);
        }
    }
}