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
using System.IO;
using System.IO.Compression;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace OdinSdk.OdinLib.Security
{
    /// <summary>
    ///
    /// </summary>
    public class Serialization
    {
        //-----------------------------------------------------------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------------------------------------------------------
        /// <summary></summary>
        private Serialization()
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------------------------------------------------------
        private static readonly Lazy<Serialization> m_lzyHelper = new Lazy<Serialization>(() =>
        {
            return new Serialization();
        });

        /// <summary></summary>
        public static Serialization SNG
        {
            get
            {
                return m_lzyHelper.Value;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// write package
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="kind_of_packing"></param>
        /// <param name="object_value"></param>
        /// <param name="crypto_key"></param>
        /// <returns></returns>
        public XmlPackage WritePackage<T>(T object_value, MKindOfPacking kind_of_packing = MKindOfPacking.Encrypted, string crypto_key = null)
        {
            var _xml_value = ObjectToString<T>(object_value);

            if (kind_of_packing.HasFlag(MKindOfPacking.Encrypted) == true)
            {
                if (Cryption.IsExistKey(crypto_key) == false)
                    crypto_key = Cryption.GetRandomKey();
            }

            if (kind_of_packing != MKindOfPacking.All)
            {
                if (kind_of_packing.HasFlag(MKindOfPacking.Encrypted) == true)
                    _xml_value = CryptHelper.SNG.PlainToChiperText(_xml_value, false, crypto_key);

                if (kind_of_packing.HasFlag(MKindOfPacking.Compressed) == true)
                    _xml_value = CompressText(_xml_value);
            }
            else
            {
                _xml_value = CryptHelper.SNG.PlainToChiperText(_xml_value, true, crypto_key);
            }

            return new XmlPackage()
            {
                Packings = kind_of_packing,
                Value = _xml_value,
                CryptoKey = crypto_key
            };
        }

        /// <summary>
        /// write package async
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="kind_of_packing"></param>
        /// <param name="object_value"></param>
        /// <param name="crypto_key"></param>
        /// <returns></returns>
        public Task<XmlPackage> WritePackageAsync<T>(T object_value, MKindOfPacking kind_of_packing = MKindOfPacking.Encrypted, string crypto_key = null)
        {
            var _task = Task<XmlPackage>.Factory.StartNew(() =>
            {
                return WritePackage<T>(object_value, kind_of_packing, crypto_key);
            });

            return _task;
        }

        /// <summary>
        /// read object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="package"></param>
        /// <returns></returns>
        public T ReadPackage<T>(XmlPackage package)
        {
            var _xml_value = (string)package.Value;
            var _cryptoKey = package.CryptoKey;

            if (package.Packings != MKindOfPacking.All)
            {
                if (package.Packings.HasFlag(MKindOfPacking.Compressed) == true)
                    _xml_value = DecompressText(_xml_value);

                if (package.Packings.HasFlag(MKindOfPacking.Encrypted) == true)
                    _xml_value = CryptHelper.SNG.ChiperTextToPlain(_xml_value, false, _cryptoKey);
            }
            else
            {
                _xml_value = CryptHelper.SNG.ChiperTextToPlain(_xml_value, true, _cryptoKey);
            }

            return StringToObject<T>(_xml_value);
        }

        /// <summary>
        /// read object async
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="package"></param>
        /// <returns></returns>
        public Task<T> ReadPackageAsync<T>(XmlPackage package)
        {
            var _task = Task<T>.Factory.StartNew(() =>
            {
                return ReadPackage<T>(package);
            });

            return _task;
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        ///
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="object_value"></param>
        /// <returns></returns>
        public byte[] ObjectToBytes<T>(T object_value)
        {
            var _result = new byte[0];

            using (MemoryStream _ws = new MemoryStream())
            {
                XmlSerializer _xs = new XmlSerializer(typeof(T));
                _xs.Serialize(_ws, object_value);

                _result = _ws.ToArray();
            }

            return _result;
        }

        /// <summary>
        ///
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="byte_array"></param>
        /// <returns></returns>
        public T BytesToObject<T>(byte[] byte_array)
        {
            T _result;

            using (MemoryStream _ws = new MemoryStream(byte_array))
            {
                XmlSerializer _xs = new XmlSerializer(typeof(T));
                _result = (T)_xs.Deserialize(_ws);
            }

            return _result;
        }

        /// <summary>
        ///
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="object_value"></param>
        /// <returns></returns>
        public string ObjectToString<T>(T object_value)
        {
            var _result = "";

            using (MemoryStream _ws = new MemoryStream())
            {
                XmlSerializer _xs = new XmlSerializer(typeof(T));
                _xs.Serialize(_ws, object_value);

                _ws.Seek(0, SeekOrigin.Begin);

                using (StreamReader _sr = new StreamReader(_ws))
                {
                    _result = _sr.ReadToEnd();
                }
            }

            return _result;
        }

        /// <summary>
        ///
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="xml_string"></param>
        /// <returns></returns>
        public T StringToObject<T>(string xml_string)
        {
            T _result;

            using (MemoryStream _ws = new MemoryStream())
            {
                using (StreamWriter _sw = new StreamWriter(_ws))
                {
                    _sw.Write(xml_string);
                    _sw.Flush();

                    _ws.Seek(0, SeekOrigin.Begin);

                    XmlSerializer _xs = new XmlSerializer(typeof(T));
                    _result = (T)_xs.Deserialize(_ws);
                }
            }

            return _result;
        }

        /// <summary>
        /// class to xml
        /// </summary>
        /// <param name="type"></param>
        /// <param name="object_value"></param>
        /// <returns></returns>
        public byte[] ClassToBytes(Type type, object object_value)
        {
            var _result = new byte[0];

            using (MemoryStream _ws = new MemoryStream())
            {
                XmlSerializer _xs = new XmlSerializer(type);
                _xs.Serialize(_ws, object_value);

                _result = _ws.ToArray();
            }

            return _result;
        }

        /// <summary>
        /// xml to class
        /// </summary>
        /// <param name="type"></param>
        /// <param name="byte_array"></param>
        /// <returns></returns>
        public object BytesToClass(Type type, byte[] byte_array)
        {
            object _result;

            using (MemoryStream _ws = new MemoryStream(byte_array))
            {
                XmlSerializer _xs = new XmlSerializer(type);
                _result = _xs.Deserialize(_ws);
            }

            return _result;
        }

        /// <summary>
        /// class to xml
        /// </summary>
        /// <param name="type"></param>
        /// <param name="object_value"></param>
        /// <returns></returns>
        public string ClassToString(Type type, object object_value)
        {
            var _result = "";

            using (MemoryStream _ws = new MemoryStream())
            {
                XmlSerializer _xs = new XmlSerializer(type);
                _xs.Serialize(_ws, object_value);

                _ws.Seek(0, SeekOrigin.Begin);

                using (StreamReader _sr = new StreamReader(_ws))
                {
                    _result = _sr.ReadToEnd();
                }
            }

            return _result;
        }

        /// <summary>
        /// xml to class
        /// </summary>
        /// <param name="type"></param>
        /// <param name="xml_string"></param>
        /// <returns></returns>
        public object StringToClass(Type type, string xml_string)
        {
            object _result;

            using (MemoryStream _ws = new MemoryStream())
            {
                using (StreamWriter _sw = new StreamWriter(_ws))
                {
                    _sw.Write(xml_string);
                    _sw.Flush();

                    _ws.Seek(0, SeekOrigin.Begin);

                    XmlSerializer _xs = new XmlSerializer(type);
                    _result = _xs.Deserialize(_ws);
                }
            }

            return _result;
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// zip
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public byte[] Compress(object source)
        {
            var _result = new byte[0];

            using (MemoryStream _os = new MemoryStream())
            {
                using (GZipStream _zs = new GZipStream(_os, CompressionMode.Compress))
                {
                    BinaryFormatter _formatter = new BinaryFormatter();
                    _formatter.Serialize(_zs, source);
                }

                _result = _os.ToArray();
            }

            return _result;
        }

        /// <summary>
        ///  unzip
        /// </summary>
        /// <param name="compressed_bytes"></param>
        /// <returns></returns>
        public object Decompress(byte[] compressed_bytes)
        {
            object _result;

            using (MemoryStream _is = new MemoryStream(compressed_bytes))
            {
                using (GZipStream _zs = new GZipStream(_is, CompressionMode.Decompress))
                {
                    BinaryFormatter _formatter = new BinaryFormatter();
                    _result = _formatter.Deserialize(_zs);
                }
            }

            return _result;
        }

        /// <summary>
        /// zip
        /// </summary>
        /// <param name="source"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public string CompressText(string source, Encoding encoding = null)
        {
            var _result = "";

            encoding = encoding ?? System.Text.Encoding.UTF8;
            byte[] _buffer = encoding.GetBytes(source);

            using (MemoryStream _ms = new MemoryStream())
            {
                using (GZipStream _ozip = new GZipStream(_ms, CompressionMode.Compress, true))
                {
                    _ozip.Write(_buffer, 0, _buffer.Length);
                }

                _ms.Position = 0;

                byte[] _compressed = new byte[_ms.Length];
                _ms.Read(_compressed, 0, _compressed.Length);

                byte[] _gzip = new byte[_compressed.Length + 4];
                System.Buffer.BlockCopy(BitConverter.GetBytes(_buffer.Length), 0, _gzip, 0, 4);
                System.Buffer.BlockCopy(_compressed, 0, _gzip, 4, _compressed.Length);

                _result = Convert.ToBase64String(_gzip);
            }

            return _result;
        }

        /// <summary>
        /// unzip
        /// </summary>
        /// <param name="compressed_text"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public string DecompressText(string compressed_text, Encoding encoding = null)
        {
            var _result = "";

            byte[] _compressed = Convert.FromBase64String(compressed_text);

            using (MemoryStream _ms = new MemoryStream())
            {
                var _length = BitConverter.ToInt32(_compressed, 0);
                _ms.Write(_compressed, 4, _compressed.Length - 4);

                byte[] _buffer = new byte[_length];

                _ms.Position = 0;

                using (GZipStream _ozip = new GZipStream(_ms, CompressionMode.Decompress))
                {
                    _ozip.Read(_buffer, 0, _buffer.Length);
                }

                encoding = encoding ?? System.Text.Encoding.UTF8;
                _result = encoding.GetString(_buffer);
            }

            return _result;
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------------------------------------------------------
    }
}