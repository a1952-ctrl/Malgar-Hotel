using UnityEngine;

namespace MalgarHotel.Core
{
    public class Bootstrap : MonoBehaviour
    {
        [SerializeField] private GameManager gameManagerPrefab;

        private void Awake()
        {
            if (GameManager.Instance == null && gameManagerPrefab != null)
            {
                Instantiate(gameManagerPrefab, transform.position, Quaternion.identity);
            }

            Time.timeScale = 1f;
        }

        private void Start()
        {
            GameManager.Instance?.ResetFuses();
        }
    }
}
