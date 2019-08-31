using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    public float horizontal;
    public float vertical;
    public float mouseX;
    public float mouseY;
    public float jump;

    [Header("Mobile Properties")]
    public bool useGyro = true;
    public float swipeDeadzoneRadius = 125;

    [SerializeField]
    private bool        isDragging;
    private Vector2     startPos;
    private Vector2     swipeDir;
    public Vector3      worldDir;
    private Vector3     gyroStartOffset;

    public bool IsDragging { get { return isDragging; } }
    public Vector2 SwipeDir { get { return swipeDir; } }

    // Start is called before the first frame update
    void Start()
    {
        Input.simulateMouseWithTouches = true;
        Input.gyro.enabled = true;
        gyroStartOffset = Input.gyro.attitude.eulerAngles;
    }

    // Update is called once per frame
    void Update()
    {
        GetInputs();
    }

    void GetInputs()
    {
        // Determine if using mobile gyro tilt information or regular axis
        if (useGyro)
        {
            Vector3 offsetAttitude = Input.gyro.attitude.eulerAngles - gyroStartOffset;
            horizontal = Quaternion.Euler(offsetAttitude).y;
            vertical = Quaternion.Euler(offsetAttitude).x;
        }
        else
        {
            horizontal = Input.GetAxis("Horizontal");
            vertical = Input.GetAxis("Vertical");
        }

        // Determine if using touch to simulate mouse
        if (Input.touchCount > 0)
        {
            mouseX = Input.GetTouch(0).deltaPosition.x;
            mouseY = Input.GetTouch(0).deltaPosition.y;
        }
        else
        {
            mouseX = Input.GetAxis("Mouse X");
            mouseY = Input.GetAxis("Mouse Y");
        }

        jump = Input.GetAxis("Jump");

        ProcessSwipes();
    }

    // Process mobile continuous swipe information
    void ProcessSwipes()
    {
        #region Standalone Input

        // Collect mouse info if needed
        if (Input.GetMouseButtonDown(0))
        {
            startPos = Input.mousePosition;
            isDragging = true;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            ResetSwipeData();
        }

        #endregion

        #region Mobile Input

        if (Input.touchCount > 0)
        {
            Touch t = Input.GetTouch(0);
            if (t.phase == TouchPhase.Began)
            {
                startPos = t.position;
                isDragging = true;
            }
            else if (t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled)
            {
                ResetSwipeData();
            }
        }

        #endregion

        swipeDir = Vector2.zero;
        if (isDragging)
        {
            if (Input.GetMouseButton(0))
            {
                swipeDir = (Vector2)Input.mousePosition - startPos;
            }
            else if (Input.touchCount > 0)
            {
                swipeDir = Input.GetTouch(0).position - startPos;
            }
        }
    }

    public void ResetSwipeData()
    {
        isDragging = false;
        startPos = swipeDir = Vector2.zero;
    }
}
