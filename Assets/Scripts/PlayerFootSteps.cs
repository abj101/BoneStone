using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerFootsteps : MonoBehaviour
{
    public AudioClip footstepClip;
    public AudioClip boneClip;
    public float stepInterval = 0.4f; // Time between each step
    [Range(0f, 1f)] public float footstepVolume = 1f; // Volume of footstep sound
    [Range(0f, 1f)] public float boneVolume = 0.1f; // Volume of bone sound
    [Tooltip("Optional: use Input System Move action for rebinding support.")]
    [SerializeField] private InputActionReference _moveAction;
    private AudioSource audioSource;
    private Rigidbody rb;
    private float stepTimer;
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        rb = GetComponent<Rigidbody>();
        stepTimer = stepInterval;
    }
    void Update()
    {
        float inputX, inputZ;
        if (_moveAction != null && _moveAction.action != null)
        {
            Vector2 m = _moveAction.action.ReadValue<Vector2>();
            inputX = m.x;
            inputZ = m.y;
        }
        else
        {
            inputX = Input.GetAxis("Horizontal");
            inputZ = Input.GetAxis("Vertical");
        }
        bool isMoving = Mathf.Abs(inputX) > 0.1f || Mathf.Abs(inputZ) > 0.1f;
        if (isMoving)
        {
            if (stepTimer <= 0f)
            {
                audioSource.pitch = Random.Range(0.95f, 1.05f); // Randomize pitch so steps sound more natural
                audioSource.PlayOneShot(footstepClip, footstepVolume);
                audioSource.PlayOneShot(boneClip, boneVolume);
                stepTimer = stepInterval; // Reset timer
            }
            stepTimer -= Time.deltaTime;
        }
        else
        {
            stepTimer = 0f; // Reset timer so first step plays instantly instead of delay.
        }
    }
}