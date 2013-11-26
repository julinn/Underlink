using System;

namespace UCIS.NaCl.crypto_stream {
	static unsafe class xsalsa20 {
		const int crypto_stream_xsalsa20_ref_KEYBYTES = 32;
		const int crypto_stream_xsalsa20_ref_NONCEBYTES = 24;

		//Never written to
		static Byte[] sigma = new Byte[16] {(Byte)'e', (Byte)'x', (Byte)'p', (Byte)'a', //[16] = "expand 32-byte k";
											(Byte)'n', (Byte)'d', (Byte)' ', (Byte)'3',
											(Byte)'2', (Byte)'-', (Byte)'b', (Byte)'y',
											(Byte)'t', (Byte)'e', (Byte)' ', (Byte)'k', }; 

		public static int crypto_stream(Byte* c, int clen, Byte* n, Byte* k) {
			Byte[] subkey = new Byte[32];
			fixed (Byte* subkeyp = subkey, sigmap = sigma) {
				crypto_core.hsalsa20.crypto_core(subkeyp, n, k, sigmap);
				return salsa20.crypto_stream(c, clen, n + 16, subkeyp);
			}
		}

		public static int crypto_stream_xor(Byte* c, Byte* m, UInt64 mlen, Byte* n, Byte* k) {
			Byte[] subkey = new Byte[32];
			fixed (Byte* subkeyp = subkey, sigmap = sigma) {
				crypto_core.hsalsa20.crypto_core(subkeyp, n, k, sigmap);
				return salsa20.crypto_stream_xor(c, m, (int)mlen, n + 16, subkeyp);
			}
		}
	}
}