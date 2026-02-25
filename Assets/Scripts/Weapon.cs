using UnityEngine;

public abstract class Weapon : MonoBehaviour
{
    protected GameObject _owner;

    public void Initialize(GameObject owner)
    {
        _owner = owner;
    }

    public abstract void Attack();
}