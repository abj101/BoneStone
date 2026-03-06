using UnityEngine;

public abstract class Weapon : MonoBehaviour
{
    public Transform Owner { get; private set; }

    public virtual void Initialize(Transform owner)
    {
        Owner = owner;
    }

    public abstract void Attack();
}