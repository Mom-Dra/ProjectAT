using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : Component
{
    private static T _instance;

    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindAnyObjectByType<T>();

                if (_instance == null)
                {
                    GameObject obj = new GameObject();
                    obj.name = typeof(T).Name;
                    _instance = obj.AddComponent<T>();
                }
            }

            return _instance;
        }
    }

    // Awake() 함수를 호출했을 때 싱글턴 컴포넌트는 자신의 인스턴스가 이미 있는지 확인한다
    // 인스턴스가 없다면 싱글턴 컴포넌트 자신이 현재 인스턴스가 된다
    // 이미 있다면 복제를 막기 위해 스스로를 제거한다
    protected virtual void Awake()
    {
        if (_instance == null)
        {
            _instance = this as T;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
