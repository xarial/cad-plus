using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xarial.XTools.Xport.Reflection
{
    internal static class EnumHelper
    {
        public static bool TryGetAttribute<TAtt>(this Enum enumer, Action<TAtt> attProc)
            where TAtt : Attribute
        {
            var enumType = enumer.GetType();
            var enumField = enumType.GetMember(enumer.ToString()).FirstOrDefault();

            var atts = enumField?.GetCustomAttributes(typeof(TAtt), false);

            if (atts != null && atts.Any())
            {
                var att = atts.First() as TAtt;
                attProc.Invoke(att);
                return true;
            }
            else
            {
                return false;
            }
        }

        //TODO: move to the framework
        internal static Enum[] GetFlags(Type enumType)
        {
            var flags = new List<Enum>();

            var flag = 0x1;

            foreach (Enum value in Enum.GetValues(enumType))
            {
                var bits = Convert.ToInt32(value);

                if (bits != 0)
                {
                    while (flag < bits)
                    {
                        flag <<= 1;
                    }
                    if (flag == bits)
                    {
                        flags.Add(value);
                    }
                }
            }

            return flags.ToArray();
        }
    }
}
