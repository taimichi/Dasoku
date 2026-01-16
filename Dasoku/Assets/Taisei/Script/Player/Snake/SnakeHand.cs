using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class SnakeController : MonoBehaviour
{
    //蛇手関連
    [Header("蛇手関連")]
    //スプリングジョイント
    [SerializeField] private SpringJoint2D joint;
    /// <summary>
    /// 蛇手のアクション状態
    /// </summary>
    private enum HANDACTION_STATE
    {
        idle,   //待機
        action, //アクション
        move,   //移動
    }
    private HANDACTION_STATE nowHandAction = HANDACTION_STATE.idle;
    
    private void SnakeHandAction()
    {

    }
}
