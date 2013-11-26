using System;

namespace UCIS.NaCl.crypto_secretbox {
	unsafe static class xsalsa20poly1305 {
		const int crypto_secretbox_KEYBYTES = 32;
		const int crypto_secretbox_NONCEBYTES = 24;
		const int crypto_secretbox_ZEROBYTES = 32;
		const int crypto_secretbox_BOXZEROBYTES = 16;

		static public int crypto_secretbox(Byte* c, Byte* m, UInt64 mlen, Byte* n, Byte* k) {
			if (mlen < 32) return -1;
			crypto_stream.xsalsa20.crypto_stream_xor(c, m, mlen, n, k);
			crypto_onetimeauth.poly1305.crypto_onetimeauth(c + 16, c + 32, mlen - 32, c);
			for (int i = 0; i < 16; ++i) c[i] = 0;
			return 0;
		}

		static public int crypto_secretbox_open(Byte* m, Byte* c, UInt64 clen, Byte* n, Byte* k) {
			if (clen < 32) return -1;
			Byte[] subkey = new Byte[32];
			fixed (Byte* subkeyp = subkey) {
				crypto_stream.xsalsa20.crypto_stream(subkeyp, 32, n, k);
				if (crypto_onetimeauth.poly1305.crypto_onetimeauth_verify(c + 16, c + 32, clen - 32, subkeyp) != 0) return -1;
			}
			crypto_stream.xsalsa20.crypto_stream_xor(m, c, clen, n, k);
			for (int i = 0; i < 32; ++i) m[i] = 0;
			return 0;
		}
	}
}