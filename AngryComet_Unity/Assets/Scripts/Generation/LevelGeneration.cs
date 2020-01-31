using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Generation
{
    [System.Serializable]
    public struct Entity
    {
        public GameObject obj;
        public int multiplier;
    }

    [System.Serializable]
    public struct EntityCategory
    {
        public string category;
        public int startLevel;
        public int entityCount;
        public Entity[] entities;
    }

    public class LevelGeneration : MonoBehaviour
    {
        public static LevelGeneration instance = null;

        void Awake()
        {
            if (instance != null)
            {
                Destroy(gameObject);
            }
            else
            {
                instance = this;
            }
        }

        private const float LevelProgressionConst = 0.04f;
        
        [SerializeField] private int maxLevel;
        [SerializeField] private int entityCount = 10;
        [SerializeField] private int startEntityCount = 5;
        [SerializeField] private LayerMask whatIsEntity;
        [SerializeField] private EntityCategory[] entityLevels = null;
        [Space] [SerializeField] private Collider2D startSpawner = null;
        [SerializeField] private Collider2D[] spawners = null;

        public int Level { get; private set; }
        private List<GameObject> _currentEntityList;
        private int _currentEntityLevel = -1;

        // Start is called before the first frame update
        void Start()
        {
            _currentEntityList = new List<GameObject>();
            Level = 1;
            UpdateEntityList();
        }

        // Update is called once per frame
        void Update()
        {
            Collider2D[] objects;

            foreach (Collider2D spawner in spawners)
            {
                Transform spawnerTransform = spawner.transform;
                Bounds spwnBnds = spawner.bounds;
                objects = Physics2D.OverlapBoxAll(spawnerTransform.position, spwnBnds.size,
                    spawnerTransform.rotation.z, whatIsEntity);

                if (objects.Length < entityCount)
                {
                    //random position within the spawners bounds
                    Vector2 randomPosition = new Vector2(Random.Range(spwnBnds.min.x, spwnBnds.max.x),
                        Random.Range(spwnBnds.min.y, spwnBnds.max.y));

                    //Check whether there is already a planet at this position
                    objects = Physics2D.OverlapCircleAll(randomPosition, 5f, whatIsEntity);

                    GameObject randPlanet = _currentEntityList[Random.Range(0, _currentEntityList.Count)];
                    //only spawn the new entity if the space is free
                    if(objects.Length == 0)
                        Instantiate(randPlanet, randomPosition, Quaternion.identity);
                }
            }
        }

        public void StartSpawn()
        {
            Collider2D[] objects;

            Transform startSpawnerTransform = startSpawner.transform;
            Bounds startSpwnBnds = startSpawner.bounds;
            bool startSpwnDone = false;
            
            if(_currentEntityList.Count == 0) UpdateEntityList();

            while (!startSpwnDone)
            {
                objects = Physics2D.OverlapBoxAll(startSpawnerTransform.position, startSpwnBnds.size,
                    startSpawnerTransform.rotation.z, whatIsEntity);

                if (objects.Length < startEntityCount)
                {
                    //random position within the spawners bounds
                    Vector2 randomPosition = new Vector2(Random.Range(startSpwnBnds.min.x, startSpwnBnds.max.x),
                        Random.Range(startSpwnBnds.min.y, startSpwnBnds.max.y));

                    //Check whether there is already a planet at this position
                    objects = Physics2D.OverlapCircleAll(randomPosition, 5f, whatIsEntity);

                    GameObject randPlanet = _currentEntityList[Random.Range(0, _currentEntityList.Count)];
                    //only spawn the new entity if the space is free
                    if(objects.Length == 0)
                        Instantiate(randPlanet, randomPosition, Quaternion.identity);
                }
                else
                {
                    startSpwnDone = true;
                }
            }
        }

        public void RestartGeneration()
        {
            GameObject[] entities = GameObject.FindGameObjectsWithTag("Planet");

            foreach (var entity in entities)
            {
                Destroy(entity);
            }

            entities = GameObject.FindGameObjectsWithTag("Projectile");

            foreach (var entity in entities)
            {
                Destroy(entity);
            }

            Level = 0;
            StartSpawn();
        }

        public void UpdateLevel(float xp)
        {
            int tempLevel = Level;

            Level = 1 + (int) (LevelProgressionConst * Mathf.Sqrt(xp));
            if (Level > maxLevel) Level = maxLevel;

            //Level-up?
            if (Level > tempLevel)
            {
                UpdateEntityList();
            }
        }

        void UpdateEntityList()
        {
            int categoryIndex = 0;

            for (int i = entityLevels.Length - 1; i >= 0 ; i--)
            {
                if (entityLevels[i].startLevel <= Level)
                {
                    categoryIndex = i;
                    break;
                }
            }

            entityCount = entityLevels[categoryIndex].entityCount;

            if (categoryIndex != _currentEntityLevel)
            {
                _currentEntityList.Clear();
                
                for (int i = 0; i < entityLevels[categoryIndex].entities.Length; i++)
                {
                    for (int y = 0; y < entityLevels[categoryIndex].entities[i].multiplier; y++)
                    {
                        _currentEntityList.Add(entityLevels[categoryIndex].entities[i].obj);
                    }
                }

                _currentEntityLevel = categoryIndex;
            }
        }
    }
}