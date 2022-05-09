using System;
using System.Collections.Generic;
using System.Text;

namespace ETRM.Models.Output
{
    public class GenerationOutput
    {
        public List<Generator> Totals { get; set; }
        public List<Day> MaxEmissionGenerators { get; set; }
        public List<ActualHeatRate> ActualHeatRates { get; set; }

        public GenerationOutput()
        {
            Totals = new List<Generator>();
            MaxEmissionGenerators = new List<Day>();
            ActualHeatRates = new List<ActualHeatRate>();
        }
    }
}
