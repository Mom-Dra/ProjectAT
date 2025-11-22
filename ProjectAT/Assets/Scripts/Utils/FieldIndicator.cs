using System.Collections;
using UnityEngine;
using UnityEngine.VFX;

public class FieldIndicator : MonoBehaviour
{
    [SerializeField] private readonly string eventName = "Click";
    [SerializeField] private Collider myCol;
    [SerializeField] private float availableTime = 0.1f;
    [SerializeField] private VisualEffect[] myVfx;

    private Coroutine indicatorCoroutine;
    private WaitForSeconds waitForSeconds;
    private int eventId = -1;

    private void Awake()
    {
        myCol = GetComponent<Collider>();
        myVfx = GetComponentsInChildren<VisualEffect>();
        waitForSeconds = new WaitForSeconds(availableTime);
        eventId = Shader.PropertyToID(eventName);
    }

    public void SpawnIndicator(Vector3 newPos)
    {
        if (indicatorCoroutine != null)
        {
            StopCoroutine(indicatorCoroutine);
        }
        indicatorCoroutine = StartCoroutine(IndicatorCoroutine(newPos));
    }

    private IEnumerator IndicatorCoroutine(Vector3 pos)
    {
        gameObject.transform.position = pos;
        myCol.enabled = true;
        for(int i = 0 ; i < myVfx.Length; ++i)
        {
            myVfx[i].SendEvent(eventId);
            //myVfx[i].Play();
        }

        yield return waitForSeconds;
        myCol.enabled = false;
    }
}
