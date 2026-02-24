using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;




public class IK_Solver : MonoBehaviour
{
    [Header("Chain Components")]
    //assign the chain bones in order of root bone-> effector bone
    public Transform[] bones;
    public Transform ikTarget;
    public Transform ikPole;

    [Header("Settings")]
    public int iterationCount = 10;
    public float maxError = 0.01f;
    public float snapBackStrength = 1f;

    //calculation components
    [Header("Debugging")]
    [SerializeField] private float[] boneLengths;
    [SerializeField] private int chainLength = 0;
    [SerializeField] private float totalLength = 0;
    [SerializeField] private Vector3[] bonePositions;
    [SerializeField]private Vector3[] startDirectionState;
    [SerializeField]private Quaternion[] startRotationBone;
    Quaternion startRotationTarget;
    Quaternion startRotationRoot;


    void Awake()
    {
        Init();
    }

    void Init()
    {
        //count how many bones in the chain and adjust storage arrays
        bones = new Transform[chainLength+1];
        boneLengths = new float[chainLength];
        bonePositions = new Vector3[chainLength+1];
        startDirectionState = new Vector3[chainLength+1];
        startRotationBone = new Quaternion[chainLength+1];
        
        startRotationTarget = ikTarget.rotation;
        Transform current = transform;
        for(int i = bones.Length-1; i>=0; i--)
        {
            bones[i] = current;
            startRotationBone[i] = current.rotation;
            if (i == bones.Length - 1)
            {
                startDirectionState[i] = ikTarget.position-current.position;
            }
            else
            {
                startDirectionState[i] = bones[i+1].position-current.position;
                boneLengths[i] = startDirectionState[i].magnitude;
                totalLength += boneLengths[i];
            }
           
            
            current = current.parent;
        }
        //current = null;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        Solve();
    }

    //solves all the transformations needed (i know, real creative naming)
    void Solve()
    {
        //return if no target assigned
        if(ikTarget == null)
        {
            return;
        }
        
        if(boneLengths.Length != chainLength)
        {
            Init();
        }

        //record positions of all bones in the chain
        for(int i = 0; i<bones.Length; i++)
        {
            bonePositions[i] = bones[i].position;
        }

        //determine distance of the root of the chain to the target

        Quaternion rootRot = (bones[0].parent != null) ? bones[0].parent.rotation : Quaternion.identity;
        Quaternion rootRotDif = rootRot*Quaternion.Inverse(startRotationRoot);
        
        float targetDistance = (ikTarget.position-bones[0].position).sqrMagnitude;

        //if the target is too far out, reach in its general direction
        if(targetDistance >= totalLength*totalLength)   
        {
            Vector3 direction = (ikTarget.position - bonePositions[0]).normalized;
            for(int i = 1; i< bones.Length; i++){
                bonePositions[i] = bonePositions[i-1] + direction * boneLengths[i-1];
            }
            //Debug.Log("stretch");
        }
        else
        {
            //otherwise, adjust positions of the joints using a forwards and backwards pass 
            //to get the most accurate solution with this method

            for(int i = 0; i<bonePositions.Length - 1; i++)
            {
                bonePositions[i+1] = Vector3.Lerp(bonePositions[i+1], bonePositions[i]+rootRotDif*startDirectionState[i], snapBackStrength);
            }
            
            for(int iteration = 0; iteration<iterationCount; iteration++)
            {
                //backward pass
                for (int i = bones.Length - 1; i>0; i--){
                    if(i==bonePositions.Length - 1)
                    {
                        bonePositions[i] = ikTarget.position;
                    }
                    else
                    {
                        bonePositions[i] = bonePositions[i+1] + (bonePositions[i]-bonePositions[i+1]).normalized * boneLengths[i];
                    }

                }

                //forward pass

                for(int i = 1; i<bones.Length; i++){
                    bonePositions[i] = bonePositions[i-1] + (bonePositions[i]-bonePositions[i-1]).normalized * boneLengths[i-1];
                }

                if(Vector3.Distance(bonePositions[bones.Length-1], ikTarget.position) < maxError)
                {   
                    break;
                }
            }

            

            
        }

        if(ikPole != null)
            {
                for(int i = 1; i< bonePositions.Length-1; i++)
                {
                    Plane plane = new Plane(bonePositions[i+1]-bonePositions[i-1], bonePositions[i-1]);
                    Vector3 projectedPole = plane.ClosestPointOnPlane(ikPole.position);
                    Vector3 projectedBone = plane.ClosestPointOnPlane(bonePositions[i]);
                    float angle = Vector3.SignedAngle(projectedBone-bonePositions[i-1], projectedPole-bonePositions[i-1], plane.normal);
                    bonePositions[i] = Quaternion.AngleAxis(angle, plane.normal) * (bonePositions[i]-bonePositions[i-1]) + bonePositions[i-1];
                }
            }

        //apply transforms
        for (int i = 0; i < bonePositions.Length; i++)
        {
            if (i == bonePositions.Length - 1)
            {
                bones[i].rotation = ikTarget.rotation * Quaternion.Inverse(startRotationTarget)*startRotationBone[i];
            }
            else
            {
                bones[i].rotation = Quaternion.FromToRotation(startDirectionState[i], bonePositions[i+1]-bonePositions[i])*startRotationBone[i];
            }
            bones[i].position = bonePositions[i];

        }

        //bones[boneCount - 1].position = bonePositions[boneCount - 1];
    }
    
}


//attributions:
// David on dev.to - https://dev.to/dslower/inverse-kinematics-solver-using-the-fabrik-method-1m92