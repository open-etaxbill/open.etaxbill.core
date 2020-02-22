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
using System.Security.Cryptography;
using System.Text;

namespace OdinSdk.OdinLib.Caching
{
    /// <summary>
    ///
    /// </summary>
    public class FileCache
    {
        //-----------------------------------------------------------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        ///
        /// </summary>
        /// <param name="file_path"></param>
        public FileCache(string file_path)
        {
            this.m_filepath = file_path;
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------------------------------------------------------
        private string m_filepath = "";

        /// <summary></summary>
        public string FileLocation
        {
            get
            {
                return this.m_filepath;
            }
            set
            {
                this.m_filepath = value;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------------------------------------------------------
        private string MD5ComputeHash(byte[] source)
        {
            return BitConverter.ToString(new MD5CryptoServiceProvider().ComputeHash(source)).Replace("-", "").ToLower();
        }

        private string SHA1ComputeHash(byte[] source)
        {
            return BitConverter.ToString(new SHA1CryptoServiceProvider().ComputeHash(source)).Replace("-", "").ToLower();
        }

        private Guid ConvertLong2Guid(long long_value)
        {
            Guid _result = Guid.Empty;

            byte[] _buffer = BitConverter.GetBytes(long_value);
            {
                Array.Resize<byte>(ref _buffer, _result.ToByteArray().Length);
                _result = new Guid(_buffer);
            }

            return _result;
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------------------------------------------------------
        private readonly long m_waterMark = -123456789L;

        private readonly long m_removMark = -987654321L;
        private readonly long m_errorMark = -246813579L;

        private long GetLongValue(FileStream file_stream, ref long offset)
        {
            long _result = -1;

            byte[] _buffer = System.BitConverter.GetBytes(_result);
            {
                file_stream.Seek(offset, SeekOrigin.Begin);
                file_stream.Read(_buffer, 0, _buffer.Length);

                offset += _buffer.Length;

                _result = System.BitConverter.ToInt64(_buffer, 0);
            }

            return _result;
        }

        private long PutLongValue(FileStream file_stream, long offset, long value)
        {
            long _result = offset;

            byte[] _buffer = System.BitConverter.GetBytes(value);
            {
                file_stream.Seek(offset, SeekOrigin.Begin);
                file_stream.Write(_buffer, 0, _buffer.Length);

                _result += _buffer.Length;
            }

            return _result;
        }

        private Guid GetGuidValue(FileStream file_stream, ref long offset)
        {
            Guid _result = Guid.Empty;

            byte[] _buffer = _result.ToByteArray();
            {
                file_stream.Seek(offset, SeekOrigin.Begin);
                file_stream.Read(_buffer, 0, _buffer.Length);

                offset += _buffer.Length;

                _result = new Guid(_buffer);
            }

            return _result;
        }

        private long PutGuidValue(FileStream file_stream, long offset, Guid guid_value)
        {
            long _result = offset;

            byte[] _buffer = guid_value.ToByteArray();
            {
                file_stream.Seek(offset, SeekOrigin.Begin);
                file_stream.Write(_buffer, 0, _buffer.Length);

                _result += _buffer.Length;
            }

            return _result;
        }

        private byte[] GetBytesSize(FileStream file_stream, ref long offset)
        {
            byte[] _buffer = null;

            long _length = this.GetLongValue(file_stream, ref offset);
            if (_length > 0)
            {
                _buffer = new byte[_length];

                file_stream.Seek(offset, SeekOrigin.Begin);
                file_stream.Read(_buffer, 0, _buffer.Length);

                offset += _buffer.Length;
            }

            return _buffer;
        }

        private long PutBytesSize(FileStream file_stream, long offset, byte[] buffer)
        {
            long _result = -1;

            offset = this.PutLongValue(file_stream, offset, buffer.Length);
            {
                file_stream.Seek(offset, SeekOrigin.Begin);
                file_stream.Write(buffer, 0, buffer.Length);

                offset += buffer.Length;
                _result = offset;
            }

            return _result;
        }

        private long WriteLinkHeader(FileStream file_stream, long front_value, long rear_value)
        {
            long _offset = 0;

            _offset = this.PutLongValue(file_stream, _offset, front_value);
            _offset = this.PutLongValue(file_stream, _offset, rear_value);

            return _offset;
        }

        private bool IsValidHashMark(FileStream file_stream, byte[] message, byte[] old_hash)
        {
            bool _result;

            byte[] _newhash = Encoding.UTF8.GetBytes(this.MD5ComputeHash(message));
            if (this.CompareBytes(_newhash, old_hash) == 0)
                _result = true;
            else
                _result = false;

            return _result;
        }

        private bool IsValidWaterMark(FileStream file_stream, ref long offset, long water_mark)
        {
            var _result = false;

            if (offset < file_stream.Length)
            {
                long _waterMark = this.GetLongValue(file_stream, ref offset);

                if (_waterMark == water_mark)
                    _result = true;
            }

            return _result;
        }

        private int CompareBytes(byte[] buffer1, byte[] buffer2)
        {
            var _result = 0;

            if (buffer1.Length == buffer2.Length)
            {
                for (int i = 0; i < buffer1.Length; i++)
                {
                    if (buffer1[i] != buffer2[i])
                    {
                        _result = buffer1[i] - buffer2[i];
                        break;
                    }
                }
            }
            else
            {
                _result = buffer1.Length - buffer2.Length;
            }

            return _result;
        }

        private long SeekProperBuffer(FileStream file_stream, long message_size)
        {
            long _result = -1;

            long _postion = 0;
            long _frontlnk = this.GetLongValue(file_stream, ref _postion);
            long _rearlink = this.GetLongValue(file_stream, ref _postion);

            while (_frontlnk > -1)
            {
                _postion = _frontlnk;

                //water mark
                if (this.IsValidWaterMark(file_stream, ref _postion, this.m_removMark) == false)
                    break;

                // uniqueue id
                this.GetGuidValue(file_stream, ref _postion);

                // previous link
                this.GetLongValue(file_stream, ref _postion);

                // next link
                long _nxtlink = this.GetLongValue(file_stream, ref _postion);

                // size og hash
                long _hashSize = this.GetLongValue(file_stream, ref _postion);

                // skip hash
                _postion += _hashSize;

                //size of message
                long _msgsize = this.GetLongValue(file_stream, ref _postion);

                // skip message
                _postion += _msgsize;

                if (_msgsize > message_size)
                {
                    _result = _frontlnk;
                    break;
                }

                if (_frontlnk == _rearlink)
                    break;

                _frontlnk = _nxtlink;
            }

            return _result;
        }

        private long AdjustProperBuffer(FileStream file_stream, long offset, long message_size)
        {
            long _result = -1;

            while (true)
            {
                long _prvlink, _nxtlink, _hashsize, _msgsize, _pktsize, _usesize;

                long _position = offset;
                {
                    //water mark
                    if (this.IsValidWaterMark(file_stream, ref _position, this.m_removMark) == false)
                        break;

                    // uniqueue id
                    this.GetGuidValue(file_stream, ref _position);

                    // previous link
                    _prvlink = this.GetLongValue(file_stream, ref _position);

                    // next link
                    _nxtlink = this.GetLongValue(file_stream, ref _position);

                    // size of hash
                    _hashsize = this.GetLongValue(file_stream, ref _position);

                    // skip hash
                    _position += _hashsize;

                    //size of message
                    _msgsize = this.GetLongValue(file_stream, ref _position);

                    // skip message
                    _position += _msgsize;

                    if (_msgsize <= message_size)
                        break;

                    _usesize = _position - offset;
                    _pktsize = _usesize - _msgsize;

                    _result = offset;
                }

                // header maintanence
                _position = 0;
                {
                    var _changehdr = false;

                    long _frontlnk = this.GetLongValue(file_stream, ref _position);
                    long _rearlink = this.GetLongValue(file_stream, ref _position);

                    if ((_msgsize - message_size) <= _pktsize)
                    {
                        if (_frontlnk == offset)
                        {
                            _changehdr = true;
                            _frontlnk = _nxtlink;
                        }

                        if (_rearlink == offset)
                        {
                            _changehdr = true;
                            _rearlink = _prvlink;
                        }

                        if (_prvlink != -1)
                            this.WriteWaterMark(file_stream, _prvlink, this.m_removMark, -2, _nxtlink);

                        if (_nxtlink != -1)
                            this.WriteWaterMark(file_stream, _nxtlink, this.m_removMark, _prvlink, -2);
                    }
                    else
                    {
                        _position = offset + (_pktsize + message_size);

                        if (_frontlnk == offset)
                        {
                            _changehdr = true;
                            _frontlnk = _position;
                        }

                        if (_rearlink == offset)
                        {
                            _changehdr = true;
                            _rearlink = _position;
                        }

                        long _length = (_msgsize - message_size) - _pktsize;
                        byte[] _buffer = new byte[_length];
                        this.PutProperBufferOffset(file_stream, _position, this.m_removMark, _buffer, Guid.Empty, _prvlink, _nxtlink);
                    }

                    if (_changehdr == true)
                        this.WriteLinkHeader(file_stream, _frontlnk, _rearlink);
                }

                break;
            }

            return _result;
        }

        private long PutProperBufferOffset(FileStream file_stream, long offset, long water_mark, byte[] message, Guid guid_value, long previous_link, long next_link)
        {
            // water mark (long: 8bytes)
            offset = this.PutLongValue(file_stream, offset, water_mark);

            // uniqueue id (guid: 16bytes)
            offset = this.PutGuidValue(file_stream, offset, guid_value);

            // previous link (long: 8bytes)
            offset = this.PutLongValue(file_stream, offset, previous_link);

            // next link (long: 8bytes)
            offset = this.PutLongValue(file_stream, offset, next_link);

            // hash (long: 8bytes + byte[]: size of hash)
            byte[] _buffer = Encoding.UTF8.GetBytes(this.MD5ComputeHash(message));
            offset = this.PutBytesSize(file_stream, offset, _buffer);

            // message (long: 8bytes + byte[]: size of message)
            offset = this.PutBytesSize(file_stream, offset, message);

            return offset;
        }

        private long ReadWaterMark(FileStream file_stream, long offset, out long previous_link, out long next_link)
        {
            // water mark
            long _waterMark = this.GetLongValue(file_stream, ref offset);

            // skip uniqueue id
            this.GetGuidValue(file_stream, ref offset);

            // previous link
            previous_link = this.GetLongValue(file_stream, ref offset);

            // next link
            next_link = this.GetLongValue(file_stream, ref offset);

            return _waterMark;
        }

        private long WriteWaterMark(FileStream file_stream, long offset, long water_mark, long previous_link, long next_link)
        {
            // water mark
            offset = this.PutLongValue(file_stream, offset, water_mark);

            // uniqueue id
            offset = this.PutGuidValue(file_stream, offset, Guid.Empty);

            // previous link
            if (previous_link != -2)
                offset = this.PutLongValue(file_stream, offset, previous_link);
            else
                this.GetLongValue(file_stream, ref offset);

            // next link
            if (next_link != -2)
                offset = this.PutLongValue(file_stream, offset, next_link);
            else
                this.GetLongValue(file_stream, ref offset);

            return offset;
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        ///
        /// </summary>
        /// <param name="message"></param>
        /// <param name="uniqueue_id"></param>
        /// <returns></returns>
        public long AppendMailMessageToLocalFile(byte[] message, long uniqueue_id)
        {
            return this.AppendMailMessageToLocalFile(message, this.ConvertLong2Guid(uniqueue_id));
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="message"></param>
        /// <param name="guid_value"></param>
        /// <returns></returns>
        public long AppendMailMessageToLocalFile(byte[] message, Guid guid_value)
        {
            long _result = -1;

            using (FileStream _fs = File.Open(this.FileLocation, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                _result = _fs.Length;
                if (_result == 0)
                    _result = this.WriteLinkHeader(_fs, -1, -1);
                else
                {
                    long _reuse = this.SeekProperBuffer(_fs, message.LongLength);
                    if (_reuse > -1)
                    {
                        _result = _reuse;
                        this.AdjustProperBuffer(_fs, _reuse, message.LongLength);
                    }
                }

                this.PutProperBufferOffset(_fs, _result, m_waterMark, message, guid_value, -1, -1);
                _fs.Flush();
            }

            return _result;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="uniqueue_id"></param>
        /// <returns></returns>
        public byte[] ReadMailMessageFromLocalFile(long offset, long uniqueue_id)
        {
            return this.ReadMailMessageFromLocalFile(offset, this.ConvertLong2Guid(uniqueue_id));
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="guid_value"></param>
        /// <returns></returns>
        public byte[] ReadMailMessageFromLocalFile(long offset, Guid guid_value)
        {
            byte[] _message = null;

            var _werror = false;
            long _offset = offset;

            using (FileStream _fs = File.Open(this.FileLocation, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                while (true)
                {
                    // verify water-mark
                    if (this.IsValidWaterMark(_fs, ref _offset, this.m_waterMark) == false)
                        break;

                    _werror = true;

                    // check uid
                    Guid _uid = this.GetGuidValue(_fs, ref _offset);
                    if (_uid.Equals(guid_value) == false)
                        break;

                    // previos link
                    this.GetLongValue(_fs, ref _offset);

                    // next link
                    this.GetLongValue(_fs, ref _offset);

                    // check crc
                    byte[] _hash = this.GetBytesSize(_fs, ref _offset);
                    if (_hash == null)
                        break;

                    // read message
                    _message = this.GetBytesSize(_fs, ref _offset);
                    if (_message == null)
                        break;

                    // check crc
                    if (this.IsValidHashMark(_fs, _message, _hash) == false)
                        _message = null;

                    _werror = false;
                    break;
                }

                if (_werror == true)
                    this.PutLongValue(_fs, offset, this.m_errorMark);

                _fs.Flush();
            }

            return _message;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="uniqueue_id"></param>
        public void DeleteMailMessageInLocalFile(long offset, long uniqueue_id)
        {
            this.DeleteMailMessageInLocalFile(offset, this.ConvertLong2Guid(uniqueue_id));
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="guid_value"></param>
        public void DeleteMailMessageInLocalFile(long offset, Guid guid_value)
        {
            using (FileStream _fs = File.Open(this.FileLocation, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                while (true)
                {
                    long _position = offset;

                    // verify water-mark
                    if (this.IsValidWaterMark(_fs, ref _position, this.m_waterMark) == false)
                        break;

                    // check uid
                    Guid _uid = this.GetGuidValue(_fs, ref _position);
                    if (_uid.Equals(guid_value) == false)
                        break;

                    // header maintenance
                    _position = 0;

                    long _frontlnk = this.GetLongValue(_fs, ref _position);
                    long _rearlink = this.GetLongValue(_fs, ref _position);

                    if (_rearlink >= _position)
                    {
                        _position = _rearlink;

                        // water mark
                        if (this.IsValidWaterMark(_fs, ref _position, this.m_removMark) == true)
                        {
                            this.WriteWaterMark(_fs, _rearlink, this.m_removMark, -2, offset);
                            this.WriteWaterMark(_fs, offset, this.m_removMark, _rearlink, -1);

                            _rearlink = offset;
                        }
                        else
                        {
                            this.WriteWaterMark(_fs, offset, this.m_removMark, -1, -1);
                            _frontlnk = _rearlink = offset;
                        }
                    }
                    else
                    {
                        this.WriteWaterMark(_fs, offset, this.m_removMark, -1, -1);
                        _frontlnk = _rearlink = offset;
                    }

                    this.WriteLinkHeader(_fs, _frontlnk, _rearlink);
                    break;
                }

                _fs.Flush();
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------------------------------------------------------
    }
}