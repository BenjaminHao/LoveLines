﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwipeManager : MonoBehaviour
{

    private bool swipingLeft, swipingRight, swipingUp, swipingDown, swipedLeft, swipedRight, swipedUp, swipedDown;
    private bool isDraging = false;
    public Vector2 startTouch, perfectPos, swipeDelta;
    public float minSwipeDistance = 5.0f;

    private void Update()
    {
        swipingLeft = swipingRight = swipingUp = swipingDown = swipedLeft = swipedRight = swipedUp = swipedDown = false;

        #region Standalone Inputs
        if (Input.GetMouseButtonDown(0))
        {
            isDraging = true;
            startTouch = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            perfectPos = new Vector2(Mathf.Round(startTouch.x) + 0.5f,
                                     Mathf.Round(startTouch.y) + 0.5f);
        }
        else if (Input.GetMouseButtonUp(0))
        {
            Reset();
        }
        #endregion

        #region Mobile Inputs
        if (Input.touches.Length > 0)
        {
            if (Input.touches[0].phase == TouchPhase.Began)
            {
                isDraging = true;
                startTouch = Camera.main.ScreenToWorldPoint(Input.touches[0].position);
                perfectPos = new Vector2(Mathf.Round(startTouch.x) + 0.5f,
                                         Mathf.Round(startTouch.y) + 0.5f);
            }
            else if (Input.touches[0].phase == TouchPhase.Ended || Input.touches[0].phase == TouchPhase.Canceled)
            {
                Reset();
            }
        }
        #endregion

        // Caculate the distance
        if (isDraging)
        {
            if (Input.touches.Length > 0)
                swipeDelta = (Vector2)Camera.main.ScreenToWorldPoint(Input.touches[0].position) - perfectPos;
            else if (Input.GetMouseButton(0))
                swipeDelta = (Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition) - perfectPos;
        }

        // Did we cross the deadzone?
        if (swipeDelta.magnitude > minSwipeDistance)
        {
            // Which direction?
            float x = swipeDelta.x;
            float y = swipeDelta.y;
            if (Mathf.Abs(x) > Mathf.Abs(y) && isDraging)
            {
                // left or right
                if (x < 0) swipingLeft = true;
                else swipingRight = true;
            }
            else if (Mathf.Abs(x) < Mathf.Abs(y) && isDraging)
            {
                // up or down
                if (y < 0) swipingDown = true;
                else swipingUp = true;
            }
            if (Mathf.Abs(x) > Mathf.Abs(y) && !isDraging)
            {
                // left or right
                if (x < 0) swipedLeft = true;
                else swipedRight = true;
                swipeDelta = Vector2.zero;
            }
            else if (Mathf.Abs(x) < Mathf.Abs(y) && !isDraging)
            {
                // up or down
                if (y < 0) swipedDown = true;
                else swipedUp = true;
                swipeDelta = Vector2.zero;
            }
        }
    }

    private void Reset()
    {
        isDraging = false;
        startTouch = Vector2.zero;
        DottedLine.Instance.DestroyDottedLine();
    }

    public Vector2 SwipeDelta
    {
        get { return swipeDelta; }
    }

    public bool SwipingLeft
    {
        get { return swipingLeft; }
    }

    public bool SwipingRight
    {
        get { return swipingRight; }
    }

    public bool SwipingUp
    {
        get { return swipingUp; }
    }

    public bool SwipingDown
    {
        get { return swipingDown; }
    }

    public bool SwippedLeft
    {
        get { return swipedLeft; }
    }

    public bool SwippedRight
    {
        get { return swipedRight; }
    }

    public bool SwipedUp
    {
        get { return swipedUp; }
    }

    public bool SwipedDown
    {
        get { return swipedDown; }
    }
}