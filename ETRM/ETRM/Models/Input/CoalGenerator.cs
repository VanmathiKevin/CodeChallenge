using System;
using System.Collections.Generic;
using System.Text;

namespace ETRM.Models.Input
{
    internal class CoalGenerator : GeneratorInput
    {
        public decimal TotalHeatInput { get; set; }
        public decimal ActualNetGeneration { get; set; }
        public decimal EmissionsRating { get; set; }
    }
}
