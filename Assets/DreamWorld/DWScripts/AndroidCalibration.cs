using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public class AndroidCalibration  {

    private AndroidJavaClass jc;
    private AndroidJavaObject jo;

    // calibration data
    private float ipd;
    private int gridSizeX;
    private int gridSizeY;
    private float cameraTilt;
    private float fov;
    private Vector2 rtResolutionAndroid;
    private Vector3 leftEyeCamRotAndroid;
    private Vector3 rightEyeCamRotAndroid;
    //Samsung//
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
    // Use this for initialization
    void Awake () {


	}


    public void UpdateCalibration()
    {
  
        jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        jo = jc.GetStatic<AndroidJavaObject>("currentActivity");

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

    public float GetEyeCamSizeAndroid_Mate10_DW()
    {
        return eyeCamSizeAndroid_Mate10_DWHub;
    }

    public Vector3 GetLeftEyeCamPosAndroid_Mate10_DW()
    {
        return leftEyeCamPosAndroid_Mate10_DWHub;
    }

    public Vector3 GetRightEyeCamPosAndroid_Mate10_DW()
    {
        return rightEyeCamPosAndroid_Mate10_DWHub;
    }


    public float GetEyeCamSizeAndroid_Mate10_GH()
    {
        return eyeCamSizeAndroid_Mate10_GreyHub;
    }

    public Vector3 GetLeftEyeCamPosAndroid_Mate10_GH()
    {
        return leftEyeCamPosAndroid_Mate10_GreyHub;
    }

    public Vector3 GetRightEyeCamPosAndroid_Mate10_GH()
    {
        return rightEyeCamPosAndroid_Mate10_GreyHub;
    }



    public float GetEyeCamSizeAndroid_Mate10_PRO()
    {
        return eyeCamSizeAndroid_Mate10_pro;
    }

    public Vector3 GetLeftEyeCamPosAndroid_Mate10_PRO()
    {
        return leftEyeCamPosAndroid_Mate10_pro;
    }

    public Vector3 GetRightEyeCamPosAndroid_Mate10_PRO()
    {
        return rightEyeCamPosAndroid_Mate10_pro;
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


    public float[] GetDistCorrectXLeft()
    {
        return distCorrectLeftX;
    }

    public float[] GetDistCorrectYLeft()
    {
        return distCorrectLeftY;
    }

    public float[] GetDistCorrectXRight()
    {
        return distCorrectRightX;
    }

    public float[] GetDistCorrectYRight()
    {
        return distCorrectRightY;
    }

}
