using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    private static LevelManager _instance;
    public static LevelManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<LevelManager>();
            }
            return _instance;
        }
    }

    [Header("Tower UI")]
    [SerializeField] private Transform _towerUIParent;
    [SerializeField] private GameObject _towerUIPrefab;

    [Header("Parent Transforms")]
    public Transform towerParent;
    public Transform enemyParent;
    public Transform bulletParent;

    [Header("Prefabs")]
    [SerializeField] private Tower[] _towerPrefabs;
    [SerializeField] private Enemy[] _enemyPrefabs;

    [Header("Enemy Spawn Config")]
    [SerializeField] private Transform[] _enemyPaths;
    [SerializeField] private float _spawnDelay = 5f;

    [Header("Level Config")]
    [SerializeField] private int _maxLives = 3;
    [SerializeField] private int _totalEnemy = 15;

    [Header("Canvas object")]
    [SerializeField] private GameObject _panel;
    [SerializeField] private Text _statusInfo;
    [SerializeField] private Text _livesInfo;
    [SerializeField] private Text _totalEnemyInfo;

    private List<Tower> _spawnedTowers = new List<Tower>();
    private List<Enemy> _spawnedEnemies = new List<Enemy>();
    private List<Bullet> _spawnedBullets = new List<Bullet>();

    private int _currentLives;
    private int _enemyCounter;
    private float _runningSpawnDelay;

    public bool IsOver { get; private set; }

    private void Start()
    {
        SetCurrentLives(_maxLives);
        SetTotalEnemy(_totalEnemy);
        InstantiateAllTowerUI();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        if (IsOver)
        {
            return;
        }

        _runningSpawnDelay -= Time.unscaledDeltaTime;

        if (_runningSpawnDelay <= 0f)
        {
            SpawnEnemy();
            _runningSpawnDelay = _spawnDelay;
        }
        foreach (Enemy enemy in _spawnedEnemies)
        {
            if (!enemy.gameObject.activeSelf)
            {
                continue;
            }

            if (Vector2.Distance(enemy.transform.position, enemy.TargetPosition) < 0.1f)
            {
                enemy.SetCurrentPathIndex(enemy.CurrentPathIndex + 1);

                if (enemy.CurrentPathIndex < _enemyPaths.Length)
                {
                    enemy.SetTargetPosition(_enemyPaths[enemy.CurrentPathIndex].position);
                }
                else
                {
                    enemy.gameObject.SetActive(false);
                    ReduceLives(1);
                }
            }

            else
            {
                enemy.MoveToTarget();
            }
        }

        foreach (Tower tower in _spawnedTowers)
        {
            tower.CheckNearestEnemy(_spawnedEnemies);
            tower.SeekTarget();
            tower.ShootTarget();
        }
    }

    // Menampilkan seluruh Tower yang tersedia pada UI Tower Selection
    private void InstantiateAllTowerUI()
    {
        foreach (Tower tower in _towerPrefabs)
        {
            GameObject newTowerUIObj = Instantiate(_towerUIPrefab.gameObject, _towerUIParent);
            TowerUI newTowerUI = newTowerUIObj.GetComponent<TowerUI>();

            newTowerUI.SetTowerPrefab(tower);
            newTowerUI.transform.name = tower.name;
        }
    }

    // Mendaftarkan Tower yang di-spawn agar bisa dikontrol oleh LevelManager
    public void RegisterSpawnedTower(Tower tower)
    {
        _spawnedTowers.Add(tower);
    }

    private void SpawnEnemy()
    {
        SetTotalEnemy(--_enemyCounter);

        if (_enemyCounter < 0)
        {
            bool isAllEnemyDestroyed = _spawnedEnemies.Find(e => e.gameObject.activeSelf) == null;

            if (isAllEnemyDestroyed)
            {
                SetGameOver(true);
            }
            return;
        }

        int randomIndex = Random.Range(0, _enemyPrefabs.Length);
        string enemyIndexString = (randomIndex + 1).ToString();

        GameObject newEnemyObj = _spawnedEnemies.Find(
            e => !e.gameObject.activeSelf && e.name.Contains(enemyIndexString)
            )?.gameObject;

        if (newEnemyObj == null)
        {
            newEnemyObj = Instantiate(_enemyPrefabs[randomIndex].gameObject, enemyParent);
        }

        Enemy newEnemy = newEnemyObj.GetComponent<Enemy>();

        if (!_spawnedEnemies.Contains(newEnemy))
        {
            _spawnedEnemies.Add(newEnemy);
        }

        newEnemy.transform.position = _enemyPaths[0].position;
        newEnemy.SetTargetPosition(_enemyPaths[1].position);
        newEnemy.SetCurrentPathIndex(1);
        newEnemy.gameObject.SetActive(true);
    }



    // Untuk menampilkan garis penghubung dalam window Scene
    // tanpa harus di-Play terlebih dahulu
    private void OnDrawGizmos()
    {
        for (int i = 0; i < _enemyPaths.Length - 1; i++)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(_enemyPaths[i].position, _enemyPaths[i + 1].position);
        }
    }

    public Bullet GetBulletFromPool(Bullet prefab)
    {
        GameObject newBulletObj = _spawnedBullets.Find(
            b => !b.gameObject.activeSelf && b.name.Contains(prefab.name)
        )?.gameObject;

        if (newBulletObj == null)
        {
            newBulletObj = Instantiate(prefab.gameObject, bulletParent);
        }

        Bullet newBullet = newBulletObj.GetComponent<Bullet>();
        if (!_spawnedBullets.Contains(newBullet))
        {
            _spawnedBullets.Add(newBullet);
        }

        return newBullet;
    }

    public void ExplodeAt(Vector2 point, float radius, int damage)
    {
        foreach (Enemy enemy in _spawnedEnemies)
        {
            if (enemy.gameObject.activeSelf)
            {
                if (Vector2.Distance(enemy.transform.position, point) <= radius)
                {
                    enemy.ReduceEnemyHealth(damage);
                }
            }
        }
    }

    public void ReduceLives(int value)
    {
        SetCurrentLives(_currentLives - value);
        if (_currentLives <= 0)
        {
            SetGameOver(false);
        }
    }

    public void SetCurrentLives(int currentLives)
    {
        _currentLives = Mathf.Max(currentLives, 0);

        _livesInfo.text = $"Lives: {_currentLives}";
    }

    public void SetTotalEnemy(int totalEnemy)
    {
        _enemyCounter = totalEnemy;

        _totalEnemyInfo.text = $"Total Enemy: {Mathf.Max(_enemyCounter, 0)}";
    }

    public void SetGameOver(bool isWin)
    {
        IsOver = true;

        _statusInfo.text = isWin ? "You Win!" : "You Lose!";
        _panel.gameObject.SetActive(true);
    }
}
