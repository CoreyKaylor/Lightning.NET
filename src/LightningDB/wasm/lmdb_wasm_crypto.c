/* lmdb_wasm_crypto.c - native page cipher/checksum for browser-wasm builds.
 *
 * Cipher: ChaCha20-Poly1305 (RFC 8439, IETF variant, detached MAC, empty AAD)
 * composed from Monocypher primitives. The on-disk format matches upstream
 * LMDB's crypto.c (libsodium crypto_aead_chacha20poly1305_ietf_*_detached):
 * data encrypted from block counter 1, Poly1305 key from block 0, 16-byte
 * detached MAC stored in the page tail (key[2]).
 *
 * Nonce: upstream crypto.c builds LE32(pgno) || txnid from the 64-bit page
 * header. wasm32 has 4-byte pgno_t/txnid_t (xsize == 8), so the 12-byte
 * nonce here is LE32(pgno) || LE32(txnid) || 00 00 00 00. Encrypted files
 * are therefore only readable by builds with the same layout (raw LMDB
 * files are not portable across 32/64-bit builds anyway).
 *
 * Checksum: BLAKE2b-256 (keyed when combined with encryption, matching the
 * upstream cryptoc.c pattern of a keyed checksum alongside the cipher).
 *
 * Installed statically via mdb_env_set_encrypt/mdb_env_set_checksum - the
 * same pattern as upstream mtest_enc.c - because mono-wasm cannot marshal
 * managed delegates as native callbacks. The entry points below are
 * P/Invoked once from LightningDB before mdb_env_open.
 */
#include <errno.h>
#include <string.h>

#define LMDB_WASM_KEYBYTES 32
#define LMDB_WASM_MACBYTES 16
#define LMDB_WASM_SUMBYTES 32

static void lmdb_wasm_nonce(const MDB_val *iv, uint8_t nonce[12])
{
	size_t tsz = iv->mv_size - sizeof(pgno_t);
	memset(nonce, 0, 12);
	memcpy(nonce, iv->mv_data, 4);	/* low 32 bits of pgno */
	if (tsz > 8) tsz = 8;
	memcpy(nonce + 4, (const char *)iv->mv_data + sizeof(pgno_t), tsz);
}

/* RFC 8439 MAC over ciphertext with empty AAD:
 * poly_key = ChaCha20 block 0; input = ct || pad16(ct) || LE64(0) || LE64(len) */
static void lmdb_wasm_mac(uint8_t mac[16], const uint8_t *ct, size_t len,
	const uint8_t *key, const uint8_t nonce[12])
{
	uint8_t polykey[32], tail[16] = {0};
	crypto_poly1305_ctx ctx;
	size_t i;

	crypto_chacha20_ietf(polykey, 0, 32, key, nonce, 0);
	crypto_poly1305_init(&ctx, polykey);
	crypto_poly1305_update(&ctx, ct, len);
	crypto_poly1305_update(&ctx, tail, (16 - (len & 15)) & 15);
	for (i = 0; i < 8; i++)
		tail[8 + i] = (uint8_t)(len >> (8 * i));
	crypto_poly1305_update(&ctx, tail, 16);
	crypto_poly1305_final(&ctx, mac);
	crypto_wipe(polykey, sizeof(polykey));
}

static int lmdb_wasm_encfunc(const MDB_val *src, MDB_val *dst, const MDB_val *key, int encdec)
{
	uint8_t nonce[12], mac[16];
	const uint8_t *k = key[0].mv_data;

	lmdb_wasm_nonce(&key[1], nonce);
	if (encdec) {
		crypto_chacha20_ietf(dst->mv_data, src->mv_data, src->mv_size, k, nonce, 1);
		lmdb_wasm_mac(key[2].mv_data, dst->mv_data, src->mv_size, k, nonce);
	} else if (key[2].mv_size) {
		lmdb_wasm_mac(mac, src->mv_data, src->mv_size, k, nonce);
		if (crypto_verify16(mac, key[2].mv_data))
			return -1;
		crypto_chacha20_ietf(dst->mv_data, src->mv_data, src->mv_size, k, nonce, 1);
	} else {
		/* MAC-less header decrypt during incremental load (see crypto.c) */
		crypto_chacha20_ietf(dst->mv_data, src->mv_data, src->mv_size, k, nonce, 1);
	}
	return 0;
}

static void lmdb_wasm_sumfunc(const MDB_val *src, MDB_val *dst, const MDB_val *key)
{
	if (key && key->mv_size)
		crypto_blake2b_keyed(dst->mv_data, LMDB_WASM_SUMBYTES,
			key->mv_data, key->mv_size, src->mv_data, src->mv_size);
	else
		crypto_blake2b(dst->mv_data, LMDB_WASM_SUMBYTES, src->mv_data, src->mv_size);
}

int lmdb_setup_encryption(MDB_env *env, const uint8_t *key, int key_len, int with_checksum)
{
	MDB_val enckey;
	int rc;

	if (key_len != LMDB_WASM_KEYBYTES)
		return EINVAL;
	enckey.mv_data = (void *)key;
	enckey.mv_size = (size_t)key_len;
	rc = mdb_env_set_encrypt(env, lmdb_wasm_encfunc, &enckey, LMDB_WASM_MACBYTES);
	if (rc)
		return rc;
	return with_checksum ? mdb_env_set_checksum(env, lmdb_wasm_sumfunc, LMDB_WASM_SUMBYTES) : 0;
}

int lmdb_setup_checksum(MDB_env *env)
{
	return mdb_env_set_checksum(env, lmdb_wasm_sumfunc, LMDB_WASM_SUMBYTES);
}
