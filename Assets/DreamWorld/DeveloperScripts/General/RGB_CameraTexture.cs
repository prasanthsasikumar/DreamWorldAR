using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RGB_CameraTexture : MonoBehaviour {

    public Renderer[] renderers; //add all the renderers you want here and this script will apply the RGB texture to it
    private Texture2D rgbTex;
    private bool textureFound;
    private float scaleFactor = 0.00008f;
   
	void Start () {

	}

	void GetSize()
    {
        float width = DWCameraRig.Instance.RGBTexture().width;
        float height = DWCameraRig.Instance.RGBTexture().height;

        foreach (Renderer rend in renderers)
        {
           if(rend != null && rend.GetComponent<MeshFilter>().name == "Plane")
           rend.transform.localScale = new Vector3(width * scaleFactor, 1.0f, height * scaleFactor);
        }

        textureFound = true;
    }

	void Update () {

        rgbTex = DWCameraRig.Instance.RGBTexture();
        if (rgbTex != null)
        {
            if (renderers.Length > 0)
            {
                if (!textureFound) GetSize();

                foreach (Renderer rend in renderers)
                {
                    if (rend != null)
                    {
                        rend.material.mainTexture = rgbTex;
                        rgbTex.Apply();
                    }
                }
            }
        }		
	}
}
