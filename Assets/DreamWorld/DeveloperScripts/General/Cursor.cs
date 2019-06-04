using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cursor : MonoBehaviour {

    public enum CursorType { Head, Phone}
    public CursorType type;
    public enum CursorMode { Fixed, Dynamic, NormalFacing} //fixed remains where it is, Dynamic moves to objects it collides with, NormalsFacing rotates to face the direction of the mesh normals.
    public CursorMode mode;
    public float startDist = 2.0f;
    public float cursorMoveSpeed = 5.0f;
    public float objectOffset = 0.2f;
    public GameObject cursorOnGo;
    public GameObject cursorOffGo;
    private Transform cursorCenter;
    private Vector3 startPos;
    private Quaternion startRot;
    private Vector3 movePos;
    private bool objectHit;
    private PhoneController phoneControl;

    void Start()
    {
        if (type == CursorType.Phone) GetPhone();
    }

    public void ColliderHit()
    {
        objectHit = true;

        if(cursorOffGo != null) cursorOffGo.SetActive(false);
        if (cursorOnGo != null) cursorOnGo.SetActive(true);        
    }

    public void NoCollider()
    {

        if (cursorOffGo != null) cursorOffGo.SetActive(true);
        if (cursorOnGo != null) cursorOnGo.SetActive(false);

        objectHit = false;

    }

    void CursorPos()
    {
        if (this.mode == CursorMode.Dynamic)
        {
           if (objectHit) {

                if(this.type == CursorType.Head) movePos = DWCameraRig.Instance.GetHitInfo().point - this.cursorCenter.forward.normalized * objectOffset;
                else if (this.type == CursorType.Phone) movePos = phoneControl.GetTargetInfo().point - this.cursorCenter.forward.normalized * objectOffset;
                this.transform.position = Vector3.Lerp(this.transform.position, movePos, Time.deltaTime * cursorMoveSpeed);
            }

           else
            {
                movePos = this.startPos;
                this.transform.localPosition = Vector3.Lerp(this.transform.localPosition, movePos, Time.deltaTime * cursorMoveSpeed);
            }
        }

        else if (this.mode == CursorMode.NormalFacing)
        {
            if (objectHit)
            {
                Vector3 newDir = new Vector3(0,0,0);
                if (this.type == CursorType.Head) newDir = DWCameraRig.Instance.GetHitInfo().normal;
                else if (this.type == CursorType.Phone) newDir = phoneControl.GetTargetInfo().normal;
                transform.rotation = Quaternion.FromToRotation(Vector3.forward, newDir);

                if (this.type == CursorType.Head) movePos = DWCameraRig.Instance.GetHitInfo().point + newDir * objectOffset;
                else if (this.type == CursorType.Phone) movePos = phoneControl.GetTargetInfo().point + newDir * objectOffset;
                this.transform.position = Vector3.Lerp(this.transform.position, movePos, Time.deltaTime * cursorMoveSpeed);

            }

            else
            {
                movePos = this.startPos;
                this.transform.localPosition = Vector3.Lerp(this.transform.localPosition, movePos, Time.deltaTime * cursorMoveSpeed);
                this.transform.rotation = cursorCenter.rotation;
            }
        }
    }

    void FocusCursor()
    {
        if(cursorCenter == null)
        {
            if (DWCameraRig.Instance.GetFocusCenter() != null)
            {
                this.transform.SetParent(GameObject.FindGameObjectWithTag("HeadRotation").transform);
                this.cursorCenter = DWCameraRig.Instance.GetFocusCenter();

                this.transform.position = cursorCenter.position;
                this.transform.rotation = cursorCenter.rotation;
                this.transform.position += transform.forward * startDist;
                this.startPos = this.transform.localPosition;
            }
        }

        else if (cursorCenter != null)
        {
            if (!objectHit && DWCameraRig.Instance.GetHitInfo().collider != null) ColliderHit();
            else if (objectHit && DWCameraRig.Instance.GetHitInfo().collider == null) NoCollider();
            if (this.mode == CursorMode.Dynamic || this.mode == CursorMode.NormalFacing) CursorPos();
        }
    }

    void PhoneCursor()
    {
        if (cursorCenter != null)
        {
            if (!objectHit && phoneControl.GetTargetInfo().collider != null) ColliderHit();
            else if (objectHit && phoneControl.GetTargetInfo().collider == null) NoCollider();
            if (this.mode == CursorMode.Dynamic || this.mode == CursorMode.NormalFacing) CursorPos();
        }
    }

    void GetPhone()
    {
        GameObject phone = GameObject.FindGameObjectWithTag("PhoneController");
        this.transform.SetParent(phone.transform);
        this.cursorCenter = phone.transform;
        this.phoneControl = phone.GetComponent<PhoneController>();
        this.transform.position = cursorCenter.position;
        this.transform.rotation = cursorCenter.rotation;
        this.transform.position += transform.forward * startDist;
        this.startPos = this.transform.localPosition;
    }

    private void Update()
    {
       
        if (this.type == CursorType.Head) FocusCursor();
        else if (this.type == CursorType.Phone) PhoneCursor();

    }
}
