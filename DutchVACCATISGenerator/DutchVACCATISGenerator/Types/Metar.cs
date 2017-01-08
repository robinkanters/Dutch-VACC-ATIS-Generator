using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using DutchVACCATISGenerator.Extensions;

namespace DutchVACCATISGenerator.Types
{
    /// <summary>
    ///     Represents the fields of a METAR.
    /// </summary>
    public class Metar : AbstractMetar
    {
        /// <summary>
        ///     Constructor of METAR without TEMPO or BECMG trend.
        /// </summary>
        /// <param name="inputMetar">METAR</param>
        public Metar(string inputMetar)
        {
            ProcessMetar(inputMetar.Split(' '), MetarType.FULL);
        }

        /// <summary>
        ///     Constructor of METAR which contains TEMPO or BECMG trend.
        /// </summary>
        /// <param name="inputMetar"></param>
        /// <param name="inputTrend">Trend part of the </param>
        /// <param name="trendType">Indicates what MetarType trend type to process.</param>
        public Metar(string inputMetar, string inputTrend, MetarType trendType)
        {
            ProcessMetar(inputMetar.Split(' '), MetarType.FULL);

            switch (trendType)
            {
                case MetarType.BECMG:
                    ProcessMetar(inputTrend.Split(' '), MetarType.BECMG);
                    break;

                case MetarType.TEMPO:
                    ProcessMetar(inputTrend.Split(' '), MetarType.TEMPO);
                    break;
            }
        }

        /// <summary>
        ///     Constructor of METAR which contains TEMPO and BECMG trends.
        /// </summary>
        /// <param name="inputMetar">METAR</param>
        /// <param name="inputTempo">Tempo part of the </param>
        /// <param name="inputBecmg">BECMG part of the </param>
        public Metar(string inputMetar, string inputTempo, string inputBecmg)
        {
            ProcessMetar(inputMetar.Split(' '), MetarType.FULL);
            ProcessMetar(inputTempo.Split(' '), MetarType.TEMPO);
            ProcessMetar(inputBecmg.Split(' '), MetarType.BECMG);
        }

        public string Dewpoint { get; set; }
        public string ICAO { get; set; }
        public AbstractMetar MetarBECMG { get; set; }
        public AbstractMetar MetarTEMPO { get; set; }
        public bool NOSIG { get; set; }
        public bool NSC { get; set; }
        public int QNH { get; set; }
        public bool RVR { get; set; }
        public Dictionary<string, int> RVRValues { get; set; } = new Dictionary<string, int>();
        public string Temperature { get; set; }
        public string Time { get; set; }

        /// <summary>
        ///     Processes the clouds of a METAR.
        /// </summary>
        /// <param name="input">Input string</param>
        /// <param name="metarType">MetarType</param>
        /// <returns>True if input is cloud, else false</returns>
        private bool ProcessClouds(string input, MetarType metarType)
        {
            if (!input.IsCloud())
                return false;

            if (input.Substring(3).Count() > 3)
            {
                if (!input.Substring(6).Contains("/"))
                {
                    switch (metarType)
                    {
                        case MetarType.FULL:
                            Clouds.Add(new MetarCloud(input.Substring(0, 3), Convert.ToInt32(input.Substring(3, 3)),
                                input.Substring(6)));
                            return true;

                        case MetarType.BECMG:
                            MetarBECMG.Clouds.Add(new MetarCloud(input.Substring(0, 3),
                                Convert.ToInt32(input.Substring(3, 3)), input.Substring(6)));
                            return true;

                        case MetarType.TEMPO:
                            MetarTEMPO.Clouds.Add(new MetarCloud(input.Substring(0, 3),
                                Convert.ToInt32(input.Substring(3, 3)), input.Substring(6)));
                            return true;
                    }

                    return false;
                }

                //If METAR does contain /// auto addition in cloud phenomena.
                switch (metarType)
                {
                    case MetarType.FULL:
                        Clouds.Add(new MetarCloud(input.Substring(0, 3), Convert.ToInt32(input.Substring(3, 3))));
                        return true;

                    case MetarType.BECMG:
                        MetarBECMG.Clouds.Add(new MetarCloud(input.Substring(0, 3),
                            Convert.ToInt32(input.Substring(3, 3))));
                        return true;

                    case MetarType.TEMPO:
                        MetarTEMPO.Clouds.Add(new MetarCloud(input.Substring(0, 3),
                            Convert.ToInt32(input.Substring(3, 3))));
                        return true;
                }

                return false;
            }

            switch (metarType)
            {
                case MetarType.FULL:
                    Clouds.Add(new MetarCloud(input.Substring(0, 3), Convert.ToInt32(input.Substring(3))));
                    return true;

                case MetarType.BECMG:
                    MetarBECMG.Clouds.Add(new MetarCloud(input.Substring(0, 3), Convert.ToInt32(input.Substring(3))));
                    return true;

                case MetarType.TEMPO:
                    MetarTEMPO.Clouds.Add(new MetarCloud(input.Substring(0, 3), Convert.ToInt32(input.Substring(3))));
                    return true;
            }

            return false;
        }

        /// <summary>
        ///     Process string METAR to fields in a METAR instance.
        /// </summary>
        /// <param name="input">Input string</param>
        /// <param name="metarType">Indicates what MetarType to process.</param>
        private void ProcessMetar(IReadOnlyList<string> input, MetarType metarType)
        {
            switch (metarType)
            {
                    #region FULL

                case MetarType.FULL:
                    foreach (var s in input)
                    {
                        //ICAO
                        if (s.All(char.IsLetter) && s.IsLength(4) && s.Equals(input[0]))
                        {
                            ICAO = s;
                            continue;
                        }

                        //Time
                        if (s.Last().Equals('Z') && s.Length > 6 && s.Substring(0, 6).IsNumbersOnly())
                        {
                            Time = s;
                            continue;
                        }

                        //Wind
                        if (ProcessWind(s, MetarType.FULL))
                            continue;

                        //Visibility
                        if (ProcessVisibility(s, MetarType.FULL))
                            continue;

                        //RVR
                        if (s.StartsWith("R") && char.IsNumber(s.ElementAt(1)) && char.IsNumber(s.ElementAt(2)) &&
                            !s.Contains("//") && s.Contains("/"))
                        {
                            RVR = true;

                            var split = s.Split(new[] {"/"}, StringSplitOptions.None);

                            var rgx = new Regex("[^0-9 -]");

                            RVRValues.Add(split[0].Substring(1), split[1].Contains('V')
                                ? Convert.ToInt32(rgx.Replace(split[1].Substring(0, split[1].IndexOf('V')), ""))
                                : Convert.ToInt32(rgx.Replace(split[1], "")));

                            continue;
                        }

                        //Phenomena
                        if (ProcessPhenomena(s, MetarType.FULL))
                            continue;

                        //Clouds
                        if (ProcessClouds(s, MetarType.FULL))
                            continue;

                        //Temperature.
                        if (s.Contains("/") &&
                            (s.Length == 5 || s.Length == 6 && s.Contains('M') || s.Length == 7 && s.Contains('M')))
                        {
                            switch (s.Length)
                            {
                                case 5:
                                    Temperature = s.Substring(0, 2);
                                    Dewpoint = s.Substring(3);
                                    break;

                                case 6:
                                    if (s.Substring(0, 3).StartsWith("M"))
                                    {
                                        Temperature = s.Substring(0, 3);
                                        Dewpoint = s.Substring(4);
                                    }
                                    else
                                    {
                                        Temperature = s.Substring(0, 2);
                                        Dewpoint = s.Substring(3);
                                    }
                                    break;

                                case 7:
                                    Temperature = s.Substring(0, 3);
                                    Dewpoint = s.Substring(4);
                                    break;
                            }
                            continue;
                        }

                        //QNHN
                        if (s.StartsWith("Q") && s.Substring(1).IsNumbersOnly())
                        {
                            QNH = Convert.ToInt32(s.Substring(1));
                            continue;
                        }

                        //NOSIG
                        if (s.Equals("NOSIG"))
                            NOSIG = true;
                    }
                    break;

                    #endregion

                    #region BECMG

                case MetarType.BECMG:
                    MetarBECMG = new AbstractMetar();

                    foreach (var s in input)
                    {
                        //Wind
                        if (ProcessWind(s, MetarType.BECMG))
                            continue;

                        //Visibility
                        if (ProcessVisibility(s, MetarType.BECMG))
                            continue;

                        //Phenomena
                        if (ProcessPhenomena(s, MetarType.BECMG))
                            continue;

                        //Clouds
                        ProcessClouds(s, MetarType.BECMG);
                    }

                    break;

                    #endregion

                    #region TEMPO

                case MetarType.TEMPO:
                    MetarTEMPO = new AbstractMetar();

                    foreach (var s in input)
                    {
                        //Wind
                        if (ProcessWind(s, MetarType.TEMPO))
                            continue;

                        //Visibility
                        if (ProcessVisibility(s, MetarType.TEMPO))
                            continue;

                        //Phenomena
                        if (ProcessPhenomena(s, MetarType.TEMPO))
                            continue;

                        //Clouds
                        ProcessClouds(s, MetarType.TEMPO);
                    }
                    break;

                    #endregion
            }
        }

        /// <summary>
        ///     Processes the phenomena of a METAR.
        /// </summary>
        /// <param name="input">Input string</param>
        /// <param name="metarType">MetarType</param>
        /// <returns>True if input is phenomena, else false</returns>
        private bool ProcessPhenomena(string input, MetarType metarType)
        {
            if (!input.IsPhenomena())
                return false;

            if (input.StartsWith("-") || input.StartsWith("-"))
                switch (metarType)
                {
                    case MetarType.FULL:
                        Phenomena.Add(new MetarPhenomena(input.StartsWith("-"), input.StartsWith("+"),
                            input.Substring(1)));
                        return true;

                    case MetarType.BECMG:
                        MetarBECMG.Phenomena.Add(new MetarPhenomena(input.StartsWith("-"), input.StartsWith("+"),
                            input.Substring(1)));
                        return true;

                    case MetarType.TEMPO:
                        MetarTEMPO.Phenomena.Add(new MetarPhenomena(input.StartsWith("-"), input.StartsWith("+"),
                            input.Substring(1)));
                        return true;
                }
            else
                switch (metarType)
                {
                    case MetarType.FULL:
                        Phenomena.Add(new MetarPhenomena(input));
                        return true;

                    case MetarType.BECMG:
                        MetarBECMG.Phenomena.Add(new MetarPhenomena(input));
                        return true;

                    case MetarType.TEMPO:
                        MetarTEMPO.Phenomena.Add(new MetarPhenomena(input));
                        return true;
                }

            return false;
        }

        /// <summary>
        ///     Processes the visibility of a METAR.
        /// </summary>
        /// <param name="input">Input string</param>
        /// <param name="metarType">MetarType</param>
        /// <returns>True if input is visibility, else false</returns>
        private bool ProcessVisibility(string input, MetarType metarType)
        {
            //Visibility
            if (input.IsNumbersOnly() && (input.IsLength(4) || input.IsLength(3)))
            {
                switch (metarType)
                {
                    case MetarType.FULL:
                        Visibility = Convert.ToInt32(input);
                        return true;

                    case MetarType.BECMG:
                        MetarBECMG.Visibility = Convert.ToInt32(input);
                        return true;

                    case MetarType.TEMPO:
                        MetarTEMPO.Visibility = Convert.ToInt32(input);
                        return true;
                }

                return false;
            }

            //CAVOK
            if (input.Equals("CAVOK"))
            {
                switch (metarType)
                {
                    case MetarType.FULL:
                        CAVOK = true;
                        return true;

                    case MetarType.BECMG:
                        MetarBECMG.CAVOK = true;
                        return true;

                    case MetarType.TEMPO:
                        MetarTEMPO.CAVOK = true;
                        return true;
                }

                return false;
            }

            //Sky clear
            if (input.StartsWith("SKC"))
            {
                switch (metarType)
                {
                    case MetarType.FULL:
                        SKC = true;
                        return true;

                    case MetarType.BECMG:
                        MetarBECMG.SKC = true;
                        return true;

                    case MetarType.TEMPO:
                        MetarTEMPO.SKC = true;
                        return true;
                }

                return false;
            }

            //No significant weather
            if (input.StartsWith("NSW"))
            {
                switch (metarType)
                {
                    case MetarType.FULL:
                        NSW = true;
                        return true;

                    case MetarType.BECMG:
                        MetarBECMG.NSW = true;
                        return true;

                    case MetarType.TEMPO:
                        MetarTEMPO.NSW = true;
                        return true;
                }

                return false;
            }

            //Vertical visibility
            if (input.StartsWith("VV") && input.Substring(2).IsNumbersOnly())
            {
                switch (metarType)
                {
                    case MetarType.FULL:
                        VerticalVisibility = Convert.ToInt32(input.Substring(2));
                        return true;

                    case MetarType.BECMG:
                        MetarBECMG.VerticalVisibility = VerticalVisibility = Convert.ToInt32(input.Substring(2));
                        return true;

                    case MetarType.TEMPO:
                        MetarTEMPO.VerticalVisibility = VerticalVisibility = Convert.ToInt32(input.Substring(2));
                        return true;
                }

                return false;
            }

            return false;
        }

        /// <summary>
        ///     Processes the wind of a METAR.
        /// </summary>
        /// <param name="input">Input string</param>
        /// <param name="metarType">MetarType</param>
        /// <returns>True if input is wind, else false</returns>
        private bool ProcessWind(string input, MetarType? metarType)
        {
            if (input.StartsWith("VRB"))
            {
                switch (metarType)
                {
                    case MetarType.FULL:
                        Wind = input.GetMetarWindCalm();
                        return true;

                    case MetarType.BECMG:
                        MetarBECMG.Wind = input.GetMetarWindCalm();
                        return true;

                    case MetarType.TEMPO:
                        MetarTEMPO.Wind = input.GetMetarWindCalm();
                        return true;
                }

                return false;
            }

            if (input.EndsWith("KT"))
            {
                switch (metarType)
                {
                    case MetarType.FULL:
                        Wind = input.GetMetarWind();
                        return true;

                    case MetarType.BECMG:
                        MetarBECMG.Wind = input.GetMetarWind();
                        return true;


                    case MetarType.TEMPO:
                        MetarTEMPO.Wind = input.GetMetarWind();
                        return true;
                }

                return false;
            }

            if (input.IsVariableWind())
            {
                switch (metarType)
                {
                    case MetarType.FULL:
                        Wind = input.GetMetarWindVariable(MetarBECMG.Wind);
                        return true;

                    case MetarType.BECMG:
                        MetarBECMG.Wind = input.GetMetarWindVariable(MetarBECMG.Wind);
                        return true;

                    case MetarType.TEMPO:
                        MetarTEMPO.Wind = input.GetMetarWindVariable(MetarTEMPO.Wind);
                        return true;
                }

                return false;
            }

            return false;
        }
    }
}