using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Windows.Forms;

namespace DutchVACCATISGenerator.Workers
{
    public class RealRunwayWorker
    {
        private List<string> DepartureRunways { get; set; }
        private readonly Forms.DutchVACCATISGenerator _dutchVACCATISGenerator;
        private List<string> LandingRunways { get; set; }

        public RealRunwayWorker(Forms.DutchVACCATISGenerator dutchVACCATISGenerator)
        {
            _dutchVACCATISGenerator = dutchVACCATISGenerator;
        }

        /// <summary>
        /// Method called when real runway background workers is started. Gets the real EHAM runway configuration.
        /// </summary>
        /// <param name="sender">Object sender</param>
        /// <param name="e">Event arguments</param>
        public void RealRunwayBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            //Initialize runway list to get rid of previous stored runways.
            DepartureRunways = new List<string>();
            LandingRunways = new List<string>();

            try
            {
                //Create web client.
                WebClient client = new WebClient();

                //Set user Agent, make the site think we're not a bot.
                client.Headers[HttpRequestHeader.UserAgent] = "Mozilla/5.0"; //(Windows; U; Windows NT 6.1; en-US; rv:1.9.2.4) Gecko/20100611 Firefox/3.6.4";

                //Make web request to http://www.lvnl.nl/nl/airtraffic.
                string data = client.DownloadString("http://www.lvnl.nl/nl/airtraffic");

                #region Remove redundant HTML code.
                try
                {
                    data = data.Split(new string[] { "<ul id=\"runwayVisual\">" }, StringSplitOptions.None)[1].Split(new string[] { "</ul>" }, StringSplitOptions.None)[0];
                }
                catch (Exception) { }
                #endregion

                //If received data contains HTML <li> tag.
                while (data.Contains("<li"))
                {
                    //Get <li>...</lI>
                    string runwayListItem = data.Substring(data.IndexOf("<li"), (data.IndexOf("</li>") + "</li>".Length) - data.IndexOf("<li"));

                    //If found list item is landing runway.
                    if (runwayListItem.Contains("class=\"lb"))
                    {
                        runwayListItem = runwayListItem.Substring(runwayListItem.IndexOf("class=\"lb") + "class=\"lb".Length, runwayListItem.Length - (runwayListItem.IndexOf("class=\"lb") + "class=\"lb".Length));
                        LandingRunways.Add(runwayListItem.Substring(0, runwayListItem.IndexOf("\">")));
                    }
                    //If found list item is departure runway.
                    else if (runwayListItem.Contains("class=\"sb"))
                    {
                        runwayListItem = runwayListItem.Substring(runwayListItem.IndexOf("class=\"sb") + "class=\"sb".Length, runwayListItem.Length - (runwayListItem.IndexOf("class=\"sb") + "class=\"sb".Length));
                        DepartureRunways.Add(runwayListItem = runwayListItem.Substring(0, runwayListItem.IndexOf("\">")));
                    }

                    //Remove list item from received data.
                    data = data.Substring(data.IndexOf("</li>") + "</li>".Length, (data.Length - (data.IndexOf("</li>") + "</li>".Length)));
                }
            }
            catch (Exception)
            {
                //Show error.
                MessageBox.Show("Unable to get real EHAM runway combination from the Internet.", "Error");
            }
        }

        /// <summary>
        /// Method called when real runway background worker is completed.
        /// </summary>
        /// <param name="sender">Object sender</param>
        /// <param name="e">Event arguments</param>
        public void RealRunwayBackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            //Clear runway combo boxes.
            _dutchVACCATISGenerator.EHAMmainDepartureRunwayComboBox.SelectedIndex =
                _dutchVACCATISGenerator.EHAMmainLandingRunwayComboBox.SelectedIndex =
                    _dutchVACCATISGenerator.EHAMsecondaryDepartureRunwayComboBox.SelectedIndex =
                        _dutchVACCATISGenerator.EHAMsecondaryLandingRunwayComboBox.SelectedIndex = -1;

            //Only one departure runway found.
            if (DepartureRunways.Count == 1)
                _dutchVACCATISGenerator.EHAMmainDepartureRunwayComboBox.Text = DepartureRunways.First();

            //Only one landing runway found.
            if (LandingRunways.Count == 1)
                _dutchVACCATISGenerator.EHAMmainLandingRunwayComboBox.Text = LandingRunways.First();

            //Two or more landing or departure runways found.
            if (LandingRunways.Count > 1 || DepartureRunways.Count > 1)
                processMultipleRunways();

            //Re-enable get select best runway button.
            _dutchVACCATISGenerator.getSelectBestRunwayButton.Enabled = true;

            if (LandingRunways.Count() > 0 || DepartureRunways.Count > 0)
                MessageBox.Show("Controller notice! Verify auto selected runway(s).", "Warning");
        }

        /// <summary>
        /// Process if multiple runways we're found and set the runway selection boxes with the founded values.
        /// </summary>
        private void processMultipleRunways()
        {
            #region LANDING RUNWAYS
            //If there are more than two landing runways.
            if (LandingRunways.Count > 1)
            {
                string firstRunway = LandingRunways.First();
                string secondRunway = LandingRunways.Last();

                //Generate landing runway combinations list.
                List<Tuple<string, string>> landingRunwayCombinations = new List<Tuple<string, string>>()
                {
                    /* 06 combinations */
                    { new Tuple<string, string>("06", "18R")},
                    { new Tuple<string, string>("06", "36R")},
                    { new Tuple<string, string>("06", "18C")},
                    { new Tuple<string, string>("06", "36C")},
                    { new Tuple<string, string>("06", "27")},
                    { new Tuple<string, string>("06", "22")},
                    { new Tuple<string, string>("06", "09")},
                    { new Tuple<string, string>("06", "04")},
                    /* 18R combinations */
                    { new Tuple<string, string>("18R", "36R")},
                    { new Tuple<string, string>("18R", "18C")},
                    { new Tuple<string, string>("18R", "36C")},
                    { new Tuple<string, string>("18R", "27")},
                    { new Tuple<string, string>("18R", "22")},
                    { new Tuple<string, string>("18R", "24")},
                    { new Tuple<string, string>("18R", "09")},
                    { new Tuple<string, string>("18R", "04")},
                    /* 36R combinations */
                    { new Tuple<string, string>("36R", "18C")},
                    { new Tuple<string, string>("36R", "36C")},
                    { new Tuple<string, string>("36R", "27")},
                    { new Tuple<string, string>("36R", "22")},
                    { new Tuple<string, string>("36R", "24")},
                    { new Tuple<string, string>("36R", "09")},
                    { new Tuple<string, string>("36R", "04")},
                    /* 18C combinations */
                    { new Tuple<string, string>("18C", "27")},
                    { new Tuple<string, string>("18C", "22")},
                    { new Tuple<string, string>("18C", "24")},
                    { new Tuple<string, string>("18C", "09")},
                    { new Tuple<string, string>("18C", "04")},
                    /* 36C combinations */
                    { new Tuple<string, string>("36C", "27")},
                    { new Tuple<string, string>("36C", "22")},
                    { new Tuple<string, string>("36C", "24")},
                    { new Tuple<string, string>("36C", "09")},
                    { new Tuple<string, string>("36C", "04")},
                    /* 27 combinations */
                    { new Tuple<string, string>("27", "22")},
                    { new Tuple<string, string>("27", "24")},
                    { new Tuple<string, string>("27", "09")},
                    { new Tuple<string, string>("27", "04")},
                    /* 22 combinations */
                    { new Tuple<string, string>("22", "24")},
                    { new Tuple<string, string>("22", "09")},
                    /* 24 combinations */
                    { new Tuple<string, string>("24", "09")},
                    { new Tuple<string, string>("24", "04")},
                    /* 09 combinations */
                    { new Tuple<string, string>("09", "04")},
                };

                //Check which runways are found and set the correct main and secondary landing runway.
                foreach (Tuple<string, string> runwayCombination in landingRunwayCombinations)
                {
                    if ((firstRunway.Equals(runwayCombination.Item1) && secondRunway.Equals(runwayCombination.Item2)) || (firstRunway.Equals(runwayCombination.Item2) && secondRunway.Equals(runwayCombination.Item1)))
                    {
                        _dutchVACCATISGenerator.EHAMmainLandingRunwayComboBox.Text = runwayCombination.Item1;
                        _dutchVACCATISGenerator.EHAMsecondaryLandingRunwayComboBox.Text = runwayCombination.Item2;
                    }
                }
            }
            #endregion

            #region DEPARTURE RUNWAYS
            //If there are more than two departure runways found.
            if (DepartureRunways.Count > 1)
            {
                string firstRunway = DepartureRunways.First();
                string secondRunway = DepartureRunways.Last();

                //Generate departure runway combinations list.
                List<Tuple<string, string>> departureRunwayCombinations = new List<Tuple<string, string>>()
                {
                    /* 36L combinations */
                    { new Tuple<string, string>("36L", "24")},
                    { new Tuple<string, string>("36L", "36C")},
                    { new Tuple<string, string>("36L", "18L")},
                    { new Tuple<string, string>("36L", "18C")},
                    { new Tuple<string, string>("36L", "09")},
                    { new Tuple<string, string>("36L", "27")},
                    { new Tuple<string, string>("36L", "06")},
                    { new Tuple<string, string>("36L", "22")},
                    { new Tuple<string, string>("36L", "04")}, 
                    /* 24 combinations */
                    { new Tuple<string, string>("24", "36C")},
                    { new Tuple<string, string>("24", "18L")},
                    { new Tuple<string, string>("24", "18C")},
                    { new Tuple<string, string>("24", "09")},
                    { new Tuple<string, string>("24", "27")},
                    { new Tuple<string, string>("24", "22")},
                    { new Tuple<string, string>("24", "04")},
                    /* 36C combinations */
                    { new Tuple<string, string>("36C", "18L")},
                    { new Tuple<string, string>("36C", "09")},
                    { new Tuple<string, string>("36C", "27")},
                    { new Tuple<string, string>("36C", "06")},
                    { new Tuple<string, string>("36C", "22")},
                    { new Tuple<string, string>("36C", "04")},
                    /* 18L combinations */
                    { new Tuple<string, string>("18L", "18C")},
                    { new Tuple<string, string>("18L", "09")},
                    { new Tuple<string, string>("18L", "27")},
                    { new Tuple<string, string>("18L", "06")},
                    { new Tuple<string, string>("18L", "22")},
                    { new Tuple<string, string>("18L", "04")},
                    /* 18C combinations */
                    { new Tuple<string, string>("18C", "09")},
                    { new Tuple<string, string>("18C", "27")},
                    { new Tuple<string, string>("18C", "06")},
                    { new Tuple<string, string>("18C", "22")},
                    { new Tuple<string, string>("18C", "04")},
                    /* 09 combinations */
                    { new Tuple<string, string>("09", "27")},
                    { new Tuple<string, string>("09", "06")},
                    { new Tuple<string, string>("09", "22")},
                    { new Tuple<string, string>("09", "04")},
                    /* 27 combinations */
                    { new Tuple<string, string>("27", "06")},
                    { new Tuple<string, string>("27", "22")},
                    { new Tuple<string, string>("27", "04")},
                    /* 06 combinations */
                    { new Tuple<string, string>("06", "22")},
                    { new Tuple<string, string>("06", "04")},
                    /* 22 combinations */
                    { new Tuple<string, string>("22", "04")},
                };

                //Check which runways are found and set the correct main and secondary departure runway.
                foreach (Tuple<string, string> runwayCombination in departureRunwayCombinations)
                {
                    if ((firstRunway.Equals(runwayCombination.Item1) && secondRunway.Equals(runwayCombination.Item2)) || (firstRunway.Equals(runwayCombination.Item2) && secondRunway.Equals(runwayCombination.Item1)))
                    {
                        _dutchVACCATISGenerator.EHAMmainDepartureRunwayComboBox.Text = runwayCombination.Item1;
                        _dutchVACCATISGenerator.EHAMsecondaryDepartureRunwayComboBox.Text = runwayCombination.Item2;
                    }
                }
            }
            #endregion
        }

    }
}
