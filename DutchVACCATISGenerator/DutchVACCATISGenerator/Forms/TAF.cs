using System;
using System.ComponentModel;
using System.Windows.Forms;
using DutchVACCATISGenerator.Workers;

namespace DutchVACCATISGenerator.Forms
{
    /// <summary>
    ///     TAF class.
    /// </summary>
    public partial class TAF : Form
    {
        private readonly ITAFWorker _tafWorker;

        /// <summary>
        ///     Constructor of TAF. Initializes new instance of TAF.
        /// </summary>
        /// <param name="icao">Selected ICAO</param>
        public TAF(string icao)
        {
            InitializeComponent();

            _tafWorker = new TAFWorker();

            //Load TAF.
            tafBackgroundWorker.RunWorkerAsync(icao);
        }

        public event EventHandler CloseEvent;

        /// <summary>
        ///     Method to determine if TAF contains AMD.
        /// </summary>
        /// <returns>String indicating TAF AMD to determine</returns>
        private static string DetermineTafamdToLoad(string icao)
        {
            switch (icao)
            {
                case "EHAM":
                    return "TAF AMD EHAM";

                case "EHBK":
                    return "TAF AMD EHBK";

                case "EHEH":
                    return "TAF AMD EHEH";

                case "EHGG":
                    return "TAF AMD EHGG";

                case "EHRD":
                    return "TAF AMD EHRD";
            }

            return string.Empty;
        }

        /// <summary>
        ///     Method to determine if TAF contains COR.
        /// </summary>
        /// <returns>String indicating TAF COR to determine</returns>
        private static string DetermineTafcorToLoad(string icao)
        {
            switch (icao)
            {
                case "EHAM":
                    return "TAF COR EHAM";

                case "EHBK":
                    return "TAF COR EHBK";

                case "EHEH":
                    return "TAF COR EHEH";

                case "EHGG":
                    return "TAF COR EHGG";

                case "EHRD":
                    return "TAF COR EHRD";
            }

            return string.Empty;
        }

        /// <summary>
        ///     Method to determine TAF to load.
        /// </summary>
        /// <returns>String indicating TAF to load</returns>
        private static string DetermineTAFToLoad(string icao)
        {
            switch (icao)
            {
                case "EHAM":
                    return "TAF EHAM";

                case "EHBK":
                    return "TAF EHBK";

                case "EHEH":
                    return "TAF EHEH";

                case "EHGG":
                    return "TAF EHGG";

                case "EHRD":
                    return "TAF EHRD";
            }

            return string.Empty;
        }

        /// <summary>
        ///     Method called if TAF form is closed.
        /// </summary>
        /// <param name="sender">Object sender</param>
        /// <param name="e">Event arguments</param>
        private void TAF_FormClosing(object sender, FormClosingEventArgs e)
        {
            CloseEvent?.Invoke(this, e);
        }

        private void tafBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            _tafWorker.TAFBackgroundWorker_DoWork(sender, e);
        }
        
        private void tafBackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            var result = (Tuple<string, string>) e.Result;

            var icao = result.Item1;
            var taf = result.Item2;

            //Remove/clear old TAF from rich text box.
            TAFRichTextBox.Clear();

            try
            {
                if (taf.Contains(DetermineTafamdToLoad(icao)))
                {
                    //Get TAF part from loaded HTML code.
                    var split =
                    (DetermineTafamdToLoad(icao) +
                     taf.Split(new[] {DetermineTafamdToLoad(icao)}, StringSplitOptions.None)[1]).Split(new[] {"="},
                        StringSplitOptions.None)[0].Split(new[] {"\r\n"}, StringSplitOptions.None);

                    foreach (var s in split)
                        TAFRichTextBox.Text += s.Trim() + "\r\n";
                }
                else if (taf.Contains(DetermineTafcorToLoad(icao)))
                {
                    //Get TAF part from loaded HTML code.
                    var split =
                    (DetermineTafcorToLoad(icao) +
                     taf.Split(new[] {DetermineTafcorToLoad(icao)}, StringSplitOptions.None)[1]).Split(new[] {"="},
                        StringSplitOptions.None)[0].Split(new[] {"\r\n"}, StringSplitOptions.None);

                    foreach (var s in split)
                        TAFRichTextBox.Text += s.Trim() + "\r\n";
                }
                else
                {
                    //Get TAF part from loaded HTML code.
                    var split =
                    (DetermineTAFToLoad(icao) +
                     taf.Split(new[] {DetermineTAFToLoad(icao)}, StringSplitOptions.None)[1]).Split(new[] {"="},
                        StringSplitOptions.None)[0].Split(new[] {"\n"}, StringSplitOptions.None);

                    foreach (var s in split)
                        TAFRichTextBox.Text += s.TrimEnd() + "\r\n";
                }
            }
            catch (Exception)
            {
                //Show error.
                MessageBox.Show("Unable to load TAF from the Internet.", "Error");
            }
        }
    }
}