using UnityEngine;

namespace MalgarHotel.Audio
{
    [DisallowMultipleComponent]
    public class SurfaceNoiseTag : MonoBehaviour
    {
        [SerializeField] private SurfaceType surfaceType = SurfaceType.Tile;

        public SurfaceType SurfaceType => surfaceType;

        public void SetSurfaceType(SurfaceType type)
        {
            surfaceType = type;
        }
    }
}
