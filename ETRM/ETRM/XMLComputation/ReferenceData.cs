using ETRM.Models.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace ETRM.Controller
{
    internal class ReferenceData
    {
        public Factor ExtractReferenceData(string filePath)
        {
            try
            {
                XDocument generatorData = XDocument.Load(filePath);
                var valueFactor = generatorData.Descendants("ValueFactor")
                    .Select(x => new ValueFactor()
                    {
                        High = Convert.ToDecimal(x.Element("High").Value),
                        Medium = Convert.ToDecimal(x.Element("Medium").Value),
                        Low = Convert.ToDecimal(x.Element("Low").Value),
                    }).SingleOrDefault();
                var emissionFactor = generatorData.Descendants("EmissionsFactor")
                    .Select(x => new EmissionFactor()
                    {
                        High = Convert.ToDecimal(x.Element("High").Value),
                        Medium = Convert.ToDecimal(x.Element("Medium").Value),
                        Low = Convert.ToDecimal(x.Element("Low").Value),
                    }).SingleOrDefault();
                return new Factor()
                {
                    ValueFactor = valueFactor,
                    EmissionFactor = emissionFactor
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }
    }
}
