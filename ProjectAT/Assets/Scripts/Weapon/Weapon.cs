using System.Collections;
using Unity.Netcode;
using Unity.Services.Matchmaker.Models;
using UnityEngine;

public abstract class Weapon : MonoBehaviour
{
    public abstract bool IsReady { get; }
    public abstract bool IsReloading { get; }

    public abstract void Attack();
}

public class Grande : Weapon
{
    public override bool IsReady => throw new System.NotImplementedException();
    public override bool IsReloading => throw new System.NotImplementedException();

    public override void Attack()
    {
        throw new System.NotImplementedException();
    }
}