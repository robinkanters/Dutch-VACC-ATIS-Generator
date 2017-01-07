using System;
using System.Net;
using System.Windows.Forms;

namespace DutchVACCATISGenerator.Workers
{
    public interface ITAFWorker
    {
        /// <summary>
        /// Called when the TAF background worker is started.
        /// </summary>
        /// <param name="sender">Object sender</param>
        /// <param name="e">Event arguments</param>
        void TAFBackgroundWorker_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e);
    }

    public class TAFWorker : ITAFWorker
    {
        /// <summary>
        /// Called when the TAF background worker is started.
        /// </summary>
        /// <param name="sender">Object sender</param>
        /// <param name="e">Event arguments</param>
        public void TAFBackgroundWorker_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            var icao = (string) e.Argument;
            
            try
            {
                //Create web client.
                var client = new WebClient
                {
                    Headers = { [HttpRequestHeader.UserAgent] = "Mozilla/5.0" }
                };

                //Set user Agent, make the site think we're not a bot.
                //(Windows; U; Windows NT 6.1; en-US; rv:1.9.2.4) Gecko/20100611 Firefox/3.6.4";

                //Make web request to get TAF.
                //taf = client.DownloadString("http://www.aviationweather.gov/adds/tafs?station_ids=EHAM&std_trans=standard&submit_taf=Get+TAFs");
                e.Result = new Tuple<string, string>(icao, client.DownloadString("https://www.knmi.nl/nederland-nu/luchtvaart/vliegveldverwachtingen"));
            }
            catch (Exception)
            {
                //Show error.
                MessageBox.Show("Unable to load TAF from the Internet.", "Error");
            }
        }
    }
}