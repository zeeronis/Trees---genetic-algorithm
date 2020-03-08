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