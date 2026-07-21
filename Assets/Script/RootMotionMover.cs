using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Rigidbody))]
public class RootMotionMover : MonoBehaviour
{
    private Animator animator;
    private Rigidbody rb;

    public float rotationSpeed = 10f;

    private Vector2 input;

    void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();

        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    }

    void Update()
    {
        input.x = Input.GetAxis("Horizontal");
        input.y = Input.GetAxis("Vertical");

        animator.SetFloat("xHorizontal", input.x);
        animator.SetFloat("yVertical", input.y);
    }

    void OnAnimatorMove()
    {
        Vector3 delta = animator.deltaPosition;
        delta.y = rb.linearVelocity.y;

        rb.MovePosition(rb.position + delta);

        // Rotate toward movement direction
        Vector3 dir = new Vector3(input.x, 0, input.y);
        if (dir.magnitude > 0.1f)
        {
            Quaternion rot = Quaternion.LookRotation(dir);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, rot, rotationSpeed * Time.deltaTime));
        }
    }
}
