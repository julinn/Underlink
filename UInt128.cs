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

        public UInt128(UInt64 Big, UInt64 Small)
        {
            this.Big = Big;
            this.Small = Small;
        }

        public override bool Equals(UInt128 Obj)
        {
 	        return Obj.Big == this.Big &&
                   Obj.Small == this.Small;
        }

        public static bool operator ==(UInt128 Left, UInt128 Right)
        {
            return Left.Equals(Right);
        }

        public static bool operator !=(UInt128 Left, UInt128 Right)
        {
            return !Left.Equals(Right);
        }

        public bool GreaterThan(UInt128 Obj)
        {
            return this.Big > Obj.Big ||
                   (this.Big == Obj.Big && this.Small > Obj.Small);
        }

        public static bool operator >(UInt128 Left, UInt128 Right)
        {
            return Left.GreaterThan(Right);
        }

        public static bool operator >=(UInt128 Left, UInt128 Right)
        {
            return Left.GreaterThan(Right) || Left.Equals(Right);
        }

        public bool LessThan(UInt128 Obj)
        {
            return this.Big < Obj.Big ||
                   (this.Big == Obj.Big && this.Small < Obj.Small);
        }

        public static bool operator <(UInt128 Left, UInt128 Right)
        {
            return Left.LessThan(Right);
        }

        public static bool operator <=(UInt128 Left, UInt128 Right)
        {
            return Left.GreaterThan(Right) || Left.Equals(Right);
        }

        public UInt128 Xor(UInt128 Obj)
        {
            UInt128 ReturnValue;

            ReturnValue.Big = this.Big ^ Obj.Big;
            ReturnValue.Small = this.Small ^ Obj.Small;

            return ReturnValue;
        }

        public static UInt128 operator ^(UInt128 Left, UInt128 Right)
        {
            return Left.Xor(Right);
        }
    }
}
