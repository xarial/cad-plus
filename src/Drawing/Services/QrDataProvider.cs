using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.CadPlus.Drawing.Data;
using Xarial.CadPlus.Plus.Exceptions;
using Xarial.XCad.Data;
using Xarial.XCad.Documents;

namespace Xarial.CadPlus.Drawing.Services
{
    public class QrDataProvider
    {
        public string GetData(IXDrawing drw, Source_e src, string arg, bool refDoc) 
        {
            IXDocument doc = drw;

            IXConfiguration conf = null;

            if (refDoc) 
            {
                doc = drw.Dependencies.First();
                //drw.Sheets.Active.DrawingViews.First().
                //TODO: find the view refernced configuration
            }

            switch (src) 
            {
                case Source_e.FilePath:
                    return doc.Path;

                case Source_e.PartNumber:
                    if (conf == null) 
                    {
                        throw new UserException("Part number can only be extracted from the configuration of part or assembly");
                    }
                    return conf.PartNumber;

                case Source_e.CustomProperty:
                    IXProperty prp;

                    if (conf != null)
                    {
                        conf.Properties.TryGet(arg, out prp);
                     }

                    doc.Properties.TryGet(arg, out prp);

                    if (prp != null)
                    {
                        return prp.Value?.ToString();
                    }
                    else 
                    {
                        throw new UserException("Specified custom property does not exist");
                    }

                case Source_e.PdmVaultLink:
                    throw new NotImplementedException();

                case Source_e.Web2Link:
                    throw new NotImplementedException();

                default:
                    throw new NotSupportedException();
            }
        }
    }
}
