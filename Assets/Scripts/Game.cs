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

    private bool isStarted = false;
    private bool isPaused = false;
    private bool isInited = false;

    private float gameTickInterval = 0.05f;
    private float nextUpdateTime = 0;

    private double iterationCount = 0;
    private double maxCurrGeneration = 0;
    private double restartsCount = 0;
    private float deltaTime = 0.0f;

    private const int mapX = 240;
    private const int mapY = 50;
    private PixelViewUI view;

    private List<Tree> trees = new List<Tree>();
    private List<Tree> growedTrees = new List<Tree>();
    private List<Tree> removeTrees = new List<Tree>();
    private List<Cell> growedCells = new List<Cell>();

    private CellType[,] lastCells = new CellType[mapX, mapY];
    private CellType[,] cells = new CellType[mapX, mapY];
    private Color[,] cellsColor = new Color[mapX, mapY];
    private Color seedColor = Color.white;

    private void Start()
    {
        StartCoroutine(InitView());
        StartCoroutine(UpdateUIPerSecond());
    }

    void Update()
    {
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
    }

    private void FixedUpdate()
    {
        if (!isStarted || isPaused)
            return;

        if (!isInited)
        {
            GenerateTrees();
            isInited = true;       
            return;
        }

        if (nextUpdateTime > Time.time)
            return;

        nextUpdateTime = Time.time + gameTickInterval;

        Iteration();
        DrawMap();
    }

    private void Iteration()
    {
        for (int y = 0; y < mapY; y++)
        {
            for (int x = 0; x < mapX; x++)
            {
                lastCells[x, y] = cells[x, y];
                cells[x, y] = CellType.empty;
            }
        }

        if (trees.Count == 0)
        {
            iterationCount = 0;
            restartsCount++;
            GenerateTrees();
            return;
        }

        iterationCount++;

        double maxCurrGenerate = 0;
        int incomeEnergy;
        foreach (var tree in trees)
        {
            if (tree.cells.Count != 0 && maxCurrGenerate < tree.cells[0].genome.generation)
            {
                maxCurrGenerate = tree.cells[0].genome.generation;
            }

            if (tree.isDie)
            {
                if (tree.cells.Count == 0)
                {
                    removeTrees.Add(tree);
                    continue;
                }
                FallTree(tree);
                continue;
            }

            tree.DecreaseEnergy();
            if(tree.age > 5 && tree.cells.Count == 1)
            {
                removeTrees.Add(tree);
                continue;
            }

            incomeEnergy = 0;

            bool allIsWood = true;
            foreach (var cell in tree.cells)
            {
                if (cell.isWood)
                {
                    cells[cell.pos.x, cell.pos.y] = CellType.wood;
                    cellsColor[cell.pos.x, cell.pos.y] = tree.color;

                    incomeEnergy += GetCellEnergyInc(cell.pos.x, cell.pos.y);
                }
                else
                {
                    cells[cell.pos.x, cell.pos.y] = CellType.seed;

                    allIsWood = false;
                    cell.needEnergy -= GetCellEnergyInc(cell.pos.x, cell.pos.y);
                    if(cell.needEnergy <= 0)
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

            tree.energy += incomeEnergy;
            if(tree.energy < 0 || tree.age > 90 || allIsWood)
            {
                tree.isDie = true;
                DestoyTree(tree);
                continue;
            }
        }
        foreach (var tree in removeTrees)
        {
            trees.Remove(tree);
        }
        removeTrees.Clear();
        foreach (var tree in growedTrees)
        {
            trees.Add(tree);
        }
        growedTrees.Clear();

        maxCurrGeneration = maxCurrGenerate;
    }

    private void SeedGrow(Tree tree, Cell cell)
    {
        cell.isWood = true;
        cells[cell.pos.x, cell.pos.y] = CellType.wood;
        cellsColor[cell.pos.x, cell.pos.y] = tree.color;


        for (int sideNUm = 0; sideNUm < 4; sideNUm++)
        {
            var growedCell = cell.GetNextSeed(sideNUm);
            if (growedCell == null)
                continue;

            switch (sideNUm)
            {
                case 1:
                    growedCell.pos = new Vector2Int(cell.pos.x, cell.pos.y + 1);
                    if (growedCell.pos.y >= mapY)
                        continue;
                    break;

                case 3:
                    growedCell.pos = new Vector2Int(cell.pos.x, cell.pos.y - 1);
                    if (growedCell.pos.y < 0)
                        continue;
                    break;

                case 0:
                    growedCell.pos = new Vector2Int(cell.pos.x - 1, cell.pos.y);
                    if (growedCell.pos.x < 0)
                        growedCell.pos.x = mapX - 1;
                    break;

                case 2:
                    growedCell.pos = new Vector2Int((cell.pos.x + 1) % (mapX - 1), cell.pos.y);
                    break;
            }
            
            if (cells[growedCell.pos.x, growedCell.pos.y] != CellType.seed && 
                cells[growedCell.pos.x, growedCell.pos.y] != CellType.wood && 
                view.IsDefaultPixel(growedCell.pos.x, growedCell.pos.y))
            {
                growedCells.Add(growedCell);
                cells[growedCell.pos.x, growedCell.pos.y] = CellType.seed;
            }
        }
    }

    private void DestoyTree(Tree tree)
    {
        for (int i = 0; i < tree.cells.Count; i++)
        {
            if (tree.cells[i].isWood)
            {
                cells[tree.cells[i].pos.x, tree.cells[i].pos.y] = CellType.empty;
                tree.cells.Remove(tree.cells[i]);
                i--;
            }
        }
    }

    private void FallTree(Tree tree)
    {
        for (int i = 0; i < tree.cells.Count; i++)
        {
            if (tree.cells[i].pos.y > 0 && !view.IsDefaultPixel(tree.cells[i].pos.x, --tree.cells[i].pos.y))
            {
                tree.cells.Remove(tree.cells[i]);
                i--;
            }
            else if (tree.cells[i].pos.y == 0)
            {
                tree.cells[i].needEnergy = 18;
                tree.cells[i].activeGen = 0;
                tree.cells[i].genome.MutateGenome(25);

                var growedTree = new Tree();

                growedTree.cells.Add(tree.cells[i]);
                growedTrees.Add(growedTree);
                tree.cells.Remove(tree.cells[i]);

                growedTree.cells[0].genome.generation++;
                if(growedTree.cells[0].genome.generation % 50 == 0)
                {
                    growedTree.color = new Color(
                        UnityEngine.Random.Range(0f, 1f), 
                        UnityEngine.Random.Range(0f, 1f), 
                        UnityEngine.Random.Range(0f, 1f));
                }
                else
                {
                    growedTree.color = new Color(tree.color.r, tree.color.g, tree.color.b);
                }
                cells[growedTree.cells[0].pos.x, growedTree.cells[0].pos.y] = CellType.seed;
                cellsColor[growedTree.cells[0].pos.x, growedTree.cells[0].pos.y] = growedTree.color;
                i--;
            }
            else if(cells[tree.cells[i].pos.x, tree.cells[i].pos.y] == CellType.empty)
            {
                cells[tree.cells[i].pos.x, tree.cells[i].pos.y] = CellType.fallSeed;
            }
        }
    }

    private int GetCellEnergyInc(int x, int y)
    {
        int energyMod = 3;

        for (int currY = y+1; currY < mapY; currY++)
        {
            if (view.pixels[x, currY].color.r != view.defaultBackgroundColor.r ||
                view.pixels[x, currY].color.g != view.defaultBackgroundColor.g ||
                view.pixels[x, currY].color.b != view.defaultBackgroundColor.b)
            {
                energyMod--;
            }

            if (energyMod == 0)
                return 0;
        }

        int sunLevel = 6 + y > 16 ? 16 : 6 + y;
        return sunLevel * energyMod;
    }

    private void DrawMap()
    {
        /*
        view.Clear();
        foreach (var tree in trees)
        {
            foreach (var cell in tree.cells)
            {
                view.SetPixelIn(cell.pos.x, cell.pos.y, cell.isWood ? tree.color : Color.white);
            }
        }*/


        for (int y = 0; y < mapY; y++)
        {
            for (int x = 0; x < mapX; x++)
            {
                if(lastCells[x,y] != cells[x, y])
                {
                    if (cells[x,y] == CellType.empty)
                    {
                        view.SetPixelIn(x, y, view.defaultBackgroundColor);
                    }
                    else if (cells[x, y] == CellType.wood)
                    {
                        view.SetPixelIn(x, y, cellsColor[x, y]);
                    }
                    else
                    {
                        view.SetPixelIn(x, y, seedColor);
                    }
                }
            }
        }
    }

    private void GenerateTrees()
    {
        for (int x = 0; x < mapX; x+=20)
        {
            GenerateNewTree(x, 0);
        }
    }

    private void GenerateNewTree(int x, int y)
    {
        if (!view.IsDefaultPixel(x, y))
            return;

        var tree = new Tree()
        {
            color = new Color(
                        UnityEngine.Random.Range(0f, 1f),
                        UnityEngine.Random.Range(0f, 1f),
                        UnityEngine.Random.Range(0f, 1f))
        };
        var cell = new Cell()
        {
            pos = new Vector2Int(x, y)
        };
        cell.genome = Genome.GetRandomeGenome();

        tree.cells.Add(cell);

        trees.Add(tree);
    }

    private IEnumerator InitView()
    {
        while (true)
        {
            yield return new WaitForEndOfFrame();
            if(PixelViewUI.Instance != null)
            {
                PixelViewUI.Instance.InitView(new Vector2Int(mapX, mapY), Color.black, 6, false);
               // PixelViewScene.Instance.InitView(new Vector2Int(mapX, mapY), Color.black);
                view = PixelViewUI.Instance;
                isStarted = true;
                break;
            }
        }
    }
    private IEnumerator UpdateUIPerSecond()
    {
        while (true)
        {
            yield return new WaitForSeconds(1);

            fpsText.text = "FPS: " + (int)(1f / Time.deltaTime);
            allDieTimesText.text = "Restarts: " + restartsCount;
            iterationText.text = "Iterations: " + iterationCount;
            generationText.text = "Best generation: " + maxCurrGeneration;
        }
    }

    public void SaveGenomes()
    {

    }

    public void LoadGenomes()
    {

    }

    public void SpeedUp()
    {
        if(gameTickInterval > 0.01f)
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
        trees.Clear();
        GenerateTrees();
    }

    public void Pause()
    {
        isPaused = !isPaused;
    }
}
