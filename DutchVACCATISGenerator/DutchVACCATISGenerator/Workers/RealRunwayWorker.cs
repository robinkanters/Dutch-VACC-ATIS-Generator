using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Windows.Forms;

namespace DutchVACCATISGenerator.Workers
{
    public interface IRealRunwayWorker
    {
        /// <summary>
        /// Method called when real runway background workers is started. Gets the real EHAM runway configuration.
        /// </summary>
        /// <param name="sender">Object sender</param>
        /// <param name="e">Event arguments</param>
        void RealRunwayBackgroundWorker_DoWork(object sender, DoWorkEventArgs e);
    }

    public class RealRunwayWorker : IRealRunwayWorker
    {
        /// <summary>
        /// Method called when real runway background workers is started. Gets the real EHAM runway configuration.
        /// </summary>
        /// <param name="sender">Object sender</param>
        /// <param name="e">Event arguments</param>
        public void RealRunwayBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            //Initialize runway list to get rid of previous stored runways.
            var departureRunways = new List<string>();
            var landingRunways = new List<string>();

            try
            {
                //Create web client.
                //Set user Agent, make the site think we're not a bot.
                //(Windows; U; Windows NT 6.1; en-US; rv:1.9.2.4) Gecko/20100611 Firefox/3.6.4";
                var client = new WebClient
                {
                    Headers = { [HttpRequestHeader.UserAgent] = "Mozilla/5.0" }
                };

                //Make web request to http://www.lvnl.nl/nl/airtraffic.
                var data = client.DownloadString("http://www.lvnl.nl/nl/airtraffic");

                //Remove redundant HTML code.
                try
                {
                    data = data.Split(new[] { "<ul id=\"runwayVisual\">" }, StringSplitOptions.None)[1].Split(new[] { "</ul>" }, StringSplitOptions.None)[0];
                }
                catch (Exception)
                {
                    //Nothing to do here...
                }

                //If received data contains HTML <li> tag.
                while (data.Contains("<li"))
                {
                    //Get <li>...</lI>
                    var runwayListItem = data.Substring(data.IndexOf("<li", StringComparison.Ordinal), (data.IndexOf("</li>", StringComparison.Ordinal) + "</li>".Length) - data.IndexOf("<li", StringComparison.Ordinal));

                    //If found list item is landing runway.
                    if (runwayListItem.Contains("class=\"lb"))
                    {
                        runwayListItem = runwayListItem.Substring(runwayListItem.IndexOf("class=\"lb", StringComparison.Ordinal) + "class=\"lb".Length, runwayListItem.Length - (runwayListItem.IndexOf("class=\"lb", StringComparison.Ordinal) + "class=\"lb".Length));
                        landingRunways.Add(runwayListItem.Substring(0, runwayListItem.IndexOf("\">", StringComparison.Ordinal)));
                    }
                    //If found list item is departure runway.
                    else if (runwayListItem.Contains("class=\"sb"))
                    {
                        runwayListItem = runwayListItem.Substring(runwayListItem.IndexOf("class=\"sb", StringComparison.Ordinal) + "class=\"sb".Length, runwayListItem.Length - (runwayListItem.IndexOf("class=\"sb", StringComparison.Ordinal) + "class=\"sb".Length));
                        departureRunways.Add(runwayListItem.Substring(0, runwayListItem.IndexOf("\">", StringComparison.Ordinal)));
                    }

                    //Remove list item from received data.
                    data = data.Substring(data.IndexOf("</li>", StringComparison.Ordinal) + "</li>".Length, (data.Length - (data.IndexOf("</li>", StringComparison.Ordinal) + "</li>".Length)));
                }
            }
            catch (Exception)
            {
                //Show error.
                MessageBox.Show("Unable to get real EHAM runway combination from the Internet.", "Error");
            }

            //Set result.
            e.Result = new Tuple<List<string>, List<string>>(departureRunways, landingRunways);
        }
    }
}
