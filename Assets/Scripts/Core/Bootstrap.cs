using UnityEngine;

namespace MalgarHotel.Core
{
    public class Bootstrap : MonoBehaviour
    {
        [SerializeField] private GameManager gameManagerPrefab;
        [SerializeField] private bool ensureFallbackGround = true;
        [SerializeField] private Vector3 fallbackGroundCenter = Vector3.zero;
        [SerializeField] private Vector2 fallbackGroundSize = new Vector2(60f, 60f);
        [SerializeField] private bool ensureKillPlane = true;
        [SerializeField] private float killPlaneHeight = -20f;

        private void Awake()
        {
            if (GameManager.Instance == null && gameManagerPrefab != null)
            {
                Instantiate(gameManagerPrefab, transform.position, Quaternion.identity);
            }

            Time.timeScale = 1f;
            EnsureFallbackGround();
            EnsureKillPlane();
        }

        private void Start()
        {
            GameManager.Instance?.ResetFuses();
            EnsureKillPlane();
        }

        private void EnsureFallbackGround()
        {
            if (!ensureFallbackGround)
            {
                return;
            }

            if (GameObject.Find("Ground") != null)
            {
                return;
            }

            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.position = fallbackGroundCenter;
            ground.transform.localScale = new Vector3(fallbackGroundSize.x / 10f, 1f, fallbackGroundSize.y / 10f);
            var collider = ground.GetComponent<Collider>();
            if (collider != null)
            {
                collider.sharedMaterial = null;
            }

            ground.layer = LayerMask.NameToLayer("Default");
        }

        private void EnsureKillPlane()
        {
            if (!ensureKillPlane)
            {
                return;
            }

            var existing = FindObjectOfType<MalgarHotel.World.KillPlane>();
            if (existing != null)
            {
                AssignKillPlaneRespawn(existing);
                return;
            }

            var killPlaneObject = new GameObject("KillPlane", typeof(BoxCollider), typeof(MalgarHotel.World.KillPlane));
            killPlaneObject.transform.position = new Vector3(0f, killPlaneHeight, 0f);
            var box = killPlaneObject.GetComponent<BoxCollider>();
            box.isTrigger = true;
            box.size = new Vector3(fallbackGroundSize.x * 2f, 5f, fallbackGroundSize.y * 2f);

            AssignKillPlaneRespawn(killPlaneObject.GetComponent<MalgarHotel.World.KillPlane>());
        }

        private void AssignKillPlaneRespawn(MalgarHotel.World.KillPlane killPlane)
        {
            if (killPlane == null)
            {
                return;
            }

            var player = FindObjectOfType<MalgarHotel.Player.PlayerController>();
            if (player != null)
            {
                killPlane.SetRespawnPoint(player.transform);
            }
        }
    }
}
