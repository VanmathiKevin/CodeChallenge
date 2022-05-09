using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ETRM.Controller
{
    internal class FileWatcher
    {
        static IXmlProcessor xmlProcessor;

        public void MonitorInputFile(string filePath)
        {
            string path = Path.GetDirectoryName(filePath);
            string file = Path.GetFileName(filePath);
            try
            {
                FileSystemWatcher fileSystemWatcher = new FileSystemWatcher();
                fileSystemWatcher.Path = path;
                fileSystemWatcher.NotifyFilter = NotifyFilters.Attributes |
                                        NotifyFilters.CreationTime |
                                        NotifyFilters.DirectoryName |
                                        NotifyFilters.FileName |
                                        NotifyFilters.LastAccess |
                                        NotifyFilters.LastWrite |
                                        NotifyFilters.Security |
                                        NotifyFilters.Size;
                fileSystemWatcher.Filter = file;

                fileSystemWatcher.Created += new FileSystemEventHandler(OnChanged);

                fileSystemWatcher.EnableRaisingEvents = true;

                ConsoleKeyInfo ch;

                Console.WriteLine("Press Escape (Esc) key to quit from the application: \n");
                do
                {
                    ch = Console.ReadKey();
                } while (ch.Key != ConsoleKey.Escape);
            }
            catch (IOException e)
            {
                Console.WriteLine("An Exception Occurred :" + e);
            }
            catch (Exception oe)
            {
                Console.WriteLine("An Exception Occurred :" + oe);
            }
        }

        public static void OnChanged(object source, FileSystemEventArgs e)
        {
            //Log here to capture file is placed in input destination

            Console.WriteLine("Input file {0} in the path {1} has been {2}", e.Name, e.FullPath, e.ChangeType);
            xmlProcessor = new XmlProcessor();
            xmlProcessor.ExtractFromXml(e,e.FullPath);
        }
    }
}
