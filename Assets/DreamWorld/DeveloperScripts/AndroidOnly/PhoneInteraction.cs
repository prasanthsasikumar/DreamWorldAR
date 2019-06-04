using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class PhoneInteraction : MonoBehaviour {

    public UnityEvent onTargetEvent;
    public UnityEvent offTargetEvent;
    public UnityEvent tapEvent;
    public UnityEvent swipeUpEvent;
    public UnityEvent swipeDownEvent;
    public UnityEvent swipeLeftEvent;
    public UnityEvent swipeRightEvent;
    public UnityEvent onHoldEvent;
    public UnityEvent onReleaseEvent;

    private bool holding;
    private float minHoldTime = 1.0f;
    private float fingerHoldTime = 0.0f;
    private float fingerStartTime = 0.0f;
    private Vector2 fingerStartPos = Vector2.zero;
    private bool isSwipe = false;
    private float minSwipeDist = 50.0f;
    private float maxSwipeTime = 0.5f;


    // Use this for initialization
    void Start () {
      
      
    }

    void OnTarget()
    {
        if (this.enabled == false) return;
        if (onTargetEvent != null)
        {
            onTargetEvent.Invoke();
        }
    }

    void OffTarget()
    {
        if (this.enabled == false) return;
        if (offTargetEvent != null)
        {
            offTargetEvent.Invoke();
        }
    }

    void OnTap()
    {
        if (this.enabled == false) return;
        if (tapEvent != null)
        {
            tapEvent.Invoke();
        }
    }

    void SwipeUp()
    {
        if (this.enabled == false) return;
        if (swipeUpEvent != null)
        {
            swipeUpEvent.Invoke();
        }
    }

    void SwipeDown()
    {
        if (this.enabled == false) return;
        if (swipeDownEvent != null)
        {
            swipeDownEvent.Invoke();
        }
    }

    void SwipeLeft()
    {
        if (this.enabled == false) return;
        if (swipeLeftEvent != null)
        {
            swipeLeftEvent.Invoke();
        }
    }

    void SwipeRight()
    {
        if (this.enabled == false) return;
        if (swipeRightEvent != null)
        {
            swipeRightEvent.Invoke();
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

    void TouchControls()
    {
        if (Input.touchCount > 0)
        {
          
            fingerHoldTime += 0.1f;
            if (fingerHoldTime > minHoldTime && !holding)
            {
                OnHold();
                holding = true;
            }

            foreach (Touch touch in Input.touches)
            {
                switch (touch.phase)
                {
                    case TouchPhase.Began:
                        /* this is a new touch */
                        isSwipe = true;
                        fingerStartTime = Time.time;
                        fingerStartPos = touch.position;
                        break;

                    case TouchPhase.Canceled:
                        /* The touch is being canceled */
                        isSwipe = false;
                        fingerHoldTime = 0.0f;
                        if (holding) holding = false;
                        break;

                    case TouchPhase.Ended:

                        float gestureTime = Time.time - fingerStartTime;
                        float gestureDist = (touch.position - fingerStartPos).magnitude;
                        fingerHoldTime = 0.0f;

                        if (holding)
                        {
                            holding = false;
                            OnRelease();
                            break;
                        }

                        if (gestureDist < minSwipeDist && gestureTime < maxSwipeTime)
                        {
                            OnTap();
                        }

                        else if (isSwipe && gestureTime < maxSwipeTime && gestureDist > minSwipeDist)
                        {
                            Vector2 direction = touch.position - fingerStartPos;
                            Vector2 swipeType = Vector2.zero;

                            if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
                            {
                                // the swipe is horizontal:
                                swipeType = Vector2.right * Mathf.Sign(direction.x);
                            }
                            else
                            {
                                // the swipe is vertical:
                                swipeType = Vector2.up * Mathf.Sign(direction.y);
                            }

                            if (swipeType.x != 0.0f)
                            {
                                if (swipeType.x > 0.0f)
                                {
                                    // MOVE RIGHT
                                    SwipeUp();

                                }
                                else
                                {
                                    // MOVE LEFT
                                    SwipeDown();

                                }
                            }

                            if (swipeType.y != 0.0f)
                            {
                                if (swipeType.y > 0.0f)
                                {
                                    // MOVE UP
                                    SwipeLeft();
                                }
                                else
                                {
                                    // MOVE DOWN
                                    SwipeRight();
                                }
                            }

                        }

                        break;
                }
            }
        }
    }

  


    // Update is called once per frame
    void Update () {
 
        TouchControls(); 
    }
}
