using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BallShooter : MonoBehaviour
{
    [SerializeField] private float launchAngle = 30f;
    [SerializeField] private float accuracySway = 18f;
    [SerializeField] private float maxShotStrength = 30f;
    [SerializeField] private Vector2 dragThresholds = new Vector2(20f, 100f);

    public Rigidbody rb { get; internal set; }
    private InputManager inputMgr;

    private Vector2 startingPos;
    private Vector2 currentPos;

    private Vector3 dragDir, aimDir;
    public Vector3 shootDir { get; internal set; }
    public float currentStrength { get; internal set; }
    private float dragPower;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        inputMgr = GetComponent<InputManager>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.Escape))
        {
            // reload scene
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    private void OnMouseDown()
    {
        startingPos = Input.mousePosition;
        //startingPos = Input.touches[0].position;
    }

    private void OnMouseDrag()
    {
        currentPos = Input.mousePosition;
        dragDir = currentPos - startingPos;
        aimDir = new Vector3(-launchAngle, -dragDir.x, 0);
        currentStrength = (Mathf.Clamp(dragDir.magnitude, 0, dragThresholds.y) / dragThresholds.y) * maxShotStrength;
        dragPower = Mathf.Clamp(dragDir.magnitude, 0, dragThresholds.y);
        shootDir = (Quaternion.Euler(aimDir) * Vector3.forward).normalized;

    }

    private void OnMouseUp()
    {
        //if (dragDirection.magnitude >= 10f)
        //{
        //    rb.AddForce(dragDirection * 20f, ForceMode.Impulse);
        //}

        if (rb.IsSleeping() && dragPower > dragThresholds.x)
            rb.AddForce(shootDir * currentStrength, ForceMode.Impulse);
        else
            ResetTouchProperties();
    }

    private void ResetTouchProperties()
    {
        shootDir = Vector3.zero;
        currentStrength = 0;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, Quaternion.Euler(aimDir) * Vector3.forward * 2 * (dragPower / dragThresholds.y));
    }

    private void OnGUI()
    {
        GUI.Label(new Rect(0, Screen.height - 60, 300, Screen.height), 
            "DIR: " + shootDir);
    
        GUI.Label(new Rect(0, Screen.height - 40, 300, Screen.height), 
            "STRENGTH: " + currentStrength);

        GUI.Label(new Rect(0, Screen.height - 20, 300, Screen.height), 
            "FORCE: " + shootDir * maxShotStrength * (dragPower / dragThresholds.y));
            //"POWER: " + (Quaternion.Euler(aimDir) * Vector3.forward).normalized * maxShotStrength);

    }
}
