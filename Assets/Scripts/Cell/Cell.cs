
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class Cell
{
    public CellType cellType = CellType.empty;
    public Color color = new Color() { a = 1f };
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
