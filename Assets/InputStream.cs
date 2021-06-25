using System;
using System.IO;

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

/**
        byte[] bufferTest = new byte[] {33,49,0,32,0,0,0,0,2,230,69,56,0,1,125,181,99,99,136,122,92,1,99,196,231,90,205,20,75,233,5,103};
        //var value = bufferTest[0]; //readUInt8
        //var value= BitConverter.ToUInt16(bufferTest, 16); //readUInt16LE
        //var value= BitConverter.ToInt16(bufferTest, 21); //readInt16LE
        //var value= BitConverter.ToUInt32(bufferTest, 21); //readUInt32LE
        //var value= BitConverter.ToInt32(bufferTest, 21); //readInt32LE
        //var value = BitConverter.ToSingle(bufferTest, 17); //readFloatLE
        //var value = BitConverter.ToDouble(bufferTest, 15); //readDoubleLE

        var value = getString(bufferTest, 10, 10);
        File.WriteAllText("./buffer.txt", value);

        Debug.Log(value);

*/