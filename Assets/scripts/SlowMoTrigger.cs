using System.Collections;
using UnityEngine;

/// <summary>
/// SlowMoTrigger - Unity 6
/// 
/// SETUP:
///   1. Create an empty GameObject and place it at the center of the ring/hoop.
///   2. Add a SphereCollider to it → check "Is Trigger" → set radius to cover the hoop area.
///   3. Attach THIS script to that same GameObject.
///   4. Assign the "Ball" field to your ball GameObject.
///
/// HOW IT WORKS:
///   When the ball enters the trigger zone, time slows down dramatically.
///   After a set duration, time smoothly returns to normal.
/// </summary>
public class SlowMoTrigger : MonoBehaviour
{
    [Header("Ball Reference")]
    [Tooltip("Drag your ball GameObject here.")]
    public Rigidbody ball;

    [Header("Slow Mo Settings")]
    [Tooltip("How slow time gets. 0.1 = 10% speed, 0.2 = 20% speed.")]
    [Range(0.01f, 0.5f)]
    public float slowMoScale = 0.15f;

    [Tooltip("How long the slow mo lasts (in real seconds, not game time).")]
    public float slowMoDuration = 2.5f;

    [Tooltip("How fast time ramps back to normal after slow mo ends.")]
    public float recoverySpeed = 5f;

    [Header("Cooldown")]
    [Tooltip("Seconds before slow mo can trigger again (prevents spam).")]
    public float cooldown = 4f;

    // ── Internals ──────────────────────────────────────────────────────────────
    private bool _isSlowMo = false;
    private bool _onCooldown = false;

    private void Update()
    {
        // Smoothly recover time scale back to 1 after slow mo ends
        if (!_isSlowMo && Time.timeScale < 1f)
        {
            Time.timeScale = Mathf.MoveTowards(
                Time.timeScale, 1f, recoverySpeed * Time.unscaledDeltaTime
            );
            Time.fixedDeltaTime = 0.02f * Time.timeScale;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Only trigger if the ball enters and we're not already in slow mo
        if (_onCooldown || _isSlowMo) return;
        if (ball != null && other.gameObject != ball.gameObject) return;

        StartCoroutine(SlowMoSequence());
    }

    private IEnumerator SlowMoSequence()
    {
        _isSlowMo = true;
        _onCooldown = true;

        // --- Ramp DOWN to slow mo ---
        float rampDuration = 0.1f; // fast ramp in
        float elapsed = 0f;
        float startScale = Time.timeScale;

        while (elapsed < rampDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / rampDuration;
            Time.timeScale = Mathf.Lerp(startScale, slowMoScale, t);
            Time.fixedDeltaTime = 0.02f * Time.timeScale;
            yield return null;
        }

        Time.timeScale = slowMoScale;
        Time.fixedDeltaTime = 0.02f * slowMoScale;

        Debug.Log("[SlowMo] Slow motion started!");

        // --- Hold slow mo for duration (unscaled so it's real seconds) ---
        yield return new WaitForSecondsRealtime(slowMoDuration);

        // --- Let Update() handle the smooth ramp back UP ---
        _isSlowMo = false;
        Debug.Log("[SlowMo] Returning to normal speed...");

        // --- Cooldown before it can trigger again ---
        yield return new WaitForSecondsRealtime(cooldown);
        _onCooldown = false;
    }

    // ── Safety net: always restore time on disable/destroy ────────────────────
    private void OnDisable()
    {
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;
        _isSlowMo = false;
        _onCooldown = false;
    }

    // ── Gizmo: shows the trigger zone in the Scene view ───────────────────────
    private void OnDrawGizmos()
    {
        SphereCollider sc = GetComponent<SphereCollider>();
        if (sc == null) return;

        Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
        Gizmos.DrawSphere(transform.position + sc.center, sc.radius);

        Gizmos.color = new Color(1f, 0.5f, 0f, 0.9f);
        Gizmos.DrawWireSphere(transform.position + sc.center, sc.radius);
    }
}