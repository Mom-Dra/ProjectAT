using UnityEngine;
using System;

namespace Rendering.BuildingDithering
{
    [RequireComponent(typeof(Collider))]
    public class BuildingInteriorTrigger : MonoBehaviour
    {
        [SerializeField] private BuildingRoofDitherController controller;
        [SerializeField] private LayerMask playerLayer;
        public event Action OnPlayerEnter;
        public event Action OnPlayerExit;

        internal void ResetParameters(BuildingRoofDitherController controller, LayerMask playerLayer)
        {
            this.controller = controller;
            GetComponent<Collider>().isTrigger = true;
            this.playerLayer = playerLayer;
        }

        private void OnTriggerEnter(Collider other)
        {
            if(!other.gameObject.IsSameLayer(playerLayer)) return;
            OnPlayerEnter?.Invoke(); //controller.AddPlayerInside();
        }

        private void OnTriggerExit(Collider other)
        {
            if(!other.gameObject.IsSameLayer(playerLayer)) return;
            OnPlayerExit?.Invoke(); //controller.RemovePlayerInside();
        }
    }
}
