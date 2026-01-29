using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleManager : MonoBehaviour
{
    /// <summary>
    /// タイトルシーンのメニューの種類
    /// </summary>
    private enum MENU_STATE
    {
        start,
        tutorial,
        end
    }

    /// <summary>
    /// メニュー構造体
    /// </summary>
    [System.Serializable]
    private struct MENUS
    {
        //メニューの種類
        [SerializeField]
        public MENU_STATE mode;
        //メニューのオブジェクト
        [SerializeField]
        public GameObject menuObj;
    }

    [SerializeField]
    private GameObject menuCursor;

    /// <summary>
    /// メニュー構造体配列
    /// </summary>
    [SerializeField]
    private MENUS[] menu;

    //現在選ばれてるメニューの種類
    private MENU_STATE nowMode = MENU_STATE.start;
    private int nowNum = 0;

    [SerializeField]
    private Animator bar_anim;

    //フェード用
    private Fade fade;

    void Start()
    {
        fade = GameObject.Find("FadeCanvas").GetComponent<Fade>();
    }

    void Update()
    {
        MenuInput();
        MenuCursorMove();
    }

    /// <summary>
    /// タイトルのキー入力処理
    /// </summary>
    private void MenuInput()
    {
        //上
        if (Input.GetKeyDown(KeyCode.W))
        {
            nowNum--;
            if(nowNum < 0)
            {
                nowNum = menu.Length - 1;
            }

            bar_anim.Play("bar_idle", 0, 0);
        }
        //下
        else if (Input.GetKeyDown(KeyCode.S))
        {
            nowNum++;
            if(nowNum > menu.Length - 1)
            {
                nowNum = 0;
            }
            bar_anim.Play("bar_idle", 0, 0);
        }

        //決定
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
        {
            MenuDecision();
        }
    }

    /// <summary>
    /// メニューカーソルの移動処理
    /// </summary>
    private void MenuCursorMove()
    {
        //カーソルの移動
        Vector2 pos = new Vector2(menuCursor.transform.position.x, menu[nowNum].menuObj.transform.position.y);
        menuCursor.transform.position = pos;

        //現在のメニューの種類
        nowMode = menu[nowNum].mode;
    }

    /// <summary>
    /// 決定押したときの処理
    /// </summary>
    private void MenuDecision()
    {
        float _fadeTime = 0.75f;
        switch (nowMode)
        {
            //スタート
            case MENU_STATE.start:
                Debug.Log("start");
                //フェードイン
                fade.FadeIn(_fadeTime, () =>
                {
                    //フェードインが終わったら
                    //ステージセレクトシーンへ移行
                    SceneManager.LoadScene("StageSelect");
                });
                break;

            //チュートリアル
            case MENU_STATE.tutorial:
                Debug.Log("チュートリアル");
                //フェードイン
                fade.FadeIn(_fadeTime, () =>
                {
                    //フェードインが終わったら
                    //チュートリアルシーンへ移行
                    SceneManager.LoadScene("Tutorial");
                });
                break;

            //ゲーム終了
            case MENU_STATE.end:
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;//ゲームプレイ終了
#else
    Application.Quit();//ゲームプレイ終了
#endif
                break;
        }
    }
}
