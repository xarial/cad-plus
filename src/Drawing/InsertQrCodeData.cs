using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.CadPlus.Drawing.Data;
using Xarial.XCad.UI.PropertyPage.Attributes;
using Xarial.XCad.UI.PropertyPage.Enums;

namespace Xarial.CadPlus.Drawing
{
    public class InsertQrCodeData
    {
        public SourceData Source { get; set; }
        public LocationData Location { get; set; }

        public InsertQrCodeData() 
        {
            Source = new SourceData();
            Location = new LocationData();
        }
    }

    public class SourceData 
    {
        public Source_e Source { get; set; }
        public bool ReferencedDocument { get; set; }
    }

    public class LocationData 
    {
        [NumberBoxOptions(NumberBoxUnitType_e.Length, 0, 100, 0.01, false, 0.02, 0.001)]
        public double Size { get; set; }

        public Dock_e Dock { get; set; }
        
        [NumberBoxOptions(NumberBoxUnitType_e.Length, -1000, 1000, 0.01, false, 0.02, 0.001)]
        public double OffsetX { get; set; }

        [NumberBoxOptions(NumberBoxUnitType_e.Length, -1000, 1000, 0.01, false, 0.02, 0.001)]
        public double OffsetY { get; set; }

        public LocationData() 
        {
            Dock = Dock_e.BottomRight;
            Size = 0.1;
        }
    }
}
