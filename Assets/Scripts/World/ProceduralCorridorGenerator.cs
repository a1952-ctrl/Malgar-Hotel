using System.Collections.Generic;
using MalgarHotel.Audio;
using MalgarHotel.Core;
using UnityEngine;

namespace MalgarHotel.World
{
    public class ProceduralCorridorGenerator : MonoBehaviour
    {
        [System.Serializable]
        private class TileDefinition
        {
            public string name = "Straight";
            public TileType type = TileType.Straight;
            public float length = 12f;
            public float width = 6f;
            public float height = 3f;
            public float weight = 1f;
        }

        private class RuntimeTile
        {
            public TileDefinition Definition;
            public GameObject Root;
            public Transform CollectibleRoot;
            public Vector3 StartPosition;
            public Vector3 EndPosition;
            public Vector3 Forward;
        }

        private enum TileType
        {
            Straight,
            Curve,
            Room
        }

        [Header("Setup")]
        [SerializeField] private Transform player;
        [SerializeField] private int maxActiveTiles = 8;
        [SerializeField] private float despawnDistance = 25f;
        [SerializeField] private TileDefinition[] tileDefinitions;

        [Header("Collectibles")]
        [SerializeField] private float batteryChance = 0.35f;
        [SerializeField] private bool enableMiniGamePanels = true;

        private readonly Queue<RuntimeTile> _activeTiles = new Queue<RuntimeTile>();
        private readonly Dictionary<TileType, Queue<RuntimeTile>> _typedPools = new Dictionary<TileType, Queue<RuntimeTile>>();

        private Vector3 _currentPosition;
        private Vector3 _currentForward;
        private int _spawnedFuses;

        private void Awake()
        {
            if (tileDefinitions == null || tileDefinitions.Length == 0)
            {
                tileDefinitions = CreateDefaultTiles();
            }
        }

        private void Start()
        {
            if (player == null && Camera.main != null)
            {
                player = Camera.main.transform;
            }

            _currentPosition = transform.position;
            _currentForward = transform.forward;

            for (int i = 0; i < maxActiveTiles; i++)
            {
                SpawnNextTile(i < 2);
            }
        }

        private void Update()
        {
            if (player == null)
            {
                return;
            }

            if (_activeTiles.Count == 0)
            {
                return;
            }

            RuntimeTile frontTile = _activeTiles.Peek();
            Vector3 toPlayer = player.position - frontTile.StartPosition;
            float progress = Vector3.Dot(toPlayer, frontTile.Forward.normalized);
            if (progress > frontTile.Definition.length + despawnDistance)
            {
                RuntimeTile recycled = _activeTiles.Dequeue();
                GetPool(recycled.Definition.type).Enqueue(recycled);
                SpawnNextTile();
            }
        }

        private void SpawnNextTile(bool forceStraight = false)
        {
            TileDefinition definition = SelectTile(forceStraight);
            RuntimeTile tile = GetTile(definition);
            PositionTile(tile);
            PopulateTile(tile);
            _activeTiles.Enqueue(tile);
        }

        private TileDefinition SelectTile(bool forceStraight)
        {
            if (forceStraight)
            {
                foreach (var definition in tileDefinitions)
                {
                    if (definition.type == TileType.Straight)
                    {
                        return definition;
                    }
                }
            }

            float totalWeight = 0f;
            foreach (var definition in tileDefinitions)
            {
                totalWeight += definition.weight;
            }

            float random = Random.Range(0f, totalWeight);
            foreach (var definition in tileDefinitions)
            {
                random -= definition.weight;
                if (random <= 0f)
                {
                    return definition;
                }
            }

            return tileDefinitions[0];
        }

        private RuntimeTile GetTile(TileDefinition definition)
        {
            Queue<RuntimeTile> pool = GetPool(definition.type);
            RuntimeTile tile = pool.Count > 0 ? pool.Dequeue() : CreateTile(definition);
            tile.Definition = definition;
            return tile;
        }

        private Queue<RuntimeTile> GetPool(TileType type)
        {
            if (!_typedPools.TryGetValue(type, out var pool))
            {
                pool = new Queue<RuntimeTile>();
                _typedPools[type] = pool;
            }

            return pool;
        }

        private RuntimeTile CreateTile(TileDefinition definition)
        {
            var root = new GameObject($"Tile_{definition.name}");
            root.transform.parent = transform;
            BuildGeometry(root.transform, definition);

            var collectibleRoot = new GameObject("Collectibles").transform;
            collectibleRoot.SetParent(root.transform, false);

            return new RuntimeTile
            {
                Definition = definition,
                Root = root,
                CollectibleRoot = collectibleRoot
            };
        }

        private void BuildGeometry(Transform parent, TileDefinition definition)
        {
            switch (definition.type)
            {
                case TileType.Straight:
                    BuildStraight(parent, definition);
                    break;
                case TileType.Curve:
                    BuildCurve(parent, definition);
                    break;
                case TileType.Room:
                    BuildRoom(parent, definition);
                    break;
            }
        }

        private void BuildStraight(Transform parent, TileDefinition definition)
        {
            CreateFloor(parent, new Vector3(definition.width, 0.2f, definition.length), new Vector3(0f, -0.1f, definition.length * 0.5f), SurfaceType.Tile);
            CreateWall(parent, new Vector3(0f, definition.height * 0.5f, definition.length * 0.5f), new Vector3(0.1f, definition.height, definition.length));
            CreateWall(parent, new Vector3(definition.width, definition.height * 0.5f, definition.length * 0.5f), new Vector3(0.1f, definition.height, definition.length));
        }

        private void BuildCurve(Transform parent, TileDefinition definition)
        {
            CreateFloor(parent, new Vector3(definition.width, 0.2f, definition.length), new Vector3(0f, -0.1f, definition.length * 0.5f), SurfaceType.Wood);
            GameObject secondary = CreateFloor(parent, new Vector3(definition.width, 0.2f, definition.length), new Vector3(definition.length * 0.5f, -0.1f, definition.length), SurfaceType.Wood, Quaternion.Euler(0f, 90f, 0f));
            secondary.name = "CurveFloor";
        }

        private void BuildRoom(Transform parent, TileDefinition definition)
        {
            float size = Mathf.Max(definition.width, definition.length);
            CreateFloor(parent, new Vector3(size, 0.2f, size), new Vector3(0f, -0.1f, size * 0.5f), SurfaceType.Carpet);
        }

        private GameObject CreateFloor(Transform parent, Vector3 scale, Vector3 localPos, SurfaceType surfaceType, Quaternion? rotation = null)
        {
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "Floor";
            floor.transform.SetParent(parent, false);
            floor.transform.localScale = scale;
            floor.transform.localPosition = localPos;
            floor.transform.localRotation = rotation ?? Quaternion.identity;
            var surface = floor.AddComponent<SurfaceNoiseTag>();
            surface.SetSurfaceType(surfaceType);
            return floor;
        }

        private void CreateWall(Transform parent, Vector3 localPos, Vector3 scale)
        {
            GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = "Wall";
            wall.transform.SetParent(parent, false);
            wall.transform.localScale = scale;
            wall.transform.localPosition = localPos;
            wall.AddComponent<NoiseOccluder>();
        }

        private void PositionTile(RuntimeTile tile)
        {
            Vector3 tileForward = _currentForward;
            tile.Root.transform.SetPositionAndRotation(_currentPosition, Quaternion.LookRotation(tileForward, Vector3.up));
            tile.StartPosition = _currentPosition;

            Vector3 right = new Vector3(tileForward.z, 0f, -tileForward.x).normalized;
            if (tile.Definition.type == TileType.Curve)
            {
                _currentPosition += tileForward * tile.Definition.length;
                _currentPosition += right * tile.Definition.length;
                _currentForward = right;
            }
            else
            {
                _currentPosition += tileForward * tile.Definition.length;
            }

            tile.EndPosition = _currentPosition;
            tile.Forward = tileForward;
        }

        private void PopulateTile(RuntimeTile tile)
        {
            foreach (Transform child in tile.CollectibleRoot)
            {
                Destroy(child.gameObject);
            }

            if (GameManager.Instance == null)
            {
                return;
            }

            if (_spawnedFuses < GameManager.Instance.RequiredFuses)
            {
                SpawnFuse(tile);
                _spawnedFuses++;
            }
            else if (Random.value < batteryChance)
            {
                SpawnBattery(tile);
            }
        }

        private void SpawnFuse(RuntimeTile tile)
        {
            if (enableMiniGamePanels && Random.value < 0.4f)
            {
                CreateMiniGamePanel(tile);
            }
            else
            {
                CreateFusePickup(tile);
            }
        }

        private void CreateFusePickup(RuntimeTile tile)
        {
            GameObject fuse = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            fuse.name = "Fuse";
            fuse.transform.SetParent(tile.CollectibleRoot, false);
            fuse.transform.localScale = new Vector3(0.4f, 0.6f, 0.4f);
            fuse.transform.localPosition = new Vector3(0f, 0.6f, tile.Definition.length * 0.5f);
            fuse.transform.localRotation = Quaternion.identity;

            var collider = fuse.GetComponent<Collider>();
            collider.isTrigger = true;
            fuse.AddComponent<FusePickup>();
            fuse.SetActive(true);
        }

        private void CreateMiniGamePanel(RuntimeTile tile)
        {
            GameObject panel = GameObject.CreatePrimitive(PrimitiveType.Cube);
            panel.name = "FusePanel";
            panel.transform.SetParent(tile.CollectibleRoot, false);
            panel.transform.localScale = new Vector3(0.5f, 1f, 0.1f);
            panel.transform.localPosition = new Vector3(0f, 0.5f, tile.Definition.length * 0.3f);

            var collider = panel.GetComponent<Collider>();
            collider.isTrigger = true;
            panel.AddComponent<FusePanelMiniGame>();
        }

        private void SpawnBattery(RuntimeTile tile)
        {
            GameObject battery = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            battery.name = "Battery";
            battery.transform.SetParent(tile.CollectibleRoot, false);
            battery.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            battery.transform.localPosition = new Vector3(0.8f, 0.4f, tile.Definition.length * 0.6f);

            var collider = battery.GetComponent<Collider>();
            collider.isTrigger = true;
            battery.AddComponent<BatteryPickup>();
        }

        private TileDefinition[] CreateDefaultTiles()
        {
            return new[]
            {
                new TileDefinition
                {
                    name = "Straight",
                    type = TileType.Straight,
                    length = 12f,
                    width = 6f,
                    height = 3f,
                    weight = 3f
                },
                new TileDefinition
                {
                    name = "Curve",
                    type = TileType.Curve,
                    length = 8f,
                    width = 6f,
                    height = 3f,
                    weight = 1.5f
                },
                new TileDefinition
                {
                    name = "Room",
                    type = TileType.Room,
                    length = 14f,
                    width = 10f,
                    height = 3f,
                    weight = 1.2f
                }
            };
        }
    }
}
