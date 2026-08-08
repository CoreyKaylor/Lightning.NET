using System.Runtime.InteropServices;

namespace LightningDB.Native;

/// <summary>
/// MDB_enc_func. A callback used to encrypt/decrypt pages in the environment.
/// Encrypt or decrypt the data in src and store the result in dst using the provided key.
/// The result must be the same number of bytes as the input.
/// </summary>
/// <param name="src">The input data to be transformed</param>
/// <param name="dst">Storage for the result</param>
/// <param name="key">A pointer to an array of THREE MDB_vals: key[0] is the encryption key,
/// key[1] is the initialization vector, and key[2] is the authentication data (written on
/// encrypt, verified on decrypt), if any</param>
/// <param name="encdec">1 to encrypt, 0 to decrypt</param>
/// <returns>Zero on success, non-zero on failure</returns>
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public unsafe delegate int EncryptFunction(MDBValue* src, MDBValue* dst, MDBValue* key, int encdec);

/// <summary>
/// MDB_sum_func. A callback used to checksum pages in the environment.
/// Compute the checksum of the data in src and store the result in dst.
/// </summary>
/// <param name="src">The input data to be transformed</param>
/// <param name="dst">Storage for the result</param>
/// <param name="key">The encryption key if encryption is also configured, otherwise null</param>
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public unsafe delegate void ChecksumFunction(MDBValue* src, MDBValue* dst, MDBValue* key);
