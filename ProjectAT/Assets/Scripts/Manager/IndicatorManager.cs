using UnityEngine;
using System;

public enum IndicatorType : ushort
{
    MoveIndicator,
    ThrowingIndicator, //SkillAoEIndicatorWithLine 도 만들어둬야할듯.
    TargettingSkillIndicator,
    SectorAoEIndicator

}

public class IndicatorManager : MonoBehaviour
{
    public static IndicatorManager Instance { get; private set; }

    [Header("Indicator Prefabs")]
    [SerializeField] private GameObject moveIndicatorPrefab;
    [SerializeField] private GameObject throwingIndicatorPrefab;
    [SerializeField] private GameObject sectorAoEIndicatorPrefab;
    [SerializeField] private Sprite targettingSkillCursor;

    [Header("Indicator References")]
    private MovePositionIndicator moveIndicator;
    private ThrowingIndicator throwingIndicator;
    private SectorAoEIndicator sectorAoEIndicator;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            Initialize();
        }
        else
        {
            Destroy(gameObject); 
        }
    }

    private void Initialize()
    {
        InitializeIndicators();
    }

    private void InitializeIndicators()
    {
        
        moveIndicator = Instantiate(moveIndicatorPrefab).GetComponent<MovePositionIndicator>();
        moveIndicator.Hide();
        
        throwingIndicator = Instantiate(throwingIndicatorPrefab).GetComponent<ThrowingIndicator>();
        throwingIndicator.Hide();
        
        sectorAoEIndicator = Instantiate(sectorAoEIndicatorPrefab).GetComponent<SectorAoEIndicator>();
        sectorAoEIndicator.Hide();

    }

    public void HideIndicator(IndicatorType type)
    {
        switch (type)
        {
            case IndicatorType.TargettingSkillIndicator:
                ResetCursor();
                break;
            case IndicatorType.ThrowingIndicator:
                HideThrowingIndicator();
                break;
            case IndicatorType.SectorAoEIndicator:
                HideSectorAoEIndicator();
                sectorAoEIndicator.Hide();
                break;
            default:
                break;
        }
    }

    public void ShowMoveIndicator(Vector3 dest)
    {
        moveIndicator.transform.position = dest;
        moveIndicator.Show();
    }

    public void ShowAimingCursor()
    {
        Cursor.SetCursor(targettingSkillCursor.texture, new Vector2(targettingSkillCursor.texture.width / 2, targettingSkillCursor.texture.height / 2), CursorMode.Auto);
    }

    private void ResetCursor()
    {
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }


    public void ShowThrowingIndicator(Transform caller, float radius)
    {
        throwingIndicator.SetTarget(caller);
        throwingIndicator.SetRadius(radius * 2f);
        throwingIndicator.Show();
    }

    public void UpdateThrowingIndicator(Vector3 toPos, float PlayerRange)
    {
        throwingIndicator.UpdateIndicator(toPos);
        throwingIndicator.DrawThrowingLine(toPos, 1f, PlayerRange);
    }

    private void HideThrowingIndicator()
    {
        throwingIndicator.Hide();
        throwingIndicator.ClearLine();
    }

    public void ShowSectorAoEIndicator(Transform attachedTarget, float radius, float length)
    {
        sectorAoEIndicator.SetTarget(attachedTarget);
        sectorAoEIndicator.SetSize(radius, length);
        sectorAoEIndicator.Show();
    }

    public void UpdateSectorAoEIndicator(Vector3 toPos)
    {
        sectorAoEIndicator.UpdateIndicator(toPos);
    }

    private void HideSectorAoEIndicator()
    {
        sectorAoEIndicator.Hide();
    }

}