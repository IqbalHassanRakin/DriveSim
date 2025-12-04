using System;
using System.Runtime.InteropServices;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Net.Sockets;
using UnityEngine;

public class ActuatorControll : MonoBehaviour
{
    // Import system function to get the current window handle, which DirectInput needs (for no good reason I can think of)
    private float force;
    private bool forceFeedbackEnabled;


    private string hostName = "127.0.0.1";
    private int port = 8403;
    UdpClient _udpClient = new UdpClient();


    [Range(0, 50000), Tooltip("フォース強さ")]
    public float forceFeedback = 25000f;             //
    [Range(0, 100), Tooltip("このスピードでMAX重さになる")]
    public float forceMaxSpeed = 25.0f;            //
    [Range(0, 1), Tooltip("止まってるとこの割合の重さになる")]
    public float forceSpeedEffective = 0.75f;      //

    [Range(0, 1), Tooltip("前後のスピード連動強さ")]
    public float LongitudinalSpeed = 0.5f;        //
    [Range(0, 0.1f), Tooltip("前後の加速度連動強さ")]
    public float LongitudinalAcc = 0.03f;        //
    [Range(0.9f, 0.999f), Tooltip("前後の戻る速さ")]
    public float LongitudinalAccRetern = 0.98f;      //
    [Range(20, 80), Tooltip("前後の初期位置")]
    public float LongitudinalOffset = 30.0f;      //

    //	public  float cylinderLpfAcc = 0.01f;      // 〃 ローパスフィルタ
    [Range(0, 0.05f), Tooltip("左右の角速度強さ")]
    public float YawAcc = 0.01f;        //
    [Range(0.9f, 0.999f), Tooltip("左右の戻る速さ")]
    public float YawRetern = 0.995f;      //
    [HideInInspector]
    public float YawOffset = 50.0f;      // 〃 センター位置
    //	private const float cylinderLpfYaw = 0.01f;      // 〃 ローパスフィルタ			

    [Range(100, 300), Tooltip("モーターが全開になる速度")]
    public float motorMaxRpmSpeed = 100.0f;       //

    [HideInInspector]
    public float steerInput = 0f;         // ステアリング(-1.0f～1.0f)
    [HideInInspector]
    public float speed = 0f;              // 速度
    [HideInInspector]
    public float acceleration = 0f;         // 加速度( m/(s^2) )
    [HideInInspector]
    private float yaw = 0f;                   // 角度( deg )
    [HideInInspector]
    private float yawAcc = 0f;                // 角速度
    [HideInInspector]
    private float pdeg;                       // 1フレーム前の角度( deg )
    [HideInInspector]
    private float pms;  // 1フレーム前の速度(m/s)

    [HideInInspector]
    private float moveX = 0;
    [HideInInspector]
    private float moveY = 0;
    [HideInInspector]
    private float motor = 0;

    [DllImport("user32")]
    private static extern int GetForegroundWindow();

    // Import functions from DirectInput c++ wrapper dll
    [DllImport("UnityForceFeedback")]
    private static extern int InitDirectInput(int HWND);

    [DllImport("UnityForceFeedback")]
    private static extern void Aquire();

    [DllImport("UnityForceFeedback")]
    private static extern int SetDeviceForcesXY(int x, int y);

    [DllImport("UnityForceFeedback")]
    private static extern bool StartEffect();

    [DllImport("UnityForceFeedback")]
    private static extern bool StopEffect();

    [DllImport("UnityForceFeedback")]
    private static extern bool SetAutoCenter(bool autoCentre);

    [DllImport("UnityForceFeedback")]
    private static extern void FreeDirectInput();

    private RCC_CarControllerV3 controller;

    public void Start()
    {
        InitialiseForceFeedback();
        SetAutoCenter(false);
        controller = GetComponent<RCC_CarControllerV3>();
    }

    //    public void Update()
    //    {
    //    }

    public void OnApplicationQuit()
    {
        ShutDownForceFeedback();
    }

    private void FixedUpdate()
    {
        // 1フレーム間に合わないことがあるので
        if (controller.velocityDirection == null)
        {
            return;
        }
        steerInput = controller.steerInput;
        speed = controller.speed * controller.direction;
        float cms = speed * 1000 / 60 / 60; // km/hをm/sに変換
        acceleration = (cms - pms) / Time.fixedDeltaTime;
        pms = cms;

        //車体ヨー角取得
        float deg = controller.velocityDirection.transform.eulerAngles.y;
        yaw = deg;
        yawAcc = (deg - pdeg);
        if (yawAcc > 180.0f)            //360度を跨いだら
            yawAcc -= 360.0f;
        if (yawAcc < -180.0f)
            yawAcc += 360.0f;
        yawAcc /= Time.fixedDeltaTime;
        pdeg = deg;

        moveX -= yawAcc * YawAcc;
        moveX *= YawRetern;
        moveY += acceleration * LongitudinalAcc;
        moveY *= LongitudinalAccRetern;

        force = steerInput * forceFeedback;            //ハンコン出力
        if (speed < forceMaxSpeed)
            force -= force * (1.0f - forceSpeedEffective) * (forceMaxSpeed - speed) / forceMaxSpeed;

        if (speed < motorMaxRpmSpeed)                       //モーター回転数
            motor = speed * 100.0f / motorMaxRpmSpeed;
        else
            motor = 100.0f;

        NetworkStream stream = new NetworkStream();
        stream.WriteByte((byte)0x7F);
        stream.WriteByte((byte)0x00);
        stream.WriteValue((short)4);

        stream.WriteValue((int)0);//Type=Cylinder
        stream.WriteValue((int)1);//ID=1
        stream.WriteValue((double)(moveY + LongitudinalOffset + speed * LongitudinalSpeed));//Left

        stream.WriteValue((int)0);//Type=Cylinder
        stream.WriteValue((int)2);//ID=2
        stream.WriteValue((double)(moveY + LongitudinalOffset + speed * LongitudinalSpeed));//Right

        stream.WriteValue((int)0);//Type=Cylinder
        stream.WriteValue((int)3);//ID=3
        stream.WriteValue((double)(moveX + YawOffset));//Yaw

        stream.WriteValue((int)2);//Type=Arduino
        stream.WriteValue((int)1);//ID=1
        stream.WriteValue((double)motor);//Speed

        byte[] packet = stream.ToArray();

        Send(packet);                       //シリンダ出力
        SetDeviceForcesXY((int)force, 0);   //ハンコン出力
    }

    private void InitialiseForceFeedback()
    {
        if (forceFeedbackEnabled)
        {
            Debug.Log("WARNING: Force feedback attempted to initialise but was aleady running!");
            return;
        }
        int hwnd = GetForegroundWindow();
        print("Window HWND = " + hwnd);
        int hr = InitDirectInput(hwnd);
        print("HRESULT = " + hr);
        Aquire();
        StartEffect();
        forceFeedbackEnabled = true;
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

    private void ShutDownForceFeedback()
    {
        StopEffect();
        if (forceFeedbackEnabled)
            FreeDirectInput();
        else
            Debug.Log("WARNING: Force feedback attempted to shutdown but wasn't running!");
    }


    private void OnGUI()
    {
        GUILayout.Label("Car infomation");
        GUILayout.Space(30);
        GUILayout.Label("Wheel = " + controller.steerInput);
        GUILayout.Label("gas = " + controller.gasInput);
        GUILayout.Label("brake = " + controller.brakeInput);
        GUILayout.Label("speed = " + (int)speed);
        GUILayout.Label("rpm = " + (int)controller.engineRPM);
        GUILayout.Label("ACC = " + (int)acceleration);
        GUILayout.Label("Z = " + (int)yaw);
        GUILayout.Label("YAW = " + yawAcc);
        GUILayout.Label("Force = " + (int)force);
        //GUILayout.Label("Cylinder X = " + (int)(moveX + cylinderOffsetYaw));
        //GUILayout.Label("CylinderY = " + (int)(moveY + cylinderOffsetAcc));
        GUILayout.Label("Motor = " + (int)motor);
    }

}