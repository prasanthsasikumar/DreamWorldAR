using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using DreamWorldDLL;


[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class Distortion : MonoBehaviour {

    private Camera rsCam;
    private bool leftEye;
    private bool android;
    private Material meshMat;
    private Transform meshCenter;
    private Transform meshCamera;

    private int xSize;
    private int ySize;

    private Vector3 scale = new Vector3(1.0f, -1.0f, 1.0f);
    private Vector3 rotation;
    private float[] dataFloatX;
    private float[] dataFloatY;
	private Mesh mesh;
	private Vector3[] vertices;
    private byte[] dataXBytes;
    private byte[] dataYBytes;
    private int curPlatform;
    private Quaternion leftCamRot = Quaternion.Euler(0,0,0);
    private Quaternion rightCamRot = Quaternion.Euler(0, 0, 0);
    private Quaternion centerRot = Quaternion.Euler(0, 0, 0);
    
    //loadingData
    private CalibrationData pcPlugin;
    private AndroidCalibration androidCalib;

    public void BuildMesh(bool left, int plat, Material mat)
    {
      
        curPlatform = plat;

        if (left) leftEye = true;

        if (curPlatform != 0)
        {
            androidCalib = new AndroidCalibration();
            androidCalib.UpdateCalibration();
            this.xSize = androidCalib.GetGridSizeX();
            this.ySize = androidCalib.GetGridSizeY();
            android = true;
        }
        else if (curPlatform == 0)
        {
            pcPlugin = new CalibrationData();
            this.xSize = pcPlugin.GridX();
            this.ySize = pcPlugin.GridY();
        }
        meshMat = mat;

        if (android == false)
        {
            this.rotation = new Vector3(0.0f, 0.0f, -90.0f);         
        }

        else if (android == true)
        {
            if (leftEye) this.rotation = new Vector3(0.0f, 0.0f, -90.0f);
            else this.rotation = new Vector3(0.0f, 0.0f, 90.0f);
        }

        GenerateMesh();
        WarpMesh();
        FindCenters();
        CreateScreenCamera();

        Destroy(GetComponent<Distortion>());
    }


    private void GenerateMesh () {
        
        if (this.curPlatform == 0)
        {
            if (leftEye) {

                dataFloatX = pcPlugin.DistXLeft();
                dataFloatY = pcPlugin.DistYLeft();
            }

            else {

                dataFloatX = pcPlugin.DistXRight();
                dataFloatY = pcPlugin.DistYRight();
            }            
        }

        else
        {
            if(leftEye) {
            dataFloatX = androidCalib.GetDistCorrectXLeft();
            dataFloatY = androidCalib.GetDistCorrectYLeft();
            }

            else {
            dataFloatX = androidCalib.GetDistCorrectXRight();
            dataFloatY = androidCalib.GetDistCorrectYRight();
            }
        }

        GetComponent<MeshFilter>().mesh = mesh = new Mesh();
        GetComponent<MeshRenderer>().material = meshMat;
		mesh.name = "Procedural Grid";

		vertices = new Vector3[(xSize + 1) * (ySize + 1)];
        Vector2[] uv = new Vector2[vertices.Length];
		Vector4[] tangents = new Vector4[vertices.Length];
		Vector4 tangent = new Vector4(1f, 0f, 0f, -1f);
		for (int i = 0, y = 0; y <= ySize; y++) {
			for (int x = 0; x <= xSize; x++, i++) {
				vertices[i] = new Vector3(x, y);
				uv[i] = new Vector2((float)x / xSize, (float)y / ySize);
				tangents[i] = tangent;
			}
		}
		mesh.vertices = vertices;
		mesh.uv = uv;
		mesh.tangents = tangents;

		int[] triangles = new int[xSize * ySize * 6];
		for (int ti = 0, vi = 0, y = 0; y < ySize; y++, vi++) {
			for (int x = 0; x < xSize; x++, ti += 6, vi++) {
				triangles[ti] = vi;
				triangles[ti + 3] = triangles[ti + 2] = vi + 1;
				triangles[ti + 4] = triangles[ti + 1] = vi + xSize + 1;
				triangles[ti + 5] = vi + xSize + 2;
			}
		}
		mesh.triangles = triangles;
		mesh.RecalculateNormals();
        this.transform.localScale = scale;

    }

    void WarpMesh()
    { 
        for (int i = 0; i < vertices.Length; i++)
        {
           vertices[i] = new Vector3(dataFloatX[i], dataFloatY[i], vertices[i].z);
        }

        mesh.vertices = vertices;
        mesh.RecalculateBounds();
    }

    void FindCenters()
    {
        GameObject centerPosition = new GameObject();
        meshCenter = centerPosition.transform;
        if(this.leftEye) meshCenter.name = "LeftMeshCenter";
        else meshCenter.name = "RightMeshCenter";
        meshCenter.transform.SetParent(this.transform.parent);
        this.transform.SetParent(meshCenter.transform);
        this.transform.localPosition = new Vector3(0.0f, 0.0f, 0.0f);
        this.transform.localRotation = Quaternion.identity;
        if(this.leftEye) meshCenter.localPosition = new Vector3(-0.03f, 0.0f, 0.0f);
        else meshCenter.localPosition = new Vector3(0.03f, 0.0f, 0.0f);

        centerRot.eulerAngles = rotation;
        this.meshCenter.transform.localRotation = centerRot;
    }

    void CreateScreenCamera()
    {
        GameObject newCam = new GameObject();
        Camera rsCam = newCam.AddComponent<Camera>();
       
        meshCamera = newCam.transform;
        rsCam.orthographic = true;
        if (android)
        {
            if (curPlatform == 1) rsCam.orthographicSize = androidCalib.GetEyeCamSizeAndroid_S8();
            else if (curPlatform == 2) rsCam.orthographicSize = androidCalib.GetEyeCamSizeAndroid_Mate10_DW();
            else if (curPlatform == 3) rsCam.orthographicSize = androidCalib.GetEyeCamSizeAndroid_Mate10_PRO();
        }

        else rsCam.orthographicSize = pcPlugin.CamSize();

        rsCam.nearClipPlane = 0.0f;
        rsCam.farClipPlane = 0.01f;
        rsCam.clearFlags = CameraClearFlags.SolidColor;
        rsCam.backgroundColor = Color.black;
        rsCam.useOcclusionCulling = false;
        meshCamera.transform.SetParent(meshCenter);

        if (this.leftEye)
        {
            if (this.android)
            {
                if (curPlatform == 1) rsCam.transform.localPosition = androidCalib.GetLeftEyeCamPosAndroid_S8();
                else if (curPlatform == 2) rsCam.transform.localPosition = androidCalib.GetLeftEyeCamPosAndroid_Mate10_DW();
                else if (curPlatform == 3) rsCam.transform.localPosition = androidCalib.GetLeftEyeCamPosAndroid_Mate10_PRO();

                leftCamRot.eulerAngles = androidCalib.GetLeftEyeCamRotAndroid();
            }

            else

            {
                rsCam.transform.localPosition = pcPlugin.LeftCamPos();
                leftCamRot.eulerAngles = pcPlugin.LeftCamRot();  
            } 

            rsCam.transform.localRotation = leftCamRot;
        }

        else if(!leftEye)
        {
            if (this.android)
            {
                if (curPlatform == 1) rsCam.transform.localPosition = androidCalib.GetRightEyeCamPosAndroid_S8();
                else if (curPlatform == 2) rsCam.transform.localPosition = androidCalib.GetRightEyeCamPosAndroid_Mate10_DW();
                else if (curPlatform == 3) rsCam.transform.localPosition = androidCalib.GetRightEyeCamPosAndroid_Mate10_PRO();

                rightCamRot.eulerAngles = androidCalib.GetRightEyeCamRotAndroid();
            }

            else
            {
                rsCam.transform.localPosition = pcPlugin.RightCamPos();
                rightCamRot.eulerAngles = pcPlugin.RightCamRot();
            }

            rsCam.transform.localRotation = rightCamRot;
        }

        if (this.leftEye)
        {
            meshCamera.name = "LeftEyeCam";

           if (android == false) rsCam.rect = new Rect(0.0f, 0.0f, 0.5f, 1.0f);
           else if (android == true) rsCam.rect = new Rect(0.0f, 0.0f, 0.5f, 1.0f);
           
        }

        else
        {
            meshCamera.name = "RightEyeCam";
            
            if (android == false) rsCam.rect = new Rect(0.5f, 0.0f, 0.5f, 1.0f);
            else if (android == true) rsCam.rect = new Rect(0.5f, 0.0f, 0.5f, 1.0f);

        }

        Graphics.DrawMeshNow(mesh, new Vector3(0, 0, 0), Quaternion.identity);
      
    } 

}