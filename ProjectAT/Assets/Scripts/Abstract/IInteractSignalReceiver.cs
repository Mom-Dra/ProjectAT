using UnityEngine;

namespace Interactable
{
    /// <summary>
    /// InteractSignalSender 클래스의 유니티 이벤트를 연결할 때 이 함수를 연결하도록 만든 인터페이스
    /// </summary>
    public interface IInteractSignalReceiver
    {
        void OnInteractSignalReceived();
    }
}
