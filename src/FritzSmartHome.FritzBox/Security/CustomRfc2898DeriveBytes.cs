﻿using System;
using System.Buffers.Binary;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Versioning;
using System.Security.Cryptography;
using System.Text;

namespace FritzSmartHome.FritzBox.Security
{
	// shamelessly stolen from dotnet/core repository to remove the 8 byte restriction on salt size!

	[UnsupportedOSPlatform("browser")]
    public class CustomRfc2898DeriveBytes : DeriveBytes
    {
        private readonly byte[] _password;
        private byte[] _salt;
        private uint _iterations;
        private HMAC _hmac;
        private readonly int _blockSize;

        private byte[] _buffer;
        private uint _block;
        private int _startIndex;
        private int _endIndex;

        /// <summary>
        /// Gets the hash algorithm used for byte derivation.
        /// </summary>
        public HashAlgorithmName HashAlgorithm { get; }

        public CustomRfc2898DeriveBytes(byte[] password, byte[] salt, int iterations)
            : this(password, salt, iterations, HashAlgorithmName.SHA1)
        {
        }

        public CustomRfc2898DeriveBytes(byte[] password, byte[] salt, int iterations, HashAlgorithmName hashAlgorithm)
        {
            if (salt == null)
                throw new ArgumentNullException(nameof(salt));
            if (salt.Length == 0)
                throw new ArgumentException("Salt must be at least one byte.", nameof(salt));
            if (iterations <= 0)
                throw new ArgumentOutOfRangeException(nameof(iterations), "Positive number required.");
            if (password == null)
                throw new NullReferenceException();  // This "should" be ArgumentNullException but for compat, we throw NullReferenceException.

            _salt = new byte[salt.Length + sizeof(uint)];
            salt.AsSpan().CopyTo(_salt);
            _iterations = (uint)iterations;
            _password = password.CloneByteArray();
            HashAlgorithm = hashAlgorithm;
            _hmac = OpenHmac();
            // _blockSize is in bytes, HashSize is in bits.
            _blockSize = _hmac.HashSize >> 3;

            Initialize();
        }

        public CustomRfc2898DeriveBytes(string password, byte[] salt)
             : this(password, salt, 1000)
        {
        }

        public CustomRfc2898DeriveBytes(string password, byte[] salt, int iterations)
            : this(password, salt, iterations, HashAlgorithmName.SHA1)
        {
        }

        public CustomRfc2898DeriveBytes(string password, byte[] salt, int iterations, HashAlgorithmName hashAlgorithm)
            : this(Encoding.UTF8.GetBytes(password), salt, iterations, hashAlgorithm)
        {
        }

        public CustomRfc2898DeriveBytes(string password, int saltSize)
            : this(password, saltSize, 1000)
        {
        }

        public CustomRfc2898DeriveBytes(string password, int saltSize, int iterations)
            : this(password, saltSize, iterations, HashAlgorithmName.SHA1)
        {
        }

        public CustomRfc2898DeriveBytes(string password, int saltSize, int iterations, HashAlgorithmName hashAlgorithm)
        {
            if (saltSize < 0)
                throw new ArgumentOutOfRangeException(nameof(saltSize), "Non-negative number required.");
            if (saltSize <= 0)
                throw new ArgumentException("Salt must be at least one byte.", nameof(saltSize));
            if (iterations <= 0)
                throw new ArgumentOutOfRangeException(nameof(iterations), "Positive number required.");

            _salt = new byte[saltSize + sizeof(uint)];
            RandomNumberGenerator.Fill(_salt.AsSpan(0, saltSize));

            _iterations = (uint)iterations;
            _password = Encoding.UTF8.GetBytes(password);
            HashAlgorithm = hashAlgorithm;
            _hmac = OpenHmac();
            // _blockSize is in bytes, HashSize is in bits.
            _blockSize = _hmac.HashSize >> 3;

            Initialize();
        }

        public int IterationCount
        {
            get
            {
                return (int)_iterations;
            }

            set
            {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException(nameof(value), "Positive number required.");
                _iterations = (uint)value;
                Initialize();
            }
        }

        public byte[] Salt
        {
            get
            {
                return _salt.AsSpan(0, _salt.Length - sizeof(uint)).ToArray();
            }

            set
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(value));
                if (value.Length == 0)
                    throw new ArgumentException("Salt must be at least one byte.");

                _salt = new byte[value.Length + sizeof(uint)];
                value.AsSpan().CopyTo(_salt);
                Initialize();
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_hmac != null)
                {
                    _hmac.Dispose();
                    _hmac = null!;
                }

                if (_buffer != null)
                    Array.Clear(_buffer, 0, _buffer.Length);
                if (_password != null)
                    Array.Clear(_password, 0, _password.Length);
                if (_salt != null)
                    Array.Clear(_salt, 0, _salt.Length);
            }
            base.Dispose(disposing);
        }

        public override byte[] GetBytes(int cb)
        {
            Debug.Assert(_blockSize > 0);

            if (cb <= 0)
                throw new ArgumentOutOfRangeException(nameof(cb), "Positive number required.");
			var password = new byte[cb];

			var offset = 0;
			var size = _endIndex - _startIndex;
            if (size > 0)
            {
                if (cb >= size)
                {
                    Buffer.BlockCopy(_buffer, _startIndex, password, 0, size);
                    _startIndex = _endIndex = 0;
                    offset += size;
                }
                else
                {
                    Buffer.BlockCopy(_buffer, _startIndex, password, 0, cb);
                    _startIndex += cb;
                    return password;
                }
            }

            Debug.Assert(_startIndex == 0 && _endIndex == 0, "Invalid start or end index in the internal buffer.");

            while (offset < cb)
            {
                Func();
				var remainder = cb - offset;
                if (remainder >= _blockSize)
                {
                    Buffer.BlockCopy(_buffer, 0, password, offset, _blockSize);
                    offset += _blockSize;
                }
                else
                {
                    Buffer.BlockCopy(_buffer, 0, password, offset, remainder);
                    _startIndex = remainder;
                    _endIndex = _buffer.Length;
                    return password;
                }
            }
            return password;
        }

        public byte[] CryptDeriveKey(string algname, string alghashname, int keySize, byte[] rgbIV)
        {
            // If this were to be implemented here, CAPI would need to be used (not CNG) because of
            // unfortunate differences between the two. Using CNG would break compatibility. Since this
            // assembly currently doesn't use CAPI it would require non-trivial additions.
            // In addition, if implemented here, only Windows would be supported as it is intended as
            // a thin wrapper over the corresponding native API.
            // Note that this method is implemented in PasswordDeriveBytes (in the Csp assembly) using CAPI.
            throw new PlatformNotSupportedException();
        }

        public override void Reset()
        {
            Initialize();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA5350", Justification = "HMACSHA1 is needed for compat. (https://github.com/dotnet/runtime/issues/17618)")]
        private HMAC OpenHmac()
        {
            Debug.Assert(_password != null);

			var hashAlgorithm = HashAlgorithm;

            if (string.IsNullOrEmpty(hashAlgorithm.Name))
                throw new CryptographicException("The hash algorithm name cannot be null or empty.");

            if (hashAlgorithm == HashAlgorithmName.SHA1)
                return new HMACSHA1(_password);
            if (hashAlgorithm == HashAlgorithmName.SHA256)
                return new HMACSHA256(_password);
            if (hashAlgorithm == HashAlgorithmName.SHA384)
                return new HMACSHA384(_password);
            if (hashAlgorithm == HashAlgorithmName.SHA512)
                return new HMACSHA512(_password);

            throw new CryptographicException($"'{hashAlgorithm.Name}' is not a known hash algorithm.");
        }

        [MemberNotNull(nameof(_buffer))]
        private void Initialize()
        {
            if (_buffer != null)
                Array.Clear(_buffer, 0, _buffer.Length);
            _buffer = new byte[_blockSize];
            _block = 0;
            _startIndex = _endIndex = 0;
        }

        // This function is defined as follows:
        // Func (S, i) = HMAC(S || i) ^ HMAC2(S || i) ^ ... ^ HMAC(iterations) (S || i)
        // where i is the block number.
        private void Func()
        {
            // Block number is going to overflow, exceeding the maximum total possible bytes
            // that can be extracted.
            if (_block == uint.MaxValue)
                throw new CryptographicException("The total number of bytes extracted cannot exceed UInt32.MaxValue * hash length.");

            BinaryPrimitives.WriteUInt32BigEndian(_salt.AsSpan(_salt.Length - sizeof(uint)), _block + 1);
            Debug.Assert(_blockSize == _buffer.Length);

            // The biggest _blockSize we have is from SHA512, which is 64 bytes.
            // Since we have a closed set of supported hash algorithms (OpenHmac())
            // we can know this always fits.
            //
            Span<byte> uiSpan = stackalloc byte[64];
            uiSpan = uiSpan.Slice(0, _blockSize);

            if (!_hmac.TryComputeHash(_salt, uiSpan, out var bytesWritten) || bytesWritten != _blockSize)
            {
                throw new CryptographicException();
            }

            uiSpan.CopyTo(_buffer);

            for (var i = 2; i <= _iterations; i++)
            {
                if (!_hmac.TryComputeHash(uiSpan, uiSpan, out bytesWritten) || bytesWritten != _blockSize)
                {
                    throw new CryptographicException();
                }

                for (var j = _buffer.Length - 1; j >= 0; j--)
                {
                    _buffer[j] ^= uiSpan[j];
                }
            }

            // increment the block count.
            _block++;
        }
    }
}
