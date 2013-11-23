using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Underlink
{
    public struct UInt128
    {
        public UInt64 Big;
        public UInt64 Small;
    }

    public static class UInt128Functions
    {
        public static bool Compare(UInt128 One, UInt128 Two)
        {
            return One.Big == Two.Big &&
                   One.Small == Two.Small;
        }

        public static bool Equals(UInt128 One, UInt128 Two)
        {
            return Compare(One, Two);
        }

        public static bool GreaterThan(UInt128 One, UInt128 Two)
        {
            return One.Big > Two.Big ||
                   (One.Big == Two.Big && One.Small > Two.Small);
        }

        public static bool LessThan(UInt128 One, UInt128 Two)
        {
            return One.Big < Two.Big ||
                   (One.Big == Two.Big && One.Small < Two.Small);
        }

        public static UInt128 Xor(UInt128 One, UInt128 Two)
        {
            UInt128 ReturnValue;

            ReturnValue.Big = One.Big ^ Two.Big;
            ReturnValue.Small = One.Small ^ Two.Small;

            return ReturnValue;
        }
    }
}
