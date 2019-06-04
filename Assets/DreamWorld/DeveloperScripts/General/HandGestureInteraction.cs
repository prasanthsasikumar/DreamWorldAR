using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


public class HandGestureInteraction : MonoBehaviour {

    public UnityEvent focusEvent;
    public UnityEvent unfocusEvent;
    public UnityEvent onClickEvent;
    public UnityEvent onOpenPalmEvent;
    public UnityEvent onHoldEvent;
    public UnityEvent onReleaseEvent;

    //public UnityEvent onSelectEvent;

    void OnFocus()
    {
        if (this.enabled == false) return;
        if (focusEvent != null)
        {
            focusEvent.Invoke();
        }
    }

    void Unfocus()
    {
        if (this.enabled == false) return;
        if (unfocusEvent != null)
        {
            unfocusEvent.Invoke();
        }
    }

    void OnClick()
    {
        if (this.enabled == false) return;
        if (onClickEvent != null)
        {
            onClickEvent.Invoke();
        }
    }

    void OnOpenPalm()
    {
        if (this.enabled == false) return;
        if (onOpenPalmEvent != null)
        {
            onOpenPalmEvent.Invoke();
        }
    }

    void OnHold()
    {
        if (this.enabled == false) return;
        if (onHoldEvent != null)
        {
            onHoldEvent.Invoke();
        }
    }

    void OnRelease()
    {

        if (this.enabled == false) return;
        if (onReleaseEvent != null)
        {
            onReleaseEvent.Invoke();
        }
    }
}
