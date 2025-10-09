using UnityEngine;
using UnityEngine.UI;

public class Test2 : MonoBehaviour
{
    private void Update()
    {
        Debug.Log(transform.eulerAngles.x);

        Debug.DrawRay(transform.position, transform.forward * 10, Color.red);
    }
}
