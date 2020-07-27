using System;

namespace Xarial.CadPlus.XToolbar.Structs
{
    public class MacroStartFunction
    {
        public string ModuleName { get; set; }
        public string SubName { get; set; }

        public static bool operator ==(MacroStartFunction x, MacroStartFunction y)
        {
            if (ReferenceEquals(x, y))
            {
                return true;
            }

            if (ReferenceEquals(x, null))
            {
                return false;
            }
            if (ReferenceEquals(y, null))
            {
                return false;
            }

            return string.Equals(x.ModuleName, y.ModuleName, StringComparison.CurrentCultureIgnoreCase)
                && string.Equals(x.SubName, y.SubName, StringComparison.CurrentCultureIgnoreCase);
        }

        public static bool operator !=(MacroStartFunction x, MacroStartFunction y)
        {
            return !(x == y);
        }

        public override bool Equals(object obj)
        {
            if (obj is MacroStartFunction)
            {
                return this == obj as MacroStartFunction;
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return 0;
        }

        public override string ToString()
        {
            if (!string.IsNullOrEmpty(ModuleName))
            {
                return $"{ModuleName}.{SubName}";
            }
            else
            {
                return SubName;
            }
        }
    }
}