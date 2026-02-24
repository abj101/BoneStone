using UnityEngine;
public class PlayerFootsteps : MonoBehaviour
{
    public AudioClip footstepClip;
    public AudioClip boneClip;  
    public float stepInterval = 0.4f; // Time between each step
    public float boneVolume = 0.1f; // Volume of bone sound
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
        // Player input
        float inputX = Input.GetAxis("Horizontal");
        float inputZ = Input.GetAxis("Vertical");
        bool isMoving = Mathf.Abs(inputX) > 0.1f || Mathf.Abs(inputZ) > 0.1f;

        if (isMoving)
        {
            if (stepTimer <= 0f)
            {
                audioSource.pitch = Random.Range(0.95f, 1.05f); // Randomize pitch so steps sound more natural
                audioSource.PlayOneShot(footstepClip);
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