using System;
using System.Collections.Generic;
using System.Text;

namespace ETRM.Controller
{
    internal interface IReferenceData
    {
        Factor ExtractReferenceData(string filePath);

    }
}
