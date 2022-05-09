using ETRM.Models.Input;
using ETRM.Models.Output;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using Microsoft.Extensions.Configuration;

namespace ETRM.Controller
{
    internal class XmlProcessor : IXmlProcessor
    {
        private static IConfigurationRoot Configuration { get; set; }
        static decimal ValueFactor;
        static decimal EmissionFactor;
        static GenerationOutput generationOutput = new GenerationOutput();
        static GenerationInput input = new GenerationInput();
        static ReferenceData data = new ReferenceData();

        //Extraction of input data from XML can alternatively done by XMLSerializer.
        public void ExtractFromXml(FileSystemEventArgs e,string filePath)
        {
            try
            {
                XDocument generatorData = XDocument.Load(filePath);

                var coalGenerators = generatorData.Descendants("CoalGenerator")
                    .Select(x => new CoalGenerator()
                    {
                        Name = x.Element("Name").Value,
                        Days = x.Descendants("Day")
                                .Select(x => new DayInput()
                                {
                                    Date = x.Element("Date").Value,
                                    Energy = Convert.ToDecimal(x.Element("Energy").Value),
                                    Price = Convert.ToDecimal(x.Element("Price").Value),

                                }).ToList(),
                        EmissionsRating = Convert.ToDecimal(x.Element("EmissionsRating").Value),
                        TotalHeatInput = Convert.ToDecimal(x.Element("TotalHeatInput").Value),
                        ActualNetGeneration = Convert.ToDecimal(x.Element("TotalHeatInput").Value)
                    }).ToList();
                var gasGenerators = generatorData.Descendants("GasGenerator")
                    .Select(x => new GasGenerator()
                    {
                        Name = x.Element("Name").Value,
                        Days = x.Descendants("Day")
                                .Select(x => new DayInput()
                                {
                                    Date = x.Element("Date").Value,
                                    Energy = Convert.ToDecimal(x.Element("Energy").Value),
                                    Price = Convert.ToDecimal(x.Element("Price").Value),

                                }).ToList(),
                        EmissionsRating = Convert.ToDecimal(x.Element("EmissionsRating").Value)
                    }).ToList();
                var windGenerators = generatorData.Descendants("WindGenerator")
                    .Select(x => new WindGenerator()
                    {
                        Name = x.Element("Name").Value,
                        Days = x.Descendants("Day")
                                .Select(x => new DayInput()
                                {
                                    Date = x.Element("Date").Value,
                                    Energy = Convert.ToDecimal(x.Element("Energy").Value),
                                    Price = Convert.ToDecimal(x.Element("Price").Value),

                                }).ToList(),
                        Location = x.Element("Location").Value.ToString()
                    }).ToList();

                input = new GenerationInput()
                {
                    Wind = windGenerators,
                    Coal = coalGenerators,
                    Gas = gasGenerators
                };

                //Log here to capture successful extraction of data from input XML

                ProcessXml(input, data);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public void ProcessXml(GenerationInput generationInput,ReferenceData referenceData)
        {
            try
            {
                IConfigurationBuilder configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json");
                Configuration = configuration.Build();

                string referenceFilePath = Configuration.GetSection("XML:ReferenceFilePath").Value.ToString();

                var data = referenceData.ExtractReferenceData(referenceFilePath);
                foreach (var generator in generationInput.Wind)
                {
                    switch (generator.Location)
                    {
                        case "Offshore":
                            ValueFactor = data.ValueFactor.Low;
                            break;
                        case "Onshore":
                            ValueFactor = data.ValueFactor.High;
                            break;
                        default:
                            Console.WriteLine("Location invalid");
                            break;
                    }
                    decimal dailyGenerationValue = CalculateDailyGenerationValue(generator);

                    PopulateDailyGenerationValueToGenerationOutput(generationOutput, generator, dailyGenerationValue);
                }

                foreach (var generator in generationInput.Coal)
                {
                    ValueFactor = data.ValueFactor.Medium;
                    decimal dailyGenerationValue = CalculateDailyGenerationValue(generator);
                    PopulateDailyGenerationValueToGenerationOutput(generationOutput, generator, dailyGenerationValue);// Total heat generated is added. 

                    EmissionFactor = data.EmissionFactor.High;
                    decimal emissionRating = generator.EmissionsRating;
                    PopulateDailyEmissionToGenerationOutput(generator, emissionRating);

                    decimal actualHeatRate = generator.TotalHeatInput / generator.ActualNetGeneration;
                    PopulateActualHeatRateToGenerationOutput(generator, actualHeatRate);
                }

                foreach (var generator in generationInput.Gas)
                {
                    ValueFactor = data.ValueFactor.Medium;
                    decimal dailyGenerationValue = CalculateDailyGenerationValue(generator);
                    PopulateDailyGenerationValueToGenerationOutput(generationOutput, generator, dailyGenerationValue);

                    EmissionFactor = data.EmissionFactor.Medium;
                    decimal emissionRating = generator.EmissionsRating;
                    PopulateDailyEmissionToGenerationOutput(generator, emissionRating);

                }
                //Log here to capture successful completion of computation on the generated data
                ProcessWriteToXml();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        // Write as XML can alternatively done by LinqToXML
        public void ProcessWriteToXml()
        {
            try
            {
                IConfigurationBuilder configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json");
                Configuration = configuration.Build();

                string resultXml = WriteToXml(generationOutput, typeof(GenerationOutput));
                string outputFilePath = Configuration.GetSection("XML:OutputFilePath").Value.ToString();
                File.WriteAllText(outputFilePath, resultXml, Encoding.UTF8);

                //Log here to capture successful completion of XML in output destination.
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public string WriteToXml(object Obj, Type ObjType)
        {
            try
            {

                //Log here to capture start of write operation

                XmlSerializer ser = new XmlSerializer(ObjType);
                MemoryStream memStream = new MemoryStream();
                XmlTextWriter xmlWriter = new XmlTextWriter(memStream, Encoding.UTF8);
                xmlWriter.Namespaces = true;
                xmlWriter.Formatting = Formatting.Indented;
                ser.Serialize(xmlWriter, Obj, GetNamespaces());
                xmlWriter.Close();
                memStream.Close();
                string xml = Encoding.UTF8.GetString(memStream.GetBuffer());
                xml = xml.Substring(xml.IndexOf(Convert.ToChar(60)));
                xml = xml.Substring(0, (xml.LastIndexOf(Convert.ToChar(62)) + 1));
                return xml;
            }
            catch (Exception ex)
            {
                Console.WriteLine (ex.Message);
                return null;
            }
        }

        public static XmlSerializerNamespaces GetNamespaces()
        {
            try
            {

            XmlSerializerNamespaces ns;
            ns = new XmlSerializerNamespaces();
            ns.Add("xsi", "http://www.w3.org/2001/XMLSchema-instance");
            ns.Add("xsd", "http://www.w3.org/2001/XMLSchema");
            return ns;
        }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }

        }

        public decimal CalculateDailyGenerationValue(GeneratorInput generator)
        {
            decimal dailyGenerationValue = 0m;
            foreach (var generation in generator.Days)
            {
                dailyGenerationValue += generation.Energy * generation.Price * ValueFactor;
            }
            return dailyGenerationValue;
        }

        public void PopulateDailyGenerationValueToGenerationOutput(GenerationOutput generationOutput, GeneratorInput generator,decimal dailyGenerationValue)
        {
            Generator generatorOutput = new Generator();
            generatorOutput.Name = generator.Name;
            generatorOutput.Total = Math.Round(dailyGenerationValue, 9);
            generationOutput.Totals.Add(generatorOutput);
        }

        public void PopulateDailyEmissionToGenerationOutput(GeneratorInput generator, decimal emissionRating)
        {
            foreach (var generation in generator.Days)
            {
                Day day = new Day();
                day.Date = generation.Date;
                day.Name = generator.Name;

                day.Emission = Math.Round(generation.Energy * emissionRating * EmissionFactor, 9);
                if (generationOutput.MaxEmissionGenerators.Exists(x => x.Date == day.Date && x.Emission < day.Emission))
                {
                    generationOutput.MaxEmissionGenerators.Remove(generationOutput.MaxEmissionGenerators.Find(x => x.Date == day.Date && x.Emission < day.Emission));
                    generationOutput.MaxEmissionGenerators.Add(day);
                }
                else if (!generationOutput.MaxEmissionGenerators.Exists(x => x.Date == day.Date))
                {
                    generationOutput.MaxEmissionGenerators.Add(day);
                }
            }
        }

        public void PopulateActualHeatRateToGenerationOutput(CoalGenerator generator, decimal actualHeatRate)
        {
            ActualHeatRate heatRate = new ActualHeatRate();
            heatRate.Name = generator.Name;
            heatRate.HeatRate = actualHeatRate;
            generationOutput.ActualHeatRates.Add(heatRate);
        }
    }
}
