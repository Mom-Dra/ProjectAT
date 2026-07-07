using UnityEngine;

namespace Rendering.BuildingDithering
{
    public class BuildingRoofHitbox : MonoBehaviour
    {
        [SerializeField] private BuildingRoofDitherController controller;

        internal BuildingRoofDitherController Controller => controller;

        private void Reset()
        {
            controller = GetComponentInParent<BuildingRoofDitherController>();
        }

        private void OnValidate()
        {
            if (controller == null)
            {
                controller = GetComponentInParent<BuildingRoofDitherController>();
            }
        }
    }
}
