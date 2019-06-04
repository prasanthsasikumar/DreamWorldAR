using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using DreamWorldDLL;

public enum TrackingType { IMU_3DOF, NOLO_6DOF, None }
public enum CameraType { IR_HandTracking, RGB_VideoTexture, RGB_VideoCapture, IRAndRGB_Android, None}
public enum CaptureCommand { spacebar, enter, onStart, none };
public enum Resolution { Low_360p, Mid_540p, High_720p };

[Serializable]
public class Features
{  
    public TrackingType tracking;
    public CameraType activeCamera;
}

[Serializable]
public class VideoCaptureSettings
{
    public CaptureCommand captureCommand;
    public Resolution resolution;
    public string VideoPath;
}

public class DWCameraRig : MonoBehaviour {

    public enum Platform { PC, Android_Samsung, Android_Huawei_Mate10, Android_Huawei_Mate10_PRO }
    public Platform platform;
    public static DWCameraRig Instance { get; private set; }
    public Features features;
    public VideoCaptureSettings videoCaptureSettings;
    public bool emulator;

    private RigSetup camRig;
    private DWPluginScript_Android androidPlugin;
    private DWPluginScript_PC pcPlugin;

    private RaycastHit hitInfo;
    private Transform raycastPos;
    private Quaternion raycastRot = Quaternion.Euler(0,0,0);
    private GameObject selectedObject;
    private bool holding = false;
    private GameObject headRot;

    private CameraCaptureScript camCaptureScript;
    private bool recording = false;

    void Awake()
    {
        Application.runInBackground = true;

        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        else
        {
            Destroy(gameObject);
        }

        //create neck pos
        headRot = GameObject.FindGameObjectWithTag("HeadRotation");
        headRot.transform.SetParent(this.transform);
        headRot.transform.localPosition = new Vector3(0.0f, -0.15f, -0.1f);


        if (emulator)
        {
            if (this.platform == Platform.PC) headRot.gameObject.AddComponent<Emulator>();
            CreateRaycast();
            return;
        }


        if (!Application.isEditor)
        {
            int platform = 0;
            int trackType = 0;
            bool cameraCapture = false;

            if (this.platform == Platform.PC) platform = 0;
            else if (this.platform == Platform.Android_Samsung) platform = 1;
            else if (this.platform == Platform.Android_Huawei_Mate10) platform = 2;
            else if (this.platform == Platform.Android_Huawei_Mate10_PRO) platform = 3;

            if (this.features.tracking == TrackingType.NOLO_6DOF) trackType = 2;
            if (this.features.activeCamera == CameraType.RGB_VideoCapture) cameraCapture = true;

            camRig = this.headRot.AddComponent<RigSetup>();
            camRig.Initialization(platform, trackType, cameraCapture);
        }

        else
        {
            if (this.features.tracking == TrackingType.NOLO_6DOF) this.transform.localPosition = new Vector3(0.0f, -.05f, -.03f);
        }

        if (this.platform == Platform.PC)
        {
            pcPlugin = this.headRot.AddComponent<DWPluginScript_PC>();

            if (features.tracking == TrackingType.IMU_3DOF) pcPlugin.EnableIMU();
            else pcPlugin.DisableIMU();

            if (features.activeCamera == CameraType.IR_HandTracking)
            {
                pcPlugin.DisableRGB();
                pcPlugin.EnableIR();
            }

            else if (features.activeCamera == CameraType.RGB_VideoTexture)
            {
                pcPlugin.DisableIR();
                pcPlugin.EnableRGB();
            }

            else if (features.activeCamera == CameraType.RGB_VideoCapture || features.activeCamera == CameraType.None)
            {
                pcPlugin.DisableRGB();
                pcPlugin.DisableIR();
            }
        }

        else if (this.platform != Platform.PC)
        {

            androidPlugin = this.headRot.AddComponent<DWPluginScript_Android>();

            if (features.tracking == TrackingType.IMU_3DOF) androidPlugin.EnableIMU();
            else androidPlugin.DisableIMU();

            if (features.activeCamera == CameraType.IR_HandTracking)
            {
                androidPlugin.DisableRGB();
                androidPlugin.EnableIR();
            }

            else if (features.activeCamera == CameraType.RGB_VideoTexture)
            {
                androidPlugin.DisableIR();
                androidPlugin.EnableRGB();
            }

            else if (features.activeCamera == CameraType.IRAndRGB_Android)
            {
                androidPlugin.EnableIR();
                androidPlugin.EnableRGB();

            }

            else if (features.activeCamera == CameraType.None)
            {
                androidPlugin.DisableRGB();
                androidPlugin.DisableIR();
            }
        }

        if (features.activeCamera == CameraType.RGB_VideoCapture)
        {
            camCaptureScript = headRot.AddComponent<CameraCaptureScript>();
            camCaptureScript.CapturePath(videoCaptureSettings.VideoPath);
            if (videoCaptureSettings.resolution == Resolution.Low_360p) camCaptureScript.CaptureSettings(1);
            else if (videoCaptureSettings.resolution == Resolution.Mid_540p) camCaptureScript.CaptureSettings(2);
            else if (videoCaptureSettings.resolution == Resolution.High_720p) camCaptureScript.CaptureSettings(3);

            if (videoCaptureSettings.captureCommand == CaptureCommand.onStart) StartCoroutine(CaptureOnStart());
        }

        CreateRaycast();
    }



    ////*Raycasting*////
    public void CreateRaycast()
    {
        float raycastTilt = new CalibrationData().Tilt(); 
        raycastRot.eulerAngles = new Vector3(raycastTilt, 0.0f, 0.0f);
        GameObject newRaycastPos = new GameObject();
        newRaycastPos.name = "RaycastPos";
        raycastPos = newRaycastPos.transform;
        raycastPos.SetParent(this.headRot.transform);
        raycastPos.localPosition = new Vector3(0.0f, 0.15f, 0.1f);
        raycastPos.localRotation = raycastRot;

    }

    void RaycastDetection()
    {
        if (Physics.Raycast(raycastPos.position, raycastPos.forward, out hitInfo))
        {
            if (selectedObject != null && hitInfo.collider.gameObject != selectedObject)
            {
                selectedObject.SendMessage("Unfocus", SendMessageOptions.DontRequireReceiver);
                selectedObject = hitInfo.collider.gameObject;
                selectedObject.SendMessage("OnFocus", SendMessageOptions.DontRequireReceiver);

                if (holding) {

                    if (features.activeCamera == CameraType.IR_HandTracking)
                    {
                        holding = false;
                        selectedObject.SendMessage("OnRelease", SendMessageOptions.DontRequireReceiver);
                    }

                    else if (features.activeCamera == CameraType.IRAndRGB_Android && platform != Platform.PC)
                    {
                        holding = false;
                        selectedObject.SendMessage("OnRelease", SendMessageOptions.DontRequireReceiver);
                    }
                }
            }

            else if (selectedObject == null)
            {
                selectedObject = hitInfo.collider.gameObject;
                selectedObject.SendMessage("OnFocus", SendMessageOptions.DontRequireReceiver);
            }
        }

        else
        {
            if (selectedObject != null)
            {
                selectedObject.SendMessage("Unfocus", SendMessageOptions.DontRequireReceiver);

                if (holding)
                {

                    if (features.activeCamera == CameraType.IR_HandTracking)
                    {
                        holding = false;
                        selectedObject.SendMessage("OnRelease", SendMessageOptions.DontRequireReceiver);
                    }

                    else if (features.activeCamera == CameraType.IRAndRGB_Android && platform != Platform.PC)
                    {
                        holding = false;
                        selectedObject.SendMessage("OnRelease", SendMessageOptions.DontRequireReceiver);
                    }
                }

                selectedObject = null;
            }
        }

        if (selectedObject != null)
        {
            //pc controls
            if (emulator)
            {
                if ((Input.GetMouseButtonUp(0) && !holding) || Input.GetKeyDown(KeyCode.Return))
                {
                    selectedObject.SendMessage("OnClick", SendMessageOptions.DontRequireReceiver);
                }

                if (Input.GetMouseButton(2))
                {
                    selectedObject.SendMessage("OnHold", SendMessageOptions.DontRequireReceiver);
                    return;
                }

                if (Input.GetMouseButtonUp(2))
                {
                    holding = false;
                    selectedObject.SendMessage("OnRelease", SendMessageOptions.DontRequireReceiver);
                    return;
                }

                else if (Input.GetMouseButtonUp(1))
                {
                    selectedObject.SendMessage("OnOpenPalm", SendMessageOptions.DontRequireReceiver);
                }

            }

            //end pc controls

            if (features.activeCamera == CameraType.IR_HandTracking)
            {
                if (pcPlugin != null)
                {
                    if (pcPlugin.IsClick()) selectedObject.SendMessage("OnClick", SendMessageOptions.DontRequireReceiver);
                    else if (pcPlugin.IsOpenPalm()) selectedObject.SendMessage("OnOpenPalm", SendMessageOptions.DontRequireReceiver);
                    else if (pcPlugin.IsHold() && !holding)
                    {

                        selectedObject.SendMessage("OnHold", SendMessageOptions.DontRequireReceiver);
                        holding = true;
                    }

                    else if (!pcPlugin.IsHold() && holding)
                    {
                        holding = false;
                        selectedObject.SendMessage("OnRelease", SendMessageOptions.DontRequireReceiver);
                    }
                }

                else if (androidPlugin != null)
                {

                    if (androidPlugin.IsClick()) selectedObject.SendMessage("OnClick", SendMessageOptions.DontRequireReceiver);
                    else if (androidPlugin.IsOpenPalm()) selectedObject.SendMessage("OnOpenPalm", SendMessageOptions.DontRequireReceiver);
                    else if (androidPlugin.IsHold() && !holding)
                    {
                        selectedObject.SendMessage("OnHold", SendMessageOptions.DontRequireReceiver);
                        holding = true;
                    }

                    else if (!androidPlugin.IsHold() && holding)
                    {

                        holding = false;
                        selectedObject.SendMessage("OnRelease", SendMessageOptions.DontRequireReceiver);
                    }
                }
            }

            else if (features.activeCamera == CameraType.IRAndRGB_Android)
            {
                 if (androidPlugin != null)
                {

                    if (androidPlugin.IsClick()) selectedObject.SendMessage("OnClick", SendMessageOptions.DontRequireReceiver);
                    else if (androidPlugin.IsOpenPalm()) selectedObject.SendMessage("OnOpenPalm", SendMessageOptions.DontRequireReceiver);
                    else if (androidPlugin.IsHold() && !holding)
                    {
                        selectedObject.SendMessage("OnHold", SendMessageOptions.DontRequireReceiver);
                        holding = true;
                    }

                    else if (!androidPlugin.IsHold() && holding)
                    {

                        holding = false;
                        selectedObject.SendMessage("OnRelease", SendMessageOptions.DontRequireReceiver);
                    }
                }
            }
        }
    }

    public Transform GetFocusCenter()
    {
        return raycastPos;
    }


    public RaycastHit GetHitInfo()
    {
        return hitInfo;
    }

    public Texture2D RGBTexture()
    {
        Texture2D tex = null;

        if (this.platform == Platform.PC && pcPlugin != null)
        {
           if(this.features.activeCamera == CameraType.RGB_VideoTexture) tex = pcPlugin.GetRGBImage();
        }

        else if (this.platform != Platform.PC && androidPlugin != null)
        {
            if (this.features.activeCamera == CameraType.RGB_VideoTexture
                || this.features.activeCamera == CameraType.IRAndRGB_Android)
            {
                tex = androidPlugin.GetRGBImage();
            }
        }

        return tex;
    }

    public void DisplayOn()
    {
        if (pcPlugin != null) pcPlugin.TurnLCDScreenOn();
    }

    public void DisplayOff()
    {
        if (pcPlugin != null) pcPlugin.TurnLCDScreenOff();
    }

    IEnumerator CaptureOnStart()
    {
        yield return new WaitForSeconds(0.5f);
        Capture();
    }

    void CaptureCommands()
    {
        if (camCaptureScript != null)
        {
            if (videoCaptureSettings.captureCommand == CaptureCommand.spacebar)
            {
                if (Input.GetKeyUp(KeyCode.Space))
                {
                    Capture();
                }
            }

            else if (videoCaptureSettings.captureCommand == CaptureCommand.enter)
            {
                if (Input.GetKeyUp(KeyCode.Return))
                {
                    Capture();
                }
            }
        }
    }

   public void Capture()
    {
        if (!recording)
        {
            camCaptureScript.StartVideoRecording();
            recording = true;
        }

        else
        {
            camCaptureScript.StopVideoRecording();
            recording = false;
        }
    }

    private void Update()
    {
       if (features.activeCamera == CameraType.RGB_VideoCapture) CaptureCommands();
       if(raycastPos != null) RaycastDetection();
    }
}


