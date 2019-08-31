using UnityEngine;

/// <summary>
/// Controls the Laser Sight for the player's aim
/// </summary>
public class TrajectorySimulation : MonoBehaviour
{
    // Reference to the LineRenderer we will use to display the simulated path
    public LineRenderer sightLine;

    // Reference to a Component that holds information about fire strength, location of cannon, etc.
    public BallShooter ballShooter;

    // Number of segments to calculate - more gives a smoother line
    public int segmentCount = 20;

    // Length scale for each segment
    public float segmentScale = 1;

    public Vector3 angle;
    public float powerMult = 1;

    // gameobject we're actually pointing at (may be useful for highlighting a target, etc.)
    private Collider _hitObject;
    public Collider hitObject { get { return _hitObject; } }

    private LayerMask mask;

    void FixedUpdate()
    {
        if (ballShooter && ballShooter.rb.IsSleeping())
            simulatePath();
    }

    /// <summary>
    /// Simulate the path of a launched ball.
    /// Slight errors are inherent in the numerical method used.
    /// </summary>
    void simulatePath()
    {
        Vector3[] segments = new Vector3[segmentCount];

        // The first line point is wherever the player's cannon, etc is
        segments[0] = ballShooter.transform.position;

        // The initial velocity
        Vector3 segVelocity = ballShooter.shootDir * ballShooter.currentStrength / ballShooter.rb.mass;
        // reset our hit object
        _hitObject = null;

        // mask for ball
        mask = ballShooter.gameObject.layer;

        for (int i = 1; i < segmentCount; i++)
        {
            // Time it takes to traverse one segment of length segScale (careful if velocity is zero)
            float segTime = (segVelocity.sqrMagnitude != 0) ? segmentScale / segVelocity.magnitude : 0;

            // Add velocity from gravity for this segment's timestep
            segVelocity += Physics.gravity * segTime;

            // Adjust velocity according to drag
            segVelocity *= Mathf.Clamp01(1f - ballShooter.rb.drag * segTime);

            // Check to see if we're going to hit a physics object
            RaycastHit hit;
            if (Physics.Raycast(segments[i - 1], segVelocity, out hit, segmentScale, mask))
            {
                // remember who we hit
                _hitObject = hit.collider;

                // set next position to the position where we hit the physics object
                segments[i] = segments[i - 1] + segVelocity.normalized * hit.distance;
                
                // correct ending velocity, since we didn't actually travel an entire segment
                segVelocity -= Physics.gravity * (segmentScale - hit.distance) / segVelocity.magnitude;

                // flip the velocity to simulate a bounce according to hit object's bounciness property
                segVelocity = Vector3.Reflect(segVelocity, hit.normal) * hit.collider.material.bounciness/* * Mathf.Clamp01(1f - ballShooter.rb.drag)*/;

                /*
				 * Here you could check if the object hit by the Raycast had some property - was 
				 * sticky, would cause the ball to explode, or was another ball in the air for 
				 * instance. You could then end the simulation by setting all further points to 
				 * this last point and then breaking this for loop.
				 */
            }
            // If our raycast hit no objects, then set the next position to the last one plus v*t
            else
            {
                segments[i] = segments[i - 1] + segVelocity * segTime;
            }
        }

        // At the end, apply our simulations to the LineRenderer

        // Set the colour of our path to the colour of the next ball
        Color startColor = Color.green;
        Color endColor = Color.red;
        startColor.a = 0.1f;
        endColor.a = 1f;
        sightLine.startColor = startColor;
        sightLine.endColor = endColor;

        sightLine.positionCount = segmentCount;
        for (int i = 0; i < segmentCount; i++)
            sightLine.SetPosition(i, segments[i]);
    }
}