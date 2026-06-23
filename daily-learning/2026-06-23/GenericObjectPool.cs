// ============================================================
// 每日脚本学习 Day 2 — 2026-06-23
// 主题: C# 通用对象池模式 (Object Pool Pattern)
// 适用: Unity 游戏开发 · 任何需要频繁创建/销毁对象的场景
// ============================================================

using System;
using System.Collections.Generic;
using UnityEngine;

// ============================================================
// 知识点 1: 泛型对象池 — 纯 C# 实现，不依赖 Unity
// ============================================================

/// <summary>
/// 线程安全的泛型对象池。
/// 适用于: 网络消息包、临时字符串构建器、计算缓冲区等
/// <para>知识点: where T : class, new() 约束确保 T 是引用类型且有无参构造函数</para>
/// </summary>
public class GenericObjectPool<T> where T : class, new()
{
    // 知识点 2: 使用 Stack<T> 而非 Queue<T>
    // Stack 的 Push/Pop 是 O(1)，且缓存局部性更好（最近归还的最先复用）
    private readonly Stack<T> _pool;
    private readonly Action<T> _onRent;    // 取出时的重置回调
    private readonly Action<T> _onReturn;  // 归还时的清理回调
    private readonly int _maxSize;

    public int Count => _pool.Count;

    public GenericObjectPool(
        int prewarmCount = 0,
        int maxSize = 100,
        Action<T> onRent = null,
        Action<T> onReturn = null)
    {
        _pool = new Stack<T>(maxSize);
        _maxSize = maxSize;
        _onRent = onRent;
        _onReturn = onReturn;

        // 知识点 3: 预热 (Prewarm) — 提前创建对象避免运行时 GC 抖动
        for (int i = 0; i < prewarmCount && i < maxSize; i++)
        {
            _pool.Push(new T());
        }
    }

    /// <summary>
    /// 从池中取出一个对象。池空时自动创建新对象。
    /// </summary>
    public T Rent()
    {
        T item = _pool.Count > 0 ? _pool.Pop() : new T();
        _onRent?.Invoke(item);
        return item;
    }

    /// <summary>
    /// 将对象归还池中。池满时直接丢弃（等待 GC 回收）。
    /// </summary>
    public void Return(T item)
    {
        if (item == null) return;

        _onReturn?.Invoke(item);

        if (_pool.Count < _maxSize)
        {
            _pool.Push(item);
        }
        // 知识点 4: 池满时静默丢弃 — 防止内存无限膨胀
    }

    /// <summary>
    /// 知识点 5: 自动收缩 — 定期清理池中多余对象，释放内存
    /// </summary>
    public void Shrink(int keepCount)
    {
        while (_pool.Count > keepCount)
        {
            _pool.Pop(); // 出栈丢弃，等待 GC
        }
        _pool.TrimExcess(); // 释放内部数组的冗余容量
    }
}

// ============================================================
// 知识点 6: Unity GameObject 对象池 — 最常用的游戏开发模式
// ============================================================

/// <summary>
/// Unity 专用 GameObject 对象池。
/// 适用于: 子弹、敌人、粒子特效、UI 列表项等
/// </summary>
public class GameObjectPool : MonoBehaviour
{
    [Header("配置")]
    [SerializeField] private GameObject _prefab;
    [SerializeField] private int _prewarmCount = 10;
    [SerializeField] private int _maxSize = 50;
    [SerializeField] private bool _autoExpand = true; // 池空时是否自动扩容

    private Queue<GameObject> _pool; // 知识点 7: 用 Queue 而非 Stack，保证"先创建先用"的时间公平性

    public int ActiveCount { get; private set; }
    public int InactiveCount => _pool?.Count ?? 0;

    private void Awake()
    {
        _pool = new Queue<GameObject>(_maxSize);
        Prewarm();
    }

    /// <summary>
    /// 知识点 8: 预热 — 在 Awake 阶段预创建对象，避免游戏中卡顿
    /// </summary>
    private void Prewarm()
    {
        for (int i = 0; i < _prewarmCount && i < _maxSize; i++)
        {
            GameObject obj = CreateNew();
            obj.SetActive(false);
            _pool.Enqueue(obj);
        }
    }

    private GameObject CreateNew()
    {
        GameObject obj = Instantiate(_prefab, transform);
        // 知识点 9: 给池对象附加标记组件，归还时可通过此标记找到所属池
        var marker = obj.AddComponent<PooledObjectMarker>();
        marker.Pool = this;
        return obj;
    }

    /// <summary>
    /// 从池中取出对象，可选设置位置和旋转
    /// </summary>
    public GameObject Rent(Vector3? position = null, Quaternion? rotation = null)
    {
        GameObject obj;

        if (_pool.Count > 0)
        {
            obj = _pool.Dequeue();
        }
        else if (_autoExpand && (ActiveCount + InactiveCount) < _maxSize)
        {
            obj = CreateNew();
        }
        else
        {
            Debug.LogWarning($"[GameObjectPool] 池已耗尽! prefab={_prefab.name}, max={_maxSize}");
            return null;
        }

        obj.transform.SetPositionAndRotation(
            position ?? Vector3.zero,
            rotation ?? Quaternion.identity);
        obj.SetActive(true);
        ActiveCount++;
        return obj;
    }

    /// <summary>
    /// 归还对象到池中（外部可直接调用，也可通过 PooledObjectMarker 自动归还）
    /// </summary>
    public void Return(GameObject obj)
    {
        if (obj == null) return;

        obj.SetActive(false);
        obj.transform.SetParent(transform); // 知识点 10: 归位到池根节点下，保持层级整洁
        _pool.Enqueue(obj);
        ActiveCount--;
    }

    /// <summary>
    /// 清空池中所有未使用的对象
    /// </summary>
    public void ClearInactive()
    {
        while (_pool.Count > 0)
        {
            Destroy(_pool.Dequeue());
        }
    }
}

// ============================================================
// 知识点 11: 池标记组件 — 对象知道自己属于哪个池，支持自动归还
// ============================================================

/// <summary>
/// 附加到每个池对象上的标记组件。
/// 可以配合事件/回调实现自动归还（例如粒子播放完毕、飞出屏幕后）
/// </summary>
public class PooledObjectMarker : MonoBehaviour
{
    public GameObjectPool Pool { get; set; }

    /// <summary>
    /// 延迟自动归还（例如子弹飞行 3 秒后自动回收）
    /// </summary>
    public void ReturnAfterDelay(float seconds)
    {
        StartCoroutine(ReturnRoutine(seconds));
    }

    private System.Collections.IEnumerator ReturnRoutine(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        Pool?.Return(gameObject);
    }

    private void OnDisable()
    {
        // 知识点 12: 取消所有协程，防止对象已归还后协程仍在运行
        StopAllCoroutines();
    }
}

// ============================================================
// 知识点 13: 使用示例 — 展示对象池的实际调用方式
// ============================================================

/// <summary>
/// 示例: 子弹管理器 — 使用对象池管理子弹
/// </summary>
public class BulletManager : MonoBehaviour
{
    [SerializeField] private GameObjectPool _bulletPool;

    public void FireBullet(Vector3 from, Vector3 direction)
    {
        // 从池中租用一个子弹
        GameObject bullet = _bulletPool.Rent(from, Quaternion.LookRotation(direction));

        if (bullet != null)
        {
            var marker = bullet.GetComponent<PooledObjectMarker>();
            // 3 秒后自动回收（防止子弹飞出屏幕后永不归还）
            marker.ReturnAfterDelay(3f);
        }
    }
}

/*
// ============================================================
// 知识点 14: 更进一步 — 纯 C# 池在 Unity 之外的用法
// ============================================================

// 示例 1: StringBuilder 池（减少字符串拼接时的 GC 分配）
var sbPool = new GenericObjectPool<System.Text.StringBuilder>(
    prewarmCount: 3,
    onRent: sb => sb.Clear(),          // 取出时清空
    onReturn: sb => sb.Clear()         // 归还时清空
);

var sb = sbPool.Rent();
sb.Append("Hello ").Append("World!");
string result = sb.ToString();
sbPool.Return(sb);

// 示例 2: List<int> 池（减少临时列表的 GC 分配）
var listPool = new GenericObjectPool<List<int>>(
    prewarmCount: 5,
    onRent: list => list.Clear(),
    onReturn: list => list.Clear()
);
*/
