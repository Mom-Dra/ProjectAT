using UnityEngine;
using TMPro;

namespace ProjectAT.FieldUI
{
    /// <summary>
    /// UI 요소 자체를 프리펩으로 만들기 위해 CountdownView라는 TMP_Text용 래퍼클래스를 생성함.
    /// </summary>
    public class CountdownView : MonoBehaviour
    {
        [SerializeField] private TMP_Text countdownText;

        public void SetRemainingSeconds(float seconds)
        {
            if(countdownText != null)
            {
                countdownText.text = seconds.ToString("F1"); // F1 : 소수점 첫째 자리까지 표시
            }
        }
    }
}