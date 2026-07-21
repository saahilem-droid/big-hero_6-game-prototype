using UnityEngine;

[RequireComponent(typeof(Animator))]
public class RootMotionMovementDriver : MonoBehaviour
{
    [Header("References")]
    public Transform cameraTransform;

    [Header("Rotation")]
    public float rotationSpeed = 10f;

    private Animator animator;

    float horizontal;
    float vertical;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        ReadInput();
        UpdateAnimatorLocomotion();
        RotateWithCamera();
    }

    // =========================
    // INPUT
    // =========================
    void ReadInput()
    {
        horizontal = Input.GetAxisRaw("Horizontal");
        vertical = Input.GetAxisRaw("Vertical");
    }

    // =========================
    // SEND TO ANIMATOR
    // =========================
    void UpdateAnimatorLocomotion()
    {
        bool isRunning =
            Input.GetKey(KeyCode.LeftShift) ||
            Input.GetKey(KeyCode.RightShift);

        // Forward value for blend tree
        float forwardValue = 0f;

        if (vertical > 0.1f)
            forwardValue = isRunning ? 1f : 0.5f;
        else if (vertical < -0.1f)
            forwardValue = -0.5f;

        animator.SetFloat("xHorizontal", horizontal, 0.15f, Time.deltaTime);
        animator.SetFloat("yVertical", forwardValue, 0.15f, Time.deltaTime);
    }

    // =========================
    // ROTATION
    // =========================
    void RotateWithCamera()
    {
        if (Mathf.Abs(horizontal) < 0.1f &&
            Mathf.Abs(vertical) < 0.1f)
            return;

        Camera cam = Camera.main;
        if (!cam) return;

        Vector3 camForward = cam.transform.forward;
        camForward.y = 0f;
        camForward.Normalize();

        Vector3 camRight = cam.transform.right;
        camRight.y = 0f;
        camRight.Normalize();

        Vector3 moveDir =
            camForward * vertical +
            camRight * horizontal;

        if (moveDir.sqrMagnitude < 0.01f)
            return;

        Quaternion targetRot =
            Quaternion.LookRotation(moveDir);

        transform.rotation =
            Quaternion.Slerp(
                transform.rotation,
                targetRot,
                rotationSpeed * Time.deltaTime);
    }
}
