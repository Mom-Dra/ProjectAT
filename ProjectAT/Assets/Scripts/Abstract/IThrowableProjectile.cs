using UnityEngine;

public interface IThrowableProjectile
{
    void Throw(Vector3 velocity);
    void IgnoreCollisionWith(GameObject owner);
}
