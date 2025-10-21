using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//プレイヤーの各データや状態を保持するスクリプト
public class PlayerData : MonoBehaviour
{
    //基礎速度
    [Tooltip("基礎速度")] public float basicSpeed = 2f;
    //回転速度
    [Tooltip("1秒に回転する角度")] public float rotSpeed = 180f;
    //胴体の数
    [Tooltip("胴体の数")] public int bodyNum = 4;
}
