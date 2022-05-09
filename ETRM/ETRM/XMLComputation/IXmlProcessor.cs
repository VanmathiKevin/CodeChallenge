using ETRM.Models.Input;
using ETRM.Models.Output;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ETRM.Controller
{
    internal interface IXmlProcessor
    {
        void ExtractFromXml(FileSystemEventArgs e,string filePath);
        void ProcessXml(GenerationInput input, ReferenceData data);
        string WriteToXml(object Obj, Type ObjType);
    }
}
