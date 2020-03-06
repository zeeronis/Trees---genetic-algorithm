using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class Tree
{
    public int age = 0;
    public int energy = 300;
    public bool isDie;
    public Color color;
    public List<Cell> cells = new List<Cell>();

    public void DecreaseEnergy()
    {
        age++;
        energy -= cells.Count *  13;
    }
}

public enum CellType
{
    empty,
    wood,
    seed,
    fallSeed
}

public class Cell
{
    public Vector2Int pos = new Vector2Int();

    public int needEnergy = 18;
    public int activeGen = 0;
    public bool isWood;
    public Genome genome;

    public Cell GetNextSeed(int side)
    {
        if (genome.gens[activeGen].sides[side] < 15)
            return new Cell()
            {
                genome = genome.Clone(),
                activeGen = genome.gens[activeGen].sides[side]
            };
        return null;
    }
}

public class Genome
{
    public int generation = 0;
    public Gen[] gens = new Gen[15];

    public void MutateGenome(int chance)
    {
        if (Random.Range(0, 101) > chance)
            return;

        gens[Random.Range(0, 15)].sides[Random.Range(0, 4)] = Random.Range(0, 31);
    }

    public static Genome GetRandomeGenome()
    {
        var genome = new Genome();
        for (int i = 0; i < genome.gens.Length; i++)
        {
            genome.gens[i] = new Gen();
        }
        foreach (var gen in genome.gens)
        {
            for (int i = 0; i < 4; i++)
            {
                gen.sides[i] = Random.Range(0, 31);
            }
        }
        return genome;
    }

    public Genome Clone()
    {
        var clone = new Genome()
        {
            generation = generation
        };
        /*for (int i = 0; i < 15; i++)
        {
            clone.gens[i] = new Gen();
            for (int j = 0; j < gens[i].sides.Length; j++ )
            {
                clone.gens[i].sides[j] = gens[i].sides[j];
            }
        }*/
        clone.gens[0] = new Gen();
        clone.gens[1] = new Gen();
        clone.gens[2] = new Gen();
        clone.gens[3] = new Gen();
        clone.gens[4] = new Gen();
        clone.gens[5] = new Gen();
        clone.gens[6] = new Gen();
        clone.gens[7] = new Gen();
        clone.gens[8] = new Gen();
        clone.gens[9] = new Gen();
        clone.gens[10] = new Gen();
        clone.gens[11] = new Gen();
        clone.gens[12] = new Gen();
        clone.gens[13] = new Gen();
        clone.gens[14] = new Gen();

        for (int j = 0; j < 4; j++)
        {
            clone.gens[0].sides[j] = gens[0].sides[j];
            clone.gens[1].sides[j] = gens[1].sides[j];
            clone.gens[2].sides[j] = gens[2].sides[j];
            clone.gens[3].sides[j] = gens[3].sides[j];
            clone.gens[4].sides[j] = gens[4].sides[j];
            clone.gens[5].sides[j] = gens[5].sides[j];
            clone.gens[6].sides[j] = gens[6].sides[j];
            clone.gens[7].sides[j] = gens[7].sides[j];
            clone.gens[8].sides[j] = gens[8].sides[j];
            clone.gens[9].sides[j] = gens[9].sides[j];
            clone.gens[10].sides[j] = gens[10].sides[j];
            clone.gens[1].sides[j] = gens[11].sides[j];
            clone.gens[12].sides[j] = gens[12].sides[j];
            clone.gens[13].sides[j] = gens[13].sides[j];
            clone.gens[14].sides[j] = gens[14].sides[j];

        }
        return clone;
    }
}

public class Gen
{
    public int[] sides = new int[4];
}

public enum Side
{
    Up,
    Down,
    Left,
    Right
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