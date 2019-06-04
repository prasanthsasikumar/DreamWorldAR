using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeometryInteractionScript : MonoBehaviour {

    public TextMesh interactionText;
    public ParticleSystem particles;
    public Transform centerPos;
    public Transform returnPos;
    private Transform focusCenter;
    private Renderer geometryRend;
    private Vector3 newPos;
    private bool holding;
    private Color col;
    private bool initilized; 

    void InitializeGeometry()
    {  
        this.focusCenter = DWCameraRig.Instance.GetFocusCenter();

        if (this.centerPos != null)
        {
            this.centerPos.position = focusCenter.position;
            this.centerPos.rotation = focusCenter.rotation;
          //  this.centerPos.SetParent(DWCameraRig.Instance.transform);
            this.centerPos.position += centerPos.transform.forward * 0.7f;
        }

        if (this.returnPos != null)
        {
            this.returnPos.position = focusCenter.position;
            this.returnPos.rotation = focusCenter.rotation;
           // this.returnPos.SetParent(DWCameraRig.Instance.transform);
            this.returnPos.position += returnPos.transform.forward * 1.1f;
        }

        newPos = this.transform.position;
        geometryRend = this.GetComponent<Renderer>();
        col = Color.red;
        geometryRend.material.color = col;
        if (particles != null) particles.startColor = col;
        initilized = true;
    }

    public void FocusOn()
    {

       interactionText.text = "FocusOn";
    }

    public void FocusOff()
    {
        interactionText.text = "FocusOff";
    }

    public void Clicked()
    {
       if(this.particles != null) particles.Play();
       interactionText.text = "Clicked";
    }

    public void OpenPalmed()
    {
        if (col == Color.red) col = Color.green;
        else if (col == Color.green) col = Color.blue;
        else if (col == Color.blue) col = Color.red;

        particles.startColor = col;
        geometryRend.material.color = col;

        interactionText.text = "OpenPalm";
    }

    public void Held()
    {
        interactionText.text = "Holding";
        holding = true;
    }

    public void LetGo()
    {
        interactionText.text = "Released";
        newPos = returnPos.position;
        holding = false;
    }

    public void MoveGeometry()
    {
        if (holding)
        {

            this.transform.position = Vector3.Lerp(this.transform.position, centerPos.position, Time.deltaTime * 15.0f);
        }

        else
        {
            this.transform.position = Vector3.Lerp(this.transform.position, newPos, Time.deltaTime * 15.0f);
        }
    }

    void Update()
    {
        if (!initilized && DWCameraRig.Instance.GetFocusCenter() != null) InitializeGeometry();
        if(initilized) MoveGeometry(); 
    }
}
