/*
This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.If not, see<http://www.gnu.org/licenses/>.
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;

namespace OdinSdk.OdinLib.Security
{
    //-----------------------------------------------------------------------------------------------------------------------------
    //
    //-----------------------------------------------------------------------------------------------------------------------------

    /// <summary>
    ///
    /// </summary>
    [Serializable]
    public class Cryption : IDisposable
    {
        //-----------------------------------------------------------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        ///
        /// </summary>
        private Cryption()
        {
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="crypto_key"></param>
        public Cryption(string crypto_key = "")
        {
            SelectedKey = IsExistKey(crypto_key) == false ? DefaultKey : crypto_key;
            CreateTransform();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="rgbkey"></param>
        /// <param name="vector"></param>
        public Cryption(string rgbkey, string vector)
        {
            SelectedKey = "";
            CreateTransform(rgbkey, vector);
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        ///
        /// </summary>
        public const string DefaultKey = "V1006151";

        private static readonly string[,] CryptoString =
        {
            {"V0503201", "vszQeBFqA9tojdJlfOLAalPLdi+pWZRFk42kG0KpC70=", "mxfQcjp6cJBA5yRnl+C1DA=="},
            {"V0510141", "DXzT3SeaKHq1LRxExKQfJvzhUxJ112NAj4zQoMJ5zek=", "gVF1AAo0QubIX57bowDcDA=="},
            {"V0511071", "Mxk2ACWrwz+UWfUkiw8cPkteHYo7TphzIgSOni4cBQE=", "QTdEP2iDrlOyvp6rdA9kFg=="},
            {"V0708091", "q8ln1+RkYfJbqQ6s6S8W0wGsDRhE2TRsiCCHybFua9A=", "gu+nT4ANRARXz+blqvHi4w=="},
            {"V1006151", "Dr/uVjYLK1hkoD1YNIP+NtuXKfIpzXxKvu2pR6AD/AU=", "y7XlqDlAGHqvULRM5Ng3vw=="},
            {"V1006152", "KdGSxDdsPp5TNGZsdFIOpIa/x0KKTum0lrMs9uXWmPY=", "6EoMJlSr1bbAMfkFJ9K8BQ=="}
        };

        /// <summary>
        /// 선택 되어진 암호화 키
        /// </summary>
        public string SelectedKey
        {
            get;
            private set;
        }

        private static object m_syncRoot = null;

        /// <summary>
        /// 액세스를 동기화하는 데 사용할 수 있는 개체를 가져옵니다.
        /// </summary>
        public object SyncRoot
        {
            get
            {
                if (m_syncRoot == null)
                    m_syncRoot = new object();

                return m_syncRoot;
            }
        }

        private static readonly Lazy<List<string>> m_keyVersions = new Lazy<List<string>>(() =>
        {
            List<string> _keyVersions = new List<string>();

            for (int _x = CryptoString.GetLowerBound(0); _x <= CryptoString.GetUpperBound(0); _x++)
                _keyVersions.Add(CryptoString[_x, 0]);

            return _keyVersions;
        });

        /// <summary>
        ///
        /// </summary>
        public static List<string> KeyVersions
        {
            get
            {
                return m_keyVersions.Value;
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="crypto_key"></param>
        /// <returns></returns>
        public static bool IsExistKey(string crypto_key)
        {
            return KeyVersions.Contains(crypto_key);
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public static string GetRandomKey()
        {
            var _random = new Random();

            var _offset = _random.Next(CryptoString.GetLowerBound(0), CryptoString.GetUpperBound(0));

            return KeyVersions[_offset];
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------------------------------------------------------
        private static readonly Lazy<RijndaelManaged> m_rcManaged = new Lazy<RijndaelManaged>(() => new RijndaelManaged());

        private static RijndaelManaged CryptManager
        {
            get
            {
                return m_rcManaged.Value;
            }
        }

        /// <summary>
        ///
        /// </summary>
        public ICryptoTransform Encryptor
        {
            get;
            set;
        }

        /// <summary>
        ///
        /// </summary>
        public ICryptoTransform Decryptor
        {
            get;
            set;
        }

        private void CreateTransform()
        {
            var _offset = KeyVersions.IndexOf(SelectedKey);
            CreateTransform(CryptoString[_offset, 1], CryptoString[_offset, 2]);
        }

        private void CreateTransform(string rgbkey, string vector)
        {
            lock (SyncRoot)
            {
                byte[] _rgbKey = Convert.FromBase64String(rgbkey);
                byte[] _rgbIV = Convert.FromBase64String(vector);

                Encryptor = CryptManager.CreateEncryptor(_rgbKey, _rgbIV);
                Decryptor = CryptManager.CreateDecryptor(_rgbKey, _rgbIV);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// 평문을 암호화 합니다.
        /// </summary>
        /// <param name="plain_text"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public string PlainToChiper(string plain_text, Encoding encoding = null)
        {
            var _result = "";

            lock (SyncRoot)
            {
                using (MemoryStream _os = new MemoryStream())
                {
                    using (CryptoStream _cs = new CryptoStream(_os, Encryptor, CryptoStreamMode.Write))
                    {
                        encoding = encoding ?? Encoding.UTF8;

                        using (StreamWriter _sw = new StreamWriter(_cs, encoding))
                        {
                            _sw.Write(plain_text);
                        }
                    }

                    _result = Convert.ToBase64String(_os.ToArray());
                }
            }

            return _result;
        }

        /// <summary>
        /// 암호화된 내역을 복호화한다.
        /// </summary>
        /// <param name="chiper_text"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public string ChiperToPlain(string chiper_text, Encoding encoding = null)
        {
            var _result = chiper_text;

            lock (SyncRoot)
            {
                using (MemoryStream _is = new MemoryStream(Convert.FromBase64String(chiper_text)))
                {
                    using (CryptoStream _cs = new CryptoStream(_is, Decryptor, CryptoStreamMode.Read))
                    {
                        encoding = encoding ?? Encoding.UTF8;

                        using (StreamReader _sr = new StreamReader(_cs, encoding))
                        {
                            _result = _sr.ReadToEnd();
                        }
                    }
                }
            }

            return _result;
        }

        /// <summary>
        /// 평문 문자열을 암호화 된 문자열로 변환 합니다.
        /// </summary>
        /// <param name="plain_text"></param>
        /// <param name="is_compress"></param>
        /// <returns></returns>
        public string PlainToChiperText(string plain_text, bool is_compress = false)
        {
            return Convert.ToBase64String(PlainToChiperBytes(plain_text, is_compress));
        }

        /// <summary>
        /// 암호화된 문자열을 내역을 복호화 합니다.
        /// </summary>
        /// <param name="chiper_text"></param>
        /// <param name="is_compress"></param>
        /// <returns></returns>
        public string ChiperTextToPlain(string chiper_text, bool is_compress = false)
        {
            return (string)ChiperBytesToPlain(Convert.FromBase64String(chiper_text), is_compress);
        }

        /// <summary>
        /// 평문을 암호화 합니다.
        /// </summary>
        /// <param name="object_value"></param>
        /// <param name="is_compress"></param>
        /// <returns></returns>
        public byte[] PlainToChiperBytes(object object_value, bool is_compress = false)
        {
            var _result = new byte[0];

            lock (SyncRoot)
            {
                using (MemoryStream _mse = new MemoryStream())
                {
                    using (CryptoStream _cse = new CryptoStream(_mse, Encryptor, CryptoStreamMode.Write))
                    {
                        if (is_compress == true)
                        {
                            using (GZipStream _gse = new GZipStream(_cse, CompressionMode.Compress))
                            {
                                BinaryFormatter _bfe = new BinaryFormatter();
                                _bfe.Serialize(_gse, object_value);
                            }
                        }
                        else
                        {
                            BinaryFormatter _bfe = new BinaryFormatter();
                            _bfe.Serialize(_cse, object_value);
                        }
                    }

                    _result = _mse.ToArray();
                }
            }

            return _result;
        }

        /// <summary>
        /// 암호화된 바이트열을 평문으로 변환 합니다.
        /// </summary>
        /// <param name="chiper_bytes"></param>
        /// <param name="is_compress"></param>
        /// <returns></returns>
        public object ChiperBytesToPlain(byte[] chiper_bytes, bool is_compress = false)
        {
            var _result = (object)null;

            lock (SyncRoot)
            {
                using (MemoryStream _msd = new MemoryStream(chiper_bytes))
                {
                    using (CryptoStream _csd = new CryptoStream(_msd, Decryptor, CryptoStreamMode.Read))
                    {
                        if (is_compress == true)
                        {
                            using (GZipStream _gsd = new GZipStream(_csd, CompressionMode.Decompress))
                            {
                                BinaryFormatter _bfd = new BinaryFormatter();
                                _result = _bfd.Deserialize(_gsd);
                            }
                        }
                        else
                        {
                            BinaryFormatter _bfd = new BinaryFormatter();
                            _result = _bfd.Deserialize(_csd);
                        }
                    }
                }
            }

            return _result;
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Generates a hash for the given plain text value and returns a
        /// byte[]-encoded result. Before the hash is computed, a random salt
        /// is generated and appended to the plain text. This salt is stored at
        /// the end of the hash value, so it can be used later for hash
        /// verification.
        /// </summary>
        /// <param name="plain_text">
        /// Plaintext value to be hashed. The function does not check whether
        /// this parameter is null.
        /// </param>
        /// <param name="hash_algorithm">
        /// Name of the hash algorithm. Allowed values are: "MD5", "SHA1",
        /// "SHA256", "SHA384", and "SHA512" (if any other value is specified
        /// MD5 hashing algorithm will be used). This value is case-insensitive.
        /// </param>
        /// <param name="salt_bytes">
        /// Salt bytes. This parameter can be null, in which case a random salt
        /// value will be generated.
        /// </param>
        /// <returns>
        /// Hash value formatted as a encoded byte[]. for SQL varbinary field.
        /// </returns>
		public byte[] ComputeHash(string plain_text, string hash_algorithm = "SHA256", byte[] salt_bytes = null)
        {
            // If salt is not specified, generate it on the fly.
            if (salt_bytes == null)
            {
                // Define min and max salt sizes.
                var _minSaltSize = 4;
                var _maxSaltSize = 8;

                // Generate a random number for the size of the salt.
                var _random = new Random();
                var _saltSize = _random.Next(_minSaltSize, _maxSaltSize);

                // Allocate a byte array, which will hold the salt.
                salt_bytes = new byte[_saltSize];

                // Initialize a random number generator.
                RNGCryptoServiceProvider _rng = new RNGCryptoServiceProvider();

                // Fill the salt with cryptographically strong byte values.
                _rng.GetNonZeroBytes(salt_bytes);
            }

            // Convert plain text into a byte array.
            byte[] _plainTextBytes = Encoding.UTF8.GetBytes(plain_text);

            // Allocate array, which will hold plain text and salt.
            byte[] _plainTextWithSaltBytes = new byte[_plainTextBytes.Length + salt_bytes.Length];

            // Copy plain text bytes into resulting array.
            for (int i = 0; i < _plainTextBytes.Length; i++)
                _plainTextWithSaltBytes[i] = _plainTextBytes[i];

            // Append salt bytes to the resulting array.
            for (int i = 0; i < salt_bytes.Length; i++)
                _plainTextWithSaltBytes[_plainTextBytes.Length + i] = salt_bytes[i];

            // Because we support multiple hashing algorithms, we must define
            // hash object as a common (abstract) base class. We will specify the
            // actual hashing algorithm class later during object creation.
            HashAlgorithm _hashAlgorithm;

            // Initialize appropriate hashing algorithm class.
            switch (hash_algorithm.ToUpper())
            {
                case "SHA1":
                    _hashAlgorithm = new SHA1Managed();
                    break;

                case "SHA256":
                    _hashAlgorithm = new SHA256Managed();
                    break;

                case "SHA384":
                    _hashAlgorithm = new SHA384Managed();
                    break;

                case "SHA512":
                    _hashAlgorithm = new SHA512Managed();
                    break;

                default:
                    _hashAlgorithm = new MD5CryptoServiceProvider();
                    break;
            }

            // Compute hash value of our plain text with appended salt.
            byte[] _hashBytes = _hashAlgorithm.ComputeHash(_plainTextWithSaltBytes);

            // Create array which will hold hash and original salt bytes.
            byte[] _result = new byte[_hashBytes.Length + salt_bytes.Length];

            // Copy hash bytes into resulting array.
            for (int i = 0; i < _hashBytes.Length; i++)
                _result[i] = _hashBytes[i];

            // Append salt bytes to the result.
            for (int i = 0; i < salt_bytes.Length; i++)
                _result[_hashBytes.Length + i] = salt_bytes[i];

            // Return the result.
            return _result;
        }

        /// <summary>
        /// Compares a hash of the specified plain text value to a given hash
        /// value. Plain text is hashed with the same salt value as the original
        /// hash.
        /// </summary>
        /// <param name="plain_text">
        /// Plain text to be verified against the specified hash. The function
        /// does not check whether this parameter is null.
        /// </param>
        /// <param name="hash_algorithm">
        /// Name of the hash algorithm. Allowed values are: "MD5", "SHA1",
        /// "SHA256", "SHA384", and "SHA512" (if any other value is specified,
        /// MD5 hashing algorithm will be used). This value is case-insensitive.
        /// </param>
        /// <param name="hash_value">
        /// byte[]-encoded hash value produced by ComputeHash function. This value
        /// includes the original salt appended to it.
        /// </param>
        /// <returns>
        /// If computed hash mathes the specified hash the function the return
        /// value is true; otherwise, the function returns false.
        /// </returns>
        public bool VerifyHash(string plain_text, string hash_algorithm = "SHA256", byte[] hash_value = null)
        {
            var _result = false;

            // We must know size of hash (without salt).
            int _hashSizeInBits, _hashSizeInBytes;

            // Size of hash is based on the specified algorithm.
            switch (hash_algorithm.ToUpper())
            {
                case "SHA1":
                    _hashSizeInBits = 160;
                    break;

                case "SHA256":
                    _hashSizeInBits = 256;
                    break;

                case "SHA384":
                    _hashSizeInBits = 384;
                    break;

                case "SHA512":
                    _hashSizeInBits = 512;
                    break;

                default: // Must be MD5
                    _hashSizeInBits = 128;
                    break;
            }

            // Convert size of hash from bits to bytes.
            _hashSizeInBytes = _hashSizeInBits / 8;

            // Make sure that the specified hash value is long enough.
            if (hash_value.Length >= _hashSizeInBytes)
            {
                // Allocate array to hold original salt bytes retrieved from hash.
                byte[] _saltBytes = new byte[hash_value.Length - _hashSizeInBytes];

                // Copy salt from the end of the hash to the new array.
                for (int i = 0; i < _saltBytes.Length; i++)
                    _saltBytes[i] = hash_value[_hashSizeInBytes + i];

                // Compute a new hash string.
                byte[] _expectedHashString = ComputeHash(plain_text, hash_algorithm, _saltBytes);

                // If the computed hash matches the specified hash,
                // the plain text value must be correct.
                var _chkCharCount = 0;

                if (hash_value.Length == _expectedHashString.Length)
                {
                    for (int i = 0; i < hash_value.Length; i++)
                    {
                        if (hash_value[i] != _expectedHashString[i])
                        {
                            _chkCharCount = hash_value[i] - _expectedHashString[i];
                            break;
                        }
                    }
                }
                else
                {
                    _chkCharCount = hash_value.Length - _expectedHashString.Length;
                }

                if (_chkCharCount == 0)
                    _result = true;
            }

            return _result;
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------------------------------------------------------

        #region IDisposable Members

        /// <summary>
        ///
        /// </summary>
        private bool IsDisposed
        {
            get;
            set;
        }

        /// <summary>
        /// Dispose of the backing store before garbage collection.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose of the backing store before garbage collection.
        /// </summary>
        /// <param name="disposing">
        /// <see langword="true"/> if disposing; otherwise, <see langword="false"/>.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (IsDisposed == false)
            {
                if (disposing == true)
                {
                    // Dispose managed resources.
                }

                // Dispose unmanaged resources.

                // Note disposing has been done.
                IsDisposed = true;
            }
        }

        /// <summary>
        /// Dispose of the backing store before garbage collection.
        /// </summary>
        ~Cryption()
        {
            Dispose(false);
        }

        #endregion IDisposable Members
    }
}