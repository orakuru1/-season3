using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

public class SaveLoad : MonoBehaviour
{
    public List<kariPlayer> players = new List<kariPlayer>();//今インスタンス化されてるプレイヤーの数
    private string SavePath => Application.persistentDataPath + "/save.json";//ファイルへのパス
    private List<StatusSaveData> statussavedata = new List<StatusSaveData>();//セーブ専用のデータが入ってるリスト

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            Save();
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            Load();
        }
    }

    public void Save()
    {

        foreach (var c in players)//今インスタンス化されてるプレイヤーの数
        {
            statussavedata.Add(new StatusSaveData //*******************セーブ専用のデータにプレイヤーのデータ達を入れてる
            {
                idname = c.Status.idname,
                characterName = c.Status.characterName,
                HP = c.Status.HP,
                maxHP = c.Status.maxHP,
                speed = c.Status.speed,
            });
        }

        SaveData savedata = new SaveData();
        savedata.statuss = statussavedata;

        string json = JsonUtility.ToJson(savedata, true);//***************JSON形式に書き換えてる。見やすい（true）, 構造体やクラスをシリアライズする仕組み

        //StreamWriter wr = new StreamWriter(SavePath, false);//上書き
        //wr.WriteLine(json);
        //wr.Close();
        File.WriteAllText(SavePath, json);//****************ReadWritterを省略した書き方。自動上書き.close()兼ねてる
        Debug.Log("保存しました:" + SavePath);
        statussavedata.Clear();
    }
    
    public void Load()
    {
        //ファイルが既に存在してるか？
        if (!File.Exists(SavePath))
        {
            Debug.Log("セーブデータがありません");
            return;
        }

        string json = File.ReadAllText(SavePath);//****************ReadWritterを省略した書き方。自動上書き.close()兼ねてる
        SaveData savedata = JsonUtility.FromJson<SaveData>(json);//JSON形式から元の形に戻す

        foreach (var saved in savedata.statuss)
        {
            var character = players.FirstOrDefault(c => c.gameObject.name == saved.idname);//一番最初にヒットしたデータを戻す。idが一緒の奴かどうか
            if (character != null)
            {
                character.Status.characterName = saved.characterName;
                character.Status.HP = saved.HP;
                character.Status.maxHP = saved.maxHP;
                character.Status.speed = saved.speed;
            }
            
        }

        Debug.Log("ロードしました:" + SavePath);
    }
}
