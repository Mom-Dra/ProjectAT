using System.Collections;
using UnityEngine;
using UnityEngine.VFX;

public class MovePositionIndicator : IndicatorBase
{
    [SerializeField] private readonly string eventName = "Click";
    [SerializeField] private Collider myCol;
    [SerializeField] private float duration = 0.1f;
    [SerializeField] private VisualEffect[] myVfx;

    private Coroutine indicatorCoroutine;
    private WaitForSeconds waitForSeconds;
    private int eventId = -1;

    private void Awake()
    {
        myCol = GetComponent<Collider>();
        myVfx = GetComponentsInChildren<VisualEffect>();
        waitForSeconds = new WaitForSeconds(duration);
        eventId = Shader.PropertyToID(eventName);
    }

    public override void Show()
    {     
        if (indicatorCoroutine != null)
        {
            StopCoroutine(indicatorCoroutine);
        }
        indicatorCoroutine = StartCoroutine(IndicatorCoroutine());
    }

    private IEnumerator IndicatorCoroutine()
    {
        for(int i = 0 ; i < myVfx.Length; ++i)
        {
            myVfx[i].SendEvent(eventId);
            //myVfx[i].Play();
        }

        yield return waitForSeconds;
        Hide();
    }

    public override void Hide()
    {
        myCol.enabled = false;
    }

    public override void UpdateIndicator(Vector3 position, Vector3 velocity) { }
}
