using UnityEngine;

/// <summary>
/// 랜덤으로 재생할 소리를 모아두는 데이터 컨테이너 클래스
/// </summary>
[System.Serializable]
public class RandomSoundCue
{
    [SerializeField] private AudioClip[] clips;
    [SerializeField, Range(0f, 1f)] private float volume = 1f;
    [SerializeField] private Vector2 pitchRange = Vector2.one;
    public AudioClip GetRandomClip()
    {
        if(clips ==null || clips.Length == 0) return null;
        return clips[Random.Range(0, clips.Length)];
    }

    public float Volume => volume;
    public float Pitch => Random.Range(pitchRange.x, pitchRange.y); //NOTE : 소리나 피치까지 랜덤일 필요는 없음. 재고 바람(26.07.03)

}
