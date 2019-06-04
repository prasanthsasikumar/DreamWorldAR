using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using TensorFlow;

// Using IntPtr
using System;
// Using DllImport
using System.Runtime.InteropServices;
// Using Encoding
using System.Text;

public class DWPluginScript_PC : MonoBehaviour
{
    [DllImport("DWPlugin_PC")]
    private static extern IntPtr GetDWPlugin();

    [DllImport("DWPlugin_PC")]
    private static extern void DestroyInstance(IntPtr obj);

    [DllImport("DWPlugin_PC")]
    private static extern void UpdateDWIMUPose(IntPtr obj, byte[] buffer);

    [DllImport("DWPlugin_PC")]
    private static extern void UpdateDWIRHandImages(IntPtr obj, byte[] buffer);

    [DllImport("DWPlugin_PC")]
    private static extern void UpdateDWRGBImages(IntPtr obj, byte[] buffer);    

    [DllImport("DWPlugin_PC")]
    private static extern void UpdateHandGesture(IntPtr obj, byte[] inBuffer, byte[] outBuffer);

    [DllImport("DWPlugin_PC")]
    private static extern void StartImu(IntPtr obj);

    [DllImport("DWPlugin_PC")]
    private static extern void StopImu(IntPtr obj);

    [DllImport("DWPlugin_PC")]
    private static extern void StartIR(IntPtr obj);

    [DllImport("DWPlugin_PC")]
    private static extern void StopIR(IntPtr obj);

    [DllImport("DWPlugin_PC")]
    private static extern void StartRGB(IntPtr obj);

    [DllImport("DWPlugin_PC")]
    private static extern void StopRGB(IntPtr obj);

    [DllImport("DWPlugin_PC")]
    private static extern int GetRGBWidth(IntPtr obj);

    [DllImport("DWPlugin_PC")]
    private static extern int GetRGBHeight(IntPtr obj);

    [DllImport("DWPlugin_PC")]
    private static extern void TurnLCDOn(IntPtr obj);

    [DllImport("DWPlugin_PC")]
    private static extern void TurnLCDOff(IntPtr obj);

    [DllImport("DWPlugin_PC")]
    private static extern int getDebugVal(IntPtr obj);    

    // private member variables
    private IntPtr dwImuPluginObj = IntPtr.Zero;
    private bool gotOffet = false;
    private float yawOffset;
    private Quaternion rawQaut;
    bool deviceDetected = true;
    int INPUT_SIZE = 64;
    private Vector3 position;
    private Quaternion pose;

    bool irRunning = false;
    bool rgbRunning = false;
    // tensorflow
    TextAsset graphModel;
    TFGraph graph;
    TFSession session;

    // gesture
    bool isClick = false;
    bool isOpenPalm = false;
    bool isHold = false;
    string gestureMsg = "n";

    // sensor status
    public bool enableIMU = true;
    public bool enableIR = false;
    public bool enableRGB = true;

    // RGB cam data
    private int rgbWidth = 0;
    private int rgbHeight = 0;

    //public GUIText gt;
    public GameObject plane;
    private Texture2D RGBImage;
    private Material material;
    private byte[] rgbBytes;

    void Start()
    {
        if (enableIR && enableRGB)
        {
            Debug.LogError("Two cameras cannot work at the same time!");
            return;
        }

        // get DWPlugin Instance
        dwImuPluginObj = GetDWPlugin();
        byte[] checkBytes = new byte[64];
        UpdateDWIMUPose(dwImuPluginObj, checkBytes);
        string msg = Encoding.ASCII.GetString(checkBytes);
        if (msg[0] == 'N')
        {
            deviceDetected = false;
            Debug.LogError("Device not detected!");
            return;
        }
                
        // start IR camera thread
        if (enableIR)
        {
            // TF Initialization
            graph = new TFGraph();
            graphModel = Resources.Load("DreamWorld/DW_hand_gesture") as TextAsset;
            graph.Import(graphModel.bytes);
            session = new TFSession(graph);
            StartIR(dwImuPluginObj);
            new Thread(() =>
            {
                IRCamThread();
            }
            ).Start();
        }        

        if (enableIMU)
        {            
            StartImu(dwImuPluginObj);
        }

        if (enableRGB)
        {
            StartRGB(dwImuPluginObj);
            rgbWidth = GetRGBWidth(dwImuPluginObj);
            rgbHeight = GetRGBHeight(dwImuPluginObj);
            if (rgbWidth == -1 || rgbHeight == -1)
            {
                Debug.LogError("RGB camra not connected!");
                enableRGB = false;
            }
            
            rgbBytes = new byte[rgbWidth * rgbHeight * 3];
            RGBImage = new Texture2D(rgbWidth, rgbHeight, TextureFormat.RGB24, false);
            if (plane != null)
            {
                material = plane.GetComponent<Renderer>().material;
                material.mainTexture = RGBImage;
            }            
        }
        
    }

    // Update is called once per frame
    void Update()
    {    
        if (enableIR && enableRGB)
        {
            return;
        }

        if (enableRGB)
        {
            UpdateDWRGBImages(dwImuPluginObj, rgbBytes);
            if (rgbBytes[0] == 'N')
            {
                return;
            }
            RGBImage.LoadRawTextureData(rgbBytes);
            if (plane != null)
            {
                RGBImage.Apply();
            }
        }

        // IMU
        if (!deviceDetected || !enableIMU)
        {
            return;
        }
        // imu message
        byte[] poseBytes = new byte[64];
        UpdateDWIMUPose(dwImuPluginObj, poseBytes);
        string msg = Encoding.ASCII.GetString(poseBytes);
        if (msg[0] == 'N')
        {
            deviceDetected = false;
            Debug.LogError("Device not detected!");
            return;
        }
        string[] sValues = msg.Split(',');
        float[] msgValue = new float[sValues.Length];
        for (int i = 0; i < sValues.Length; ++i)
        {
            msgValue[i] = Convert.ToSingle(sValues[i], System.Globalization.CultureInfo.InvariantCulture);
        }
        rawQaut = new Quaternion(msgValue[3], msgValue[4], msgValue[5], msgValue[6]);
        if (!gotOffet && rawQaut.eulerAngles.y != 0)
        {
            yawOffset = rawQaut.eulerAngles.y;
            gotOffet = true;
        }
        // update position and pose
        // position = new Vector3(msgValue[0], msgValue[1], msgValue[2]);
        pose = Quaternion.Euler(rawQaut.eulerAngles.x, rawQaut.eulerAngles.y - yawOffset, rawQaut.eulerAngles.z);

        // this.transform.position = position;
        this.transform.localRotation = pose;

        /// gesture response test        
        // CheckGesture();
    }

    void CheckGesture()
    {
        if (IsClick())
        {
            Debug.Log("click!");
        }
        if (IsOpenPalm())
        {
            Debug.Log("Open Palm!");
        }
        if (IsHold())
        {
            Debug.Log("Hold!");
        }
    }

    void IRCamThread()
    {
        var runner = session.GetRunner();
        irRunning = true;
        while (irRunning)
        {
            byte[] imgsBytes = new byte[1 + 5 * INPUT_SIZE * INPUT_SIZE];
            UpdateDWIRHandImages(dwImuPluginObj, imgsBytes);
            if (imgsBytes[0] == 110) // hand imges not updated yet
            {
                Thread.Sleep(5);
                continue;
            }
            int imgNum = imgsBytes[0];

            int pixInd = 1;
            int bestLabel = 3;
            float bestScore = 0.0f;
            for (int i = 0; i < imgNum; ++i)
            {
                // get image pixels
                float[] imgArray = new float[64 * 64];
                for (int ind = 0; ind < 64 * 64; ++ind)
                {
                    imgArray[ind] = imgsBytes[pixInd] / 255.0f;
                    pixInd++;
                }
                try
                {
                    // classify
                    runner = session.GetRunner();
                    TFTensor inputImg = TFTensor.FromBuffer(new TFShape(new long[] { 1, 64, 64, 1 }), imgArray, 0, 64 * 64);
                    TFTensor inputDropout = new TFTensor(1.0f);
                    runner.AddInput(graph["data/X_placeholder"][0], inputImg);
                    runner.AddInput(graph["dropout"][0], inputDropout);
                    runner.Fetch(graph["softmax_linear/logits"][0]);
                    TFTensor[] output = runner.Run();
                    float[,] recurrent_tensor = output[0].GetValue() as float[,];

                    // Dispose TFTensors
                    for (int ind = 0; ind < output.Length; ++ind)
                    {
                        output[ind].Dispose();
                    }
                    inputImg.Dispose();
                    inputDropout.Dispose();


                    // get best classification
                    int predict = 0;
                    float prob = recurrent_tensor[0, 0];
                    for (int k = 1; k < 4; ++k)
                    {
                        if (recurrent_tensor[0, k] > prob)
                        {
                            prob = recurrent_tensor[0, k];
                            predict = k;
                        }
                    }
                    // classification != 3 means there is a valid gesture
                    /*
                     * 0: fist
                     * 1: finger
                     * 2: palm
                     * 3: other
                     */
                    if (predict != 3 && prob > bestScore)
                    {
                        bestScore = prob;
                        bestLabel = predict;
                    }
                }
                catch (Exception e)
                {
                    Debug.Log(e.ToString());
                }
            }
            
            // feed static gesture and get dynamic gesture
            byte[] outbuf = Encoding.ASCII.GetBytes(bestLabel.ToString());
            byte[] inbyte = new byte[2];            
            UpdateHandGesture(dwImuPluginObj, outbuf, inbyte);            
            gestureMsg = Encoding.ASCII.GetString(inbyte);
            UpdateGesture();
        }
         Debug.Log("DWPluginScripy_PC IRCamThread quit!");
    }

    void UpdateGesture()
    {
        if (gestureMsg[0] == '0')
        {
            //Debug.Log("_click");
            isClick = true;
            isHold = false;
        }
        else if (gestureMsg[0] == '1')
        {
            //Debug.Log("_open palm");
            isOpenPalm = true;
            isHold = false;
        }
        else if (gestureMsg[0] == '2')
        {
            //Debug.Log("_hold");
            isHold = true;
        }
        else
        {
            isHold = false;
        }
    }

    void OnDestroy()
    {
        // Debug.Log("DWPluginScripy_PC OnDestroy()");
        if (dwImuPluginObj != IntPtr.Zero)
        {
            // Destroy the object created by dll
            DestroyInstance(dwImuPluginObj);
            dwImuPluginObj = IntPtr.Zero;
        }
        irRunning = false;
        rgbRunning = false;
    }

    /************** public functions **************/
    /// <summary>
    /// Get camera's pose
    /// </summary>
    /// <returns>Quaternion to be set as Object.transform.rotation</returns>
    public Quaternion GetPose()
    {
        return pose;
    }

    /// <summary>
    /// Reset Yall to be 0
    /// </summary>
    public void ResetHead()
    {
        yawOffset = rawQaut.eulerAngles.y;
    }

    /// <summary>
    /// Check if there is a click in current frame
    /// </summary>
    /// <returns>bool variable</returns>
    public bool IsClick()
    {
        bool res = isClick;
        if (isClick)
        {
            isClick = false;
        }

        return res;
    }

    /// <summary>
    /// Check if there is an open palm in current frame
    /// </summary>
    /// <returns>bool variable</returns>
    public bool IsOpenPalm()
    {
        bool res = isOpenPalm;
        if (isOpenPalm)
        {
            isOpenPalm = false;
        }

        return res;
    }

    /// <summary>
    /// Check if it is hold gesture in current frame
    /// </summary>
    /// <returns>bool variable</returns>
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

    public Texture2D GetRGBImage()
    {
        return RGBImage;
    }

    public void TurnLCDScreenOn()
    {
        TurnLCDOn(dwImuPluginObj);
    }

    public void TurnLCDScreenOff()
    {
        TurnLCDOff(dwImuPluginObj);
    }
}