using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DutchVACCATISGenerator.Logic.Metar
{
    public class CloudLogic
    {
        private readonly MetarLogic _metarLogic;

        public CloudLogic(MetarLogic metarLogic)
        {
            _metarLogic = metarLogic;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cloudType"></param>
        /// <returns></returns>
        private string CloudTypeToFullSpelling(string cloudType)
        {
            switch (cloudType)
            {
                case "FEW":
                    _metarLogic.AddToAtisSamples("few");
                    return " FEW";

                case "BKN":
                    _metarLogic.AddToAtisSamples("bkn");
                    return " BROKEN";

                case "OVC":
                    _metarLogic.AddToAtisSamples("ovc");
                    return " OVERCAST";

                case "SCT":
                    _metarLogic.AddToAtisSamples("sct");
                    return " SCATTERED";
            }

            return string.Empty;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="addition"></param>
        /// <returns></returns>
        private string CloudAdditionToFullSpelling(string addition)
        {
            switch (addition)
            {
                case "CB":
                    _metarLogic.AddToAtisSamples("cb");
                    return " CUMULONIMBUS";

                case "TCU":
                    _metarLogic.AddToAtisSamples("tcu");
                    return " TOWERING CUMULONIMBUS";
            }

            return string.Empty;
        }
    }
}
