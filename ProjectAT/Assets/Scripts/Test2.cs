using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.PackageManager;
using UnityEditor.Profiling.Memory.Experimental;
using UnityEngine;
using UnityEngine.UI;

public class Test2 : MonoBehaviour
{
    private string name;

    private void Start()
    {
        Managers.Instance.EventManager.Subscribe<int>(EventType.Last, Foo);
    }

    private void OnDisable()
    {
        Managers.Instance.EventManager.UnSubscribe<int>(EventType.Last, Foo);
    }

    private void Foo(int kk)
    {
        Debug.Log(kk);
    }

    [ContextMenu("Bar")]
    private void Bar()
    {
        Managers.Instance.EventManager.Publish<int>(EventType.Last, 77);
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
