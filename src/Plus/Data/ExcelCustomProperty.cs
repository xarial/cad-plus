using System;

namespace Xarial.CadPlus.Plus.Data
{
    [Serializable]
    public class ExcelCustomProperty 
    {
        public string Name { get; }
        public object Value { get; }

        public ExcelCustomProperty(string name, object value)
        {
            Name = name;
            Value = value;
        }
    }
}
