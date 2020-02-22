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
using System.Data;
using System.IO;
using System.IO.Compression;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;

namespace OdinSdk.OdinLib.Data
{
    /// <summary>
    /// Summary description for ZipDataSet.
    /// </summary>
    public class ZipDataSet
    {
        //**********************************************************************************************************//
        ///
        //**********************************************************************************************************//
        private ZipDataSet()
        {
        }

        //**********************************************************************************************************//
        //
        //**********************************************************************************************************//
        private static readonly Lazy<ZipDataSet> m_zipHelper = new Lazy<ZipDataSet>(() =>
        {
            return new ZipDataSet();
        });

        /// <summary></summary>
        public static ZipDataSet SNG
        {
            get
            {
                return m_zipHelper.Value;
            }
        }

        //**********************************************************************************************************//
        //
        //**********************************************************************************************************//

        /// <summary>
        /// 스트림을 Byte 배열로 변환
        /// </summary>
        /// <param name="in_stream"></param>
        /// <returns></returns>
        private byte[] ReadFullStream(Stream in_stream)
        {
            MemoryStream _result = new MemoryStream();

            byte[] _buffer = new byte[32768];

            while (true)
            {
                var _read = in_stream.Read(_buffer, 0, _buffer.Length);
                if (_read <= 0)
                    break;

                _result.Write(_buffer, 0, _read);
            }

            return _result.ToArray();
        }

        //**********************************************************************************************************//
        // DataSet
        //**********************************************************************************************************//

        /// <summary>
        /// DataSet을 Base64 형태의 스트링으로 압축하여 리턴한다.
        /// </summary>
        /// <param name="plain_data_set"></param>
        /// <returns></returns>
        public string CompressDataSet(DataSet plain_data_set)
        {
            var _result = "";

            using (MemoryStream _is = new MemoryStream())
            {
                //1. 데이터셋 Serialize
                plain_data_set.RemotingFormat = SerializationFormat.Binary;

                BinaryFormatter _bform = new BinaryFormatter();
                _bform.Serialize(_is, plain_data_set);

                //2. 데이터 압축
                using (MemoryStream _os = new MemoryStream())
                {
                    using (DeflateStream _zs = new DeflateStream(_os, CompressionMode.Compress))
                    {
                        byte[] _pbuff = _is.ToArray();
                        _zs.Write(_pbuff, 0, _pbuff.Length);
                        _zs.Flush();
                    }

                    //3. 데이터 리턴
                    byte[] _zbuff = _os.ToArray();
                    _result = Convert.ToBase64String(_zbuff, 0, _zbuff.Length);
                }
            }

            return _result;
        }

        /// <summary>
        /// DataSet을 Base64 형태의 스트링으로 압축하여 저장한다.
        /// </summary>
        /// <param name="plain_data_set"></param>
        /// <param name="zip_file"></param>
        public void CompressDataSetToFile(DataSet plain_data_set, string zip_file)
        {
            using (MemoryStream _is = new MemoryStream())
            {
                //1. 데이터셋 Serialize(Serialize()할 때 메모리가 모자라면 에러가 발생함)
                plain_data_set.RemotingFormat = SerializationFormat.Binary;

                BinaryFormatter _bform = new BinaryFormatter();
                _bform.Serialize(_is, plain_data_set);

                //2. 데이터 압축
                using (FileStream _os = new FileStream(zip_file, FileMode.Create))
                {
                    using (DeflateStream _zs = new DeflateStream(_os, CompressionMode.Compress))
                    {
                        byte[] _pbuff = _is.ToArray();
                        _zs.Write(_pbuff, 0, _pbuff.Length);
                        _zs.Flush();
                    }
                }
            }
        }

        /// <summary>
        /// Base64 형태의 스트링을 DataSet으로 변환하여 리턴한다.
        /// </summary>
        /// <param name="zip_data_set"></param>
        /// <returns></returns>
        public DataSet DecompressDataSet(string zip_data_set)
        {
            var _result = new DataSet();

            //1. 압축객체 생성- 압축 풀기(FromBase64String()할 때 메모리가 모자라면 에러가 발생함)
            byte[] _zbuff = Convert.FromBase64String(zip_data_set);

            using (MemoryStream _is = new MemoryStream(_zbuff))
            {
                using (DeflateStream _zs = new DeflateStream(_is, CompressionMode.Decompress, true))
                {
                    byte[] _pbuff = ReadFullStream(_zs);

                    //2. 스트림으로 다시변환
                    using (MemoryStream _os = new MemoryStream(_pbuff))
                    {
                        //3. 데이터셋으로 Deserialize
                        BinaryFormatter _bform = new BinaryFormatter();
                        _result.RemotingFormat = SerializationFormat.Binary;
                        _result = (DataSet)_bform.Deserialize(_os, null);
                    }
                }
            }

            return _result;
        }

        /// <summary>
        /// Base64 형태의 파일을 DataSet으로 변환하여 리턴한다.
        /// </summary>
        /// <param name="zip_file"></param>
        /// <returns></returns>
        public DataSet DecompressDataSetFromFile(string zip_file)
        {
            var _result = new DataSet();

            //1. 압축객체 생성- 압축 풀기
            using (FileStream _is = new FileStream(zip_file, FileMode.Open))
            {
                using (DeflateStream _zs = new DeflateStream(_is, CompressionMode.Decompress, true))
                {
                    byte[] _pbuff = ReadFullStream(_zs);

                    //2. 스트림으로 다시변환
                    using (MemoryStream _os = new MemoryStream(_pbuff))
                    {
                        //3. 데이터셋으로 Deserialize
                        BinaryFormatter _bform = new BinaryFormatter();
                        _result.RemotingFormat = SerializationFormat.Binary;
                        _result = (DataSet)_bform.Deserialize(_os, null);
                    }
                }
            }

            return _result;
        }

        //**********************************************************************************************************//
        // Object
        //**********************************************************************************************************//

        /// <summary>
        /// Object를 Base64 형태의 스트링으로 압축하여 리턴한다.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="object_value"></param>
        /// <returns></returns>
        public string CompressObject(Type type, object object_value)
        {
            var _result = "";

            //1. 데이터셋 Serialize
            using (MemoryStream _is = new MemoryStream())
            {
                XmlSerializer _xs = new XmlSerializer(type);
                _xs.Serialize(_is, object_value);

                //2. 데이터 압축
                using (MemoryStream _os = new MemoryStream())
                {
                    using (DeflateStream _zs = new DeflateStream(_os, CompressionMode.Compress))
                    {
                        byte[] _pbuff = _is.ToArray();
                        _zs.Write(_pbuff, 0, _pbuff.Length);
                        _zs.Flush();
                    }

                    //3. 데이터 리턴
                    byte[] _zbuff = _os.ToArray();
                    _result = Convert.ToBase64String(_zbuff, 0, _zbuff.Length);
                }
            }

            return _result;
        }

        /// <summary>
        /// Object를 Base64 형태의 스트링으로 압축하여 저장한다.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="object_value"></param>
        /// <param name="zip_file"></param>
        public void CompressObjectToFile(Type type, object object_value, string zip_file)
        {
            //1. 데이터셋 Serialize
            using (MemoryStream _is = new MemoryStream())
            {
                XmlSerializer _xs = new XmlSerializer(type);
                _xs.Serialize(_is, object_value);

                //2. 데이터 압축
                using (FileStream _os = new FileStream(zip_file, FileMode.Create))
                {
                    using (DeflateStream _zs = new DeflateStream(_os, CompressionMode.Compress))
                    {
                        byte[] _pbuff = _is.ToArray();
                        _zs.Write(_pbuff, 0, _pbuff.Length);
                        _zs.Flush();
                    }
                }
            }
        }

        /// <summary>
        /// Base64 형태의 스트링을 Object로 변환하여 리턴한다.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="zip_string"></param>
        /// <returns></returns>
        public object DecompressObject(Type type, string zip_string)
        {
            object _result = null;

            //1. 압축객체 생성- 압축 풀기
            byte[] _zbuff = Convert.FromBase64String(zip_string);

            using (MemoryStream _is = new MemoryStream(_zbuff))
            {
                using (DeflateStream _zs = new DeflateStream(_is, CompressionMode.Decompress, true))
                {
                    byte[] _pbuff = ReadFullStream(_zs);

                    //2. 스트림으로 다시변환
                    using (MemoryStream _os = new MemoryStream(_pbuff))
                    {
                        //3. 데이터셋으로 Deserialize
                        XmlSerializer _xs = new XmlSerializer(type);
                        _result = _xs.Deserialize(_os);
                    }
                }
            }

            return _result;
        }

        /// <summary>
        /// Base64 형태로 저장된 파일을 읽어 Object로 변환하여 리턴한다.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="zip_file"></param>
        /// <returns></returns>
        public object DecompressObjectFromFile(Type type, string zip_file)
        {
            object _result = null;

            //1. 압축객체 생성- 압축 풀기
            using (FileStream _is = new FileStream(zip_file, FileMode.Open))
            {
                using (DeflateStream _zs = new DeflateStream(_is, CompressionMode.Decompress, true))
                {
                    byte[] _pbuff = ReadFullStream(_zs);

                    //2. 스트림으로 다시변환
                    using (MemoryStream _os = new MemoryStream(_pbuff))
                    {
                        //3. 데이터셋으로 Deserialize
                        XmlSerializer _xs = new XmlSerializer(type);
                        _result = _xs.Deserialize(_os);
                    }
                }
            }

            return _result;
        }

        //**********************************************************************************************************//
        //
        //**********************************************************************************************************//

        /// <summary>
        ///
        /// </summary>
        /// <param name="source_bytes"></param>
        /// <returns></returns>
        public byte[] CompressBytes(byte[] source_bytes)
        {
            MemoryStream _result = new MemoryStream();

            using (DeflateStream _zs = new DeflateStream(_result, CompressionMode.Compress))
            {
                _zs.Write(source_bytes, 0, source_bytes.Length);
                _zs.Flush();
            }

            return _result.ToArray();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="zip_bytes"></param>
        /// <returns></returns>
        public byte[] DecompressBytes(byte[] zip_bytes)
        {
            using (MemoryStream _result = new MemoryStream())
            {
                using (MemoryStream _is = new MemoryStream(zip_bytes))
                {
                    using (DeflateStream _zs = new DeflateStream(_is, CompressionMode.Decompress, true))
                    {
                        byte[] _pbuff = ReadFullStream(_zs);
                        _result.Write(_pbuff, 0, _pbuff.Length);
                        _result.Flush();
                    }
                }

                return _result.ToArray();
            }
        }

        //**********************************************************************************************************//
        //
        //**********************************************************************************************************//

        /// <summary>
        /// MemoryStream을 Base64 형태의 스트링으로 압축하여 리턴한다.
        /// </summary>
        /// <param name="in_stream"></param>
        /// <returns></returns>
        public string CompressStream(MemoryStream in_stream)
        {
            var _result = "";

            //2. 데이터 압축
            using (MemoryStream _os = new MemoryStream())
            {
                using (DeflateStream _zs = new DeflateStream(_os, CompressionMode.Compress))
                {
                    byte[] _pbuff = in_stream.ToArray();

                    _zs.Write(_pbuff, 0, _pbuff.Length);
                    _zs.Flush();
                }

                //3. 데이터 리턴
                byte[] _zbuff = _os.ToArray();
                _result = Convert.ToBase64String(_zbuff, 0, _zbuff.Length);
            }

            return _result;
        }

        /// <summary>
        /// MemoryStream을 Base64 형태의 스트링으로 압축하여 저장한다.
        /// </summary>
        /// <param name="in_stream"></param>
        /// <param name="zip_file"></param>
        public void CompressStreamToFile(MemoryStream in_stream, string zip_file)
        {
            //2. 데이터 압축
            using (FileStream _os = new FileStream(zip_file, FileMode.Create))
            {
                using (DeflateStream _zs = new DeflateStream(_os, CompressionMode.Compress))
                {
                    byte[] _pbuff = in_stream.ToArray();

                    _zs.Write(_pbuff, 0, _pbuff.Length);
                    _zs.Flush();
                }
            }
        }

        /// <summary>
        /// Base64 형태의 스트링을 MemoryStream로 변환하여 리턴한다.
        /// </summary>
        /// <param name="zip_string"></param>
        /// <returns></returns>
        public MemoryStream DecompressStream(string zip_string)
        {
            MemoryStream _result = new MemoryStream();

            //1. 압축객체 생성- 압축 풀기
            byte[] _zbuff = Convert.FromBase64String(zip_string);

            using (MemoryStream _is = new MemoryStream(_zbuff))
            {
                using (DeflateStream _zs = new DeflateStream(_is, CompressionMode.Decompress, true))
                {
                    byte[] _pbuff = ReadFullStream(_zs);
                    _result.Write(_pbuff, 0, _pbuff.Length);
                }
            }

            return _result;
        }

        /// <summary>
        /// 파일로 저장된 내용을 MemoryStream로 변환하여 리턴한다.
        /// </summary>
        /// <param name="zip_file"></param>
        /// <returns></returns>
        public MemoryStream DecompressStreamFromFile(string zip_file)
        {
            MemoryStream _result = new MemoryStream();

            //1. 압축객체 생성- 압축 풀기
            using (FileStream _is = new FileStream(zip_file, FileMode.Open))
            {
                using (DeflateStream _zs = new DeflateStream(_is, CompressionMode.Decompress, true))
                {
                    byte[] _pbuff = ReadFullStream(_zs);
                    _result.Write(_pbuff, 0, _pbuff.Length);
                }
            }

            return _result;
        }

        //**********************************************************************************************************//
        //
        //**********************************************************************************************************//
    }
}