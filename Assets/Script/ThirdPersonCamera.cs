using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    [Header("Target")]
    public Transform target;

    [Header("Distance")]
    public float distance = 4.5f;
    public float height = 1.6f;

    [Header("Mouse")]
    public float sensitivityX = 250f;
    public float sensitivityY = 180f;

    public float minY = -35f;
    public float maxY = 65f;

    [Header("Smoothing")]
    public float rotationSmooth = 12f;
    public float positionSmooth = 12f;

    [Header("Collision")]
    public float collisionRadius = 0.3f;
    public LayerMask collisionMask;

    [Header("Shoulder Offset")]
    public Vector3 shoulderOffset = Vector3.zero;


    float yaw;
    float pitch;

    Vector3 currentVelocity;

    void Start()
{
    Cursor.lockState = CursorLockMode.Locked;
    Cursor.visible = false;

    // Align camera with player forward
    yaw = target.eulerAngles.y;
    pitch = 10f;

    transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
}


    void LateUpdate()
    {
        LookInput();
        UpdateRotation();
        UpdatePosition();
    }

    // =====================
    // INPUT
    // =====================
    void LookInput()
    {
        yaw += Input.GetAxis("Mouse X") * sensitivityX * Time.deltaTime;
        pitch -= Input.GetAxis("Mouse Y") * sensitivityY * Time.deltaTime;

        pitch = Mathf.Clamp(pitch, minY, maxY);
    }

    // =====================
    // ROTATION
    // =====================
    void UpdateRotation()
    {
        Quaternion rot = Quaternion.Euler(pitch, yaw, 0f);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            rot,
            rotationSmooth * Time.deltaTime);
    }

    // =====================
    // POSITION
    // =====================
    void UpdatePosition()
    {
        Vector3 targetPos =
            target.position +
            Vector3.up * height +
            transform.rotation * shoulderOffset;

        Vector3 desiredPos =
            targetPos -
            transform.forward * distance;

        // Collision check
        Vector3 dir = desiredPos - targetPos;
        float dist = dir.magnitude;

        if (Physics.SphereCast(
            targetPos,
            collisionRadius,
            dir.normalized,
            out RaycastHit hit,
            dist,
            collisionMask))
        {
            desiredPos = hit.point + hit.normal * collisionRadius;

        }

        transform.position = Vector3.SmoothDamp(
            transform.position,
            desiredPos,
            ref currentVelocity,
            1f / positionSmooth);
    }
}
