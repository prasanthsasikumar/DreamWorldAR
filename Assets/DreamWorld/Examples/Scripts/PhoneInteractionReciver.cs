using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhoneInteractionReciver : MonoBehaviour {

   
    public TextMesh inputText;
    public Transform centerPos;
    private Renderer rend;
    private int currentGeo;
    private Vector3 newPos;
    private bool holding;
    private bool targeted;
    
    // Use this for initialization
	void Start () {

          newPos = this.transform.position;
         rend = this.GetComponent<Renderer>();
 	}

    public void Target()
    {
        rend.material.color = Color.yellow;
        targeted = true;
    }

    public void OffTarget()
    {
        rend.material.color = Color.white;
        targeted = false;
    }

    public void Tapped()
    {
        inputText.text = "Tap";
    }

    public void Holding()
    {
        if(targeted) holding = true;
        inputText.text = "Holding";
    }

    public void Released()
    {
        if (holding)
        {
            this.newPos = centerPos.transform.forward * 1.3f;
            holding = false;
        }

        inputText.text = "Released";
    }

    public void SwipedLeft()
    {

        inputText.text = "SwipedLeft";

    }

    public void SwipedRight()
    {
        inputText.text = "SwipedRight";
    }

    public void SwipedUp()
    {
        inputText.text = "SwipedUp";
    }

    public void SwipedDown()
    {
        inputText.text = "SwipedDown";
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
        MoveGeometry();
    }
}
