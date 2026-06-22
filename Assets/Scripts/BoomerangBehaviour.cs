using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(Rigidbody))]
public class BoomerangBehaviour : MonoBehaviour
{
    public Transform player;          // assign player/XR rig
    public float curveStrength = 2f;  // how strongly it bends back
    public float spinSpeed = 720f;
    public float minSpeedToCurve = 0.5f;
    [Range(0f, 1f)] public float gravityCompensation = 0.85f; // 1 = fully cancels gravity, 0 = full gravity

    private Rigidbody rb;
    private XRGrabInteractable grabInteractable;
    private bool isFlying = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        grabInteractable = GetComponent<XRGrabInteractable>();
        grabInteractable.selectExited.AddListener(OnRelease);
        grabInteractable.selectEntered.AddListener(OnGrab);
    }

    void OnGrab(SelectEnterEventArgs args)
    {
        isFlying = false;
        rb.useGravity = false;
    }

    void OnRelease(SelectExitEventArgs args)
    {
        Debug.Log("Release velocity: " + rb.linearVelocity.magnitude);
        if (rb.linearVelocity.magnitude > minSpeedToCurve)
            isFlying = true;
    }

    void FixedUpdate()
    {
        Debug.Log(isFlying);

        if (!isFlying) return;

        transform.Rotate(Vector3.up, spinSpeed * Time.fixedDeltaTime, Space.Self);

        // counteract gravity (fully or partially)
        rb.AddForce(-Physics.gravity * rb.mass * gravityCompensation, ForceMode.Force);

        Vector3 toPlayer = (player.position - transform.position).normalized;
        Vector3 curveDir = Vector3.Cross(toPlayer, Vector3.up);
        Vector3 targetDir = (toPlayer + curveDir).normalized;

        Vector3 newDir = Vector3.Slerp(rb.linearVelocity.normalized, targetDir, curveStrength * Time.fixedDeltaTime);
        rb.linearVelocity = newDir * rb.linearVelocity.magnitude;

        if (Vector3.Distance(transform.position, player.position) < 1f)
        {
            isFlying = false;
            rb.linearVelocity = Vector3.zero;
        }
    }


    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.CompareTag("Player"))
        {
            isFlying = false;
            rb.linearVelocity = Vector3.zero;
        }
    }
}
