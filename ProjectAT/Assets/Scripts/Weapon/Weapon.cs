using System.Collections;
using Unity.Netcode;
using Unity.Services.Matchmaker.Models;
using UnityEngine;

public abstract class Weapon : MonoBehaviour
{
    public abstract void Attack();
}

public class Grande : Weapon
{
    public override void Attack()
    {
        throw new System.NotImplementedException();
    }
}