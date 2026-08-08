using System;
using System.Runtime.InteropServices;
using static LightningDB.Native.Lmdb;

namespace LightningDB.Native;

/// <summary>
/// Owns the native resources behind an environment's encryption/checksum configuration:
/// the callback delegates (which must outlive the environment because native code holds
/// their function pointers) and an unmanaged copy of the encryption key (which LMDB reads
/// on every page operation).
/// </summary>
internal sealed unsafe class PageCallbackKeepAlive : IDisposable
{
    private readonly LightningCipher? _cipher;
    private readonly LightningChecksum? _checksum;
    private readonly EncryptFunction? _encryptFunction;
    private readonly ChecksumFunction? _checksumFunction;
    private GCHandle _encryptHandle;
    private GCHandle _checksumHandle;
    private nint _keyPtr;
    private readonly int _keyLength;

    public PageCallbackKeepAlive(EncryptionConfiguration? encryption, LightningChecksum? checksum)
    {
        _checksum = checksum;
        if (checksum != null)
        {
            _checksumFunction = OnChecksum;
            _checksumHandle = GCHandle.Alloc(_checksumFunction);
        }

        if (encryption == null)
            return;
        _cipher = encryption.Cipher;
        _encryptFunction = OnEncrypt;
        _encryptHandle = GCHandle.Alloc(_encryptFunction);
        _keyLength = encryption.Key.Length;
        _keyPtr = Marshal.AllocHGlobal(_keyLength);
        Marshal.Copy(encryption.Key, 0, _keyPtr, _keyLength);
    }

    public void Install(nint envHandle)
    {
        if (_encryptFunction != null)
        {
            var key = new MDBValue(_keyLength, (byte*)_keyPtr);
            mdb_env_set_encrypt(envHandle, _encryptFunction, ref key, (uint)_cipher!.AuthDataSize).ThrowOnError();
        }
        if (_checksumFunction != null)
        {
            mdb_env_set_checksum(envHandle, _checksumFunction, (uint)_checksum!.Size).ThrowOnError();
        }
    }

    private int OnEncrypt(MDBValue* src, MDBValue* dst, MDBValue* key, int encdec)
    {
        try
        {
            var ok = encdec != 0
                ? _cipher!.Encrypt(src->AsSpan(), dst->AsWritableSpan(),
                    key[0].AsSpan(), key[1].AsSpan(), key[2].AsWritableSpan())
                : _cipher!.Decrypt(src->AsSpan(), dst->AsWritableSpan(),
                    key[0].AsSpan(), key[1].AsSpan(), key[2].AsSpan());
            return ok ? 0 : -1;
        }
        catch
        {
            //exceptions must never cross the native boundary
            return -1;
        }
    }

    private void OnChecksum(MDBValue* src, MDBValue* dst, MDBValue* key)
    {
        try
        {
            _checksum!.Compute(src->AsSpan(), dst->AsWritableSpan(),
                key == null ? default : key->AsSpan());
        }
        catch
        {
            //exceptions must never cross the native boundary; a wrong checksum
            //surfaces as MDBResultCode.BadChecksum on the next read
        }
    }

    public void Dispose()
    {
        if (_keyPtr != 0)
        {
            new Span<byte>((void*)_keyPtr, _keyLength).Clear();
            Marshal.FreeHGlobal(_keyPtr);
            _keyPtr = 0;
        }
        if (_encryptHandle.IsAllocated)
            _encryptHandle.Free();
        if (_checksumHandle.IsAllocated)
            _checksumHandle.Free();
        (_cipher as IDisposable)?.Dispose();
    }
}
