using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DreamWorldDLL;

public class RigSetup : MonoBehaviour {

    private Camera editorCam;
    private Camera rtLeftCam;
    private Camera rtRightCam;
    private Material leftMeshMat;
    private Material rightMeshMat;

    private CalibrationData pcPlugin;
    private AndroidCalibration androidPlugin;

    private Quaternion cameraRotation = Quaternion.Euler(0, 0, 0);
    private RenderTexture rtLeft;
    private RenderTexture rtRight;
    private float ipd;
    private Vector3 leftEyeCamPos;
    private Vector3 rightEyeCamPos;
    private Vector2 rtResolutionPC;
    private Vector2 rtResolutionAndroid;
    private float cameraTilt;
    private float fov;
    private float eyeHeight = 0.15f;
    private float eyeDepth = 0.1f;
    private int platform;
    private int tracking;

    public void Initialization(int plat, int track, bool capture)
    {
        editorCam = this.GetComponent<Camera>();
        platform = plat;
        tracking = track;

        if (platform == 0)
        {
            pcPlugin = new CalibrationData();
        }

        else if (platform != 0)
        {
            androidPlugin = new AndroidCalibration();
            androidPlugin.UpdateCalibration();
            if (platform == 1) Screen.orientation = ScreenOrientation.LandscapeRight;
            else if (platform > 1) Screen.orientation = ScreenOrientation.LandscapeLeft;
            
        }

        RigSettings(); 
        if(!capture) Destroy(editorCam);
        RTCameraPosition();
        CreateMeshes();

        if(track == 2) this.transform.localPosition = new Vector3(0.0f, -.05f, -.03f);
        else this.transform.localPosition = new Vector3(0.0f, -eyeHeight, -eyeDepth);
        Destroy(GetComponent<RigSetup>());
    }


    void RigSettings()
    {
        if (platform == 0)
        {
            this.ipd = pcPlugin.IPD();
            this.rtResolutionPC = pcPlugin.RTRez();
            this.cameraTilt = pcPlugin.Tilt();
            this.fov = pcPlugin.FOV();

        }
        
        else 
        {
            this.ipd = androidPlugin.GetIpd();
            this.rtResolutionAndroid = androidPlugin.GetRtResolutionAndroid();
            this.cameraTilt = androidPlugin.GetCameraTilt();
            this.fov = androidPlugin.GetFov();
        }
    }

    void RTCameraPosition()
    {
        GameObject newCamLeft = new GameObject();
        newCamLeft.name = "LeftCamera";
        rtLeftCam = newCamLeft.AddComponent<Camera>();

        GameObject newCamRight = new GameObject();
        newCamRight.name = "RightCamera";
        rtRightCam = newCamRight.AddComponent<Camera>();

        rtLeftCam.enabled = true;
        rtRightCam.enabled = true;

        rtLeftCam.clearFlags = CameraClearFlags.SolidColor;
        rtLeftCam.backgroundColor = new Color(0.0f, 0.0f, 0.0f, 0.0f);
        rtLeftCam.nearClipPlane = 0.1f;
        rtLeftCam.fieldOfView = 48;

        rtRightCam.clearFlags = CameraClearFlags.SolidColor;
        rtRightCam.backgroundColor = new Color(0.0f, 0.0f, 0.0f, 0.0f);
        rtRightCam.nearClipPlane = 0.1f;
        rtRightCam.fieldOfView = 48;

        rtLeftCam.transform.SetParent(this.transform);
        rtRightCam.transform.SetParent(this.transform);

        if (tracking == 2)
        {
            rtLeftCam.transform.localPosition = new Vector3(-ipd / 2.0f, 0.0f, 0.0f);
            rtRightCam.transform.localPosition = new Vector3(ipd / 2.0f, 0.0f, 0.0f);
        }

        else
        {
            rtLeftCam.transform.localPosition = new Vector3(-ipd / 2.0f, eyeHeight, eyeDepth);
            rtRightCam.transform.localPosition = new Vector3(ipd / 2.0f, eyeHeight, eyeDepth);
        }

        cameraRotation.eulerAngles = new Vector3(cameraTilt, 0.0f, 0.0f);
        rtLeftCam.transform.localRotation = cameraRotation;
        rtRightCam.transform.localRotation = cameraRotation;

        rtLeftCam.fieldOfView = this.fov;
        rtRightCam.fieldOfView = this.fov;

        if (platform == 0)
        {
            rtLeft = new RenderTexture((int)rtResolutionPC.x, (int)rtResolutionPC.y, 24, RenderTextureFormat.ARGB32);
            rtRight = new RenderTexture((int)rtResolutionPC.x, (int)rtResolutionPC.y, 24, RenderTextureFormat.ARGB32);
        }

        else if (platform != 0)
        {
            rtLeft = new RenderTexture((int)rtResolutionAndroid.x, (int)rtResolutionAndroid.y, 24, RenderTextureFormat.ARGB32);
            rtRight = new RenderTexture((int)rtResolutionAndroid.x, (int)rtResolutionAndroid.y, 24, RenderTextureFormat.ARGB32);
        }

        rtLeftCam.targetTexture = rtLeft;
        rtRightCam.targetTexture = rtRight;

    }

    public void CreateMeshes()
    {

        leftMeshMat = new Material(Shader.Find("Unlit/Texture"));
        rightMeshMat = new Material(Shader.Find("Unlit/Texture"));
        leftMeshMat.mainTexture = rtLeft;
        rightMeshMat.mainTexture = rtRight;

        GameObject leftEyeMesh = new GameObject();
        leftEyeMesh.name = "LeftEyeMesh";
        leftEyeMesh.transform.SetParent(this.transform);
        leftEyeMesh.transform.localPosition = new Vector3(0.0f, 0.0f, 0.0f);
        leftEyeMesh.AddComponent<Distortion>();

        GameObject rightEyeMesh = new GameObject();
        rightEyeMesh.name = "RightEyeMesh";
        rightEyeMesh.transform.SetParent(this.transform);
        rightEyeMesh.transform.localPosition = new Vector3(0.0f, 0.0f, 0.0f);
        rightEyeMesh.AddComponent<Distortion>();

        leftEyeMesh.GetComponent<Distortion>().BuildMesh(true, platform, leftMeshMat);
        rightEyeMesh.GetComponent<Distortion>().BuildMesh(false, platform, rightMeshMat);

    }

}
