using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Test2 : MonoBehaviour
{
    [SerializeField]
    private float t;

    [SerializeField]
    private float length;

    [SerializeField]
    private float rotateSpeed;

    [SerializeField]
    private float rotateAngle;

    private void Start()
    {
        //StartCoroutine(TestCoroutine());
        //Vector3 abc = Quaternion.Euler(0f, 90f, 0f) * Vector3.forward;
        //Vector3 cba = Quaternion.Euler(0f, 90f, 0f) * abc;


        //try
        //{
        //    Debug.LogError("Error!!");
        //}
        //catch (Exception e)
        //{
        //    Debug.Log($"KiaOra!: {e}");
        //}

        //Debug.Log($"{cba}");
    }

    //private IEnumerator TestCoroutine()
    //{
    //    WaitForSeconds wait = new WaitForSeconds(2f);

    //    while (true)
    //    {
    //        Debug.Log("TestCoroutine");

    //        yield return wait;
    //    }
    //}

    private void Update()
    {
        //if (Physics.Raycast(Vector3.zero, Vector3.up, 10f, ))
    }
}
