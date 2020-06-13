using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class SimSettings: MonoBehaviour
{
    [SerializeField] public Vector2Int mapSize = new Vector2Int(115, 50);

    [Range(0f, 1f)]
    [SerializeField] public float gameTickInterval = 0;

    [Space]
    [SerializeField] public int maxTreeAge = 90;
    [SerializeField] public int maxSeedAge = 5;

    [SerializeField] public int startTreeEnergy = 300;
    [SerializeField] public int startSunLevel = 6;

    [SerializeField] public int maxSunLevel = 16;
    [SerializeField] public int sunLevelModify = 3;

    [SerializeField] public int reqEnergyForLife = 13;
    [SerializeField] public int reqEnergyForGrow = 18;

    [SerializeField] public int mutateGenomeChance = 25;
    [SerializeField] public int ColorChangeGenerationsNum = 50;

    [Space]
    [Header("Experemental")]
    [SerializeField] public bool allowSaveSeedIfNoGrowed = false;


    private static SimSettings _instance;
    public static SimSettings Instance => _instance;


    private void Awake()
    {
        if (_instance == null)
            _instance = this;
    }



    // Энергия для проростания зерна
    // Прирост энергии за каждую клетку ввысь
    // Кол-ыо энергии для поддержания жизни клетки дерева

    // ИДЕЯ 
    // Листва будет давать энергии больше чем зерно

}
