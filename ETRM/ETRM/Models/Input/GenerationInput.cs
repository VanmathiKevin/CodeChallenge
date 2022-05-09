using System;
using System.Collections.Generic;
using System.Text;

namespace ETRM.Models.Input
{
    internal class GenerationInput
    {
        public List<WindGenerator> Wind { get; set; }
        public List<CoalGenerator> Coal { get; set; }
        public List<GasGenerator> Gas { get; set; }
    }
}
