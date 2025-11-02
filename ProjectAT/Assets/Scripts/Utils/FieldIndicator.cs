using System.Collections;
using TMPro;
using UnityEngine;

public class FieldIndicator : MonoBehaviour
{
    [SerializeField] private Material mat;
    [SerializeField] private Collider myCol;
    [SerializeField] private float disappearSpeed = 0.1f;

    private Coroutine indicatorCoroutine;
    private WaitForSeconds WaitForSeconds = new WaitForSeconds(0.02f);
    private Color currentColor;
    private Color initialColor;

    private void Awake()
    {
        mat = GetComponent<Renderer>().material;
        myCol = GetComponent<Collider>();
        initialColor = mat.color;
        mat.color = Color.clear;
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
        mat.color = initialColor;
        currentColor = initialColor;

        while(currentColor.a > 0f)
        {
            currentColor.a -= disappearSpeed;
            mat.color = currentColor;
            yield return WaitForSeconds;
        }

        myCol.enabled = false;
    }
}
