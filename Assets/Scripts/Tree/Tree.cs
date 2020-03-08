using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class Tree
{
    public bool isDie;
    public int age;
    public int energy;
    public List<Cell> cells = new List<Cell>();

    public void SetInitialValues()
    {
        isDie = false;
        age = 0;
        energy = 300;
    }

    public void DecreaseEnergy()
    {
        age++;
        energy -= cells.Count * 13;
    }

    public void ClearAllCells()
    {
        foreach (var cell in cells)
        {
            cell.cellType = CellType.empty;
        }
        cells.Clear();
    }

    public void ClearWoodCells()
    {
        for (int i = 0; i < cells.Count; i++)
        {
            if (cells[i].cellType == CellType.wood)
            {
                cells[i].cellType = CellType.empty;
                cells.Remove(cells[i]);
                i--;
            }
        }
    }
}

public class Cell
{
    public CellType cellType = CellType.empty;
    public Color color = new Color() { a = 1f};
    public Vector2Int pos;

    public int needEnergy;
    public int activeGen;

    public Genome genome = new Genome();

    public void SetInitialValues()
    {
        cellType = CellType.seed;
        needEnergy = 18;
        activeGen = 0;
    }

    public void SetRandomGenome()
    {
        genome.generation = 0;
        foreach (var gen in genome.gens)
        {
            for (int i = 0; i < gen.sides.Length; i++)
            {
                gen.sides[i] = Random.Range(0, 31);
            }
        }
    }

    public void CopyGenome(Genome _genome)
    {
        genome.generation = _genome.generation;
        for (int i = 0; i < _genome.gens.Length; i++)
        {
            for (int j = 0; j < _genome.gens[i].sides.Length; j++)
            {
                genome.gens[i].sides[j] = _genome.gens[i].sides[j];
            }
        }
    }

    public void SetRandomColor()
    {
        color.r = Random.Range(0f, 1f);
        color.g = Random.Range(0f, 1f);
        color.b = Random.Range(0f, 1f);
    }

    public void ClearGenome()
    {
        genome.generation = 0;
    }
}

public class Genome
{
    public int generation = 0;
    public Gen[] gens = new Gen[15];
}

public class Gen
{
    public int[] sides = new int[4];
}

public enum CellType
{
    empty,
    wood,
    seed,
    fallSeed
}

//эволюция: 25% у нового семечка происходит мутация. Меняется случайным образом одно из чисел в геноме
//мир замкнут по Х координате
//прирост энергии от wood = (1~3) * Y; (Клетки загораживают друг друга. каждый слой над текущим -1 к энергии)
//seeds тратят получаемую энергию на свой рост
//уровни солнца от 6 до 16
//расход энергии на каждую клетку = 13
//начальная энергия нового дерева = 300
// необходимая энергия для роста seed = 18

//геном состоит из 16ти генов в каждом из которов 4 направления роста и значения (0-30 какой активный ген будет у след семечка) числа больше 15 ничего не делают. начальное семечко имеет ген 0

//88-92 хода-смэрть