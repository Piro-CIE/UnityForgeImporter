// Copyright (c) Alexandre Piro - Piro CIE. All rights reserved
// This code is made from the package Forge-Convert-Utils by Petr Broz (Autodesk)
// Most parts of this code are Typescript -> C# adaptation

using System;

namespace PiroCIE.Utils
{
    public class InputStream {
        protected byte[] buffer;
        protected int m_offset;
        protected int m_length;

        public int offset {
            get {
                return m_offset;
            }
            set {
                m_offset = value;
            }
        }

        public int length {
            get {
                return m_length;
            }
            set {
                m_length = value;
            }
        }

        public InputStream (byte[] _buffer)
        {
            buffer = _buffer;
            offset = 0;
            length = buffer.Length;
        }

        public void seek(int _offset)
        {
            offset = _offset;
        }

        public byte getUint8()
        {
            var val = buffer[offset];
            offset +=1;
            return val;
        }

        public ushort getUint16()
        {
            var val = BitConverter.ToUInt16(buffer, offset);
            offset +=2;
            return val;
        }

        public short getInt16()
        {
            var val = BitConverter.ToInt16(buffer, offset);
            offset +=2;
            return val;
        }

        public uint getUint32()
        {
            var val = BitConverter.ToUInt32(buffer, offset);
            offset +=4;
            return val;
        }

        public int getInt32()
        {
            var val = BitConverter.ToInt32(buffer, offset);
            offset +=4;
            return val;
        }  

        public float getFloat32()
        {
            var val = BitConverter.ToSingle(buffer, offset);
            offset +=4;
            return val;
        }  

        public double getFloat64()
        {
            var val = BitConverter.ToDouble(buffer, offset);
            offset +=8;
            return val;
        }

        public int getVarint()
        {
            byte _byte;
            int val = 0;
            int shift = 0;
            do {
                _byte = buffer[offset++];
                val |= (_byte & 0x7f) << shift;
                shift += 7;
            } while ((_byte & 0x80) != 0);
            return val;
        }

        public string getString(int len) 
        {
            var val = System.Text.Encoding.UTF8.GetString(buffer, offset, len);
            offset += len;
            return val;
        }
    }
}