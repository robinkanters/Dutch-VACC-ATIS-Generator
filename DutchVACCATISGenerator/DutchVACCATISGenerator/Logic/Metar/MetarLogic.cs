using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using DutchVACCATISGenerator.Extensions;
using DutchVACCATISGenerator.Types;

namespace DutchVACCATISGenerator.Logic.Metar
{
    public class MetarLogic
    {
        public List<string> atisSamples { get; set; }
        public Types.Metar Metar { get; set; }
        public MetarProcessor _metarProcessor { set; get; }


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
                processMilitaryMetar(metar);
            }
            
                //If METAR contains both BECMG and TEMPO trends.
            else if (metar.Contains("BECMG") && metar.Contains("TEMPO"))
            {
                //If BECMG is the first trend.
                _metarProcessor = metar.IndexOf("BECMG") < metar.IndexOf("TEMPO")
                    ? new MetarProcessor(splitMetar(metar, "BECMG")[0].Trim(),
                        splitMetar(metar, "TEMPO")[1].Trim(),
                        splitMetar(splitMetar(metar, "BECMG")[1].Trim(), "TEMPO")[0].Trim())
                    : new MetarProcessor(splitMetar(metar, "TEMPO")[0].Trim(),
                        splitMetar(splitMetar(metar, "TEMPO")[1].Trim(), "BECMG")[0].Trim(),
                        splitMetar(metar, "BECMG")[1].Trim());
            }
            //If METAR only contains BECMG.
            else if (metar.Contains("BECMG"))
                _metarProcessor = new MetarProcessor(splitMetar(metar, "BECMG")[0].Trim(), 
                    splitMetar(metar, "BECMG")[1].Trim(), 
                    MetarType.BECMG);
            //If METAR only contains TEMPO.
            else if (metar.Contains("TEMPO"))
                _metarProcessor = new MetarProcessor(splitMetar(metar, "TEMPO")[0].Trim(), 
                    splitMetar(metar, "TEMPO")[1].Trim(), MetarType.TEMPO);
            //Process non trend containing METAR.
            else
                _metarProcessor = new MetarProcessor(metar);
        }

        /// <summary>
        /// Calculate transition level from QNH and temperature.
        /// </summary>
        /// <returns>Calculated TL</returns>
        public int CalculateTransitionLevel()
        {
            int temp;

            //If METAR contains M (negative value), multiply by -1 to make an negative integer.
            if (_metarProcessor.metar.Temperature.StartsWith("M"))
                temp = Convert.ToInt32(_metarProcessor.metar.Temperature.Substring(1)) * -1;

            else
                temp = Convert.ToInt32(_metarProcessor.metar.Temperature);

            //Calculate TL level. TL = 307.8-0.13986*T-0.26224*Q (thanks to Stefan Blauw for this formula).
            return (int)Math.Ceiling((307.8 - (0.13986 * temp) - (0.26224 * _metarProcessor.metar.QNH)) / 5) * 5;
        }

        private void processMilitaryMetar(string metar)
        {
     
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
                                _metarProcessor = new MetarProcessor(splitMetar(metar, militaryColor)[0].Trim() /* BASE METAR */,
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
                                _metarProcessor = new MetarProcessor(splitMetar(metar, militaryColor)[0].Trim() /* BASE METAR */,
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
                                _metarProcessor = new MetarProcessor(splitMetar(metar, militaryColor)[0].Trim() /* BASE METAR */,
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
                                _metarProcessor = new MetarProcessor(splitMetar(metar, militaryColor)[0].Trim() /* BASE METAR */,
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
                            _metarProcessor = new MetarProcessor(splitMetar(metar, militaryColor)[0].Trim());
                    }
                }
            }
        }


        /// <summary>
        /// Generate wind output.
        /// </summary>
        /// <param name="input">String</param>
        /// <returns>String output</returns>
        private String windToOutput(MetarWind input)
        {
            String output = String.Empty;

            //If MetarWind has a calm wind.
            if (input.VRB)
            {
                #region ADD SAMPLES TO ATISSAMPLES
                atisSamples.Add("vrb");

                addIndividualDigitsToATISSamples(input.windKnots);

                atisSamples.Add("kt");
                #endregion

                output += " VARIABLE " + input.windKnots + " KNOTS";
            }
            //If MetarWind has a gusting wind.
            else if (input.windGustMin != null)
            {
                #region ADD SAMPLES TO ATISSAMPLES
                addIndividualDigitsToATISSamples(input.windHeading);

                atisSamples.Add("deg");

                addIndividualDigitsToATISSamples(input.windGustMin);

                atisSamples.Add("max");

                addIndividualDigitsToATISSamples(input.windGustMax);

                atisSamples.Add("kt");
                #endregion

                output += " " + input.windHeading + " DEGREES" + input.windGustMin + " MAXIMUM " + input.windGustMax + " KNOTS";
            }
            //If MetarWind has a normal wind.
            else
            {
                #region ADD SAMPLES TO ATISSAMPLES
                addIndividualDigitsToATISSamples(input.windHeading);

                atisSamples.Add("deg");

                addIndividualDigitsToATISSamples(input.windKnots);

                atisSamples.Add("kt");
                #endregion

                output += " " + input.windHeading + " DEGREES " + input.windKnots + " KNOTS";
            }

            /*Variable wind*/
            if (input.windVariableLeft != null)
            {
                #region ADD SAMPLES TO ATISSAMPLES
                atisSamples.Add("vrbbtn");

                addIndividualDigitsToATISSamples(input.windVariableLeft);

                atisSamples.Add("and");

                addIndividualDigitsToATISSamples(input.windVariableRight);

                atisSamples.Add("deg");
                #endregion

                output += " VARIABLE BETWEEN " + input.windVariableLeft + " AND " + input.windVariableRight + " DEGREES";
            }

            return output;
        }

        private void addIndividualDigitsToATISSamples(string input)
        {
            var processed = Regex.Split(input, @"([0-9])");

            foreach (var digit in processed)
            {
                if (!string.IsNullOrEmpty(digit))
                    atisSamples.Add(digit);
            }
        }

        public void GenerateAtis()
        {
            atisSamples = new List<String>();

            String output = String.Empty;

            #region ICAO
            //Generate output from processed METAR ICAO.
            switch (_metarProcessor.metar.ICAO)
            {
                //If processed ICAO is EHAM.
                case "EHAM":
                    atisSamples.Add("ehamatis");
                    output += "THIS IS SCHIPHOL INFORMATION";
                    break;

                //If processed ICAO is EHBK.
                case "EHBK":
                    atisSamples.Add("ehbkatis");
                    output += "THIS IS BEEK INFORMATION";
                    break;

                //If processed ICAO is EHEH.
                case "EHEH":
                    atisSamples.Add("ehehatis");
                    output += "THIS IS EINDHOVEN INFORMATION";
                    break;

                //If processed ICAO is EHGG.
                case "EHGG":
                    atisSamples.Add("ehggatis");
                    output += "THIS IS EELDE INFORMATION";
                    break;

                //If processed ICAO is EHRD.
                case "EHRD":
                    atisSamples.Add("ehrdatis");
                    output += "THIS IS ROTTERDAM INFORMATION";
                    break;
            }
            #endregion

            #region ATIS LETTER
            //Add ATIS letter to output
            //TODO Fixen
            //output += " " + PhoneticAlphabet[ATISIndex].AtisLetterToFullSpelling();
            #endregion

            atisSamples.Add("pause");

            #region RUNWAYS
            //Add runway output to output.
            output += generateRunwayOutput();
            #endregion

            #region TL
            //Add transition level to output.
            atisSamples.Add("trl");
            output += " TRANSITION LEVEL";
            //Calculate and add transition level to output.
            addIndividualDigitsToATISSamples(CalculateTransitionLevel().ToString());
            output += " " + CalculateTransitionLevel();
            #endregion

            #region OPERATIONAL REPORTS
            //Generate and add operational report to output.
            output += operationalReportToOutput();
            #endregion

            atisSamples.Add("pause");

            #region WIND
            //If processed METAR has wind, generate and add wind output to output. 
            //TODO Fixen
            //if (addWindRecordCheckBox.Checked)
            //{
            //    atisSamples.Add("wind");
            //    output += " WIND";
            //}
            if (_metarProcessor.metar.Wind != null) output += windToOutput(_metarProcessor.metar.Wind);
            #endregion

            #region CAVOK
            //If processed METAR has CAVOK, add CAVOK to output.
            if (_metarProcessor.metar.CAVOK)
            {
                atisSamples.Add("cavok");
                output += " CAVOK";
            }
            #endregion

            #region VISIBILITY
            //If processed METAR has a visibility greater than 0, generate and add visibility output to output. 
            if (_metarProcessor.metar.Visibility > 0) output += visibilityToOutput(_metarProcessor.metar.Visibility);
            #endregion

            #region RVRONATC
            //If processed METAR has RVR, add RVR to output. 
            if (_metarProcessor.metar.RVR)
            {
                atisSamples.Add("rvronatc");
                output += " RVR AVAILABLE ON ATC FREQUENCY";
            }
            #endregion

            #region PHENOMENA
            //Generate and add weather phenomena to output.
            output += listToOutput(_metarProcessor.metar.Phenomena);
            #endregion

            #region CLOUDS OPTIONS
            //If processed METAR has SKC, add SKC to output. 
            if (_metarProcessor.metar.SKC)
            {
                atisSamples.Add("skc");
                output += " SKY CLEAR";
            }
            //If processed METAR has NSC, add NSC to output. 
            if (_metarProcessor.metar.NSC)
            {
                atisSamples.Add("sc");
                output += " NO SIGNIFICANT CLOUDS";
            }
            #endregion

            #region CLOUDS
            //Generate and add weather clouds to output. 
            output += listToOutput(_metarProcessor.metar.Clouds);
            #endregion

            #region VERTICAL VISIBILITY
            //If processed METAR has a vertical visibility greater than 0, add vertical visibility to output.
            if (_metarProcessor.metar.VerticalVisibility > 0)
            {
                atisSamples.Add("vv");
                addIndividualDigitsToATISSamples(_metarProcessor.metar.VerticalVisibility.ToString());
                atisSamples.Add("hunderd");
                atisSamples.Add("meters");

                output += " VERTICAL VISIBILITY " + _metarProcessor.metar.VerticalVisibility + " HUNDERD METERS";
            }
            #endregion

            #region TEMPERATURE
            //Add temperature to output.
            atisSamples.Add("temp");
            output += " TEMPERATURE";

            //If processed METAR has a minus temperature.
            if (_metarProcessor.metar.Temperature.StartsWith("M"))
            {
                atisSamples.Add("minus");

                addIndividualDigitsToATISSamples(Convert.ToInt32(_metarProcessor.metar.Temperature.ToString().Substring(1, 2)).ToString());

                output += " MINUS " + Convert.ToInt32(_metarProcessor.metar.Temperature.ToString().Substring(1, 2));
            }
            //Positive temperature.
            else
            {
                addIndividualDigitsToATISSamples(Convert.ToInt32(_metarProcessor.metar.Temperature.ToString()).ToString());

                output += " " + Convert.ToInt32(_metarProcessor.metar.Temperature.ToString());
            }
            #endregion

            #region DEWPOINT
            //Add dewpoint to output.
            atisSamples.Add("dp");
            output += " DEWPOINT";

            //If processed METAR has a minus dewpoint.
            if (_metarProcessor.metar.Dewpoint.StartsWith("M"))
            {
                atisSamples.Add("minus");

                addIndividualDigitsToATISSamples(Convert.ToInt32(_metarProcessor.metar.Dewpoint.ToString().Substring(1, 2)).ToString());

                output += " MINUS " + Convert.ToInt32(_metarProcessor.metar.Dewpoint.ToString().Substring(1, 2));
            }

            //Positive dewpoint.
            else
            {
                addIndividualDigitsToATISSamples(Convert.ToInt32(_metarProcessor.metar.Dewpoint.ToString()).ToString());

                output += " " + Convert.ToInt32(_metarProcessor.metar.Dewpoint.ToString());
            }
            #endregion

            #region QNH
            //Add QNH to output.
            atisSamples.Add("qnh");
            output += " QNH";
            addIndividualDigitsToATISSamples(_metarProcessor.metar.QNH.ToString());
            output += " " + _metarProcessor.metar.QNH.ToString();
            atisSamples.Add("hpa");
            output += " HECTOPASCAL";
            #endregion

            #region NOSIG
            //If processed METAR has NOSIG, add NOSIG to output.
            if (_metarProcessor.metar.NOSIG)
            {
                atisSamples.Add("nosig");
                output += " NO SIGNIFICANT CHANGE";
            }
            #endregion

            #region TEMPO
            //If processed METAR has a TEMPO trend.
            if (_metarProcessor.metar.metarTEMPO != null)
            {
                //Add TEMPO to output.
                atisSamples.Add("tempo");
                output += " TEMPORARY";

                #region TEMPO WIND
                //If processed TEMPO trend has wind, generate and add wind output to output. 
                if (_metarProcessor.metar.metarTEMPO.Wind != null) output += windToOutput(_metarProcessor.metar.metarTEMPO.Wind);
                #endregion

                #region TEMPO CAVOK
                //If processed TEMPO trend has CAVOK, add CAVOK to output.
                if (_metarProcessor.metar.metarTEMPO.CAVOK)
                {
                    atisSamples.Add("cavok");
                    output += " CAVOK";
                }
                #endregion

                #region TEMPO VISIBILITY
                //If processed TEMPO trend has a visibility greater than 0, generate and add visibility output to output. 
                if (_metarProcessor.metar.metarTEMPO.Visibility > 0) output += visibilityToOutput(_metarProcessor.metar.metarTEMPO.Visibility);
                #endregion

                #region TEMPO PHENOMENA
                //If TEMPO trend has 1 or more weather phenomena, generate and add TEMPO trend weather phenomena to output.
                if (_metarProcessor.metar.metarTEMPO.Phenomena.Count > 0) output += listToOutput(_metarProcessor.metar.metarTEMPO.Phenomena);
                #endregion

                #region TEMPO SKC
                //If TEMPO trend has SKC, add SKC to output. 
                if (_metarProcessor.metar.metarTEMPO.SKC)
                {
                    atisSamples.Add("skc");
                    output += " SKY CLEAR";
                }
                #endregion

                #region TEMPO NSW
                //If TEMPO trend has NSW, add NSW to output. 
                if (_metarProcessor.metar.metarTEMPO.NSW)
                {
                    atisSamples.Add("nsw");
                    output += " NO SIGNIFICANT WEATHER";
                }
                #endregion

                #region TEMPO CLOUDS
                //If TEMPO trend has 1 or more weather clouds, generate and add TEMPO weather clouds to output. 
                if (_metarProcessor.metar.metarTEMPO.Clouds.Count > 0) output += listToOutput(_metarProcessor.metar.metarTEMPO.Clouds);
                #endregion

                #region TEMPO VERTICAL VISIBILITY
                //If TEMPO trend has a vertical visibility greater than 0, add TEMPO trend vertical visibility to output.
                if (_metarProcessor.metar.metarTEMPO.VerticalVisibility > 0)
                {
                    atisSamples.Add("vv");
                    addIndividualDigitsToATISSamples(_metarProcessor.metar.metarTEMPO.VerticalVisibility.ToString());
                    atisSamples.Add("hunderd");
                    atisSamples.Add("meters");

                    output += " VERTICAL VISIBILITY " + _metarProcessor.metar.metarTEMPO.VerticalVisibility + " HUNDERD METERS";
                }
                #endregion
            }
            #endregion

            #region BECMG
            //If processed METAR has e BECMG trend.
            if (_metarProcessor.metar.metarBECMG != null)
            {
                //Add BECMG to output.
                atisSamples.Add("becmg");
                output += " BECOMING";

                #region BECMG WIND
                //If processed BECMG trend has wind, generate and add wind output to output.
                if (_metarProcessor.metar.metarBECMG.Wind != null) output += windToOutput(_metarProcessor.metar.metarBECMG.Wind);
                #endregion

                #region BECMG CAVOK
                //If processed BECMG trend has CAVOK, add CAVOK to output.
                if (_metarProcessor.metar.metarBECMG.CAVOK)
                {
                    atisSamples.Add("cavok");
                    output += " CAVOK";
                }
                #endregion

                #region BECMG VISIBILITY
                //If processed BECMG trend has a visibility greater than 0, generate and add visibility output to output. 
                if (_metarProcessor.metar.metarBECMG.Visibility > 0) output += visibilityToOutput(_metarProcessor.metar.metarBECMG.Visibility);
                #endregion

                #region BECMG PHENOMENA
                //If BECMG trend has 1 or more weather phenomena, generate and add BECMG trend weather phenomena to output.
                if (_metarProcessor.metar.metarBECMG.Phenomena.Count > 0) output += listToOutput(_metarProcessor.metar.metarBECMG.Phenomena);
                #endregion

                #region BECMG SKC
                //If BECMG trend has SKC, add SKC to output. 
                if (_metarProcessor.metar.metarBECMG.SKC)
                {
                    atisSamples.Add("skc");
                    output += " SKY CLEAR";
                }
                #endregion

                #region BECMG NSW
                //If BECMG trend has NSW, add NSW to output. 
                if (_metarProcessor.metar.metarBECMG.NSW)
                {
                    atisSamples.Add("nsw");
                    output += " NO SIGNIFICANT WEATHER";
                }
                #endregion

                #region BECMG CLOUDS
                //If BECMG trend has 1 or more weather clouds, generate and add BECMG weather clouds to output. 
                if (_metarProcessor.metar.metarBECMG.Clouds.Count > 0) output += listToOutput(_metarProcessor.metar.metarBECMG.Clouds);
                #endregion

                #region BECMG VERTICAL VISIBILITY
                //If BECMG trend has a vertical visibility greater than 0, add BECMG trend vertical visibility to output.
                if (_metarProcessor.metar.metarBECMG.VerticalVisibility > 0)
                {
                    atisSamples.Add("vv");
                    addIndividualDigitsToATISSamples(_metarProcessor.metar.metarBECMG.VerticalVisibility.ToString());
                    atisSamples.Add("hunderd");
                    atisSamples.Add("meters");

                    output += " VERTICAL VISIBILITY" + _metarProcessor.metar.metarBECMG.VerticalVisibility + " HUNDERD METERS";
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
            atisSamples.Add("end");
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

            foreach (var sample in atisSamples)
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
                    atisSamples.Add("left");
                    return "L";

                case "C":
                    atisSamples.Add("center");
                    return "C";

                case "R":
                    atisSamples.Add("right");
                    return "R";
            }

            return String.Empty;
        }

        /// <summary>
        /// Generate output from List<T>
        /// </summary>
        /// <typeparam name="T">List type</typeparam>
        /// <param name="input">List<T></param>
        /// <returns>String output</returns>
        private String listToOutput<T>(List<T> input)
        {
            String output = String.Empty;

            #region MetarPhenomena
            //If list is a MetarPhenomena list.
            if (input is List<MetarPhenomena>)
            {
                foreach (MetarPhenomena metarPhenomena in input as List<MetarPhenomena>)
                {
                    //If phenomena has intensity.
                    if (metarPhenomena.hasIntensity)
                    {
                        atisSamples.Add("-");
                        output += " LIGHT";
                    }

                    //If phenomena is 4 character phenomena (BCFG | MIFG | SHRA | VCSH | VCTS).
                    if (metarPhenomena.phenomena.Equals("BCFG") || metarPhenomena.phenomena.Equals("MIFG") || metarPhenomena.phenomena.Equals("SHRA") || metarPhenomena.phenomena.Equals("VCSH") || metarPhenomena.phenomena.Equals("VCTS"))
                    {
                        switch (metarPhenomena.phenomena)
                        {
                            case "BCFG":
                                atisSamples.Add("bcfg");
                                output += " PATCHES OF FOG";
                                break;

                            case "MIFG":
                                atisSamples.Add("mifg");
                                output += " SHALLOW FOG";
                                break;

                            case "SHRA":
                                atisSamples.Add("shra");
                                output += " SHOWERS OF RAIN";
                                break;

                            case "VCSH":
                                atisSamples.Add("vcsh");
                                output += " SHOWERS IN VICINITY";
                                break;

                            case "VCTS":
                                atisSamples.Add("vcts");
                                output += " THUNDERSTORMS IN VICINITY";
                                break;
                        }
                    }
                    //If phenomena is multi-phenomena (count > 2).
                    else if (metarPhenomena.phenomena.Count() > 2)
                    {
                        int length = metarPhenomena.phenomena.Length;

                        if ((length % 2) == 0)
                        {
                            int index = 0;

                            while (index != length)
                            {
                                if (length - index != 2)
                                {
                                    output += metarPhenomena.phenomena.Substring(index, 2).PhenomenaToFullSpelling();
                                    atisSamples.Add(metarPhenomena.phenomena.Substring(index, 2));
                                }
                                else
                                {
                                    output += metarPhenomena.phenomena.Substring(index).PhenomenaToFullSpelling();
                                    atisSamples.Add(metarPhenomena.phenomena.Substring(index));
                                }

                                index = index + 2;
                            }
                        }
                    }
                    //If phenomena is 2 char phenomena.
                    else
                    {
                        output += metarPhenomena.phenomena.PhenomenaToFullSpelling();
                        atisSamples.Add(metarPhenomena.phenomena);
                    }

                    //If loop phenomena is not the last phenomena of the list, add [and].
                    if (metarPhenomena != (MetarPhenomena)Convert.ChangeType(input.Last(), typeof(MetarPhenomena)))
                    {
                        atisSamples.Add("and");
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
                    if (metarCloud.altitude == 0)
                    {
                        addIndividualDigitsToATISSamples(metarCloud.altitude.ToString());

                        output += " " + metarCloud.altitude;
                    }

                    //If cloud altitude is round ten-thousand (e.g. 10000 (100), 20000 (200), 30000 (300)).
                    else if (metarCloud.altitude % 100 == 0)
                    {
                        addIndividualDigitsToATISSamples(Math.Floor(Convert.ToDouble(metarCloud.altitude / 100)).ToString() + "0");
                        atisSamples.Add("thousand");

                        output += " " + Math.Floor(Convert.ToDouble(metarCloud.altitude / 100)).ToString() + "0" + " THOUSAND";
                    }

                    else
                    {
                        //If cloud altitude is greater than a ten-thousand (e.g. 12000 (120), 23500 (235), 45000 (450)).
                        if (metarCloud.altitude / 100 > 0)
                        {
                            addIndividualDigitsToATISSamples(Math.Floor(Convert.ToDouble(metarCloud.altitude / 100)).ToString());

                            output += " " + Math.Floor(Convert.ToDouble(metarCloud.altitude / 100)).ToString();

                            //If cloud altitude has a ten-thousand and hundred value (e.g. 10200 (102), 20800 (208), 40700 (407)).
                            if (metarCloud.altitude.ToString().Substring(1, 1).Equals("0"))
                            {
                                atisSamples.Add("0");
                                atisSamples.Add("thousand");
                                output += " 0 THOUSAND";
                            }
                        }

                        //If cloud altitude has a thousand (e.g. 2000 (020), 4000 (040), 5000 (050)).
                        if ((metarCloud.altitude / 10) % 10 > 0)
                        {
                            addIndividualDigitsToATISSamples(Math.Floor(Convert.ToDouble((metarCloud.altitude / 10) % 10)).ToString());
                            atisSamples.Add("thousand");

                            output += " " + Math.Floor(Convert.ToDouble((metarCloud.altitude / 10) % 10)) + " THOUSAND";
                        }

                        //If cloud altitude has a hundred (e.g. 200 (002), 400 (004), 500 (005)).
                        if (metarCloud.altitude % 10 > 0)
                        {
                            addIndividualDigitsToATISSamples(Convert.ToString(metarCloud.altitude % 10));
                            atisSamples.Add("hundred");

                            output += " " + metarCloud.altitude % 10 + " HUNDRED";
                        }
                    }

                    atisSamples.Add("ft");
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
        private String runwayToOutput(String runway, ComboBox runwayComboBox)
        {
            String output = runway;
            output += " ";

            //Split runway digit identifier from runway identifier.
            String[] splitArray = Regex.Split(runwayComboBox.SelectedItem.ToString().Substring(0, 2), @"([0-9])");

            foreach (String digit in splitArray)
            {
                if (!string.IsNullOrEmpty(digit))
                    atisSamples.Add(digit);
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
        private String operationalReportToOutput()
        {
            atisSamples.Add("opr");
            String output = " OPERATIONAL REPORT";

            #region LOW LEVEL VISIBILITY
            //If visibility is not 0 or less than 1500 meter, add low visibility procedure phrase.
            if (_metarProcessor.metar.Visibility != 0 && _metarProcessor.metar.Visibility < 1500)
            {
                atisSamples.Add("lvp");
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
                    atisSamples.Insert(atisSamples.IndexOf("independent"), "and");
                    output = output.Insert(output.IndexOf("IND"), " AND ");
                }

                else
                {
                    atisSamples.Insert(atisSamples.IndexOf("convapp"), "and");
                    output = output.Insert(output.IndexOf("CON"), " AND");
                }
            }
            #endregion

            if (!output.Equals(" OPERATIONAL REPORT")) return output;
            else
            {
                atisSamples.Remove("opr");
                return "";
            }
        }

        /// <summary>
        /// Generate visibility output.
        /// </summary>
        /// <param name="input">Integer</param>
        /// <returns>String output</returns>
        private String visibilityToOutput(int input)
        {
            atisSamples.Add("vis");
            String output = " VISIBILITY";

            //If visibility is lower than 800 meters (less than 800 meters phrase).
            if (input < 800)
            {
                atisSamples.Add("<");
                addIndividualDigitsToATISSamples("800");
                atisSamples.Add("meters");

                output += " LESS THAN 8 HUNDRED METERS";
            }
            //If visibility is lower than 1000 meters (add hundred).
            else if (input < 1000)
            {
                addIndividualDigitsToATISSamples(Convert.ToString(input / 100));
                atisSamples.Add("hundred");
                atisSamples.Add("meters");

                output += " " + Convert.ToString(input / 100) + " HUNDRED METERS";
            }
            //If visibility is lower than 5000 meters and visibility is not a thousand number.
            else if (input < 5000 && (input % 1000) != 0)
            {
                addIndividualDigitsToATISSamples(Convert.ToString(input / 1000));
                atisSamples.Add("thousand");
                addIndividualDigitsToATISSamples(Convert.ToString((input % 1000) / 100));
                atisSamples.Add("hundred");
                atisSamples.Add("meters");

                output += " " + Convert.ToString(input / 1000) + " THOUSAND " + Convert.ToString((input % 1000) / 100) + " HUNDRED METERS";
            }
            //If visibility is >= 9999 (10 km phrase).
            else if (input >= 9999)
            {
                addIndividualDigitsToATISSamples("10");
                atisSamples.Add("km");

                output += " 10 KILOMETERS";
            }
            //If visibility is thousand.
            else
            {
                addIndividualDigitsToATISSamples(Convert.ToString(input / 1000));
                atisSamples.Add("km");

                output += " " + Convert.ToString(input / 1000) + " KILOMETERS";
            }

            return output;
        }

        /// <summary>
        /// Method to generate route output.
        /// </summary>
        /// <returns>String containing the runway output of the selected airport tab.</returns>
        private String generateRunwayOutput()
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
            atisSamples.Add(sample);
        }
    }
}
