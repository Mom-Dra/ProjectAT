using UnityEngine;
using TMPro;

namespace ProjectAT.FieldUI
{
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