# 📅 每日脚本学习 — 2026-06-23

> **今日主题**: C# 通用对象池模式 (Object Pool Pattern)

---

## 📖 学习内容概览

对象池是游戏开发中 **最重要的性能优化模式之一**。它通过复用对象而非频繁创建/销毁，避免 GC (垃圾回收) 导致的卡顿。

今天实现两个层级的对象池：

| 层级 | 类 | 适用场景 |
|------|-----|---------|
| 通用 C# | `GenericObjectPool<T>` | StringBuilder、List、网络包等纯数据对象 |
| Unity 层 | `GameObjectPool` | 子弹、敌人、粒子、UI 列表项等 GameObject |

---

## 🔑 14 个核心知识点

### 1. 泛型约束 `where T : class, new()`
```csharp
public class GenericObjectPool<T> where T : class, new()
```
- `class` — T 必须是引用类型（不能是 int、float 等值类型）
- `new()` — T 必须有公开的无参构造函数，保证池能用 `new T()` 创建对象

### 2. Stack\<T\> vs Queue\<T\> 的选择
```csharp
// Stack: 最后归还的最先复用 → 缓存局部性更好
private readonly Stack<T> _pool;

// Queue: 先创建的先使用 → 时间公平性，适合 GameObject
private Queue<GameObject> _pool;
```
选择依据：纯数据用 Stack（热缓存），GameObject 用 Queue（均匀老化）。

### 3. 预热 (Prewarm)
```csharp
for (int i = 0; i < prewarmCount; i++)
    _pool.Push(new T());
```
在加载阶段预创建对象，避免游戏中触发 GC Alloc。关键：预热数量 = 预计最大并发数。

### 4. 池满静默丢弃
```csharp
if (_pool.Count < _maxSize) _pool.Push(item);
// 超过 maxSize 时丢弃，等待 GC 回收
```
防止内存无限膨胀。`_maxSize` 是安全阀。

### 5. 自动收缩 (Shrink)
```csharp
public void Shrink(int keepCount) {
    while (_pool.Count > keepCount) _pool.Pop();
    _pool.TrimExcess(); // 释放内部数组冗余容量
}
```
关卡切换或内存紧张时调用，释放闲置对象。

### 6. Unity GameObject 池
```csharp
public class GameObjectPool : MonoBehaviour
```
继承 MonoBehaviour 可以直接在 Inspector 中配置 prefab、预热数量等参数。

### 7. Queue 保证时间公平性
GameObject 用 Queue 而非 Stack：先创建的对象先复用，避免某些对象在池底长期不被使用导致"老化"问题。

### 8. Awake 阶段预热
```csharp
private void Awake() { Prewarm(); }
```
Awake 在场景加载时执行，此时预热不会影响游戏帧率。

### 9. 池标记组件 (PooledObjectMarker)
```csharp
var marker = obj.AddComponent<PooledObjectMarker>();
marker.Pool = this;
```
对象知道自己属于哪个池 → 支持自动归还（粒子播完、飞出屏幕、定时器到期）。

### 10. 归位到池根节点
```csharp
obj.transform.SetParent(transform);
```
归还时重新挂到池对象的 Transform 下，层级管理清晰。

### 11. 延迟自动归还
```csharp
IEnumerator ReturnRoutine(float seconds) {
    yield return new WaitForSeconds(seconds);
    Pool?.Return(gameObject);
}
```
防止子弹/特效飞出屏幕后永远不归还导致池泄漏。

### 12. OnDisable 中取消协程
```csharp
private void OnDisable() { StopAllCoroutines(); }
```
对象归还后（SetActive(false)），必须取消正在运行的协程，否则协程会继续执行并重复归还。

### 13. 使用示例 — BulletManager
```csharp
GameObject bullet = _bulletPool.Rent(from, direction);
marker.ReturnAfterDelay(3f);
```
只需两行代码，即可从池中取出子弹并自动回收。

### 14. 纯 C# 池的非游戏用途
```csharp
// StringBuilder 池 → 减少字符串拼接 GC
var sbPool = new GenericObjectPool<StringBuilder>(
    onRent: sb => sb.Clear(),
    onReturn: sb => sb.Clear()
);

// List<int> 池 → 减少临时集合 GC
var listPool = new GenericObjectPool<List<int>>(
    onRent: list => list.Clear()
);
```

---

## 🛠️ 在 Unity 中使用步骤

1. 在场景中创建空 GameObject，挂载 `GameObjectPool` 脚本
2. 在 Inspector 中拖入 Prefab，设置预热数量
3. 在需要生成对象的脚本中引用该池：

```csharp
public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private GameObjectPool _enemyPool;

    public void SpawnEnemy(Vector3 pos)
    {
        GameObject enemy = _enemyPool.Rent(pos);
        if (enemy != null)
        {
            // 初始化敌人...
        }
    }
}
```

---

## 💡 为什么对象池对游戏开发如此重要?

| 问题 | 没有对象池 | 有对象池 |
|------|-----------|---------|
| 频繁 Instantiate/Destroy | 每次触发 GC Alloc | 仅预热时分配 |
| GC.Collect 卡顿 | 帧率突降 (50→30ms+) | 无 GC 影响 |
| 对象生命周期 | 不确定，无法追踪 | 池统一管理 |
| 性能 Profile | GC.Alloc 占大头 | GC.Alloc 几乎为 0 |

### 实际数据对比 (Unity Profiler 典型数据)

| 指标 | Instantiate/Destroy | 对象池 |
|------|--------------------|--------|
| GC Alloc / 帧 | ~2 KB per 对象 | 0 (预热后) |
| 创建耗时 | ~0.05ms per 对象 | ~0.002ms (出栈) |
| 销毁耗时 | ~0.03ms + GC 回收 | ~0.001ms (入栈) |

---

## 📊 延伸思考

1. **多预制体池管理**: 可封装 `PoolManager` 单例，用 `Dictionary<string, GameObjectPool>` 管理多种对象池。

2. **自动扩容策略**: 当前实现中池耗尽返回 null，可改为动态扩容（Instantiate 新对象临时加入）。

3. **Unity 2021+ 内置对象池**: `UnityEngine.Pool.ObjectPool<T>` 是 Unity 官方实现，原理与本文相同，可直接使用。但手写一遍理解原理更扎实。

4. **与 ECS/DOTS 对比**: ECS 用 Archetype Chunk 管理实体内存，天然避免了对象创建开销，是对象池思想的架构级体现。

---

## 🏷️ 标签

`#CSharp` `#Unity` `#设计模式` `#对象池` `#性能优化` `#GC`

---

*2026-06-23 学习记录 · 每日脚本知识积累 Day 2*
