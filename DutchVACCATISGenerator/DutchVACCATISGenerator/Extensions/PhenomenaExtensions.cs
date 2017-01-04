using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using DutchVACCATISGenerator.Resources;

namespace DutchVACCATISGenerator.Extensions
{
    public static class PhenomenaExtensions
    {
        public static string PhenomenaToFullSpelling(this string cloudType)
        {
            return Phenomena.ResourceManager.GetString(cloudType);
        }
    }
}
