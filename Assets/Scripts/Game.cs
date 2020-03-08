using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Game : MonoBehaviour
{
    [SerializeField] private Text fpsText;
    [SerializeField] private Text iterationText;
    [SerializeField] private Text allDieTimesText;
    [SerializeField] private Text generationText;

    private double iterationCount = 0;
    private double maxGeneration = 0;
    private double restartsCount = 0;
    private float deltaTime = 0.0f;

    public bool isRunning = false;
    private float gameTickInterval = 0.05f;
    private float nextUpdateTime = 0;

    private PixelViewCanvas view;
    private readonly Vector2Int mapSize = new Vector2Int(115, 50);

    private Color seedColor = Color.white;
    private Color defaultColor = Color.black;

    private List<Tree> trees = new List<Tree>();
    private List<Tree> growedTrees = new List<Tree>();
    private List<Tree> removeTrees = new List<Tree>();
    private List<Cell> growedCells = new List<Cell>();

    private Cell[,] cells;
    private Color[,] lastCellsColor;

    private void Start()
    {
        view = PixelViewCanvas.Instance;
        view.InitView(new Vector2Int(mapSize.x, mapSize.y), Color.black, 6, false);

        cells = new Cell[mapSize.x, mapSize.y];
        lastCellsColor = new Color[mapSize.x, mapSize.y];
        for (int y = 0; y < mapSize.y; y++)
        {
            for (int x = 0; x < mapSize.x; x++)
            {
                lastCellsColor[x, y] = new Color();

                cells[x, y] = new Cell
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
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;

        if (!isRunning || nextUpdateTime > Time.time)
            return;

        nextUpdateTime = Time.time + gameTickInterval;


        Iteration();
        DrawMap();
    }

    private void Iteration()
    {
        if (trees.Count == 0)
        {
            iterationCount = 0;
            restartsCount++;
            InitNewGeneration();
            return;
        }
        iterationCount++;

        double treeMaxGenerate = 0;
        foreach (var tree in trees)
        {
            if (tree.isDie)
            {
                if (tree.cells.Count == 0)
                {
                    removeTrees.Add(tree);
                }
                else
                {
                    FallSeeds(tree);
                }
                continue;
            }

            if (tree.cells.Count == 1 && tree.age >= 5)
            {
                removeTrees.Add(tree);
            }


            if (treeMaxGenerate < tree.cells[0].genome.generation)
            {
                treeMaxGenerate = tree.cells[0].genome.generation;
            }

            int treeIncomeEnergy = 0;
            bool allIsWood = true;
            foreach (var cell in tree.cells)
            {
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
                        SeedGrow(tree, cell);
                    }
                }
            }
            foreach (var item in growedCells)
            {
                tree.cells.Add(item);
            }
            growedCells.Clear();

            tree.energy += treeIncomeEnergy;
            tree.DecreaseEnergy();
            if (tree.energy < 0 || tree.age > 90 || allIsWood)
            {
                if (allIsWood)
                {
                    removeTrees.Add(tree);
                }

                tree.isDie = true;
                tree.ClearWoodCells();
                continue;
            }
            
        }
        foreach (var tree in removeTrees)
        {
            tree.ClearAllCells();
            trees.Remove(tree);
        }
        removeTrees.Clear();
        foreach (var tree in growedTrees)
        {
            trees.Add(tree);
        }
        growedTrees.Clear();

        maxGeneration = treeMaxGenerate;
    }

    private void SeedGrow(Tree tree, Cell cell)
    {
        cell.cellType = CellType.wood;

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

            if (seedCell.cellType == CellType.empty)
            {
                growedCells.Add(seedCell);
                seedCell.SetInitialValues();
                seedCell.CopyGenome(cell.genome);
                seedCell.activeGen = cell.genome.gens[cell.activeGen].sides[sideNum];
                seedCell.color = cell.color;
            }
        }
    }

    private int GetCellEnergyInc(int x, int y)
    {
        int energyModify = 3;

        for (int currY = y + 1; currY < mapSize.y; currY++)
        {
            if (cells[x, currY].cellType == CellType.wood)
            {
                energyModify--;
            }

            if (energyModify == 0)
                return 0;
        }

        int sunLevel = 6 + y > 16 ? 16 : 6 + y;
        return sunLevel * energyModify;
    }

    private void FallSeeds(Tree tree)
    {
        for (int i = 0; i < tree.cells.Count; i++)
        {
            if(tree.cells[i].cellType == CellType.seed)
            {
                tree.cells[i].cellType = CellType.fallSeed;
            }

            if(tree.cells[i].pos.y == 0)
            {
                var growedTree = new Tree();
                growedTree.SetInitialValues();
                growedTrees.Add(growedTree);

                growedTree.cells.Add(tree.cells[i]);
                tree.cells.Remove(tree.cells[i]);
                i--;

                growedTree.cells[0].SetInitialValues();
                growedTree.cells[0].genome.generation++;
                if (growedTree.cells[0].genome.generation % 50 == 0)
                {
                    growedTree.cells[0].SetRandomColor();
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
        view.Clear();
        for (int x = 0; x < mapSize.x; x++)
        {
            for (int y = 0; y < mapSize.y; y++)
            {
                lastCellsColor[x, y] = Color.black;
            }
        }

        foreach (var tree in trees)
        {
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
        var tree = new Tree();
        tree.SetInitialValues();

        tree.cells.Add(cells[x, y]);
        cells[x, y].SetInitialValues();
        cells[x, y].SetRandomGenome();

        return tree;
    }

    private void DrawMap()
    {
        for (int y = 0; y < mapSize.y; y++)
        {
            for (int x = 0; x < mapSize.x; x++)
            {
                if (cells[x, y].cellType == CellType.empty &&
                    !ColorsEqual(ref lastCellsColor[x, y], ref defaultColor))
                {
                    view.SetPixelIn(x, y, defaultColor);
                    lastCellsColor[x, y] = defaultColor;
                }
                else if (cells[x, y].cellType == CellType.wood &&
                    !ColorsEqual(ref lastCellsColor[x, y], ref cells[x, y].color))
                {
                    view.SetPixelIn(x, y, cells[x, y].color);
                    lastCellsColor[x, y] = cells[x, y].color;
                }
                else if((cells[x, y].cellType == CellType.seed || 
                    cells[x, y].cellType == CellType.fallSeed) && 
                    !ColorsEqual(ref lastCellsColor[x, y], ref seedColor))
                {
                    view.SetPixelIn(x, y, seedColor);
                    lastCellsColor[x, y] = seedColor;
                }
            }
        }
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
        if (gameTickInterval > 0.01f)
        {
            gameTickInterval -= 0.01f;
        }
    }

    public void SpeedDown()
    {
        gameTickInterval += 0.01f;
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
