using System.Collections;
using System.Collections.Generic;
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

    [SerializeField] private Transform _towerUIParent;
    [SerializeField] private GameObject _towerUIPrefab;

    [SerializeField] private Tower[] _towerPrefabs;

    private List<Tower> _spawnedTowers = new List<Tower>();

    private void Start()
    {
        InstantiateAllTowerUI();
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
}
