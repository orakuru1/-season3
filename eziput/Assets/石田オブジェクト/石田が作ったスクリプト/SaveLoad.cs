using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

[System.Serializable]
public class PlayerSaveData
{
    public WeaponMasterySet weaponMasterySet;
    public List<WeaponSkillLoadoutSaveData> weaponSkillLoadouts;
    public List<int> godids;
}

[System.Serializable]
public class WeaponSkillLoadoutSaveData
{
    public WeaponType weaponType;
    public List<int> skillIds;
}


public class SaveLoad : MonoBehaviour
{
    public static SaveLoad instance { get; private set; }
    private string SavePath => Application.persistentDataPath + "/save.json";//ファイルへのパス
    private PlayerSaveData saveData = new PlayerSaveData();//保存するデータの入れ物
    private void Awake()
    {
        if (instance == null) instance = this; else Destroy(gameObject);
    }

    public void CreateSaveData(PlayerUnit player, GodPlayer godPlayer)//神の加護のセーブのために、GodPlayerを引数に追加。追加。
    {
        saveData.weaponMasterySet = player.weaponMasterySet;
        saveData.weaponSkillLoadouts = new List<WeaponSkillLoadoutSaveData>();
        saveData.godids = new List<int>();//神の加護のセーブのために追加

        foreach (var loadout in player.weaponSkillLoadouts)
        {
            WeaponSkillLoadoutSaveData loadoutData = new WeaponSkillLoadoutSaveData();
            loadoutData.weaponType = loadout.weaponType;
            loadoutData.skillIds = new List<int>();

            foreach (var skill in loadout.skills)
            {
                loadoutData.skillIds.Add(skill.skillID);
            }

            saveData.weaponSkillLoadouts.Add(loadoutData);
        }

        foreach(var god in godPlayer.ownedGods)//神の加護のセーブのために追加
        {
            saveData.godids.Add(god.id);
        }

        Save();
    }

    public void Save()
    {
        string json = JsonUtility.ToJson(saveData, true);//***************JSON形式に書き換えてる。見やすい（true）, 構造体やクラスをシリアライズする仕組み

        //StreamWriter wr = new StreamWriter(SavePath, false);//上書き
        //wr.WriteLine(json);
        //wr.Close();
        File.WriteAllText(SavePath, json);//****************ReadWritterを省略した書き方。自動上書き.close()兼ねてる
        Debug.Log("保存しました:" + SavePath);
    }
    
    public void Load(PlayerUnit player, GodPlayer godPlayer)//神の加護のロードのために、GodPlayerを引数に追加。追加。
    {
        //ファイルが既に存在してるか？
        if (!File.Exists(SavePath))
        {
            Debug.Log("セーブデータがありません");
            return;
        }

        string json = File.ReadAllText(SavePath);//****************ReadWritterを省略した書き方。自動上書き.close()兼ねてる
        PlayerSaveData savedata = JsonUtility.FromJson<PlayerSaveData>(json);//JSON形式から元の形に戻す

        player.weaponMasterySet.CopyFrom(savedata.weaponMasterySet);
        player.weaponSkillLoadouts.Clear();
        godPlayer.ownedGods.Clear();//神の加護のロードのために追加

        foreach(var loadoutData in savedata.weaponSkillLoadouts)
        {
            WeaponSkillLoadout loadout = new WeaponSkillLoadout();
            loadout.weaponType = loadoutData.weaponType;
            loadout.skills = new List<SkillData>();

            foreach(var skillId in loadoutData.skillIds)
            {
                SkillData skill = player.allSkillsDatabase.GetById(skillId);
                if(skill != null)
                {
                    loadout.skills.Add(skill);
                }
            }

            player.weaponSkillLoadouts.Add(loadout);
        }

        foreach(var godId in savedata.godids)//神の加護のロードのために追加
        {
            GodData god = GodManeger.Instance.godDatabase.GetById(godId);
            if(god != null)
            {
                godPlayer.ownedGods.Add(god);
            }
        }

        Debug.Log("ロードしました:" + SavePath);
        Debug.Log(savedata.weaponMasterySet);
    }
}
