using System;

namespace UCIS.NaCl {
	class randombytes {
		static Random rnd = new Random();
		public static void generate(Byte[] x) {
			rnd.NextBytes(x);
		}
	}
}