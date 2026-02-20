using System.Collections;
using UnityEngine;
using Unity.Cinemachine;

public class DynamicCameraAngles : MonoBehaviour
{
    [Header("Target")]
    public Transform target;

    [Header("Angle Switch Timing")]
    public float switchInterval = 10f;

    [Header("Orbit Settings")]
    public float minDistance = 5f;
    public float maxDistance = 12f;
    public float minHeight = 2f;
    public float maxHeight = 7f;

    [Header("Angle Transition Speed")]
    [Tooltip("How smoothly it glides to the new angle. Keep this low (1-3) for cinematic feel.")]
    [Range(0.5f, 5f)]
    public float angleTransitionSpeed = 1.5f;

    private CinemachineCamera _vcam;
    private CinemachineThirdPersonFollow _thirdPersonFollow;
    private Vector3 _desiredOffset;
    private Vector3 _currentOffset;
    private bool _isReady = false;

    private void Awake()
    {
        _vcam = GetComponent<CinemachineCamera>();
        _thirdPersonFollow = GetComponent<CinemachineThirdPersonFollow>();
    }

    private void Start()
    {
        if (target == null)
        {
            Debug.LogWarning("[DynamicCameraAngles] No Target assigned!");
            return;
        }

        if (_thirdPersonFollow == null)
        {
            Debug.LogError("[DynamicCameraAngles] No CinemachineThirdPersonFollow found! " +
                           "Make sure you are using a Follow Camera.");
            return;
        }

        // Assign target to Cinemachine — it handles all movement and stabilization
        _vcam.Follow = target;
        _vcam.LookAt = target;

        // Set initial offset
        _desiredOffset = RandomOffset();
        _currentOffset = _desiredOffset;
        ApplyOffset(_currentOffset);

        _isReady = true;
        StartCoroutine(SwitchAngleLoop());
    }

    private void LateUpdate()
    {
        if (!_isReady || target == null) return;

        // Only handle angle switching — Cinemachine handles all movement
        _currentOffset = Vector3.Lerp(
            _currentOffset,
            _desiredOffset,
            Time.deltaTime * angleTransitionSpeed
        );

        ApplyOffset(_currentOffset);
    }

    private void ApplyOffset(Vector3 offset)
    {
        // Feed the offset into Cinemachine's Third Person Follow
        // Cinemachine then handles smooth following and stabilization natively
        _thirdPersonFollow.ShoulderOffset = new Vector3(offset.x, offset.y, 0f);
        _thirdPersonFollow.CameraDistance = Mathf.Abs(offset.z) < 1f
            ? Vector3.Distance(Vector3.zero, offset)
            : Mathf.Abs(offset.z);
    }

    private IEnumerator SwitchAngleLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(switchInterval);
            _desiredOffset = RandomOffset();
            Debug.Log("[DynamicCameraAngles] Switching to new angle.");
        }
    }

    private Vector3 RandomOffset()
    {
        float angle = Random.Range(0f, 360f);
        float distance = Random.Range(minDistance, maxDistance);
        float height = Random.Range(minHeight, maxHeight);
        float rad = angle * Mathf.Deg2Rad;

        return new Vector3(
            Mathf.Sin(rad) * distance,
            height,
            Mathf.Cos(rad) * distance
        );
    }

    private void OnDisable()
    {
        _isReady = false;
    }

    private void OnDrawGizmosSelected()
    {
        if (target == null) return;

        Gizmos.color = new Color(0f, 1f, 1f, 0.3f);
        Gizmos.DrawWireSphere(target.position, minDistance);

        Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
        Gizmos.DrawWireSphere(target.position, maxDistance);
    }
}