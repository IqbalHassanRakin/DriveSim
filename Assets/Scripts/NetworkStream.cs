using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

class NetworkStream : MemoryStream
{
	public NetworkStream()
	{
	}

	public NetworkStream(byte[] buffer) : base(buffer)
	{
	}

	public byte[] ReadBytes(int count)
	{
		byte[] buffer = new byte[count];
		Read(buffer, 0, buffer.Length);
		Array.Reverse(buffer);
		return buffer;
	}

	public short ToInt16()
	{
		return BitConverter.ToInt16(ReadBytes(2), 0);
	}

	public int ToInt32()
	{
		return BitConverter.ToInt32(ReadBytes(4), 0);
	}

	public double ToDouble()
	{
		return BitConverter.ToDouble(ReadBytes(8), 0);
	}

	public void WritreBytes(byte[] buffer)
	{
		Array.Reverse(buffer);
		Write(buffer, 0, buffer.Length);
	}

	public void WriteValue(short value)
	{
		WritreBytes(BitConverter.GetBytes(value));
	}

	public void WriteValue(int value)
	{
		WritreBytes(BitConverter.GetBytes(value));
	}

	public void WriteValue(double value)
	{
		WritreBytes(BitConverter.GetBytes(value));
	}
}
