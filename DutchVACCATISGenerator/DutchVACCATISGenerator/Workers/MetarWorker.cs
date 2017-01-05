using System.ComponentModel;
using System.IO;
using System.Net;
using System.Windows.Forms;

namespace DutchVACCATISGenerator.Workers
{
    public interface IMetarWorker
    {
        /// <summary>
        /// Method called when METAR background workers is started. Pulls METAR from VATSIM METAR website.
        /// </summary>
        /// <param name="sender">Object sender</param>
        /// <param name="e">Event arguments</param>
        void MetarBackgroundWorker_DoWork(object sender, DoWorkEventArgs e);
    }

    public class MetarWorker : IMetarWorker
    {
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

                var metar = reader.ReadToEnd().Trim();

                //Remove spaces.
                if (metar.StartsWith(e.Argument.ToString()))
                    e.Result = metar;
            }
            catch (WebException)
            {
                MessageBox.Show("Unable to fetch the METAR from the Internet.\nProvide a METAR manually.", "Error");
            }
        }
    }
}
