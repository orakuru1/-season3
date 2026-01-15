using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Database/GodDatabase")]
public class GodDatabase : ScriptableObject
{
    public List<GodData> gods;

    private Dictionary<int, GodData> godDict;

    public void BuildDictionary()
    {
        godDict = new Dictionary<int, GodData>();

        foreach(var god in gods)
        {
            if(!godDict.ContainsKey(god.id))
            {
                godDict.Add(god.id, god);
            }
        }
    }

    public GodData GetById(int id)
    {
        if(godDict.TryGetValue(id, out GodData god))
        {
            return god;
        }
        Debug.LogWarning($"GodDatabase: ID {id} の神が見つかりません。");
        return null;
    }
}
