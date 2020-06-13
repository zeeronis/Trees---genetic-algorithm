using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using stg = SimSettings;

public class Game : MonoBehaviour
{
    [SerializeField] private Text fpsText;
    [SerializeField] private Text iterationText;
    [SerializeField] private Text allDieTimesText;
    [SerializeField] private Text generationText;


    private double iterationCount = 0;
    private double maxGeneration = 0;
    private double restartsCount = 0;

    public bool isRunning = false;
    private bool isCanUpdate = true;
    private float nextUpdateTime = 0;


    private PixelViewCanvas view;
    private Vector2Int mapSize;

    private Color seedColor = Color.white;
    private Color defaultColor = Color.black;

    private List<Tree> trees = new List<Tree>();
    private List<Tree> treesPool = new List<Tree>();
    private List<Tree> growedTrees = new List<Tree>();
    private List<Cell> growedCells = new List<Cell>();

    private Cell[,] cells;
    private CellType[,] lastCellsType;


    private void Start()
    {
        isCanUpdate = true;

        mapSize = stg.Instance.mapSize;
        view = PixelViewCanvas.Instance;
        view.InitView(new Vector2Int(mapSize.x, mapSize.y), Color.black, 6);

        cells = new Cell[mapSize.x, mapSize.y];
        lastCellsType = new CellType[mapSize.x, mapSize.y];

        for (int y = 0; y < mapSize.y; y++)
        {
            for (int x = 0; x < mapSize.x; x++)
            {
                lastCellsType[x, y] = CellType.empty;

                cells[x, y] = new Cell()
                {
                    pos = new Vector2Int(x, y)
                };
                for (int i = 0; i < cells[x, y].genome.gens.Length; i++)
                {
                    cells[x, y].genome.gens[i] = new Gen();
                }
            }
        }

        InitNewGeneration();

        isRunning = true;
        StartCoroutine(UpdateUIPerSecond());
    }

    private IEnumerator UpdateUIPerSecond()
    {
        while (true)
        {
            yield return new WaitForSeconds(1);

            fpsText.text = "FPS: " + (int)(1f / Time.deltaTime);
            allDieTimesText.text = "Restarts: " + restartsCount;
            iterationText.text = "Iterations: " + iterationCount;
            generationText.text = "Best generation: " + maxGeneration;
        }
    }

    private void Update()
    {

        if (!isCanUpdate || !isRunning || nextUpdateTime > Time.time)
            return;


        isCanUpdate = false;
        nextUpdateTime = Time.time + stg.Instance.gameTickInterval;

        Iteration();
        DrawMap();

        isCanUpdate = true;
    }

    private void Iteration()
    {
        if (trees.Count == 0)
        {
            restartsCount++;
            iterationCount = 0;
            InitNewGeneration();

            return;
        }

        iterationCount++;

        double treeMaxGenerate = 0;
        for (int treeIndex = 0; treeIndex < trees.Count; treeIndex++)
        {
            Tree tree = trees[treeIndex];

            if (tree.isDie)
            {
                if (tree.cells.Count == 0)
                {
                    ReturnTreeToPool(tree);
                    treeIndex--;
                }
                else
                {
                    FallSeeds(tree);
                }

                continue;
            }
            else if (tree.cells.Count == 1 && tree.age >= stg.Instance.maxSeedAge)
            {
                tree.ClearAllCells();
                ReturnTreeToPool(tree);
                treeIndex--;

                continue;
            }

            if (treeMaxGenerate < tree.cells[0].genome.generation)
            {
                treeMaxGenerate = tree.cells[0].genome.generation;
            }

            int treeIncomeEnergy = 0;
            bool allIsWood = true;
            for (int cellIndex = 0; cellIndex < tree.cells.Count; cellIndex++)
            {
                Cell cell = tree.cells[cellIndex];

                if (cell.cellType == CellType.wood)
                {
                    treeIncomeEnergy += GetCellEnergyInc(cell.pos.x, cell.pos.y);
                }
                else 
                {
                    allIsWood = false;
                    cell.needEnergy -= GetCellEnergyInc(cell.pos.x, cell.pos.y);

                    if (cell.needEnergy <= 0)
                    {
                        GrowSeed(tree, cell);
                    }
                }
            }

            for (int i = 0; i < growedCells.Count; i++)
            {
                tree.cells.Add(growedCells[i]);
            }
            growedCells.Clear();

            tree.energy += treeIncomeEnergy;
            tree.DecreaseEnergy();
            if (allIsWood || tree.energy <= 0 || tree.age >= stg.Instance.maxTreeAge)
            {
                tree.isDie = true;
                tree.ClearWoodCells();

                if (allIsWood)
                {
                    ReturnTreeToPool(tree);
                    treeIndex--;
                }
               
                continue;
            }

        }

        foreach (var tree in growedTrees)
        {
            trees.Add(tree);
        }
        growedTrees.Clear();

        maxGeneration = treeMaxGenerate;
    }

    private void GrowSeed(Tree tree, Cell cell)
    {
        int growedSideCount = 0;
        for (int sideNum = 0; sideNum < 4; sideNum++)
        {

            if (cell.genome.gens[cell.activeGen].sides[sideNum] >= 15)
                continue;

            Cell seedCell = null;
            switch (sideNum)
            {
                case 1:
                    if (cell.pos.y + 1 >= mapSize.y)
                        continue;
                    seedCell = cells[cell.pos.x, cell.pos.y + 1];
                    break;

                case 3:
                    if(cell.pos.y - 1 < 0)
                        continue;
                    seedCell = cells[cell.pos.x, cell.pos.y - 1];
                    break;

                case 0:
                    if (cell.pos.x == 0)
                    {
                        seedCell = cells[mapSize.x - 1, cell.pos.y];
                    }
                    else
                    {
                        seedCell = cells[cell.pos.x - 1, cell.pos.y];
                    }
                    break;

                case 2:
                    seedCell = cells[(cell.pos.x + 1) % (mapSize.x - 1), cell.pos.y];
                    break;
            }
            if (seedCell == null)
                continue;

            if (seedCell.cellType == CellType.fallSeed || 
                seedCell.cellType == CellType.empty)
            {
                if (seedCell.cellType == CellType.fallSeed)
                {
                    seedCell.fallSeedTree.cells.Remove(seedCell);
                }

                growedCells.Add(seedCell);
                seedCell.SetSeedInitialValues();
                seedCell.CopyGenome(cell.genome);
                seedCell.activeGen = cell.genome.gens[cell.activeGen].sides[sideNum];
                seedCell.color = cell.color;

                growedSideCount++;
            }
        }

        if (stg.Instance.allowSaveSeedIfNoGrowed)
        {
            if (growedSideCount > 0)
            {
                cell.cellType = CellType.wood;
            }
        }
        else
        {
            cell.cellType = CellType.wood;
        }
    }

    private int GetCellEnergyInc(int x, int y)
    {
        int maxY = mapSize.y;
        int energyModify = stg.Instance.sunLevelModify;

        for (int currY = y + 1; currY < maxY; currY++)
        {
            if (cells[x, currY].cellType == CellType.wood)
            {
                energyModify--;

                if (energyModify == 0)
                    return 0;
            }
        }


        if (stg.Instance.startSunLevel + y > stg.Instance.maxSunLevel)
        {
            return stg.Instance.maxSunLevel * energyModify;
        }
        
        return (stg.Instance.startSunLevel + y) * energyModify;
    }

    private void FallSeeds(Tree tree)
    {
        for (int i = 0; i < tree.cells.Count; i++)
        {
            if (tree.cells[i].cellType == CellType.seed)
            {
                tree.cells[i].cellType = CellType.fallSeed;
                tree.cells[i].fallSeedTree = tree;
            }

            if (tree.cells[i].pos.y == 0)
            {
                var growedTree = GetNewTree();
                growedTree.SetInitialValues();
                growedTrees.Add(growedTree);

                growedTree.cells.Add(tree.cells[i]);
                tree.cells.Remove(tree.cells[i]);
                i--;

                growedTree.cells[0].SetSeedInitialValues();
                growedTree.cells[0].genome.generation++;

                if (growedTree.cells[0].genome.generation 
                    % stg.Instance.ColorChangeGenerationsNum == 0)
                {
                    growedTree.cells[0].SetRandomColor();
                }

                if (UnityEngine.Random.Range(0,101) < stg.Instance.mutateGenomeChance)
                {
                    growedTree.cells[0].MutateGenome();
                }

                continue;
            }

            int x = tree.cells[i].pos.x;
            int y = tree.cells[i].pos.y;
            if (cells[x, y - 1].cellType == CellType.empty)
            {
                var buffer = cells[x, y - 1];
                cells[x, y - 1] = cells[x, y];
                cells[x, y] = buffer;

                cells[x, y].pos.y = y;
                cells[x, y - 1].pos.y = y - 1;
            }
            else
            {
                tree.cells[i].cellType = CellType.empty;
                tree.cells.Remove(tree.cells[i]);
                i--;
            }

        }
    }

    private void InitNewGeneration()
    {
        iterationCount = 0;

        view.Clear();
        for (int x = 0; x < mapSize.x; x++)
        {
            for (int y = 0; y < mapSize.y; y++)
            {
                lastCellsType[x, y] = CellType.empty;
            }
        }

        foreach (var tree in trees)
        {
            treesPool.Add(tree);
            tree.ClearAllCells();
        }
        trees.Clear();

        for (int x = 0; x < mapSize.x; x += 20)
        {
            trees.Add(InitNewRandomTree(x, 0));
            cells[x, 0].SetRandomColor();
        }
    }

    private Tree InitNewRandomTree(int x, int y)
    {
        var tree = GetNewTree();
        tree.SetInitialValues();

        tree.cells.Add(cells[x, y]);
        cells[x, y].SetSeedInitialValues();
        cells[x, y].SetRandomGenome();

        return tree;
    }

    private void DrawMap()
    {
        int maxY = mapSize.y;
        int maxX = mapSize.x;

        for (int y = 0; y < maxY; y++)
        {
            for (int x = 0; x < maxX; x++)
            {
                if (cells[x, y].cellType != lastCellsType[x, y])
                {
                    if (cells[x, y].cellType == CellType.empty)
                    {
                        view.SetPixelIn(x, y, defaultColor);
                        lastCellsType[x, y] = cells[x, y].cellType;
                    }
                    else if (cells[x, y].cellType == CellType.wood)
                    {
                        view.SetPixelIn(x, y, cells[x, y].color);
                        lastCellsType[x, y] = cells[x, y].cellType;
                    }
                    else if ((cells[x, y].cellType == CellType.seed || 
                              cells[x, y].cellType == CellType.fallSeed))
                    {
                        view.SetPixelIn(x, y, seedColor);
                        lastCellsType[x, y] = cells[x, y].cellType;
                    }
                }
            }
        }
    }

    private Tree GetNewTree()
    {
        if (treesPool.Count > 0)
        {
            Tree tree = treesPool[0];
            treesPool.RemoveAt(0);

            return tree;
        }

        return new Tree();
    }

    private void ReturnTreeToPool(Tree tree)
    {
        treesPool.Add(tree);
        trees.Remove(tree);
    }

    private bool ColorsEqual(ref Color c1, ref Color c2)
    {
        if (c1.r != c2.r || c1.g != c2.g || c1.b != c2.b)
            return false;
        return true;
    }


    public void SaveGenomes()
    {

    }

    public void LoadGenomes()
    {

    }


    public void SpeedUp()
    {
        if (stg.Instance.gameTickInterval > 0)
        {
            stg.Instance.gameTickInterval -= 0.01f;
        }
    }

    public void SpeedDown()
    {
        stg.Instance.gameTickInterval += 0.01f;
    }

    public void Restart()
    {
        InitNewGeneration();
    }

    public void Pause()
    {
        isRunning = !isRunning;
    }
}
