using UnityEngine;
using System.Collections.Generic;

public class TrapPlacer : MonoBehaviour
{
    public int trapCount = 5;
    public List<GridBlock> placedTraps = new List<GridBlock>();//確認用

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
            int index = Random.Range(0, candidates.Count);
            var block = candidates[index];
            candidates.RemoveAt(index);

            block.trapType = TrapType.Damage;
            block.trapValue = Random.Range(5, 10);
            block.ArrowTrapSE = trapse;
            //block.OnDrawGizmos();
            placedTraps.Add(block);
        }
    }
}
