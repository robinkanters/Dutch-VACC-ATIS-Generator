using System.ComponentModel;
using System.IO;
using System.Net;
using System.Windows.Forms;

namespace DutchVACCATISGenerator.Workers
{
    public class MetarWorker
    {
        private readonly Forms.DutchVACCATISGenerator _dutchVACCATISGenerator;

        public MetarWorker(Forms.DutchVACCATISGenerator dutchVACCATISGenerator)
        {
            _dutchVACCATISGenerator = dutchVACCATISGenerator;
        }

        /// <summary>
        /// Method called when METAR background workers is started. Pulls METAR from VATSIM METAR website.
        /// </summary>
        /// <param name="sender">Object sender</param>
        /// <param name="e">Event arguments</param>
        public void MetarBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            //Try to pull the METAR from http://metar.vatsim.net/metar.php.
            try
            {
                //Get METAR.
                var request = WebRequest.Create("http://metar.vatsim.net/metar.php?id=" + e.Argument);
                var response = request.GetResponse();
                var reader = new StreamReader(response.GetResponseStream());
                _dutchVACCATISGenerator.Metar = reader.ReadToEnd();

                //Remove spaces.
                if (_dutchVACCATISGenerator.Metar.StartsWith(e.Argument.ToString()))
                    _dutchVACCATISGenerator.Metar = _dutchVACCATISGenerator.Metar.Trim();
            }
            catch (WebException)
            {
                MessageBox.Show("Unable to fetch the METAR from the Internet.\nProvide a METAR manually.", "Error");
            }
        }

        /// <summary>
        /// Method called when METAR background worker has completed its task. Sets pulled METAR from VATSIM METAR website into the MetarTextBox.
        /// </summary>
        /// <param name="sender">Object sender</param>
        /// <param name="e">Event arguments</param>
        public void MetarBackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            //Set pulled METAR in the METAR text box.
            _dutchVACCATISGenerator.MetarTextBox.Text = _dutchVACCATISGenerator.Metar;

            //If auto process METAR check box is checked, automatically process the METAR.
            if (_dutchVACCATISGenerator.AutoProcessMETARToolStripMenuItem.Checked && _dutchVACCATISGenerator.Metar != null)
                _dutchVACCATISGenerator.ProcessMetarButton_Click(null, null);

            //Re-enable the METAR button.
            _dutchVACCATISGenerator.GetMetarButton.Enabled = true;
        }
    }
}
