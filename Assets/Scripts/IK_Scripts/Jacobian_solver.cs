using System;
using UnityEngine;

public class Jacobian_solver : MonoBehaviour
{
    [Header("Chain Components")]
    [SerializeField]private int chainLength = 0;
    public Transform ikTarget;
    public Transform ikPole;
    public Transform[] bones;

    [Header("Settings")]
    public int iterationCount = 10;
    public float maxError = 0.01f;

    [Header("Debugging")]
    [SerializeField] private Vector3[] bonePositions;
    [SerializeField] private Vector3[] boneRotations;
    [SerializeField] private Vector3[] boneTranspose;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        Init();
    }

    void Init()
    {
        //initialize the storage components to their proper size
        bones = new Transform[chainLength+1];
        bonePositions = new Vector3[chainLength+1];
        boneRotations = new Vector3[chainLength+1];

        //dynamically create our chain based on the chainLength arguement
        Transform current = transform;
        for(int i = bones.Length-1; i>=0; i--)
        {
            bones[i] = current;
            bonePositions[i] = current.position;
            //safeguard to let you know the chain length is too big
            if(current.parent == null)
            {
                Debug.LogWarning("chain length exceeds bone count, " + current + "is the root");
                break;
            }
            current = current.parent;
        }
    
    }

    void Solve()
    {
        for(int iteration= 0; iteration<iterationCount; iteration++)
        {
            Transform endEffector = bones[bones.Length-1];

            float distance = Vector3.Distance(endEffector.position, ikTarget.position);

            //begin calcultions
            if (distance > maxError)
            {
                
            }
        }
    }

    void Update()
    {
        Solve();
    }
}
