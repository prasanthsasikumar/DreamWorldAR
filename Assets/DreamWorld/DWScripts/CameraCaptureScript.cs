/********************************************************************/
/* Remember to DISABLE Hand Gesture and RGB camera in the SDK!!!!!  */
/*        Also, do remember to specify video saving path!           */
/********************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using System.IO;
using System.Threading;
using System.Text;
// Using DllImport
using System.Runtime.InteropServices;

public class CameraCaptureScript : MonoBehaviour {
    [DllImport("DWOverlayPlugin")]
    private static extern IntPtr GetOverlayPlugin(int width, int height, string path);

    [DllImport("DWOverlayPlugin")]
    private static extern IntPtr DestroyInstance(IntPtr obj);

    [DllImport("DWOverlayPlugin")]
    private static extern void UpdateContent(IntPtr obj, byte[] bufImg, byte[] bufAlpha);

    [DllImport("DWOverlayPlugin")]
    private static extern int DebugMSG(IntPtr obj);

    [DllImport("DWOverlayPlugin")]
    private static extern void StartRecording(IntPtr obj);

    [DllImport("DWOverlayPlugin")]
    private static extern void StopRecording(IntPtr obj);

    private Color[] colors;
    private RenderTexture rt;
    private int resWidth = 1280;
    private int resHeight = 720;
    private int FPS = 1000;
    private Camera mCamera;
    Texture2D screenShot;
    byte[] bufImg;
    byte[] bufAlpha;
    System.Object imageLock;
    
    private Thread m_thread = null;
    bool running = true;

    private IntPtr dwOverlayPluginObj = IntPtr.Zero;

    private enum Resolution{ Low_360p, Mid_540p, High_720p };
    private Resolution resolution;
    private string VideoPath;

    void Start () {

        mCamera = this.GetComponent<Camera>();
        rt = new RenderTexture(resWidth, resHeight, 1);
        mCamera.targetTexture = rt;
        screenShot = new Texture2D(resWidth, resHeight, TextureFormat.ARGB32, false);
        imageLock = new System.Object();
        colors = new Color[resWidth * resHeight];
        dwOverlayPluginObj = GetOverlayPlugin(resWidth, resHeight, VideoPath);

        m_thread = new Thread(() =>
        {
            Communicate();
        }
        );
        m_thread.Start();
    }

    void Communicate()
    {
        bufImg = new byte[resWidth * resHeight * 3];
        bufAlpha = new byte[resWidth * resHeight];
        while (running)
        {           
            lock (imageLock)
            {
                UpdateMessage();
                UpdateContent(dwOverlayPluginObj, bufImg, bufAlpha);
            }            
            Thread.Sleep(1000 / FPS);
        }
    }

    void UpdateMessage()
    {
        for (int i = 0; i < colors.Length; ++i)
        {
            bufImg[3 * i] = (byte)((colors[i].b * 255));
            bufImg[3 * i + 1] = (byte)((colors[i].g * 255));
            bufImg[3 * i + 2] = (byte)((colors[i].r * 255));
            if (colors[i].a != 0)
            {
                bufAlpha[i] = (byte)((colors[i].a * 255));
            }
            else if (bufImg[3 * i] != 0 || bufImg[3 * i + 1] != 0 || bufImg[3 * i + 2] != 0)
            {
                bufAlpha[i] = (byte)(255);
            }
            else if (bufImg[3 * i] == 0 && bufImg[3 * i + 1] == 0 && bufImg[3 * i + 2] == 0)
            {
                bufAlpha[i] = (byte)(0);
            }

        }
    }

    void UpdateColors()
    {
        RenderTexture.active = rt;
        screenShot.ReadPixels(new UnityEngine.Rect(0, 0, resWidth, resHeight), 0, 0, false);
        lock (imageLock)
        {
            colors = screenShot.GetPixels();
        }
        Thread.Sleep(1);
    }

    void Update () {
        UpdateColors();        
    }

    void OnDestroy()
    {
        StopRecording(dwOverlayPluginObj);
        DestroyInstance(dwOverlayPluginObj);
        running = false;
    }

    /********************* Public Methods **************************/
    public void CapturePath(string path)
    {
        VideoPath = path;
    }

    public void CaptureSettings(int resolution)
    {
        if (resolution == 1)
        {
            resWidth = 640;
            resHeight = 360;
        }
        else if (resolution == 2)
        {
            resWidth = 960;
            resHeight = 540;
        }
        else if (resolution == 3)
        {
            resWidth = 1280;
            resHeight = 720;
        }
    }

public void StartVideoRecording()
    {
        StartRecording(dwOverlayPluginObj);
    }

    public void StopVideoRecording()
    {
        StopRecording(dwOverlayPluginObj);
    }
}
