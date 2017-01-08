using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using DutchVACCATISGenerator.Extensions;
using DutchVACCATISGenerator.Types;

namespace DutchVACCATISGenerator.Logic
{
    public class MetarLogic
    {
        public List<string> ATISSamples { get; set; }
        public Metar Metar { get; set; }

        /// <summary>
        /// Split METAR on split word.
        /// </summary>
        /// <param name="metar">METAR to split.</param>
        /// <param name="splitWord">Word to split on. (BECMG, TEMPO)</param>
        /// <returns>String array of split METAR</returns>
        private string[] splitMetar(string metar, string splitWord)
        {
            Regex regex = null;

            switch (splitWord)
            {
                case "BECMG":
                    regex = new Regex(@"\bBECMG\b");
                    break;

                case "TEMPO":
                    regex = new Regex(@"\bTEMPO\b");
                    break;

                case "BLU":
                    regex = new Regex(@"\bBLU\b");
                    break;

                case "WHT":
                    regex = new Regex(@"\bWHT\b");
                    break;

                case "GRN":
                    regex = new Regex(@"\bGRN\b");
                    break;

                case "YLO":
                    regex = new Regex(@"\bYLO\b");
                    break;

                case "AMB":
                    regex = new Regex(@"\bAMB\b");
                    break;

                case "RED":
                    regex = new Regex(@"\bRED\b");
                    break;

                case "BLACK":
                    regex = new Regex(@"\bBLACK\b");
                    break;

                default:
                    regex = new Regex("");
                    break;
            }

            return regex.Split(metar);
        }

        /// <summary>
        /// 
        /// </summary>
        public void ProcessMetar(string metar)
        {
            //If METAR contains military visibility code.
            if (Regex.IsMatch(metar, @"(^|\s)BLU(\s|$)") ||
                Regex.IsMatch(metar, @"(^|\s)WHT(\s|$)") ||
                Regex.IsMatch(metar, @"(^|\s)GRN(\s|$)") ||
                Regex.IsMatch(metar, @"(^|\s)YLO(\s|$)") ||
                Regex.IsMatch(metar, @"(^|\s)AMB(\s|$)") ||
                Regex.IsMatch(metar, @"(^|\s)RED(\s|$)") ||
                Regex.IsMatch(metar, @"(^|\s)BLACK(\s|$)"))

            {
                ProcessMilitaryMetar(metar);
            }
            //If METAR contains both BECMG and TEMPO trends.
            else if (metar.Contains("BECMG") && metar.Contains("TEMPO"))
            {
                //If BECMG is the first trend.
                Metar = metar.IndexOf("BECMG") < metar.IndexOf("TEMPO")
                    ? new Metar(splitMetar(metar, "BECMG")[0].Trim(),
                        splitMetar(metar, "TEMPO")[1].Trim(),
                        splitMetar(splitMetar(metar, "BECMG")[1].Trim(), "TEMPO")[0].Trim())
                    : new Metar(splitMetar(metar, "TEMPO")[0].Trim(),
                        splitMetar(splitMetar(metar, "TEMPO")[1].Trim(), "BECMG")[0].Trim(),
                        splitMetar(metar, "BECMG")[1].Trim());
            }
            //If METAR only contains BECMG.
            else if (metar.Contains("BECMG"))
                Metar = new Metar(splitMetar(metar, "BECMG")[0].Trim(),
                    splitMetar(metar, "BECMG")[1].Trim(),
                    MetarType.BECMG);
            //If METAR only contains TEMPO.
            else if (metar.Contains("TEMPO"))
                Metar = new Metar(splitMetar(metar, "TEMPO")[0].Trim(),
                    splitMetar(metar, "TEMPO")[1].Trim(), MetarType.TEMPO);
            //Process non trend containing METAR.
            else
                Metar = new Metar(metar);
        }

        /// <summary>
        /// Processes a military METAR.
        /// </summary>
        /// <param name="metar">String - METAR</param>
        private void ProcessMilitaryMetar(string metar)
        {
            //Military visibility codes.
            string[] militaryColors = { "BLU", "WHT", "GRN", "YLO", "AMB", "RED", "BLACK" };

            //If METAR contains BECMG and TEMPO
            if (metar.Contains("BECMG") && metar.Contains("TEMPO"))
            {
                if (metar.IndexOf("BECMG", StringComparison.Ordinal) < metar.IndexOf("TEMPO", StringComparison.Ordinal))
                {
                    //Check which military visibility code the METAR contains.
                    foreach (var militaryColor in militaryColors)
                    {
                        if (Regex.IsMatch(metar, @"(^|\s)" + militaryColor + @"(\s|$)"))
                            Metar = new Metar(splitMetar(metar, militaryColor)[0].Trim() /* BASE METAR */,
                                splitMetar(splitMetar(splitMetar(metar, militaryColor)[1].Trim(), "BECMG")[1].Trim(), "TEMPO")[1].Trim() /* TEMPO TREND */,
                                splitMetar(splitMetar(splitMetar(metar, militaryColor)[1].Trim(), "BECMG")[1].Trim(), "TEMPO")[0].Trim() /* BECMG TREND */);
                    }
                }
                else
                {
                    //Check which military visibility code the METAR contains.
                    foreach (var militaryColor in militaryColors)
                    {
                        if (Regex.IsMatch(metar, @"(^|\s)" + militaryColor + @"(\s|$)"))
                            Metar = new Metar(splitMetar(metar, militaryColor)[0].Trim() /* BASE METAR */,
                                splitMetar(splitMetar(splitMetar(metar, militaryColor)[1].Trim(), "TEMPO")[1].Trim(), "BECMG")[0].Trim() /* TEMPO TREND */,
                                splitMetar(splitMetar(splitMetar(metar, militaryColor)[1].Trim(), "TEMPO")[1].Trim(), "BECMG")[1].Trim() /* BECMG TREND */);
                    }
                }
            }
            //If METAR contains BECMG or TEMPO
            else if (metar.Contains("BECMG") || metar.Contains("TEMPO"))
            {
                //If METAR contains BECMG.
                if (metar.Contains("BECMG"))
                {
                    //Check which military visibility code the METAR contains.
                    foreach (var militaryColor in militaryColors)
                    {
                        if (Regex.IsMatch(metar, @"(^|\s)" + militaryColor + @"(\s|$)"))
                            Metar = new Metar(splitMetar(metar, militaryColor)[0].Trim() /* BASE METAR */,
                                splitMetar(splitMetar(metar, militaryColor)[1].Trim(), "BECMG")[1].Trim() /* BECMG TREND */,
                                MetarType.BECMG);
                    }
                }
                else
                {
                    //Check which military visibility code the METAR contains.
                    foreach (var militaryColor in militaryColors)
                    {
                        if (Regex.IsMatch(metar, @"(^|\s)" + militaryColor + @"(\s|$)"))
                            Metar = new Metar(splitMetar(metar, militaryColor)[0].Trim() /* BASE METAR */,
                                splitMetar(splitMetar(metar, militaryColor)[1].Trim(), "TEMPO")[1].Trim() /* TEMPO TREND */,
                                MetarType.TEMPO);
                    }
                }
            }
            //If METAR only contains military visibility code.
            else
            {
                //Check which military visibility code the METAR contains.
                foreach (var militaryColor in militaryColors)
                {
                    if (Regex.IsMatch(metar, @"(^|\s)" + militaryColor + @"(\s|$)"))
                        Metar = new Metar(splitMetar(metar, militaryColor)[0].Trim());
                }
            }
        }
        
        /// <summary>
        /// Calculate transition level from QNH and temperature.
        /// </summary>
        /// <returns>Calculated TL</returns>
        public int CalculateTransitionLevel()
        {
            int temp;

            //If METAR contains M (negative value), multiply by -1 to make an negative integer.
            if (Metar.Temperature.StartsWith("M"))
                temp = Convert.ToInt32(Metar.Temperature.Substring(1)) * -1;

            else
                temp = Convert.ToInt32(Metar.Temperature);

            //Calculate TL level. TL = 307.8-0.13986*T-0.26224*Q (thanks to Stefan Blauw for this formula).
            return (int)Math.Ceiling((307.8 - (0.13986 * temp) - (0.26224 * Metar.QNH)) / 5) * 5;
        }

        /// <summary>
        /// Generate wind output.
        /// </summary>
        /// <param name="input">String</param>
        /// <returns>String output</returns>
        private string windToOutput(MetarWind input)
        {
            string output = string.Empty;

            //If MetarWind has a calm wind.
            if (input.Vrb)
            {
                #region ADD SAMPLES TO ATISSAMPLES
                ATISSamples.Add("vrb");

                addIndividualDigitsToATISSamples(input.WindKnots);

                ATISSamples.Add("kt");
                #endregion

                output += " VARIABLE " + input.WindKnots + " KNOTS";
            }
            //If MetarWind has a gusting wind.
            else if (input.WindGustMin != null)
            {
                #region ADD SAMPLES TO ATISSAMPLES
                addIndividualDigitsToATISSamples(input.WindHeading);

                ATISSamples.Add("deg");

                addIndividualDigitsToATISSamples(input.WindGustMin);

                ATISSamples.Add("max");

                addIndividualDigitsToATISSamples(input.WindGustMax);

                ATISSamples.Add("kt");
                #endregion

                output += " " + input.WindHeading + " DEGREES" + input.WindGustMin + " MAXIMUM " + input.WindGustMax + " KNOTS";
            }
            //If MetarWind has a normal wind.
            else
            {
                #region ADD SAMPLES TO ATISSAMPLES
                addIndividualDigitsToATISSamples(input.WindHeading);

                ATISSamples.Add("deg");

                addIndividualDigitsToATISSamples(input.WindKnots);

                ATISSamples.Add("kt");
                #endregion

                output += " " + input.WindHeading + " DEGREES " + input.WindKnots + " KNOTS";
            }

            /*Variable wind*/
            if (input.WindVariableLeft != null)
            {
                #region ADD SAMPLES TO ATISSAMPLES
                ATISSamples.Add("vrbbtn");

                addIndividualDigitsToATISSamples(input.WindVariableLeft);

                ATISSamples.Add("and");

                addIndividualDigitsToATISSamples(input.WindVariableRight);

                ATISSamples.Add("deg");
                #endregion

                output += " VARIABLE BETWEEN " + input.WindVariableLeft + " AND " + input.WindVariableRight + " DEGREES";
            }

            return output;
        }

        private void addIndividualDigitsToATISSamples(string input)
        {
            var processed = Regex.Split(input, @"([0-9])");

            foreach (var digit in processed)
            {
                if (!string.IsNullOrEmpty(digit))
                    ATISSamples.Add(digit);
            }
        }

        public void GenerateAtis()
        {
            ATISSamples = new List<string>();

            string output = string.Empty;

            #region ICAO
            //Generate output from processed METAR ICAO.
            switch (Metar.ICAO)
            {
                //If processed ICAO is EHAM.
                case "EHAM":
                    ATISSamples.Add("ehamatis");
                    output += "THIS IS SCHIPHOL INFORMATION";
                    break;

                //If processed ICAO is EHBK.
                case "EHBK":
                    ATISSamples.Add("ehbkatis");
                    output += "THIS IS BEEK INFORMATION";
                    break;

                //If processed ICAO is EHEH.
                case "EHEH":
                    ATISSamples.Add("ehehatis");
                    output += "THIS IS EINDHOVEN INFORMATION";
                    break;

                //If processed ICAO is EHGG.
                case "EHGG":
                    ATISSamples.Add("ehggatis");
                    output += "THIS IS EELDE INFORMATION";
                    break;

                //If processed ICAO is EHRD.
                case "EHRD":
                    ATISSamples.Add("ehrdatis");
                    output += "THIS IS ROTTERDAM INFORMATION";
                    break;
            }
            #endregion

            #region ATIS LETTER
            //Add ATIS letter to output
            //TODO Fixen
            //output += " " + PhoneticAlphabet[ATISIndex].AtisLetterToFullSpelling();
            #endregion

            ATISSamples.Add("pause");

            #region RUNWAYS
            //Add runway output to output.
            output += generateRunwayOutput();
            #endregion

            #region TL
            //Add transition level to output.
            ATISSamples.Add("trl");
            output += " TRANSITION LEVEL";
            //Calculate and add transition level to output.
            addIndividualDigitsToATISSamples(CalculateTransitionLevel().ToString());
            output += " " + CalculateTransitionLevel();
            #endregion

            #region OPERATIONAL REPORTS
            //Generate and add operational report to output.
            output += operationalReportToOutput();
            #endregion

            ATISSamples.Add("pause");

            #region WIND
            //If processed METAR has wind, generate and add wind output to output. 
            //TODO Fixen
            //if (addWindRecordCheckBox.Checked)
            //{
            //    atisSamples.Add("wind");
            //    output += " WIND";
            //}
            if (Metar.Wind != null) output += windToOutput(Metar.Wind);
            #endregion

            #region CAVOK
            //If processed METAR has CAVOK, add CAVOK to output.
            if (Metar.CAVOK)
            {
                ATISSamples.Add("cavok");
                output += " CAVOK";
            }
            #endregion

            #region VISIBILITY
            //If processed METAR has a visibility greater than 0, generate and add visibility output to output. 
            if (Metar.Visibility > 0) output += visibilityToOutput(Metar.Visibility);
            #endregion

            #region RVRONATC
            //If processed METAR has RVR, add RVR to output. 
            if (Metar.RVR)
            {
                ATISSamples.Add("rvronatc");
                output += " RVR AVAILABLE ON ATC FREQUENCY";
            }
            #endregion

            #region PHENOMENA
            //Generate and add weather phenomena to output.
            output += listToOutput(Metar.Phenomena);
            #endregion

            #region CLOUDS OPTIONS
            //If processed METAR has SKC, add SKC to output. 
            if (Metar.SKC)
            {
                ATISSamples.Add("skc");
                output += " SKY CLEAR";
            }
            //If processed METAR has NSC, add NSC to output. 
            if (Metar.NSC)
            {
                ATISSamples.Add("sc");
                output += " NO SIGNIFICANT CLOUDS";
            }
            #endregion

            #region CLOUDS
            //Generate and add weather clouds to output. 
            output += listToOutput(Metar.Clouds);
            #endregion

            #region VERTICAL VISIBILITY
            //If processed METAR has a vertical visibility greater than 0, add vertical visibility to output.
            if (Metar.VerticalVisibility > 0)
            {
                ATISSamples.Add("vv");
                addIndividualDigitsToATISSamples(Metar.VerticalVisibility.ToString());
                ATISSamples.Add("hunderd");
                ATISSamples.Add("meters");

                output += " VERTICAL VISIBILITY " + Metar.VerticalVisibility + " HUNDERD METERS";
            }
            #endregion

            #region TEMPERATURE
            //Add temperature to output.
            ATISSamples.Add("temp");
            output += " TEMPERATURE";

            //If processed METAR has a minus temperature.
            if (Metar.Temperature.StartsWith("M"))
            {
                ATISSamples.Add("minus");

                addIndividualDigitsToATISSamples(Convert.ToInt32(Metar.Temperature.ToString().Substring(1, 2)).ToString());

                output += " MINUS " + Convert.ToInt32(Metar.Temperature.ToString().Substring(1, 2));
            }
            //Positive temperature.
            else
            {
                addIndividualDigitsToATISSamples(Convert.ToInt32(Metar.Temperature.ToString()).ToString());

                output += " " + Convert.ToInt32(Metar.Temperature.ToString());
            }
            #endregion

            #region DEWPOINT
            //Add dewpoint to output.
            ATISSamples.Add("dp");
            output += " DEWPOINT";

            //If processed METAR has a minus dewpoint.
            if (Metar.Dewpoint.StartsWith("M"))
            {
                ATISSamples.Add("minus");

                addIndividualDigitsToATISSamples(Convert.ToInt32(Metar.Dewpoint.ToString().Substring(1, 2)).ToString());

                output += " MINUS " + Convert.ToInt32(Metar.Dewpoint.ToString().Substring(1, 2));
            }

            //Positive dewpoint.
            else
            {
                addIndividualDigitsToATISSamples(Convert.ToInt32(Metar.Dewpoint.ToString()).ToString());

                output += " " + Convert.ToInt32(Metar.Dewpoint.ToString());
            }
            #endregion

            #region QNH
            //Add QNH to output.
            ATISSamples.Add("qnh");
            output += " QNH";
            addIndividualDigitsToATISSamples(Metar.QNH.ToString());
            output += " " + Metar.QNH.ToString();
            ATISSamples.Add("hpa");
            output += " HECTOPASCAL";
            #endregion

            #region NOSIG
            //If processed METAR has NOSIG, add NOSIG to output.
            if (Metar.NOSIG)
            {
                ATISSamples.Add("nosig");
                output += " NO SIGNIFICANT CHANGE";
            }
            #endregion

            #region TEMPO
            //If processed METAR has a TEMPO trend.
            if (Metar.MetarTEMPO != null)
            {
                //Add TEMPO to output.
                ATISSamples.Add("tempo");
                output += " TEMPORARY";

                #region TEMPO WIND
                //If processed TEMPO trend has wind, generate and add wind output to output. 
                if (Metar.MetarTEMPO.Wind != null) output += windToOutput(Metar.MetarTEMPO.Wind);
                #endregion

                #region TEMPO CAVOK
                //If processed TEMPO trend has CAVOK, add CAVOK to output.
                if (Metar.MetarTEMPO.CAVOK)
                {
                    ATISSamples.Add("cavok");
                    output += " CAVOK";
                }
                #endregion

                #region TEMPO VISIBILITY
                //If processed TEMPO trend has a visibility greater than 0, generate and add visibility output to output. 
                if (Metar.MetarTEMPO.Visibility > 0) output += visibilityToOutput(Metar.MetarTEMPO.Visibility);
                #endregion

                #region TEMPO PHENOMENA
                //If TEMPO trend has 1 or more weather phenomena, generate and add TEMPO trend weather phenomena to output.
                if (Metar.MetarTEMPO.Phenomena.Count > 0) output += listToOutput(Metar.MetarTEMPO.Phenomena);
                #endregion

                #region TEMPO SKC
                //If TEMPO trend has SKC, add SKC to output. 
                if (Metar.MetarTEMPO.SKC)
                {
                    ATISSamples.Add("skc");
                    output += " SKY CLEAR";
                }
                #endregion

                #region TEMPO NSW
                //If TEMPO trend has NSW, add NSW to output. 
                if (Metar.MetarTEMPO.NSW)
                {
                    ATISSamples.Add("nsw");
                    output += " NO SIGNIFICANT WEATHER";
                }
                #endregion

                #region TEMPO CLOUDS
                //If TEMPO trend has 1 or more weather clouds, generate and add TEMPO weather clouds to output. 
                if (Metar.MetarTEMPO.Clouds.Count > 0) output += listToOutput(Metar.MetarTEMPO.Clouds);
                #endregion

                #region TEMPO VERTICAL VISIBILITY
                //If TEMPO trend has a vertical visibility greater than 0, add TEMPO trend vertical visibility to output.
                if (Metar.MetarTEMPO.VerticalVisibility > 0)
                {
                    ATISSamples.Add("vv");
                    addIndividualDigitsToATISSamples(Metar.MetarTEMPO.VerticalVisibility.ToString());
                    ATISSamples.Add("hunderd");
                    ATISSamples.Add("meters");

                    output += " VERTICAL VISIBILITY " + Metar.MetarTEMPO.VerticalVisibility + " HUNDERD METERS";
                }
                #endregion
            }
            #endregion

            #region BECMG
            //If processed METAR has e BECMG trend.
            if (Metar.MetarBECMG != null)
            {
                //Add BECMG to output.
                ATISSamples.Add("becmg");
                output += " BECOMING";

                #region BECMG WIND
                //If processed BECMG trend has wind, generate and add wind output to output.
                if (Metar.MetarBECMG.Wind != null) output += windToOutput(Metar.MetarBECMG.Wind);
                #endregion

                #region BECMG CAVOK
                //If processed BECMG trend has CAVOK, add CAVOK to output.
                if (Metar.MetarBECMG.CAVOK)
                {
                    ATISSamples.Add("cavok");
                    output += " CAVOK";
                }
                #endregion

                #region BECMG VISIBILITY
                //If processed BECMG trend has a visibility greater than 0, generate and add visibility output to output. 
                if (Metar.MetarBECMG.Visibility > 0) output += visibilityToOutput(Metar.MetarBECMG.Visibility);
                #endregion

                #region BECMG PHENOMENA
                //If BECMG trend has 1 or more weather phenomena, generate and add BECMG trend weather phenomena to output.
                if (Metar.MetarBECMG.Phenomena.Count > 0) output += listToOutput(Metar.MetarBECMG.Phenomena);
                #endregion

                #region BECMG SKC
                //If BECMG trend has SKC, add SKC to output. 
                if (Metar.MetarBECMG.SKC)
                {
                    ATISSamples.Add("skc");
                    output += " SKY CLEAR";
                }
                #endregion

                #region BECMG NSW
                //If BECMG trend has NSW, add NSW to output. 
                if (Metar.MetarBECMG.NSW)
                {
                    ATISSamples.Add("nsw");
                    output += " NO SIGNIFICANT WEATHER";
                }
                #endregion

                #region BECMG CLOUDS
                //If BECMG trend has 1 or more weather clouds, generate and add BECMG weather clouds to output. 
                if (Metar.MetarBECMG.Clouds.Count > 0) output += listToOutput(Metar.MetarBECMG.Clouds);
                #endregion

                #region BECMG VERTICAL VISIBILITY
                //If BECMG trend has a vertical visibility greater than 0, add BECMG trend vertical visibility to output.
                if (Metar.MetarBECMG.VerticalVisibility > 0)
                {
                    ATISSamples.Add("vv");
                    addIndividualDigitsToATISSamples(Metar.MetarBECMG.VerticalVisibility.ToString());
                    ATISSamples.Add("hunderd");
                    ATISSamples.Add("meters");

                    output += " VERTICAL VISIBILITY" + Metar.MetarBECMG.VerticalVisibility + " HUNDERD METERS";
                }
                #endregion
            }
            #endregion

            #region OPTIONAL
            //If inverted surface temperature check box is checked.
            //TODO Fixen
            //if (markTempCheckBox.Checked)
            //{
            //    atisSamples.Add("marktemp");
            //    output += " MARK TEMPERATURE INVERSION NEAR THE SURFACE";
            //}
            ////If arrival only check box is checked.
            //if (arrOnlyCheckBox.Checked)
            //{
            //    atisSamples.Add("call1");
            //    output += " CONTACT ARRIVAL CALLSIGN ONLY";
            //}
            ////If approach only check box is checked.
            //if (appOnlyCheckBox.Checked)
            //{
            //    atisSamples.Add("call2");
            //    output += " CONTACT APPROACH CALLSIGN ONLY";
            //}
            ////If arrival and approach only check box is checked.
            //if (appArrOnlyCheckBox.Checked)
            //{
            //    atisSamples.Add("call3");
            //    output += " CONTACT APPROACH AND ARRIVAL CALLSIGN ONLY";
            //}
            #endregion

            #region END
            //Add end to output.
            ATISSamples.Add("end");
            output += " END OF INFORMATION";
            //TODO Fixen
            //output += " " + PhoneticAlphabet[ATISIndex].AtisLetterToFullSpelling();
            #endregion

            #region USER WAVE
            //if (userDefinedExtraCheckBox.Checked)
            //{
            //    atisSamples.Add("extra");
            //    output += " EXTRA (VOICE ONLY)";
            //}
            #endregion

            //If copy output check box is checked, copy ATIS output to clipboard.
            //if (copyOutputCheckBox.Checked) Clipboard.SetText(output);

            //Set generated ATIS output in output text box.
            //outputTextBox.Text = output;

#if DEBUG
            Console.WriteLine();

            foreach (var sample in ATISSamples)
            {
                if (sample.All(char.IsDigit))
                    Console.Write(sample);

                else
                    Console.Write("[" + sample + "]");
            }

            Console.WriteLine();
#endif

            //Build ATIS file.
            //sound.buildAtis(atisSamples);
        }

        /// <summary>
        /// Parse runway identifier letter to ATIS output text.
        /// </summary>
        /// <param name="input">Runway identifier letter (L, C, R).</param>
        /// <returns>Runway identifier ATIS output</returns>
        private string GetRunwayMarker(string input)
        {
            switch (input)
            {
                case "L":
                    ATISSamples.Add("left");
                    return "L";

                case "C":
                    ATISSamples.Add("center");
                    return "C";

                case "R":
                    ATISSamples.Add("right");
                    return "R";
            }

            return string.Empty;
        }

        /// <summary>
        /// Generate output from List<T>
        /// </summary>
        /// <typeparam name="T">List type</typeparam>
        /// <param name="input">List<T></param>
        /// <returns>String output</returns>
        private string listToOutput<T>(List<T> input)
        {
            string output = string.Empty;

            #region MetarPhenomena
            //If list is a MetarPhenomena list.
            if (input is List<MetarPhenomena>)
            {
                foreach (MetarPhenomena metarPhenomena in input as List<MetarPhenomena>)
                {
                    //TODO Light, Moderate, Heavy
                    //If phenomena has intensity.
                    //if (metarPhenomena.HasIntensity)
                    //{
                    //    ATISSamples.Add("-");
                    //    output += " LIGHT";
                    //}

                    //If phenomena is 4 character phenomena (BCFG | MIFG | SHRA | VCSH | VCTS).
                    if (metarPhenomena.Phenomena.Equals("BCFG") || metarPhenomena.Phenomena.Equals("MIFG") || metarPhenomena.Phenomena.Equals("SHRA") || metarPhenomena.Phenomena.Equals("VCSH") || metarPhenomena.Phenomena.Equals("VCTS"))
                    {
                        switch (metarPhenomena.Phenomena)
                        {
                            case "BCFG":
                                ATISSamples.Add("bcfg");
                                output += " PATCHES OF FOG";
                                break;

                            case "MIFG":
                                ATISSamples.Add("mifg");
                                output += " SHALLOW FOG";
                                break;

                            case "SHRA":
                                ATISSamples.Add("shra");
                                output += " SHOWERS OF RAIN";
                                break;

                            case "VCSH":
                                ATISSamples.Add("vcsh");
                                output += " SHOWERS IN VICINITY";
                                break;

                            case "VCTS":
                                ATISSamples.Add("vcts");
                                output += " THUNDERSTORMS IN VICINITY";
                                break;
                        }
                    }
                    //If phenomena is multi-phenomena (count > 2).
                    else if (metarPhenomena.Phenomena.Count() > 2)
                    {
                        int length = metarPhenomena.Phenomena.Length;

                        if ((length % 2) == 0)
                        {
                            int index = 0;

                            while (index != length)
                            {
                                if (length - index != 2)
                                {
                                    output += metarPhenomena.Phenomena.Substring(index, 2).PhenomenaToFullSpelling();
                                    ATISSamples.Add(metarPhenomena.Phenomena.Substring(index, 2));
                                }
                                else
                                {
                                    output += metarPhenomena.Phenomena.Substring(index).PhenomenaToFullSpelling();
                                    ATISSamples.Add(metarPhenomena.Phenomena.Substring(index));
                                }

                                index = index + 2;
                            }
                        }
                    }
                    //If phenomena is 2 char phenomena.
                    else
                    {
                        output += metarPhenomena.Phenomena.PhenomenaToFullSpelling();
                        ATISSamples.Add(metarPhenomena.Phenomena);
                    }

                    //If loop phenomena is not the last phenomena of the list, add [and].
                    if (metarPhenomena != (MetarPhenomena)Convert.ChangeType(input.Last(), typeof(MetarPhenomena)))
                    {
                        ATISSamples.Add("and");
                        output += " AND";
                    }
                }
            }
            #endregion

            #region MetarCloud
            //If list is a MetarCloud list.
            else if (input is List<MetarCloud>)
            {
                foreach (MetarCloud metarCloud in input as List<MetarCloud>)
                {
                    //TODO Fixen
                    //Add cloud type identifier.
                    //output += cloudTypeToFullSpelling(metarCloud.cloudType);

                    //If cloud altitude equals ground level.
                    if (metarCloud.Altitude == 0)
                    {
                        addIndividualDigitsToATISSamples(metarCloud.Altitude.ToString());

                        output += " " + metarCloud.Altitude;
                    }

                    //If cloud altitude is round ten-thousand (e.g. 10000 (100), 20000 (200), 30000 (300)).
                    else if (metarCloud.Altitude % 100 == 0)
                    {
                        addIndividualDigitsToATISSamples(Math.Floor(Convert.ToDouble(metarCloud.Altitude / 100)).ToString() + "0");
                        ATISSamples.Add("thousand");

                        output += " " + Math.Floor(Convert.ToDouble(metarCloud.Altitude / 100)).ToString() + "0" + " THOUSAND";
                    }

                    else
                    {
                        //If cloud altitude is greater than a ten-thousand (e.g. 12000 (120), 23500 (235), 45000 (450)).
                        if (metarCloud.Altitude / 100 > 0)
                        {
                            addIndividualDigitsToATISSamples(Math.Floor(Convert.ToDouble(metarCloud.Altitude / 100)).ToString());

                            output += " " + Math.Floor(Convert.ToDouble(metarCloud.Altitude / 100)).ToString();

                            //If cloud altitude has a ten-thousand and hundred value (e.g. 10200 (102), 20800 (208), 40700 (407)).
                            if (metarCloud.Altitude.ToString().Substring(1, 1).Equals("0"))
                            {
                                ATISSamples.Add("0");
                                ATISSamples.Add("thousand");
                                output += " 0 THOUSAND";
                            }
                        }

                        //If cloud altitude has a thousand (e.g. 2000 (020), 4000 (040), 5000 (050)).
                        if ((metarCloud.Altitude / 10) % 10 > 0)
                        {
                            addIndividualDigitsToATISSamples(Math.Floor(Convert.ToDouble((metarCloud.Altitude / 10) % 10)).ToString());
                            ATISSamples.Add("thousand");

                            output += " " + Math.Floor(Convert.ToDouble((metarCloud.Altitude / 10) % 10)) + " THOUSAND";
                        }

                        //If cloud altitude has a hundred (e.g. 200 (002), 400 (004), 500 (005)).
                        if (metarCloud.Altitude % 10 > 0)
                        {
                            addIndividualDigitsToATISSamples(Convert.ToString(metarCloud.Altitude % 10));
                            ATISSamples.Add("hundred");

                            output += " " + metarCloud.Altitude % 10 + " HUNDRED";
                        }
                    }

                    ATISSamples.Add("ft");
                    output += " FEET";

                    //TODO Fixen
                    //If cloud type has addition (e.g. CB, TCU).
                    //if (metarCloud.addition != null) output += cloudAddiationToFullSpelling(metarCloud.addition);
                }
            }
            #endregion

            return output;
        }

        /// <summary>
        /// Generate configuration of runway.
        /// </summary>
        /// <param name="runway"></param>
        /// <param name="runwayComboBox"></param>
        /// <returns>String of runway output</returns>
        private string runwayToOutput(string runway, ComboBox runwayComboBox)
        {
            string output = runway;
            output += " ";

            //Split runway digit identifier from runway identifier.
            string[] splitArray = Regex.Split(runwayComboBox.SelectedItem.ToString().Substring(0, 2), @"([0-9])");

            foreach (string digit in splitArray)
            {
                if (!string.IsNullOrEmpty(digit))
                    ATISSamples.Add(digit);
            }

            //If selected runway contains runway identifier letter.
            if (runwayComboBox.SelectedItem.ToString().Length > 2)
            {
                //Add runway identifier numbers to output.
                output += runwayComboBox.SelectedItem.ToString().Substring(0, 2);
                //Add runway identifier letter to output.
                return output += GetRunwayMarker(runwayComboBox.SelectedItem.ToString().Substring(2));
            }
            else return output += runwayComboBox.SelectedItem.ToString();
        }

        /// <summary>
        /// Generate operational report.
        /// </summary>
        /// <returns>String output</returns>
        private string operationalReportToOutput()
        {
            ATISSamples.Add("opr");
            string output = " OPERATIONAL REPORT";

            #region LOW LEVEL VISIBILITY
            //If visibility is not 0 or less than 1500 meter, add low visibility procedure phrase.
            if (Metar.Visibility != 0 && Metar.Visibility < 1500)
            {
                ATISSamples.Add("lvp");
                output += " LOW VISIBILITY PROCEDURES IN PROGRESS";
            }
            #endregion

            #region RWY CONFIGURATIONS
            //TODO Fixen
            //if (EHAMmainLandingRunwayCheckBox.Checked && EHAMsecondaryLandingRunwayCheckBox.Checked)
            //{
            //    #region INDEPENDENT APPROACHES
            //    /* 18R & 18C */
            //    if (EHAMmainLandingRunwayComboBox.Text.Equals("18R") && EHAMsecondaryLandingRunwayComboBox.Text.Equals("18C"))
            //    {
            //        atisSamples.Add("independent");
            //        output += " INDEPENDENT PARALLEL APPROACHES IN PROGRESS";
            //    }

            //    else if (EHAMmainLandingRunwayComboBox.Text.Equals("18C") && EHAMsecondaryLandingRunwayComboBox.Text.Equals("18R"))
            //    {
            //        atisSamples.Add("independent");
            //        output += " INDEPENDENT PARALLEL APPROACHES IN PROGRESS";
            //    }
            //    /* 36R & 36C */
            //    else if (EHAMmainLandingRunwayComboBox.Text.Equals("36R") && EHAMsecondaryLandingRunwayComboBox.Text.Equals("36C"))
            //    {
            //        atisSamples.Add("independent");
            //        output += " INDEPENDENT PARALLEL APPROACHES IN PROGRESS";
            //    }

            //    else if (EHAMmainLandingRunwayComboBox.Text.Equals("36C") && EHAMsecondaryLandingRunwayComboBox.Text.Equals("36R"))
            //    {
            //        atisSamples.Add("independent");
            //        output += " INDEPENDENT PARALLEL APPROACHES IN PROGRESS";
            //    }
            //    #endregion
            //    #region CONVERGING APPROACHES
            //    /* 06 & 36R */
            //    else if (EHAMmainLandingRunwayComboBox.Text.Equals("06") && EHAMsecondaryLandingRunwayComboBox.Text.Equals("36R"))
            //    {
            //        atisSamples.Add("convapp");
            //        output += " CONVERGING APPROACHES IN PROGRESS";
            //    }

            //    else if (EHAMmainLandingRunwayComboBox.Text.Equals("36R") && EHAMsecondaryLandingRunwayComboBox.Text.Equals("06"))
            //    {
            //        atisSamples.Add("convapp");
            //        output += " CONVERGING APPROACHES IN PROGRESS";
            //    }
            //    /* 06 & 27 */
            //    else if (EHAMmainLandingRunwayComboBox.Text.Equals("06") && EHAMsecondaryLandingRunwayComboBox.Text.Equals("27"))
            //    {
            //        atisSamples.Add("convapp");
            //        output += " CONVERGING APPROACHES IN PROGRESS";
            //    }

            //    else if (EHAMmainLandingRunwayComboBox.Text.Equals("27") && EHAMsecondaryLandingRunwayComboBox.Text.Equals("06"))
            //    {
            //        atisSamples.Add("convapp");
            //        output += " CONVERGING APPROACHES IN PROGRESS";
            //    }
            //    /* 06 & 09 */
            //    else if (EHAMmainLandingRunwayComboBox.Text.Equals("06") && EHAMsecondaryLandingRunwayComboBox.Text.Equals("09"))
            //    {
            //        atisSamples.Add("convapp");
            //        output += " CONVERGING APPROACHES IN PROGRESS";
            //    }

            //    else if (EHAMmainLandingRunwayComboBox.Text.Equals("09") && EHAMsecondaryLandingRunwayComboBox.Text.Equals("06"))
            //    {
            //        atisSamples.Add("convapp");
            //        output += " CONVERGING APPROACHES IN PROGRESS";
            //    }
            //    /* 06 & 18C */
            //    else if (EHAMmainLandingRunwayComboBox.Text.Equals("06") && EHAMsecondaryLandingRunwayComboBox.Text.Equals("18C"))
            //    {
            //        atisSamples.Add("convapp");
            //        output += " CONVERGING APPROACHES IN PROGRESS";
            //    }

            //    else if (EHAMmainLandingRunwayComboBox.Text.Equals("18C") && EHAMsecondaryLandingRunwayComboBox.Text.Equals("06"))
            //    {
            //        atisSamples.Add("convapp");
            //        output += " CONVERGING APPROACHES IN PROGRESS";
            //    }

            //    /* 27 & 18C */
            //    else if (EHAMmainLandingRunwayComboBox.Text.Equals("18C") && EHAMsecondaryLandingRunwayComboBox.Text.Equals("27"))
            //    {
            //        atisSamples.Add("convapp");
            //        output += " CONVERGING APPROACHES IN PROGRESS";
            //    }

            //    else if (EHAMmainLandingRunwayComboBox.Text.Equals("27") && EHAMsecondaryLandingRunwayComboBox.Text.Equals("18C"))
            //    {
            //        atisSamples.Add("convapp");
            //        output += " CONVERGING APPROACHES IN PROGRESS";
            //    }
            //    /* 27 & 18R */
            //    else if (EHAMmainLandingRunwayComboBox.Text.Equals("18R") && EHAMsecondaryLandingRunwayComboBox.Text.Equals("27"))
            //    {
            //        atisSamples.Add("convapp");
            //        output += " CONVERGING APPROACHES IN PROGRESS";
            //    }

            //    else if (EHAMmainLandingRunwayComboBox.Text.Equals("27") && EHAMsecondaryLandingRunwayComboBox.Text.Equals("18R"))
            //    {
            //        atisSamples.Add("convapp");
            //        output += " CONVERGING APPROACHES IN PROGRESS";
            //    }
            //    /* 27 & 36C */
            //    else if (EHAMmainLandingRunwayComboBox.Text.Equals("27") && EHAMsecondaryLandingRunwayComboBox.Text.Equals("36C"))
            //    {
            //        atisSamples.Add("convapp");
            //        output += " CONVERGING APPROACHES IN PROGRESS";
            //    }

            //    else if (EHAMmainLandingRunwayComboBox.Text.Equals("36C") && EHAMsecondaryLandingRunwayComboBox.Text.Equals("27"))
            //    {
            //        atisSamples.Add("convapp");
            //        output += " CONVERGING APPROACHES IN PROGRESS";
            //    }
            //    /* 27 & 36R */
            //    else if (EHAMmainLandingRunwayComboBox.Text.Equals("27") && EHAMsecondaryLandingRunwayComboBox.Text.Equals("36R"))
            //    {
            //        atisSamples.Add("convapp");
            //        output += " CONVERGING APPROACHES IN PROGRESS";
            //    }

            //    else if (EHAMmainLandingRunwayComboBox.Text.Equals("36R") && EHAMsecondaryLandingRunwayComboBox.Text.Equals("27"))
            //    {
            //        atisSamples.Add("convapp");
            //        output += " CONVERGING APPROACHES IN PROGRESS";
            //    }
            //    #endregion
            //}
            #endregion

            #region CHECK FOR ADDING [AND]
            if (output.Contains("LOW VISIBILITY PROCEDURES IN PROGRESS") && (output.Contains("INDEPENDENT PARALLEL APPROACHES IN PROGRESS") || output.Contains("CONVERGING APPROACHES IN PROGRESS")))
            {
                if (output.Contains(" INDEPENDENT PARALLEL APPROACHES IN PROGRESS"))
                {
                    ATISSamples.Insert(ATISSamples.IndexOf("independent"), "and");
                    output = output.Insert(output.IndexOf("IND"), " AND ");
                }

                else
                {
                    ATISSamples.Insert(ATISSamples.IndexOf("convapp"), "and");
                    output = output.Insert(output.IndexOf("CON"), " AND");
                }
            }
            #endregion

            if (!output.Equals(" OPERATIONAL REPORT")) return output;
            else
            {
                ATISSamples.Remove("opr");
                return "";
            }
        }

        /// <summary>
        /// Generate visibility output.
        /// </summary>
        /// <param name="input">Integer</param>
        /// <returns>String output</returns>
        private string visibilityToOutput(int input)
        {
            ATISSamples.Add("vis");
            string output = " VISIBILITY";

            //If visibility is lower than 800 meters (less than 800 meters phrase).
            if (input < 800)
            {
                ATISSamples.Add("<");
                addIndividualDigitsToATISSamples("800");
                ATISSamples.Add("meters");

                output += " LESS THAN 8 HUNDRED METERS";
            }
            //If visibility is lower than 1000 meters (add hundred).
            else if (input < 1000)
            {
                addIndividualDigitsToATISSamples(Convert.ToString(input / 100));
                ATISSamples.Add("hundred");
                ATISSamples.Add("meters");

                output += " " + Convert.ToString(input / 100) + " HUNDRED METERS";
            }
            //If visibility is lower than 5000 meters and visibility is not a thousand number.
            else if (input < 5000 && (input % 1000) != 0)
            {
                addIndividualDigitsToATISSamples(Convert.ToString(input / 1000));
                ATISSamples.Add("thousand");
                addIndividualDigitsToATISSamples(Convert.ToString((input % 1000) / 100));
                ATISSamples.Add("hundred");
                ATISSamples.Add("meters");

                output += " " + Convert.ToString(input / 1000) + " THOUSAND " + Convert.ToString((input % 1000) / 100) + " HUNDRED METERS";
            }
            //If visibility is >= 9999 (10 km phrase).
            else if (input >= 9999)
            {
                addIndividualDigitsToATISSamples("10");
                ATISSamples.Add("km");

                output += " 10 KILOMETERS";
            }
            //If visibility is thousand.
            else
            {
                addIndividualDigitsToATISSamples(Convert.ToString(input / 1000));
                ATISSamples.Add("km");

                output += " " + Convert.ToString(input / 1000) + " KILOMETERS";
            }

            return output;
        }

        /// <summary>
        /// Method to generate route output.
        /// </summary>
        /// <returns>String containing the runway output of the selected airport tab.</returns>
        private string generateRunwayOutput()
        {
            return string.Empty;
            //TODO Fixen
            //String output = String.Empty;

            //switch (ICAOTabControl.SelectedTab.Name)
            //{
            //    #region EHAM
            //    //If selected ICAO tab is EHAM.
            //    case "EHAM":
            //        #region EHAM MAIN LANDING RUNWAY
            //        //If the EHAM main landing runway check box is checked AND the EHAM main landing runway combo box value doesn't equal the EHAM main departure runway combo box value, generate runway output with value from EHAM main landing runway combo box.
            //        if (EHAMmainLandingRunwayCheckBox.Checked && !EHAMmainLandingRunwayComboBox.Text.Equals(EHAMmainDepartureRunwayComboBox.Text))
            //        {
            //            atisSamples.Add("mlrwy");
            //            output += runwayToOutput(" MAIN LANDING RUNWAY", EHAMmainLandingRunwayComboBox);
            //        }
            //        //Else generate runway output with the value from the EHAM main landing runway combo box.
            //        else
            //        {
            //            atisSamples.Add("mlrwy");
            //            output += runwayToOutput(" MAIN LANDING RUNWAY", EHAMmainLandingRunwayComboBox);
            //        }
            //        #endregion

            //        #region EHAM SECONDARY LANDING RUNWAY
            //        //If the EHAM secondary landing runway check box is checked, generate runway output with the value from the EHAM secondary landing runway combo box.
            //        if (EHAMsecondaryLandingRunwayCheckBox.Checked)
            //        {
            //            atisSamples.Add("slrwy");
            //            output += runwayToOutput(" SECONDARY LANDING RUNWAY", EHAMsecondaryLandingRunwayComboBox);
            //        }
            //        #endregion

            //        #region EHAM MAIN DEPARTURE RUNWAY
            //        //If the EHAM main departure runway check box is checked, generate runway output with the value from the EHAM main departure runway combo box.
            //        if (EHAMmainDepartureRunwayCheckBox.Checked && !EHAMmainLandingRunwayComboBox.Text.Equals(EHAMmainDepartureRunwayComboBox.Text))
            //        {
            //            atisSamples.Add("mtrwy");
            //            output += runwayToOutput(" MAIN TAKEOFF RUNWAY", EHAMmainDepartureRunwayComboBox);
            //        }
            //        #endregion

            //        #region EHAM SECONDARY DEPARTURE RUNWAY
            //        //If the EHAM secondary departure runway check box is checked, generate runway output with the value from the EHAM secondary departure runway combo box.
            //        if (EHAMsecondaryDepartureRunwayCheckBox.Checked)
            //        {
            //            atisSamples.Add("strwy");
            //            output += runwayToOutput(" SECONDARY TAKEOFF RUNWAY", EHAMsecondaryDepartureRunwayComboBox);
            //        }
            //        #endregion
            //        break;
            //    #endregion

            //    #region EHBK
            //    //If selected ICAO tab is EHBK.
            //    case "EHBK":
            //        //If EHBK main runway check box is checked, generate runway output with value from EHBK main runway combo box.
            //        if (EHBKmainRunwayCheckBox.Checked)
            //        {
            //            atisSamples.Add("mlrwy");
            //            output += runwayToOutput(" MAIN LANDING RUNWAY", EHBKmainRunwayComboBox);
            //        }
            //        break;
            //    #endregion

            //    #region EHEH
            //    //If selected ICAO tab is EHEH.
            //    case "EHEH":
            //        //If EHEH main runway check box is checked, generate runway output with value from EHEH main runway combo box.
            //        if (EHEHmainRunwayCheckBox.Checked)
            //        {
            //            atisSamples.Add("mlrwy");
            //            output += runwayToOutput(" MAIN LANDING RUNWAY", EHEHmainRunwayComboBox);
            //        }
            //        break;
            //    #endregion

            //    #region EHGG
            //    //If selected ICAO tab is EHGG.
            //    case "EHGG":
            //        //If EHGG main runway check box is checked, generate runway output with value from EHGG main runway combo box.
            //        if (EHGGmainRunwayCheckBox.Checked)
            //        {
            //            atisSamples.Add("mlrwy");
            //            output += runwayToOutput(" MAIN LANDING RUNWAY", EHGGmainRunwayComboBox);
            //        }
            //        break;
            //    #endregion

            //    #region EHRD
            //    //If selected ICAO tab is EHRD.
            //    case "EHRD":
            //        //If EHRD main runway check box is checked, generate runway output with value from EHRD main runway combo box.
            //        if (EHRDmainRunwayCheckBox.Checked)
            //        {
            //            atisSamples.Add("mlrwy");
            //            output += runwayToOutput(" MAIN LANDING RUNWAY", EHRDmainRunwayComboBox);
            //        }
            //        break;
            //        #endregion
            //}

            //return output;
        }

        public void AddToAtisSamples(string sample)
        {
            ATISSamples.Add(sample);
        }
    }
}
