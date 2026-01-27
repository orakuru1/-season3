using UnityEngine;
using System.Collections.Generic;

public class TrapPlacer : MonoBehaviour
{
    public int trapCount = 5;
    public List<GridBlock> DamageTrap = new List<GridBlock>();//確認用
    public List<GridBlock> StunTrap = new List<GridBlock>();//確認用

    [Header("Trap Sound Effect")]
    public AudioClip trapse;

    void Start()
    {
        //PlaceTraps();
    }

    public void PlaceTraps()
    {
        var blocks = GridManager.Instance.GetAllBlocks();
        var candidates = new List<GridBlock>();

        foreach (var b in blocks)
        {
            if (b.isWalkable && b.trapType == TrapType.None)
                candidates.Add(b);
        }

        for (int i = 0; i < trapCount && candidates.Count > 0; i++)
        {
            if (Random.Range(0f, 1f) < 0.5f)
            {
            int index = Random.Range(0, candidates.Count);
            var block = candidates[index];
            candidates.RemoveAt(index);

            block.trapType = TrapType.Damage;
            block.trapValue = Random.Range(5, 10);
            block.ArrowTrapSE = trapse;
            DamageTrap.Add(block); 
            }
            else
            {
            int index2 = Random.Range(0, candidates.Count);
            var block2 = candidates[index2];
            candidates.RemoveAt(index2);

            block2.trapType = TrapType.Stun;
            block2.trapValue = 2; //スタンターン数
            block2.ArrowTrapSE = trapse;
            StunTrap.Add(block2);
            }



        }
    }
}
