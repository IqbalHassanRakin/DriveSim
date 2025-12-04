using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;

public class ActuatorStartUp : MonoBehaviour {

	private string hostName = "127.0.0.1";
	private int port = 8403;
	UdpClient _udpClient = new UdpClient();

	void Start () {
		NetworkStream stream = new NetworkStream();
		stream.WriteByte((byte)0x30);
		byte[] packet = stream.ToArray();
		Send(packet);
	}

	protected int Send(byte[] dgram)
	{
		try
		{
			return _udpClient.Send(dgram, dgram.Length, hostName, port);
		}
		catch (System.Exception e)
		{
			Debug.LogWarning(e);
		}
		return -1;
	}
}
