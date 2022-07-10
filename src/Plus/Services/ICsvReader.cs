﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.CadPlus.Plus.Data;

namespace Xarial.CadPlus.Plus.Services
{
    public interface ICsvReader
    {
        CsvRow[] Read(TextReader reader, char separator = ',');
    }
}
