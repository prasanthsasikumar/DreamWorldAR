using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhoneController : MonoBehaviour {

    public enum RotationReset { doubleTap, hold, none }
    public RotationReset rotationReset;
    private Transform cameraRig;
    private Gyroscope gyro;
    private Quaternion gyroRot;
    private Quaternion targetQuat;

    private float offset = 0.0f;
    //private bool offSetInitialized;
    private float xDir = 1.0f;
    private float yDir = -1.0f;
    private float zDir = -1.0f;

    private GameObject selectedObject;
    private RaycastHit targetInfo;

    //rotation reset
    private float currentHoldTime;
    private float holdResetTime = 4.0f;
    private float tapResetTime = 1.0f;
    private bool tapReset;

    void Start () {

        Input.gyro.enabled = true;
        Input.compass.enabled = false;
        cameraRig = GameObject.FindGameObjectWithTag("CameraRig").transform;

        StartCoroutine(InitialReset());
    }



    public void ResetController()
    {
        offset = gyroRot.eulerAngles.z + cameraRig.rotation.eulerAngles.y;
    }

    public bool EnableGyro()
    {
        if (SystemInfo.supportsGyroscope)
        {
            gyro = Input.gyro;

            gyro.enabled = true;

            return true;
        }

        else return false;
    }

    public void PhoneRotation()
    {

        gyroRot = Input.gyro.attitude;
        /*
        if (!offSetInitialized)
        {
            ResetController();
            offSetInitialized = true;
        }
        */

        float x = gyroRot.x;
        float y = gyroRot.y;
        float z = gyroRot.z;
        float w = gyroRot.w;
        Quaternion newQuat = new Quaternion(y, z, x, w);

        targetQuat = Quaternion.Euler(newQuat.eulerAngles.x * xDir, (newQuat.eulerAngles.y - offset) * yDir, newQuat.eulerAngles.z * zDir);
        this.transform.rotation = targetQuat;

    }

    void PhoneRaycast()
    {
        //RayCast Debug

       /* Ray ray = new Ray(this.transform.localPosition, this.transform.forward);
        Debug.DrawRay(this.transform.localPosition, this.transform.forward * 500f, Color.green); */

        if (Physics.Raycast(this.transform.localPosition, this.transform.forward, out targetInfo))
        {
            if (selectedObject != null && targetInfo.collider.gameObject != selectedObject)
            {
                selectedObject.SendMessage("OnTarget", SendMessageOptions.DontRequireReceiver);
                selectedObject = targetInfo.collider.gameObject;
                selectedObject.SendMessage("OffTarget", SendMessageOptions.DontRequireReceiver);

            }
            else if (selectedObject == null)
            {
                selectedObject = targetInfo.collider.gameObject;
                selectedObject.SendMessage("OnTarget", SendMessageOptions.DontRequireReceiver);
            }
        }
        else
        {
            if (selectedObject != null)
            {
                selectedObject.SendMessage("OffTarget", SendMessageOptions.DontRequireReceiver);
                selectedObject = null;
            }
        }
    }


    public RaycastHit GetTargetInfo()
    {
        return targetInfo;
    }

    void ResetRotation()
    {
        if (this.rotationReset == RotationReset.doubleTap)
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (!tapReset) StartCoroutine(ResetTimer());
                else ResetController();
            }
        }

        else if(this.rotationReset == RotationReset.hold)
        {
            if (Input.touchCount == 1)
            {
                currentHoldTime += 0.1f;

                if (currentHoldTime >= holdResetTime)
                {
                    ResetController();
                }
            }

            else currentHoldTime = 0.0f;
        }
    }

    IEnumerator ResetTimer()
    {
        tapReset = true;
        yield return new WaitForSeconds(tapResetTime);
        tapReset = false;

    }

    IEnumerator InitialReset()
    {
        yield return new WaitForSeconds(1.0f);
        ResetController();
    }

    // Update is called once per frame
    void Update () {

        PhoneRotation();
        PhoneRaycast();
        if(rotationReset != RotationReset.none) ResetRotation();
    }
}
