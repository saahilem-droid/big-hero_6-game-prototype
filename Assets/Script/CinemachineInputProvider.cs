using UnityEngine;
using Unity.Cinemachine;

public class CinemachineInputProvider : MonoBehaviour
{
    [Header("Camera Sensitivity")]
    public float horizontalSpeed = 150f;
    public float verticalSpeed = 1.5f;
    
    private CinemachineFreeLook freeLookCam;
    
    void Start()
    {
        freeLookCam = GetComponent<CinemachineFreeLook>();
        
        if (freeLookCam != null)
        {
            // Disable default input
            freeLookCam.m_XAxis.m_InputAxisName = "";
            freeLookCam.m_YAxis.m_InputAxisName = "";
        }
    }
    
    void Update()
    {
        if (freeLookCam == null) return;
        
        // Get mouse delta (movement)
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");
        
        // Apply rotation using delta values
        freeLookCam.m_XAxis.m_InputAxisValue = mouseX * horizontalSpeed * Time.deltaTime;
        freeLookCam.m_YAxis.m_InputAxisValue = mouseY * verticalSpeed * Time.deltaTime;
    }
}
