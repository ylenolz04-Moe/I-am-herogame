using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwingTrapAxis : MonoBehaviour
{
    [Header("摆动设置")]
    [Tooltip("单侧摆动最大角度")]
    public float swingRange = 60f;
    [Tooltip("摆动速度")]
    public float swingSpeed = 2f;
    [Tooltip("摆动转轴，2D游戏默认绕Z轴旋转")]
    public Vector3 swingAxis = new Vector3(0, 0, 1);

    [Header("陷阱开关")]
    public bool isActive = true;

    // 记录初始旋转
    private Quaternion initialRotation;

    private void Start()
    {
        initialRotation = transform.localRotation;
    }

    private void Update()
    {
        if (!isActive) return;

        // Sin 函数产生平滑往复摆动
        float currentAngle = Mathf.Sin(Time.time * swingSpeed) * swingRange;
        transform.localRotation = initialRotation * Quaternion.Euler(swingAxis * currentAngle);
    }

    /// <summary>
    /// 外部调用，开关陷阱
    /// </summary>
    public void SetTrapActive(bool state)
    {
        isActive = state;
    }
}
