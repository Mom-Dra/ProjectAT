using System;
using UnityEngine;

public class FirstClassEnemy : Enemy
{
    
}


public class SecondClassEnemy : Enemy, IThrowable
{
    public void Throw()
    {

    }
}
