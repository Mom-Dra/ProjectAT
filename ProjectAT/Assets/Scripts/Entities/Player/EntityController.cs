using Unity.Netcode;
using UnityEngine;

public interface IEntityController
{
    public abstract void Move(Vector3 pos);
    public abstract void Run(Vector3 to);

    //public abstract void Attack();

}
