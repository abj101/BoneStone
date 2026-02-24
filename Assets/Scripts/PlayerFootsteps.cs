using UnityEngine;
public class PlayerFootsteps : MonoBehaviour
{
    public AudioClip footstepClip;
    public AudioClip boneClip;  
    public float stepInterval = 0.4f;
    public float boneVolume = 0.1f;
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
        float inputX = Input.GetAxis("Horizontal");
        float inputZ = Input.GetAxis("Vertical");
        bool isMoving = Mathf.Abs(inputX) > 0.1f || Mathf.Abs(inputZ) > 0.1f;

        if (isMoving)
        {
            if (stepTimer <= 0f)
            {
                audioSource.pitch = Random.Range(0.95f, 1.05f);
                audioSource.PlayOneShot(footstepClip);
                audioSource.PlayOneShot(boneClip, boneVolume); 
                stepTimer = stepInterval;
            }
            stepTimer -= Time.deltaTime;
        }
        else
        {
            stepTimer = 0f;
        }
    }
}