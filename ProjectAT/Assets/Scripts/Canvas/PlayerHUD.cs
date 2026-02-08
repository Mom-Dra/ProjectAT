using UnityEngine;
using UnityEngine.UIElements;

public class PlayerHUD : MonoBehaviour
{
    [SerializeField]private UIDocument _uiDocument;
    private RadialProgressBar _healthBar; // ProgressBar 타입 사용

    // 테스트용 변수 (인스펙터에서 조절해보세요)
    public float currentHealth = 100f;
    public float maxHealth = 100f;

    void OnEnable()
    {
        InitUIElements();
    }

    void Update()
    {
        // 실제 게임에선 맞았을 때만 호출하겠지만, 테스트를 위해 Update에 둡니다.
        UpdateCurrentHealthUI();
    }

    private void InitUIElements()
    {
        _uiDocument = GetComponent<UIDocument>();
        var root = _uiDocument.rootVisualElement;

        // UI Builder에서 지은 이름 "HealthBar"로 찾기
        _healthBar = root.Q<RadialProgressBar>("HealthBar");
    }

    public void UpdateCurrentHealthUI()
    {
        if (_healthBar != null)
        {
            // ProgressBar의 값 설정
            _healthBar.Progress = (currentHealth / maxHealth) * 100f;
        }
        else
        {
            Debug.LogWarning("HealthBar UI element not found!");
        }
    }

    public void UpdateCurrentHealthUI(float currentHealth, float maxHealth)
    {
        this.currentHealth = currentHealth;
        this.maxHealth = maxHealth;
        UpdateCurrentHealthUI();
    }

    public void SetMaxHealthUI(float maxHealth)
    {
        this.maxHealth = maxHealth;
        UpdateCurrentHealthUI();
    }
}