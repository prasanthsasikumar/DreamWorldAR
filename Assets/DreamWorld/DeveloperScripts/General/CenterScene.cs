using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CenterScene : MonoBehaviour {

    public enum CenterControl { enter, backSpace, none }
    public CenterControl centerScene;
    private Transform headRot;
    // Use this for initialization
    void Start()
    {
        headRot = GameObject.FindGameObjectWithTag("HeadRotation").transform;
    }
	
	public void ResetCenter()
    {
        Vector3 newRot = headRot.eulerAngles;
        newRot.x = 0.0f;
        newRot.z = 0.0f;
        this.transform.eulerAngles = newRot;
    }

    void Controls()
    {
        if (centerScene == CenterControl.enter)
        {
            if (Input.GetKeyUp(KeyCode.Return)) ResetCenter();
        }

        else if (centerScene == CenterControl.backSpace)
        {
            if (Input.GetKeyUp(KeyCode.Backspace)) ResetCenter();
        }
    }

    void Update()
    {
        if (centerScene != CenterControl.none) Controls();
    }
}
