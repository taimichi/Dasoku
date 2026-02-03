using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StageSelectManager : MonoBehaviour
{
    private enum MENU_STATE
    {
        stage1,
        stage2,
        stage3,
        stage4,
        stage5,
        title
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
    private MENU_STATE nowMode = MENU_STATE.stage1;
    private int nowNum = 0;

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
    /// キー入力処理
    /// </summary>
    private void MenuInput()
    {
        //左
        if (Input.GetKeyDown(KeyCode.A))
        {
            nowNum--;
            if (nowNum < 0)
            {
                nowNum = menu.Length - 1;
            }
        }
        //右
        else if (Input.GetKeyDown(KeyCode.D))
        {
            nowNum++;
            if (nowNum > menu.Length - 1)
            {
                nowNum = 0;
            }

        }
        //上
        else if (Input.GetKeyDown(KeyCode.W))
        {
            if(nowNum >= menu.Length / 2)
            {
                nowNum -= 3;
                if(nowNum < 0)
                {
                    nowNum += 3;
                }
            }
        }
        //下
        else if (Input.GetKeyDown(KeyCode.S))
        {
            if (nowNum < menu.Length / 2)
            {
                nowNum += 3;
                if(nowNum > menu.Length - 1)
                {
                    nowNum = menu.Length - 1;
                }
            }
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
        Vector2 pos = new Vector2(menu[nowNum].menuObj.transform.position.x, menu[nowNum].menuObj.transform.position.y);
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
            case MENU_STATE.stage1:
                fade.FadeIn(_fadeTime, () =>
                {
                    SceneManager.LoadScene("StageScene2");
                });
                break;

            case MENU_STATE.stage2:
                fade.FadeIn(_fadeTime, () =>
                {
                    SceneManager.LoadScene("StageScene3");
                });
                break;

            case MENU_STATE.stage3:
                fade.FadeIn(_fadeTime, () =>
                {
                    SceneManager.LoadScene("StageScene4");
                });
                break;

            case MENU_STATE.stage4:
                fade.FadeIn(_fadeTime, () =>
                {
                    SceneManager.LoadScene("StageScene5");
                });
                break;

            case MENU_STATE.stage5:
                fade.FadeIn(_fadeTime, () =>
                {
                    SceneManager.LoadScene("");
                });
                break;

            case MENU_STATE.title:
                fade.FadeIn(_fadeTime, () =>
                {
                    SceneManager.LoadScene("Title");
                });
                break;
                
        }
    }
}
