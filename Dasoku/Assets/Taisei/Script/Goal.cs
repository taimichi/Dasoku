using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Goal : MonoBehaviour
{
    public static Goal Instance;

    [SerializeField]private GameObject NoGetKey;

    /// <summary>
    /// カギの構造体
    /// </summary>
    public struct KEYS
    {
        //カギのゲームオブジェクト
        public GameObject KeyObject;
        //カギを取得しているか
        //false=取得していない / true=取得している
        public bool isGetKey;
    }
    //カギ構造体のリスト
    public List<KEYS> KeyList = new List<KEYS>();

    private ClearManager clearMG;

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        clearMG = GameObject.Find("ClearCanvas").GetComponent<ClearManager>();
        clearMG.CloseClearCanvas();
    }


    void Update()
    {
        if (!CheckKey())
        {
            if (!NoGetKey.activeSelf)
            {
                NoGetKey.SetActive(true);
            }
        }
        else
        {
            if (NoGetKey.activeSelf)
            {
                NoGetKey.SetActive(false);
            }
        }
    }

    /// <summary>
    /// カギをすべて取得しているか
    /// </summary>
    /// <returns>false=取得していない / true=取得しているorカギが存在しない</returns>
    private bool CheckKey()
    {
        //カギが存在しないとき
        if(KeyList.Count == 0)
        {
            return true;
        }

        bool _checkKey = false;
        
        //カギ構造体リストのisGetKeyを全てチェック
        for(int i = 0; i < KeyList.Count; i++)
        {
            //1つでも未取得があればfalseに
            if (!KeyList[i].isGetKey)
            {
                _checkKey = false;
                break;
            }

            _checkKey = true;
        }

        return _checkKey;
    }
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        //触れたのがプレイヤーの時 かつ カギをすべて取得
        if (collision.CompareTag("Player") && CheckKey())
        {
            Debug.Log("クリア");
            GameManager.Instance.isClear = true;

            if(SceneManager.GetActiveScene().name == "Tutorial")
            {
                GameData.GameEntity.tutorialClearNum++;
            }

            clearMG.DisplayClearCanvas();
        }
    }

    /// <summary>
    /// 新たなカギをカギ構造体リストに追加
    /// </summary>
    /// <param name="_key">新しいカギオブジェクト</param>
    public void AddKeyObject(GameObject _key)
    {
        //新しいカギ構造体
        KEYS getKey;
        //取得したカギオブジェクトを構造体に
        getKey.KeyObject = _key;
        getKey.isGetKey = false;

        //設定したカギ構造体をリストに追加
        KeyList.Add(getKey);
    }

    public void DeleteKey(GameObject _key)
    {
        for(int i = 0; i < KeyList.Count; i++)
        {
            if(_key == KeyList[i].KeyObject)
            {
                //リストからカギを削除
                KeyList.Remove(KeyList[i]);
                //オブジェクトのカギを削除
                Destroy(_key);
            }
        }
    }

    /// <summary>
    /// カギを入手
    /// </summary>
    /// <param name="_key">入手したカギオブジェクト</param>
    public void GetKey(GameObject _key)
    {
        for(int i = 0; i < KeyList.Count; i++)
        {
            if(_key == KeyList[i].KeyObject)
            {
                //構造体を置き換える
                KEYS getKey;
                //取得したカギオブジェクトを構造体に
                getKey.KeyObject = _key;
                getKey.isGetKey = true;

                KeyList[i] = getKey;
                break;
            }
        }
    }

    /// <summary>
    /// このスクリプトがついているゴールオブジェクトを返す
    /// </summary>
    /// <returns>ゴールオブジェクト</returns>
    public GameObject ReturnGoal() => this.gameObject;
}
