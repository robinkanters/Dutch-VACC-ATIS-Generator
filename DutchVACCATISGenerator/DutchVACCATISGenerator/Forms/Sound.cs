using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using DutchVACCATISGenerator.Extensions;
using DutchVACCATISGenerator.Logic;
using DutchVACCATISGenerator.Properties;
using NAudio.Wave;

namespace DutchVACCATISGenerator.Forms
{
    /// <summary>
    ///     Sound class.
    /// </summary>
    public partial class Sound : Form
    {
        private readonly int _bottom;
        private readonly int _left;
        private AudioFileReader _audio;
        private readonly MetarLogic _metarLogic;

        /// <summary>
        ///     Constructor of Sound. Initializes new instance of Sound.
        /// </summary>
        public Sound(bool enableBuild, int left, int bottom, MetarLogic metarLogic)
        {
            InitializeComponent();

            //Enable the build ATIS button if the ATIS has already been build.
            buildATISButton.Enabled = enableBuild;

            //Set form position relative to Dutch VACC ATIS Generator form.
            _left = left;
            _bottom = bottom;

            //Set MetarLogic
            _metarLogic = metarLogic;

            //Get and set the property of the path to the ATIS folder if it has been saved before.
            if (!Settings.Default.atisehamPath.Equals(string.Empty))
                atisehamFileTextBox.Text = Settings.Default.atisehamPath;
            //Else sets the path to the user document folder + \EuroScope\atis\atiseham.txt.
            else
                atisehamFileTextBox.Text = Environment.GetFolderPath(Environment.SpecialFolder.Personal) +
                                           @"\EuroScope\atis\atiseham.txt";
        }

        private IWavePlayer _wavePlayer { get; set; }

        /// <summary>
        ///     Method called when browse button is clicked.
        /// </summary>
        /// <param name="sender">Object sender</param>
        /// <param name="e">Event arguments</param>
        private void browseButton_Click(object sender, EventArgs e)
        {
            //If user has selected a file.
            if (openFileDialog.ShowDialog() != DialogResult.OK)
                return;

            atisehamFileTextBox.Text = openFileDialog.FileName;

            //Save selected path to settings.
            Settings.Default.atisehamPath = atisehamFileTextBox.Text;

            //Save setting.
            Settings.Default.Save();
        }

        /// <summary>
        ///     Build atis.wav file.
        /// </summary>
        /// <param name="atisSamples">List<String> ATIS samples to build atis.wav from</String></param>
        public void BuildAtis(List<string> atisSamples)
        {
            //Try to read atiseham.txt and start the build ATIS background worker.
            try
            {
                string line;

                //If path to atiseham.txt is not set.
                if (atisehamFileTextBox.Text.Trim().Equals(string.Empty))
                {
                    MessageBox.Show("No path to atiseham.txt provided.", "Error");

                    //Force user to select atiseham.txt.
                    browseButton.PerformClick();
                }

                //Try to open and read the atisamples.txt file.
                try
                {
                    using (
                        var sr =
                            new StreamReader(Path.GetDirectoryName(atisehamFileTextBox.Text) +
                                             "\\samples\\ehamsamples.txt"))
                    {
                        line = sr.ReadToEnd();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Unable to open atiseham.txt. Check if the correct atiseham.txt file is selected.\n\nError: {ex.Message}", "Error");

                    //Re-enable the play ATIS button if reading of the atiseham.text has failed.
                    playATISButton.Enabled = true;
                    return;
                }

                //Split read atiseham.txt file on end of line.
                var fileLines = line.Split(new[] {"\r\n", "\n"}, StringSplitOptions.None);

                //Initialize new List of Strings with the split array.
                var linesWithItem = new List<string>(fileLines);

                //Remove any empty entries at the end of the linesWithItem list.
                while (linesWithItem.Last().Equals(string.Empty)) linesWithItem.RemoveAt(linesWithItem.Count() - 1);

                //Create new dictionary to store .wav files value in. I.E.: a = ehamatis1_a.wav
                var records = new Dictionary<string, string>();

                //Add linesWithItem items to the records dictionary.
                foreach (var s in linesWithItem)
                {
                    if (!s.StartsWith("RECORD"))
                        continue;

                    var split = Regex.Split(s, @":");

                    records.Add(split[1], split[2]);
                }

                //Start build ATIS backgroundWorker to start building the atis.wav file.
                buildATISbackgroundWorker.RunWorkerAsync(new object[] {atisSamples, records});
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unable to build ATIS. Check if the right atiseham.txt is selected.\n\nError: {ex.Message}", "Error");
            }
        }

        /// <summary>
        ///     Method called when build ATIS background worker is started.
        /// </summary>
        /// <param name="sender">Object sender</param>
        /// <param name="e">Event arguments</param>
        private void buildATISbackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            var buffer = new byte[1024];

            WaveFileWriter waveFileWriter = null;

            //If file is in use by another process.
            if (new FileInfo(Path.GetDirectoryName(atisehamFileTextBox.Text) + "\\atis.wav").IsFileLocked())
            {
                MessageBox.Show("Cannot generate new atis.wav file. File does not exist or is in use by another process.", "Error");
                return;
            }

            //Try to generate and build atis.wav.
            try
            {
                var i = 0;

                //For each String in textToPlay list.
                foreach (var sourceFile in (e.Argument as object[])[0] as List<string>)
                    try
                    {
                        //Using the WaveFileReader, get the file to write to the atis.wave from the records list.
                        using (
                            var reader =
                                new WaveFileReader(Path.GetDirectoryName(atisehamFileTextBox.Text) + "\\samples\\" +
                                                   ((e.Argument as object[])[1] as Dictionary<string, string>)[
                                                       sourceFile]))
                        {
                            //Initialize new WaveFileWriter.
                            if (waveFileWriter == null)
                            {
                                waveFileWriter =
                                    new WaveFileWriter(Path.GetDirectoryName(atisehamFileTextBox.Text) + "\\atis.wav",
                                        reader.WaveFormat);
                            }

                            else
                            {
                                //If loaded .wav does not watch the format of the atis.wav output file.
                                if (!reader.WaveFormat.Equals(waveFileWriter.WaveFormat))
                                    throw new InvalidOperationException(
                                        "Can't concatenate .wav files that don't share the same format");
                            }

                            int read;

                            //Write loaded .wav file to atis.wav.
                            while ((read = reader.Read(buffer, 0, buffer.Length)) > 0)
                                waveFileWriter.Write(buffer, 0, read);
                        }

                        //Update progress bar by sending an report progress call to the build ATIS background worker.
                        var percentage = (i + 1) * 100 / ((e.Argument as object[])[0] as List<string>).Count();
                        buildATISbackgroundWorker.ReportProgress(percentage);
                        i++;
                    }
                    catch (KeyNotFoundException)
                    {
                        //Update progress bar by sending an report progress call to the build ATIS background worker.
                        var percentage = (i + 1) * 100 / ((e.Argument as object[])[0] as List<string>).Count();
                        buildATISbackgroundWorker.ReportProgress(percentage);
                        i++;
                    }
                    catch (InvalidOperationException)
                    {
                        //Update progress bar by sending an report progress call to the build ATIS background worker.
                        var percentage = (i + 1) * 100 / ((e.Argument as object[])[0] as List<string>).Count();
                        buildATISbackgroundWorker.ReportProgress(percentage);
                        i++;
                    }
                    catch (FileNotFoundException ex)
                    {
                        MessageBox.Show(ex.Message + "\n\natis.wav will be generated without this file.", "Error");

                        //Update progress bar by sending an report progress call to the build ATIS background worker.
                        var percentage = (i + 1) * 100 / ((e.Argument as object[])[0] as List<string>).Count();
                        buildATISbackgroundWorker.ReportProgress(percentage);
                        i++;
                    }
            }
            //Dispose waveFileWriter when finished.
            finally
            {
                if (waveFileWriter != null) waveFileWriter.Dispose();
            }
        }

        /// <summary>
        ///     Method called when build ATIS background worker posts a progress update.
        /// </summary>
        /// <param name="sender">Object sender</param>
        /// <param name="e">Event arguments</param>
        private void buildATISbackgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar.Value = e.ProgressPercentage;
        }

        /// <summary>
        ///     Method called when build ATIS background workers has completed its work.
        /// </summary>
        /// <param name="sender">Object sender</param>
        /// <param name="e">Event arguments</param>
        private void buildATISbackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            buildATISButton.Enabled = true;
            playATISButton.Enabled = true;
        }

        /// <summary>
        ///     Method called when build ATIS button is clicked.
        /// </summary>
        /// <param name="sender">Object sender</param>
        /// <param name="e">Event arguments</param>
        private void buildATISButton_Click(object sender, EventArgs e)
        {
            buildATISButton.Enabled = false;

            //Build ATIS.
            if(_metarLogic.ATISSamples != null && _metarLogic.ATISSamples.Count != 0)
                BuildAtis(_metarLogic.ATISSamples);
        }

        /// <summary>
        ///     Method called when play ATIS button is clicked.
        /// </summary>
        /// <param name="sender">Object sender</param>
        /// <param name="e">Event arguments</param>
        private void playATISButton_Click(object sender, EventArgs e)
        {
            //If ATIS is not playing.
            if (playATISButton.Text.Equals("Play ATIS"))
            {
                //If path to atiseham.txt is not set.
                if (atisehamFileTextBox.Text.Trim().Equals(string.Empty))
                {
                    MessageBox.Show("No path to atiseham.txt provided.", "Warning");

                    //Open file dialog for user to set the path to atiseham.txt.
                    if (openFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        atisehamFileTextBox.Text = openFileDialog.FileName;

                        //Write value to settings.
                        Settings.Default.atisehamPath = atisehamFileTextBox.Text;

                        //Save setting.
                        Settings.Default.Save();
                    }
                    //If user didn't selected a file
                    else
                    {
                        return;
                    }
                }

                //Change the text of play ATIS button to...
                playATISButton.Text = "Stop ATIS";

                //Try to read atis.wav file.
                try
                {
                    _audio = new AudioFileReader(Path.GetDirectoryName(atisehamFileTextBox.Text) + "\\atis.wav");
                }
                catch (FileNotFoundException ex)
                {
                    MessageBox.Show(
                        string.Format(
                            "Unable to play ATIS. Check if the atiseham.txt file is in the same folder as the ATIS sounds (atis.wav, etc.).\n\nError: {0}",
                            ex.Message), "Error");
                    return;
                }

                //Initialize wavePlayer.
                _wavePlayer = new WaveOut(WaveCallbackInfo.FunctionCallback());
                _wavePlayer.Init(_audio);
                _wavePlayer.PlaybackStopped += wavePlayer_PlaybackStopped;

                //Disable build ATIS button, prevents user from trying to build and play atis.wav at the same time.
                buildATISButton.Enabled = false;

                //Play the atis.wav file.
                _wavePlayer.Play();
            }

            //If ATIS is playing, stop playing.
            else
            {
                _wavePlayer.Stop();
            }
        }

        /// <summary>
        ///     Sets the RunwayInform form position relative to DutchVACCATISGenerator form.
        /// </summary>
        /// <param name="left">Left position of Dutch VACC ATIS Generator form</param>
        /// <param name="bottom">Right position of Dutch VACC ATIS Generator form</param>
        public void ShowRelativeToDutchVACCATISGenerator(int left, int bottom)
        {
            Left = left;
            Top = bottom;
            Refresh();
        }

        /// <summary>
        ///     Method called if Sound form is closed.
        /// </summary>
        /// <param name="sender">Object sender</param>
        /// <param name="e">Event arguments</param>
        private void Sound_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
        }

        /// <summary>
        ///     Method called when Sound form is loaded.
        /// </summary>
        /// <param name="sender">Object sender</param>
        /// <param name="e">Event arguments</param>
        private void Sound_Load(object sender, EventArgs e)
        {
            ShowRelativeToDutchVACCATISGenerator(_left, _bottom);
        }

        /// <summary>
        ///     Stops the ATIS from playing.
        /// </summary>
        public void StopPlaying()
        {
            _wavePlayer?.Stop();
        }

        /// <summary>
        ///     Method called when wave player has finish playing a sound, or when it is stopped.
        /// </summary>
        /// <param name="sender">Object sender</param>
        /// <param name="e">Event arguments</param>
        private void wavePlayer_PlaybackStopped(object sender, StoppedEventArgs e)
        {
            //Set play ATIS button text to...
            playATISButton.Text = "Play ATIS";

            //Re-enable the build ATIS button.
            if (_metarLogic.ATISSamples != null && _metarLogic.ATISSamples.Count != 0)
                BuildAtis(_metarLogic.ATISSamples);

            //Dispose the AudioFileReader to release the file.
            try
            {
                _audio.Dispose();
            }
            catch (Exception)
            {
            }

            //Dispose the wavePlayer.
            _wavePlayer.Dispose();
        }
    }
}