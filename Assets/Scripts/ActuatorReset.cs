using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;

public class ActuatorReset : MonoBehaviour {
    public KeyCode[] endKeys = { KeyCode.Alpha0, KeyCode.Keypad0 };

    private string hostName = "127.0.0.1";
	private int port = 8403;
	UdpClient _udpClient = new UdpClient();

    private void Update()
    {
        foreach (var key in endKeys)
        {
            if (Input.GetKeyDown(key))
            {
                NetworkStream stream = new NetworkStream();
                stream.WriteByte((byte)'r');        // "r"の送信
                byte[] packet = stream.ToArray();
                Send(packet);
            }
        }
    }
    
	public int Send(byte[] dgram)
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
