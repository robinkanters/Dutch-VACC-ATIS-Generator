﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DutchVACCATISGenerator.Types;

namespace DutchVACCATISGenerator.Forms
{
    /// <summary>
    /// RunwayInfo class.
    /// </summary>
    public partial class RunwayInfo : Form
    {
        private readonly int _bottom;
        private readonly int _left;
        private const int OKCOLUMN = 4;
        
        public Metar _metar { get; set; }
        
        //Tuple<RunwayHeading, OpositeRunwayHeading, Day Preference, Night Preference>
        private Dictionary<string, Tuple<int, int, string, string>> EHAMlandingRunways = new Dictionary<string, Tuple<int, int, string, string>>()
        {
            {"04", new Tuple<int, int, string, string>(041, 221,  "9" , "--")},
            {"06", new Tuple<int, int, string, string>(058, 238, "1", "1")},
            {"09", new Tuple<int, int, string, string>(087, 267, "8", "--")},
            {"18C", new Tuple<int, int, string, string>(183, 003, "4", "--")},
            {"18R", new Tuple<int, int, string, string>(183, 003, "2", "2")},
            {"22", new Tuple<int, int, string, string>(221, 041, "7", "--")},
            {"24", new Tuple<int, int, string, string>(238, 058, "7", "--")},
            {"27", new Tuple<int, int, string, string>(267, 087, "6", "--")},
            {"36C", new Tuple<int, int, string, string>(003, 183, "5", "3")},
            {"36R", new Tuple<int, int, string, string>(003, 183, "3", "--")}
        };

        //Tuple<RunwayHeading, OpositeRunwayHeading, Day Preference, Night Preference>
        private Dictionary<string, Tuple<int, int, string, string>> EHAMdepartureRunways = new Dictionary<string, Tuple<int, int, string, string>>()
        {
            {"04", new Tuple<int, int, string, string>(041, 221, "10", "--")},
            {"06", new Tuple<int, int, string, string>(058, 238, "8", "4")},
            {"09", new Tuple<int, int, string, string>(087, 267, "6", "--")},
            {"18L", new Tuple<int, int, string, string>(183, 003, "4", "--")},
            {"18C", new Tuple<int, int, string, string>(183, 003, "5", "3")},
            {"22", new Tuple<int, int, string, string>(221, 041, "9", "--")},
            {"24", new Tuple<int, int, string, string>(238, 058, "2", "2")},
            {"27", new Tuple<int, int, string, string>(267, 087, "7", "--")},
            {"36L", new Tuple<int, int, string, string>(003, 183, "1", "1")},
            {"36C", new Tuple<int, int, string, string>(003, 183, "3", "--")},
        };

        //Tuple<RunwayHeading, OpositeRunwayHeading, Preference>
        private Dictionary<string, Tuple<int, int, string>> EHBKRunways = new Dictionary<string, Tuple<int, int, string>>()
        {
            {"03", new Tuple<int, int, string>(032, 212, "2")},
            {"21", new Tuple<int, int, string>(212, 032, "1")},
        };

        //Tuple<RunwayHeading, OpositeRunwayHeading, Preference>
        private Dictionary<string, Tuple<int, int, string>> EHRDRunways = new Dictionary<string, Tuple<int, int, string>>()
        {
            {"06", new Tuple<int, int, string>(057, 257, "2")},
            {"24", new Tuple<int, int, string>(237, 037, "1")},
        };

        //Tuple<RunwayHeading, OpositeRunwayHeading, Preference>
        private Dictionary<string, Tuple<int, int, string>> EHEHRunways = new Dictionary<string, Tuple<int, int, string>>()
        {
            {"03", new Tuple<int, int, string>(034, 214, "2")},
            {"21", new Tuple<int, int, string>(214, 034, "1")},
        };

        //Tuple<RunwayHeading, OpositeRunwayHeading, Preference>
        private Dictionary<string, Tuple<int, int, string>> EHGGRunways = new Dictionary<string, Tuple<int, int, string>>()
        {
            {"01", new Tuple<int, int, string>(008, 214, "4")},
            {"05", new Tuple<int, int, string>(051, 231, "2")},
            {"19", new Tuple<int, int, string>(188, 008, "3")},
            {"23", new Tuple<int, int, string>(231, 051, "1")},
        };

        /// <summary>
        /// Constructor of RunwayInfo class. Initializes new instance of RunwayInfo.
        /// </summary>
        /// <param name="metar">METAR processed in DutchVACCATISGenerator.</param>
        public RunwayInfo(Metar metar, int left, int bottom, string icao)
        {
            InitializeComponent();

            //Set form position relative to Dutch VACC ATIS Generator form.
            _left = left;
            _bottom = bottom;
            
            _metar = metar;

            setVisibleRunwayInfoDataGrid(icao);

            //Set runway friction combo box selection to first item.
            runwayFrictionComboBox.SelectedIndex = 0;
        }

        /// <summary>
        /// Sets runway info data grid to be visible depending on the selected ICAO tab in DutchVACCATISGenerator.
        /// </summary>
        /// <param name="icaoTab">ICAO tab selected.</param>
        public void setVisibleRunwayInfoDataGrid(string icaoTab)
        {
            switch (icaoTab)
            {
                case "EHAM":
                    EHAMdepartureRunwayInfoDataGridView.Visible = EHAMlandingRunwayInfoDataGridView.Visible = EHAMdepartureRunwaysGroupBox.Visible = EHAMLandingRunwaysGroupBox.Visible = true;
                    runwayInfoDataGridView.Visible = false;

                    //Set the form size to 414 by 645.
                    this.Size = new Size(414, 645);                    
                    break;

                case "EHBK": case "EHRD": case "EHGG": case "EHEH":
                    EHAMdepartureRunwayInfoDataGridView.Visible = EHAMlandingRunwayInfoDataGridView.Visible = EHAMdepartureRunwaysGroupBox.Visible = EHAMLandingRunwaysGroupBox.Visible = false;
                    runwayInfoDataGridView.Visible = true;

                    //Set the form size to 414 by 373.
                    this.Size = new Size(414, 373);
                    break;
            }
        }

        /// <summary>
        /// Selects a dictionary to process depending on the selected ICAO tab in DutchVACCATISGenerator.
        /// </summary>
        /// <param name="icaoTab">ICAO tab selected.</param>
        private void ICAODirectoryToProcess(string icaoTab)
        {
            switch (icaoTab)
            {
                case "EHBK":
                    fillRunwayInfoDataGrid(EHBKRunways);
                    break;

                case "EHRD":
                    fillRunwayInfoDataGrid(EHRDRunways);
                    break;

                case "EHGG":
                    fillRunwayInfoDataGrid(EHGGRunways);
                    break;

                case "EHEH":
                    fillRunwayInfoDataGrid(EHEHRunways);
                    break;
            }
        }

        /// <summary>
        /// Fills the EHAM runway info data grids.
        /// </summary>
        private void fillEHAMRunwayInfoDataGrids()
        {
            //Clear the EHAM landing runway info DataGridView.
            EHAMlandingRunwayInfoDataGridView.Rows.Clear();

            foreach (KeyValuePair<string, Tuple<int, int, string, string>> pair in EHAMlandingRunways)
            {
                DataGridViewRow row = new DataGridViewRow();
                row.CreateCells(EHAMlandingRunwayInfoDataGridView);
                row.Cells[0].Value = pair.Key;
                row.Cells[1].Value = calculateCrosswindComponent(pair.Value.Item1);
                row.Cells[2].Value = calculateTailwindComponent(pair.Value.Item2) * -1; //Q&D
                row.Cells[3].Value = pair.Value.Item3;
                row.Cells[4].Value = pair.Value.Item4;
                row.Cells[5].Value = checkRunwayComply(pair.Key, calculateCrosswindComponent(pair.Value.Item1), calculateTailwindComponent(pair.Value.Item2));

                //Add built DataGridViewRow to EHAM landing runway info DataGridView.
                EHAMlandingRunwayInfoDataGridView.Rows.Add(row);
            }

            //Clear the EHAM departure runway info DataGridView.
            EHAMdepartureRunwayInfoDataGridView.Rows.Clear();

            foreach (KeyValuePair<string, Tuple<int, int, string, string>> pair in EHAMdepartureRunways)
            {
                DataGridViewRow row = new DataGridViewRow();
                row.CreateCells(EHAMdepartureRunwayInfoDataGridView);
                row.Cells[0].Value = pair.Key;
                row.Cells[1].Value = calculateCrosswindComponent(pair.Value.Item1);
                row.Cells[2].Value = calculateTailwindComponent(pair.Value.Item2) * -1; //Q&D
                row.Cells[3].Value = pair.Value.Item3;
                row.Cells[4].Value = pair.Value.Item4;
                row.Cells[5].Value = checkRunwayComply(pair.Key, calculateCrosswindComponent(pair.Value.Item1), calculateTailwindComponent(pair.Value.Item2));

                //Add built DataGridViewRow to EHAM departure runway info DataGridView.
                EHAMdepartureRunwayInfoDataGridView.Rows.Add(row);
            }

            addToolTipToEHAMRunwayInfoGridViews();
        }

        /// <summary>
        /// Fills the runway info data grids.
        /// </summary>
        /// <param name="runways">Dictionary to process.</param>
        private void fillRunwayInfoDataGrid(Dictionary<string, Tuple<int, int, string>> runways)
        {
            //Clear the runway info DataGridView.
            runwayInfoDataGridView.Rows.Clear();

            foreach (KeyValuePair<string, Tuple<int, int, string>> pair in runways)
            {
                DataGridViewRow row = new DataGridViewRow();
                row.CreateCells(runwayInfoDataGridView);
                row.Cells[0].Value = pair.Key;
                row.Cells[1].Value = calculateCrosswindComponent(pair.Value.Item1);
                row.Cells[2].Value = calculateTailwindComponent(pair.Value.Item2) * -1; //Q&D
                row.Cells[3].Value = pair.Value.Item3;
                row.Cells[4].Value = checkRunwayComply(pair.Key, calculateCrosswindComponent(pair.Value.Item1), calculateTailwindComponent(pair.Value.Item2));

                //Add built DataGridViewRow to runway info DataGridView.
                runwayInfoDataGridView.Rows.Add(row);
            }
        }

        /// <summary>
        /// Adds a tool tip to the EHAM runway info grids if the value of a cell is --.
        /// </summary>
        private void addToolTipToEHAMRunwayInfoGridViews()
        {
            //Add tool tip to EHAM landing- runway info DataGridView.
            foreach (DataGridViewRow row in EHAMlandingRunwayInfoDataGridView.Rows)
            {
                if(row.Cells[3].FormattedValue.Equals("--")) row.Cells[3].ToolTipText = "-- = Not allowed during night hours.";
                if (row.Cells[4].FormattedValue.Equals("--")) row.Cells[4].ToolTipText = "-- = Not allowed during night hours.";
            }

            //Add tool tip to EHAM departure runway info DataGridView.
            foreach (DataGridViewRow row in EHAMdepartureRunwayInfoDataGridView.Rows)
            {
                if (row.Cells[3].FormattedValue.Equals("--")) row.Cells[3].ToolTipText = "-- = Not allowed during night hours.";
                if (row.Cells[4].FormattedValue.Equals("--")) row.Cells[4].ToolTipText = "-- = Not allowed during night hours.";
            }
        }

        /// <summary>
        /// Calculate the tail wind component of a runway.
        /// </summary>
        /// <param name="runwayHeading">Opposite runway heading.</param>
        /// <returns>Calculated tail wind component of a runway.</returns>
        private int calculateTailwindComponent(int runwayHeading)
        {
            //If METAR has a gust wind.
            if(_metar.Wind.WindGustMin != null)
            {
                //If gust is greater than 10 knots, include gust wind.
                if (Math.Abs(Convert.ToInt32(_metar.Wind.WindGustMax) - Convert.ToInt32(_metar.Wind.WindGustMin)) >= 10) return Convert.ToInt32(Math.Cos(degreeToRadian(Math.Abs(Convert.ToInt32(_metar.Wind.WindHeading) - runwayHeading))) * Convert.ToInt32(_metar.Wind.WindGustMax));
                
                //Else do not include gust, calculate with min gust wind.
                else return Convert.ToInt32(Math.Cos(degreeToRadian(Math.Abs(Convert.ToInt32(_metar.Wind.WindHeading) - runwayHeading))) * Convert.ToInt32(_metar.Wind.WindGustMin)); 
            }
            else return Convert.ToInt32(Math.Cos(degreeToRadian(Math.Abs(Convert.ToInt32(_metar.Wind.WindHeading) - runwayHeading))) * Convert.ToInt32(_metar.Wind.WindKnots));
        }

        /// <summary>
        /// Calculate the cross wind component of a runway.
        /// </summary>
        /// <param name="runwayHeading">Runway heading.</param>
        /// <returns>Calculated cross wind component of a runway.</returns>
        private int calculateCrosswindComponent(int runwayHeading)
        {
            int crosswind;

            //If METAR has a gust wind.
            if(_metar.Wind.WindGustMin != null)
            {
                //If gust is greater than 10 knots, include gust wind.
                if (Math.Abs(Convert.ToInt32(_metar.Wind.WindGustMax) - Convert.ToInt32(_metar.Wind.WindGustMin)) >= 10) crosswind = Convert.ToInt32(Math.Sin(degreeToRadian(Math.Abs(Convert.ToInt32(_metar.Wind.WindHeading) - runwayHeading))) * Convert.ToInt32(_metar.Wind.WindGustMax));

                //Else do not include gust, calculate with min gust wind.
                else crosswind = Convert.ToInt32(Math.Sin(degreeToRadian(Math.Abs(Convert.ToInt32(_metar.Wind.WindHeading) - runwayHeading))) * Convert.ToInt32(_metar.Wind.WindGustMin));
            }
            else crosswind = Convert.ToInt32(Math.Sin(degreeToRadian(Math.Abs(Convert.ToInt32(_metar.Wind.WindHeading) - runwayHeading))) * Convert.ToInt32(_metar.Wind.WindKnots));

            //If calculated crosswind is negative, multiply by -1 to make it positive.
            if (crosswind < -0) return crosswind * -1;
            else return crosswind;
        }

        /// <summary>
        /// Convert degree to radian.
        /// </summary>
        /// <param name="input">Angle in degree.</param>
        /// <returns>Angle in radian.</returns>
        private double degreeToRadian(int input)
        {
            return input * (Math.PI / 180);
        }

        /// <summary>
        /// Set RunwayInform position relative to DutchVACCATISGenerator.
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
        /// Check if a runway complies with weather criteria for a runway.
        /// </summary>
        /// <param name="crosswind">Runway cross wind component.</param>
        /// <param name="tailwind">Runway tail wind component.</param>
        /// <returns>Indication if the a runway complies with the weather criteria for that runway.</returns>
        private string checkRunwayComply(string rwy, int crosswind, int tailwind)
        {
            switch(runwayFrictionComboBox.SelectedIndex)
            {
                case 0:
                    return checkRunwayVisbility(rwy) ? checkRunwayComplyWithWind(20, 7, crosswind, tailwind) : checkRunwayComplyWithWind(15, 7, crosswind, tailwind);

                case 1: case 2:
                    return checkRunwayVisbility(rwy) ? checkRunwayComplyWithWind(10, 0, crosswind, tailwind) : checkRunwayComplyWithWind(10, 0, crosswind, tailwind);

                case 3: case 4:
                    return checkRunwayVisbility(rwy) ? checkRunwayComplyWithWind(5, 0, crosswind, tailwind) : checkRunwayComplyWithWind(5, 0, crosswind, tailwind);

                default: return "Error";
            }
        }

        /// <summary>
        /// Check if a runway visibility complies with the viability criteria for that runway.
        /// </summary>
        /// <returns>Boolean indicating if the runway visibility complies with the viability criteria for that runway.</returns>
        private Boolean checkRunwayVisbility(string rwy)
        {
            //If METAR has a RVR.
            if(_metar.RVR)
            {
                foreach (KeyValuePair<string, int> pair in _metar.RVRValues)
                {
                    //Check if runway complies with RVR criteria based on the RWY RVR value.
                    if (pair.Key.Equals(rwy)) return runwayCompliesWithRVR(pair.Value);
                }

                //Check if runway complies with RVR criteria based on the visibility.
                return runwayCompliesWithRVR(_metar.Visibility);
            }
            else
            {
                //If cloud layer > 200 feet.
                if (_metar.Clouds.Count != 0 ? _metar.Clouds.First().Altitude >= 2 : true) return true;
                else return false;
            }
        }

        /// <summary>
        /// Check if a runway visibility complies with the RVR criteria for that runway.
        /// </summary>
        /// <param name="rvr">RVR to check to.</param>
        /// <returns>Boolean indicating if the runway visibility complies with the RVR criteria for that runway.</returns>
        private Boolean runwayCompliesWithRVR(int rvr)
        {
            //If RVR > 500 and cloud layer is > 200 feet.
            if (rvr >= 550 && (_metar.Clouds.Count != 0 ? _metar.Clouds.First().Altitude >= 2 : true)) return true;

            //If RVR < 500 and cloud layer is < 200 feet.
            else if (rvr < 550 || (_metar.Clouds.Count != 0 && _metar.Clouds.First().Altitude < 2)) return false;
            else return false;
        }

        /// <summary>
        /// Check if a runway wind complies with the wind criteria for that runway.
        /// </summary>
        /// <param name="maxCrosswind">Maximum crosswind component.</param>
        /// <param name="maxTailWind">Maximum tailwind component.</param>
        /// <param name="crosswind">Actual crosswind component.</param>
        /// <param name="tailwind">Actual tailwind component.</param>
        /// <returns>Indication if the a runway complies with the weather criteria for that runway.</returns>
        private string checkRunwayComplyWithWind(int maxCrosswind, int maxTailWind, int crosswind, int tailwind)
        {
            //If runway complies with criteria return OK.
            if (crosswind <= maxCrosswind && tailwind <= maxTailWind) return "OK";

            //Else return -.
            else return "-";
        }

        /// <summary>
        /// Method called if runway info form is loaded.
        /// </summary>
        /// <param name="sender">Object sender</param>
        /// <param name="e">Event arguments</param>
        private void RunwayInfo_Load(object sender, EventArgs e)
        {
            ShowRelativeToDutchVACCATISGenerator(_left, _bottom);
        }
        
        /// <summary>
        /// Method called if the selected index of runway friction combo box is changed.
        /// </summary>
        /// <param name="sender">Object sender</param>
        /// <param name="e">Event arguments</param>
        private void runwayFrictionComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            checkICAOTabSelected();
        }

        /// <summary>
        /// Check which ICAO tab is selected in DutchVACCATISGenerator.
        /// </summary>
        public void checkICAOTabSelected()
        {
            //TODO Fixen
            ////Check if selected ICAO tab matches the ICAO of the processed METAR.
            //if (!(_metar.ICAO.Equals(dutchVACCATISGenerator.ICAOTabControl.SelectedTab.Text))) MessageBox.Show(String.Format("Last processed METAR ICAO does not match the selected ICAO tab.\nRunway criteria will be calculated of the wrong METAR ({0})!", _metar.ICAO), "Warning");

            ////If selected ICAO tab is EHAM.
            //if (!(dutchVACCATISGenerator.ICAOTabControl.SelectedTab.Text.Equals("EHAM"))) ICAODirectoryToProcess(dutchVACCATISGenerator.ICAOTabControl.SelectedTab.Text);
            //else fillEHAMRunwayInfoDataGrids();
        }

        /// <summary>
        /// Method called if runway info form is closed.
        /// </summary>
        /// <param name="sender">Object sender</param>
        /// <param name="e">Event arguments</param>
        private void RunwayInfo_FormClosing(object sender, FormClosingEventArgs e)
        {
            //TODO Fixen
            ////Set runway info button back to >.
            //if (dutchVACCATISGenerator.runwayInfoButton.Text.Equals("<")) dutchVACCATISGenerator.runwayInfoButton.Text = ">";

            ////Set runway info tool strip menu item back color to control.
            //dutchVACCATISGenerator.runwayInfoToolStripMenuItem.BackColor = SystemColors.Control;
        }

        /// <summary>
        /// Method called when a column in EHAM departure runway info DataGridView is sorted.
        /// </summary>
        /// <param name="sender">Object sender</param>
        /// <param name="e">Event arguments</param>
        private void EHAMdepartureRunwayInfoDataGridView_SortCompare(object sender, DataGridViewSortCompareEventArgs e)
        {
            if (e.Column.Index == 3)
            {
                if (double.Parse(e.CellValue1.ToString()) > double.Parse(e.CellValue2.ToString())) e.SortResult = 1;
                
                else if (double.Parse(e.CellValue1.ToString()) < double.Parse(e.CellValue2.ToString())) e.SortResult = -1;
                
                else e.SortResult = 0;
                
                e.Handled = true;
            }
        }

        /// <summary>
        /// Set runway combo box with best preferred runway for selected ICAO.
        /// </summary>
        /// <param name="icaoTab">ICAO tab selected.</param>
        public void ICAOBestRunway(string icaoTab)
        {
            //TODO Fixen
            //switch (icaoTab)
            //{
            //    case "EHBK":
            //        dutchVACCATISGenerator.EHBKmainRunwayComboBox.SelectedIndex = dutchVACCATISGenerator.EHBKmainRunwayComboBox.Items.IndexOf(getBestRunway(runwayInfoDataGridView, EHBKRunways));
            //        break;

            //    case "EHRD":
            //        dutchVACCATISGenerator.EHRDmainRunwayComboBox.SelectedIndex = dutchVACCATISGenerator.EHRDmainRunwayComboBox.Items.IndexOf(getBestRunway(runwayInfoDataGridView, EHRDRunways));
            //        break;

            //    case "EHGG":
            //        dutchVACCATISGenerator.EHGGmainRunwayComboBox.SelectedIndex = dutchVACCATISGenerator.EHGGmainRunwayComboBox.Items.IndexOf(getBestRunway(runwayInfoDataGridView, EHGGRunways));
            //        break;

            //    case "EHEH":
            //        dutchVACCATISGenerator.EHEHmainRunwayComboBox.SelectedIndex = dutchVACCATISGenerator.EHEHmainRunwayComboBox.Items.IndexOf(getBestRunway(runwayInfoDataGridView, EHEHRunways));
            //        break;
            //}
        }

        /// <summary>
        /// Get best preferred runway by DataGridView.
        /// </summary>
        /// <param name="runwayInfoDataGridView"></param>
        /// <param name="runwayList"></param>
        /// <param name="prefColumn">Array position of pref column</param>
        /// <param name="OKColumn">Array position of OK column</param>
        /// <returns></returns>
        private string getBestRunway(DataGridView runwayInfoDataGridView, Dictionary<string, Tuple<int, int, string>> runwayList)
        {
            //Best runway holder.
            string runwayString = string.Empty;
            //Highest preference.
            int runwayPref = int.MaxValue;

            //Iterate through each data row of the provided DataGridView.
            foreach (DataGridViewRow row in runwayInfoDataGridView.Rows)
            {
                //If RWY is OK.
                if (!(row.Cells[OKCOLUMN].Value.Equals("OK")))
                {
                    runwayList.Remove(row.Cells[0].Value.ToString());
                }
            }

            foreach (KeyValuePair<string, Tuple<int, int, string>> pair in runwayList)
            {
                if (runwayString.Equals(string.Empty))
                {
                    runwayString = pair.Key;
                    runwayPref = Convert.ToInt32(pair.Value.Item3);
                }

                if (Convert.ToInt32(pair.Value.Item3) < runwayPref)
                {
                    runwayString = pair.Key;
                    runwayPref = Convert.ToInt32(pair.Value.Item3);
                }
            }
            
            return runwayString;
        }
    }
}
