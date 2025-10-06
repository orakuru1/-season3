using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

public class otamesi : MonoBehaviour
{
    /*
    public List<Player> players;

    private string SavePath => Application.persistentDataPath  + "/save.json";//ファイルへのパス

    [SerializeField]private GameObject PlayerButton;

    [SerializeField]private Transform PlayerContent;

    [SerializeField]private TitleUI titleUI;//ロードすることでセーブ前のデータも全部保存しようとしてる。後、番号は改正
    void Start()
    {
        //データがセーブするたびに消えなくなった。後は、変更したデータが元の位置で保存されるようにする。
    }

    // Update is called once per frame
    void Update()
    {
        //新しく生成された奴が変化しても値が変わらないのはクリックイベントを登録していないから。
        //セーブしたときに値が変わるのは、それまでのデータなども全部あるからまとめて更新している。
        //このスクリプトがシーン遷移でなくなったら、もう一回ロードしないと、みんなのデータがわからない。
        //だから、あっちのシーン専用のセーブシステムを作るべきかもしれない。
        //IDを持って行って、そのIDが入っているパスを開いてロードする処理かもしれない。
    }

    public void Save()
    {
        GameSaveData savedata = new GameSaveData();//セーブ専用のデータが入ってるリスト

        foreach(var c in players)//今インスタンス化されてるプレイヤーの数
        {
            savedata.statuss.Add(new StatusSaveData //*******************セーブ専用のデータにプレイヤーのデータ達を入れてる
            {
                id = c.instanceData.id,
                characterName = c.instanceData.characterName,
                HP = c.instanceData.HP,
                maxHP = c.instanceData.maxHP,
                speed = c.instanceData.speed
            });
        }
        savedata.counts.Count = TitleUI.ID;
        GameData.selectedPlayers = savedata.statuss;
        
        foreach(var a in GameData.selectedPlayers)
        {
            Debug.Log(a.id);
        }

        string json = JsonUtility.ToJson(savedata, true);//***************JSON形式に書き換えてる。見やすい（true）
        //StreamWriter wr = new StreamWriter(SavePath, false);//上書き
        //wr.WriteLine(json);
        //wr.Close();
        File.WriteAllText(SavePath, json);//****************ReadWritterを省略した書き方。自動上書き.close()兼ねてる
        Debug.Log("保存しました:" + SavePath);
    }

    public void Load()
    {
        if(!File.Exists(SavePath))//ファイルがあるかどうかだけ見てる
        {
            Debug.LogWarning("セーブファイルが見つかりません");
            return;
        }

        string json = File.ReadAllText(SavePath);//パスの中にあるテキストを全部読み込んでる
        GameSaveData saveData = JsonUtility.FromJson<GameSaveData>(json);//元のデータの方に戻してる。セーブ用のリスト型に戻した。

        TitleUI.ID = saveData.counts.Count;

        //最初のタイトルシーンだけやる。
        if(PlayerButton != null)
        {
            foreach(var saved in saveData.statuss)//戻したリストの数だけやる
            {
                GameObject go = Instantiate(PlayerButton, PlayerContent);//プレイヤーUIを生成,表示してる
                Player playeruis = go.GetComponent<Player>();
                playeruis.Init();
                playeruis.SetSave(this);
                Button button = go.GetComponent<Button>();
                button.onClick.AddListener(playeruis.OnPush); 
                button.onClick.AddListener(titleUI.OnClickPlayer);
                players.Add(playeruis);

                playeruis.instanceData.id = saved.id;
                playeruis.instanceData.characterName = saved.characterName;
                playeruis.instanceData.HP = saved.HP;
                playeruis.instanceData.maxHP = saved.maxHP;
                playeruis.instanceData.speed = saved.speed;
            }

            GameData.selectedPlayers = saveData.statuss;//staticに最初のキャラクターたちを保存

            Save();
        }


        


        foreach(var saved in saveData.statuss)//戻したリストの数だけやる
        {
            var character = players.FirstOrDefault(c => c.instanceData.id == saved.id);//一番最初にヒットしたデータを戻す。idが一緒の奴かどうか
            if(character != null)//今インスタンス化されてるプレイヤーたちに、保存されていたデータを読み込んでる。
            {
                character.instanceData.characterName = saved.characterName;//IDのロードはしなくていいんだっけ？いや、いるくね？いや、すでにIDは一緒かIDは違う所で渡す。生成するときにだな。
                character.instanceData.HP = saved.HP;
                character.instanceData.maxHP = saved.maxHP;
                character.instanceData.speed = saved.speed;
            }
        }

        Debug.Log("ロード完了");
    }
    */
}
