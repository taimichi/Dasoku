using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ClearManager : MonoBehaviour
{
    /// <summary>
    /// クリアキャンバスのメニューの種類
    /// </summary>
    private enum MENU_STATE
    {
        select,
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
    private MENU_STATE nowMode = MENU_STATE.select;
    private int nowNum = 0;

    //クリアテキストのアニメーター
    [SerializeField] private Animator ClearTextAnim;

    //クリアキャンバスのメニュー
    [SerializeField] private GameObject clearMenu;

    //アニメーションを再生したか
    private bool isAnimPlay = false;

    //フェード用
    private Fade fade;

    void Start()
    {
        fade = GameObject.Find("FadeCanvas").GetComponent<Fade>();
    }
    void Update()
    {
        if (!isAnimPlay)
        {
            if(ClearTextAnim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1)
            {
                clearMenu.SetActive(true);
                isAnimPlay = true;
            }
            return;
        }

        MenuInput();
        MenuCursorMove();
    }


    /// <summary>
    /// クリアキャンバスのキー入力処理
    /// </summary>
    private void MenuInput()
    {
        //上
        if (Input.GetKeyDown(KeyCode.W))
        {
            nowNum--;
            if (nowNum < 0)
            {
                nowNum = menu.Length - 1;
            }
        }
        //下
        else if (Input.GetKeyDown(KeyCode.S))
        {
            nowNum++;
            if (nowNum > menu.Length - 1)
            {
                nowNum = 0;
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
        float fadeTime = 0.75f;
        switch (nowMode)
        {
            //セレクト
            case MENU_STATE.select:
                fade.FadeIn(fadeTime, ()=>
                {
                    SceneManager.LoadScene("StageSelect");
                });
                break;

            //タイトル
            case MENU_STATE.title:
                fade.FadeIn(fadeTime, () =>
                {
                    SceneManager.LoadScene("Title");
                });
                break;
        }
    }

    /// <summary>
    /// クリアキャンバス表示
    /// </summary>
    public void DisplayClearCanvas()
    {
        this.gameObject.SetActive(true);
    }

    /// <summary>
    /// クリアキャンバス非表示
    /// </summary>
    public void CloseClearCanvas()
    {
        this.gameObject.SetActive(false);
    }
}
