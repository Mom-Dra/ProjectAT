using UnityEngine;
using Interactable;

public class EnemyDownedBody : DownedBody
{
    private void Start()
    {
        if (rb != null)
        {
            rb.isKinematic = true;
        }
    }
 
}
