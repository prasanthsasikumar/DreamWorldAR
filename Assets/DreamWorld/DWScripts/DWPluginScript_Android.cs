using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;
using System.Linq;
using System.Text;

using System.Net;
using System.Net.Sockets;

using System.Runtime.InteropServices;

public class DWPluginScript_Android : MonoBehaviour {
    public GUIText pluginText;

    [DllImport("rgbbuf")]
    protected static extern System.IntPtr GetBuffer();

    private AndroidJavaClass plugin;
    private int testNum = 0;
    private Thread initializeThread = null;
    private AndroidJavaClass jc;
    private AndroidJavaObject jo;
    private string url = "N/A";

    // head tracking
    Quaternion tmp;
    bool gotOffet = false;
    float yawOffset;

    string msg = "initial";
    string gestureMsg = "initial gesture";
    bool quit = false;
       
    bool isClick = false;
    bool isOpenPalm = false;
    bool isHold = false;
    string prevGestureMsg = ""; // in case two consequtive frames visits the same message

    public bool enableIR = true;
    public bool enableIMU = true;
    public bool enableRGB = false;
    bool sensorStatusUpdated = false;

    // RGB cam data
    private int rgbWidth = 1920;
    private int rgbHeight = 1080;
    //public GUIText gt;
    public GameObject plane;
    private Texture2D RGBImage;
    private Material material;
    private byte[] rgbBytes;

    // calibration data
    private float ipd;
    private int gridSizeX;
    private int gridSizeY;
    private float cameraTilt;
    private float fov;
    private Vector2 rtResolutionAndroid;
    private Vector3 leftEyeCamRotAndroid;
    private Vector3 rightEyeCamRotAndroid;
    //Samsung S8// 
    private float eyeCamSizeAndroid_S8;
    private Vector3 leftEyeCamPosAndroid_S8;
    private Vector3 rightEyeCamPosAndroid_S8;
    //Mate10
    private float eyeCamSizeAndroid_Mate10_pro;
    private Vector3 leftEyeCamPosAndroid_Mate10_pro;
    private Vector3 rightEyeCamPosAndroid_Mate10_pro;
    private float eyeCamSizeAndroid_Mate10_DWHub;
    private Vector3 leftEyeCamPosAndroid_Mate10_DWHub;
    private Vector3 rightEyeCamPosAndroid_Mate10_DWHub;
    private float eyeCamSizeAndroid_Mate10_GreyHub;
    private Vector3 leftEyeCamPosAndroid_Mate10_GreyHub;
    private Vector3 rightEyeCamPosAndroid_Mate10_GreyHub;
    private float[] distCorrectLeftX;
    private float[] distCorrectLeftY;
    private float[] distCorrectRightX;
    private float[] distCorrectRightY;
    private bool calibrationDataUpdated = false;


    // Use this for initialization
    void Awake()
    {
        try
        {
            jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");

            jo = jc.GetStatic<AndroidJavaObject>("currentActivity");

            UpdateParameters();
        }
        catch (Exception e)
        {
            if (pluginText != null)
            {
                pluginText.text = "exception of start(): " + e.ToString();
            }            
        }
        // StartQRDetection();
    }

    void Start()
    {
        RGBImage = new Texture2D(rgbWidth, rgbHeight, TextureFormat.RGB565, false);
        if (plane != null)
        {
            material = plane.GetComponent<Renderer>().material;
            material.mainTexture = RGBImage;
        }
    }

    void Update()
    {
        try
        {
            if (!sensorStatusUpdated)
            {
                UpdateSensorStatus();
            }
        }
        catch (Exception e)
        {
            if (pluginText != null)
            {
                pluginText.text = "exception of Update(): " + e.ToString();
            }
            return;
        }
        
        if (enableIR)
        {
            try
            {
                gestureMsg = jo.Call<string>("getGesture");
                if (gestureMsg.Substring(0, 5) == prevGestureMsg && (gestureMsg.Substring(0, 5) == "1,0,0" || gestureMsg.Substring(0, 5) == "0,1,0"))
                {
                    return;
                }
                prevGestureMsg = gestureMsg.Substring(0, 5);
                UdpateGesture();
                if (pluginText != null)
                {
                   pluginText.text = gestureMsg;
                }
            }
            catch (Exception e)
            {
                if (pluginText != null)
                {
                    pluginText.text = "exception of Update(): " + e.ToString();
                }
                return;
            }
            
        }
        
        if (enableIMU)
        {
            try
            {
                msg = jo.Call<string>("getPose");
                SetPose(msg);
            }
            catch (Exception e)
            {
                if (pluginText != null)
                {
                    pluginText.text = msg;
                }
            }
        }

        if (enableRGB)
        {
            try
            {
                System.IntPtr bufferPointer = GetBuffer();
                if (bufferPointer != System.IntPtr.Zero)
                {
                    RGBImage.LoadRawTextureData(bufferPointer, rgbWidth * rgbHeight * 4);
                    if (plane != null)
                    {
                        RGBImage.Apply();
                    }
                }
            }
            catch (Exception e)
            {
                if (pluginText != null)
                {
                    pluginText.text = "exception of bufferPointer(msg): " + e.ToString();
                }
            }

        }
    }

    void UdpateGesture()
    {
        if (gestureMsg[0] == '1')
        {
            isClick = true;
            isHold = false;
        }
        else if (gestureMsg[2] == '1')
        {
            isOpenPalm = true;
            isHold = false;
        }
        else if (gestureMsg[4] == '1')
        {
            isHold = true;
        }
        else
        {
            isHold = false;
        }
    }

    void SetPose(string msg)
    {
        string[] sValues = msg.Split(',');
        float[] msgValue = new float[sValues.Length];

        for (int i = 0; i < sValues.Length; ++i)
        {
            msgValue[i] = Convert.ToSingle(sValues[i]);
        }

        tmp = new Quaternion(msgValue[3], msgValue[4], msgValue[5], msgValue[6]); // for DW Device
        // tmp = new Quaternion(msgValue[0], msgValue[1] * -1.0f, msgValue[2] * -1.0f, msgValue[3]); // for LG

        if (!gotOffet)
        {
            yawOffset = tmp.eulerAngles.y;
            gotOffet = true;
        }

        this.transform.localRotation = Quaternion.Euler(tmp.eulerAngles.x, tmp.eulerAngles.y - yawOffset, tmp.eulerAngles.z);
    }

    private void UpdateSensorStatus()
    {
        // check IMU
        if (!enableIMU && jo.Call<bool>("isImuRunning"))
        {
            jo.Call("stopIMU");
        }
        // check IR
        if (!enableIR && jo.Call<bool>("isIRRunning"))
        {
            jo.Call("stopIR");
        }
        // check RGB
        if (!enableRGB && jo.Call<bool>("isRGBRunning"))
        {
            jo.Call("stopRGB");
        }

        // check IMU
        if (enableIMU && !jo.Call<bool>("isImuRunning"))
        {
            jo.Call("startIMU");
        }
        // check IR
        if (enableIR && !jo.Call<bool>("isIRRunning"))
        {
            jo.Call("startIR");
        }
        // check RGB
        if (enableRGB && !jo.Call<bool>("isRGBRunning"))
        {
            jo.Call("startRGB");
        }

        sensorStatusUpdated = true;
        jo.Call("sensorStatusAssigned");
    }

    void OnDestroy()
    {
        quit = true;
    }

    private void UpdateParameters()
    {
        UpdateIpd();
        UpdateGridSizeX();
        UpdateGridSizeY();
        UpdateCameraTilt();
        UpdateFov();
        UpdateRtResolutionAndroid();
        UpdateLeftEyeCamRotAndroid();
        UpdateRightEyeCamRotAndroid();
        UpdateEyeCamSizeAndroid_S8();
        UpdateLeftEyeCamPosAndroid_S8();
        UpdateRightEyeCamPosAndroid_S8();
        UpdateEyeCamSizeAndroid_Mate10_pro();
        UpdateLeftEyeCamPosAndroid_Mate10_pro();
        UpdateRightEyeCamPosAndroid_Mate10_pro();
        UpdateEyeCamSizeAndroid_Mate10_DWHub();
        UpdateLeftEyeCamPosAndroid_Mate10_DWHub();
        UpdateRightEyeCamPosAndroid_Mate10_DWHub();
        UpdateEyeCamSizeAndroid_Mate10_GreyHub();
        UpdateLeftEyeCamPosAndroid_Mate10_GreyHub();
        UpdateRightEyeCamPosAndroid_Mate10_GreyHub();
        UpdateDistCorrectLeftX();
        UpdateDistCorrectLeftY();
        UpdateDistCorrectRightX();
        UpdateDistCorrectRightY();      

        calibrationDataUpdated = true;
    }

    private void UpdateIpd()
    {
        string tmp = jo.Call<string>("getIpd");
        ipd = Convert.ToSingle(tmp);
    }

    private void UpdateGridSizeX()
    {
        string tmp = jo.Call<string>("getGridSizeX");
        gridSizeX = Convert.ToInt32(tmp);
    }

    private void UpdateGridSizeY()
    {
        string tmp = jo.Call<string>("getGridSizeY");
        gridSizeY = Convert.ToInt32(tmp);
    }

    private void UpdateCameraTilt()
    {
        string tmp = jo.Call<string>("getCameraTilt");
        cameraTilt = Convert.ToInt32(tmp);
    }

    private void UpdateFov()
    {
        string tmp = jo.Call<string>("getFov");
        fov = Convert.ToSingle(tmp);
    }

    private void UpdateRtResolutionAndroid()
    {
        string tmp = jo.Call<string>("getRtResolutionAndroid");
        string[] sValues = tmp.Split(',');
        int[] msgValue = new int[sValues.Length];
        for (int i = 0; i < sValues.Length; ++i)
        {
            msgValue[i] = Convert.ToInt32(sValues[i]);
        }
        rtResolutionAndroid = new Vector2(msgValue[0], msgValue[1]);        
    }

    private void UpdateLeftEyeCamRotAndroid()
    {
        string tmp = jo.Call<string>("getLeftEyeCamRotAndroid");
        string[] sValues = tmp.Split(',');
        float[] msgValue = new float[sValues.Length];
        for (int i = 0; i < sValues.Length; ++i)
        {
            msgValue[i] = Convert.ToSingle(sValues[i]);
        }
        leftEyeCamRotAndroid = new Vector3(msgValue[0], msgValue[1], msgValue[2]);       
    }

    private void UpdateRightEyeCamRotAndroid()
    {
        string tmp = jo.Call<string>("getRightEyeCamRotAndroid");
        string[] sValues = tmp.Split(',');
        float[] msgValue = new float[sValues.Length];
        for (int i = 0; i < sValues.Length; ++i)
        {
            msgValue[i] = Convert.ToSingle(sValues[i]);
        }
        rightEyeCamRotAndroid = new Vector3(msgValue[0], msgValue[1], msgValue[2]);
    }

    private void UpdateEyeCamSizeAndroid_S8()
    {
        string tmp = jo.Call<string>("getEyeCamSizeAndroid_S8");
        eyeCamSizeAndroid_S8 = Convert.ToSingle(tmp);
    }

    private void UpdateLeftEyeCamPosAndroid_S8()
    {
        string tmp = jo.Call<string>("getLeftEyeCamPosAndroid_S8");
        string[] sValues = tmp.Split(',');
        float[] msgValue = new float[sValues.Length];
        for (int i = 0; i < sValues.Length; ++i)
        {
            msgValue[i] = Convert.ToSingle(sValues[i]);
        }
        leftEyeCamPosAndroid_S8 = new Vector3(msgValue[0], msgValue[1], msgValue[2]);
    }

    private void UpdateRightEyeCamPosAndroid_S8()
    {
        string tmp = jo.Call<string>("getRightEyeCamPosAndroid_S8");
        string[] sValues = tmp.Split(',');
        float[] msgValue = new float[sValues.Length];
        for (int i = 0; i < sValues.Length; ++i)
        {
            msgValue[i] = Convert.ToSingle(sValues[i]);
        }
        rightEyeCamPosAndroid_S8 = new Vector3(msgValue[0], msgValue[1], msgValue[2]);
    }

    private void UpdateEyeCamSizeAndroid_Mate10_pro()
    {
        string tmp = jo.Call<string>("getEyeCamSizeAndroid_Mate10_pro");
        eyeCamSizeAndroid_Mate10_pro = Convert.ToSingle(tmp);        
    }

    private void UpdateLeftEyeCamPosAndroid_Mate10_pro()
    {
        string tmp = jo.Call<string>("getLeftEyeCamPosAndroid_Mate10_pro");
        string[] sValues = tmp.Split(',');
        float[] msgValue = new float[sValues.Length];
        for (int i = 0; i < sValues.Length; ++i)
        {
            msgValue[i] = Convert.ToSingle(sValues[i]);
        }
        leftEyeCamPosAndroid_Mate10_pro = new Vector3(msgValue[0], msgValue[1], msgValue[2]);
    }

    private void UpdateRightEyeCamPosAndroid_Mate10_pro()
    {
        string tmp = jo.Call<string>("getRightEyeCamPosAndroid_Mate10_pro");
        string[] sValues = tmp.Split(',');
        float[] msgValue = new float[sValues.Length];
        for (int i = 0; i < sValues.Length; ++i)
        {
            msgValue[i] = Convert.ToSingle(sValues[i]);
        }
        rightEyeCamPosAndroid_Mate10_pro = new Vector3(msgValue[0], msgValue[1], msgValue[2]);
    }

    private void UpdateEyeCamSizeAndroid_Mate10_DWHub()
    {
        string tmp = jo.Call<string>("getEyeCamSizeAndroid_Mate10_DWHub");
        eyeCamSizeAndroid_Mate10_DWHub = Convert.ToSingle(tmp);
    }

    private void UpdateLeftEyeCamPosAndroid_Mate10_DWHub()
    {
        string tmp = jo.Call<string>("getLeftEyeCamPosAndroid_Mate10_DWHub");
        string[] sValues = tmp.Split(',');
        float[] msgValue = new float[sValues.Length];
        for (int i = 0; i < sValues.Length; ++i)
        {
            msgValue[i] = Convert.ToSingle(sValues[i]);
        }
        leftEyeCamPosAndroid_Mate10_DWHub = new Vector3(msgValue[0], msgValue[1], msgValue[2]);
    }

    private void UpdateRightEyeCamPosAndroid_Mate10_DWHub()
    {
        string tmp = jo.Call<string>("getRightEyeCamPosAndroid_Mate10_DWHub");
        string[] sValues = tmp.Split(',');
        float[] msgValue = new float[sValues.Length];
        for (int i = 0; i < sValues.Length; ++i)
        {
            msgValue[i] = Convert.ToSingle(sValues[i]);
        }
        rightEyeCamPosAndroid_Mate10_DWHub = new Vector3(msgValue[0], msgValue[1], msgValue[2]);
    }

    private void UpdateEyeCamSizeAndroid_Mate10_GreyHub()
    {
        string tmp = jo.Call<string>("getEyeCamSizeAndroid_Mate10_GreyHub");
        eyeCamSizeAndroid_Mate10_GreyHub = Convert.ToSingle(tmp);
    }

    private void UpdateLeftEyeCamPosAndroid_Mate10_GreyHub()
    {
        string tmp = jo.Call<string>("getLeftEyeCamPosAndroid_Mate10_GreyHub");
        string[] sValues = tmp.Split(',');
        float[] msgValue = new float[sValues.Length];
        for (int i = 0; i < sValues.Length; ++i)
        {
            msgValue[i] = Convert.ToSingle(sValues[i]);
        }
        leftEyeCamPosAndroid_Mate10_GreyHub = new Vector3(msgValue[0], msgValue[1], msgValue[2]);
    }

    private void UpdateRightEyeCamPosAndroid_Mate10_GreyHub()
    {
        string tmp = jo.Call<string>("getRightEyeCamPosAndroid_Mate10_GreyHub");
        string[] sValues = tmp.Split(',');
        float[] msgValue = new float[sValues.Length];
        for (int i = 0; i < sValues.Length; ++i)
        {
            msgValue[i] = Convert.ToSingle(sValues[i]);
        }
        rightEyeCamPosAndroid_Mate10_GreyHub = new Vector3(msgValue[0], msgValue[1], msgValue[2]);
    }

    private void UpdateDistCorrectLeftX()
    {
        string tmp = jo.Call<string>("getDistCorrectLeftX");
        string[] sValues = tmp.Split(',');
        distCorrectLeftX = new float[sValues.Length];
        for (int i = 0; i < sValues.Length; ++i)
        {
            distCorrectLeftX[i] = Convert.ToSingle(sValues[i]);
        }
    }

    private void UpdateDistCorrectLeftY()
    {
        string tmp = jo.Call<string>("getDistCorrectLeftY");
        string[] sValues = tmp.Split(',');
        distCorrectLeftY = new float[sValues.Length];
        for (int i = 0; i < sValues.Length; ++i)
        {
            distCorrectLeftY[i] = Convert.ToSingle(sValues[i]);
        }
    }

    private void UpdateDistCorrectRightX()
    {
        string tmp = jo.Call<string>("getDistCorrectRightX");
        string[] sValues = tmp.Split(',');
        distCorrectRightX = new float[sValues.Length];
        for (int i = 0; i < sValues.Length; ++i)
        {
            distCorrectRightX[i] = Convert.ToSingle(sValues[i]);
        }
    }

    private void UpdateDistCorrectRightY()
    {
        string tmp = jo.Call<string>("getDistCorrectRightY");
        string[] sValues = tmp.Split(',');
        distCorrectRightY = new float[sValues.Length];
        for (int i = 0; i < sValues.Length; ++i)
        {
            distCorrectRightY[i] = Convert.ToSingle(sValues[i]);
        }
    }

    /*************** Public Functions ***************/
    public string GetURL()
    {
        return url;
    }

    public void ReturnToYoutube()
    {
        jo.Call("returnToYoutube");
    }

    public bool IsClick()
    {
        bool res = isClick;
        if (isClick)
        {
            isClick = false;
        }

        return res;
    }

    public bool IsOpenPalm()
    {
        bool res = isOpenPalm;
        if (isOpenPalm)
        {
            isOpenPalm = false;
        }

        return res;
    }

    public bool IsHold()
    {
        return isHold;
    }

    public void EnableIMU()
    {
        enableIMU = true;
    }

    public void DisableIMU()
    {
        enableIMU = false;
    }

    public void EnableIR()
    {
        enableIR = true;
    }

    public void DisableIR()
    {
        enableIR = false;
    }

    public void EnableRGB()
    {
        enableRGB = true;
    }

    public void DisableRGB()
    {
        enableRGB = false;
    }

    public void StartQRDetection()
    {
        if (jo != null)
        {
            jo.Call("startQRDetect");
        }
        else
        {
            Debug.Log("Java Object not instantiated!");
        }
    }

    public void StopQRDetection()
    {
        if (jo != null)
        {
            jo.Call("stopQRDetect");
        }
        else
        {
            Debug.Log("Java Object not instantiated!");
        }
    }

    public string CheckQRDetectionRes()
    {
        string qrStr = jo.Call<string>("getQRRes");
        if (qrStr.Length > 0)
        {
            return qrStr;
        }
        else
        {
            return null;
        }        
    }

    // calibration params
    public bool IsCalibrationDataUpdated()
    {
        return calibrationDataUpdated;
    }

    public float GetIpd()
    {
        return ipd;
    }

    public int GetGridSizeX()
    {
        return gridSizeX;
    }

    public int GetGridSizeY()
    {
        return gridSizeY;
    }

    public float GetCameraTilt()
    {
        return cameraTilt;
    }

    public float GetFov()
    {
        return fov;
    }

    public Vector2 GetRtResolutionAndroid()
    {
        return rtResolutionAndroid;
    }

    public Vector3 GetLeftEyeCamRotAndroid()
    {
        return leftEyeCamRotAndroid;
    }

    public Vector3 GetRightEyeCamRotAndroid()
    {
        return rightEyeCamRotAndroid;
    }

    public float GetEyeCamSizeAndroid_S8()
    {
        return eyeCamSizeAndroid_S8;
    }

    public Vector3 GetLeftEyeCamPosAndroid_S8()
    {
        return leftEyeCamPosAndroid_S8;
    }

    public Vector3 GetRightEyeCamPosAndroid_S8()
    {
        return rightEyeCamPosAndroid_S8;
    }

    public float GetEyeCamSizeAndroid_Mate10_pro()
    {
        return eyeCamSizeAndroid_Mate10_pro;
    }

    public Vector3 GetLeftEyeCamPosAndroid_Mate10_pro()
    {
        return leftEyeCamPosAndroid_Mate10_pro;
    }

    public Vector3 GetRightEyeCamPosAndroid_Mate10_pro()
    {
        return rightEyeCamPosAndroid_Mate10_pro;
    }

    public float GetEyeCamSizeAndroid_Mate10_DWHub()
    {
        return eyeCamSizeAndroid_Mate10_DWHub;
    }

    public Vector3 GetLeftEyeCamPosAndroid_Mate10_DWHub()
    {
        return leftEyeCamPosAndroid_Mate10_DWHub;
    }

    public Vector3 GetRightEyeCamPosAndroid_Mate10_DWHub()
    {
        return rightEyeCamPosAndroid_Mate10_DWHub;
    }

    public float GetEyeCamSizeAndroid_Mate10_GreyHub()
    {
        return eyeCamSizeAndroid_Mate10_GreyHub;
    }

    public Vector3 GetLeftEyeCamPosAndroid_Mate10_GreyHub()
    {
        return leftEyeCamPosAndroid_Mate10_GreyHub;
    }

    public Vector3 GetRightEyeCamPosAndroid_Mate10_GreyHub()
    {
        return rightEyeCamPosAndroid_Mate10_GreyHub;
    }

    public float[] GetDistCorrectLeftX()
    {
        return distCorrectLeftX;
    }

    public float[] GetDistCorrectLeftY()
    {
        return distCorrectLeftY;
    }

    public float[] GetDistCorrectRightX()
    {
        return distCorrectRightX;
    }

    public float[] GetDistCorrectRightY()
    {
        return distCorrectRightY;
    }

    public Texture2D GetRGBImage()
    {
        return RGBImage;
    }
}