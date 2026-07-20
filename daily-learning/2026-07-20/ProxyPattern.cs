// ============================================================
// 每日脚本学习 Day 13 — 2026-07-20
// 主题: C# 代理模式 (Proxy Pattern)
// 适用: Unity 游戏开发 · 资源懒加载 · 权限控制 · 日志监控 · 网络缓存
// ============================================================

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ============================================================
// 知识点 1: 代理模式核心思想 — "替身替你办事"
//
//   客户端 ──► ISubject (接口)
//                  ├── RealSubject (真正干活的)
//                  └── Proxy (代理, 替身)
//                       │
//                       └── 持有 RealSubject, 在前后加逻辑
//
// 现实类比: 银行卡就是现金的代理 —
//   你刷卡(Proxy), 银行系统(RealSubject)在背后转账
//   你不用带现金, 银行加了一层安全验证(代理的额外逻辑)
// ============================================================

// ============================================================
// Part 1: 虚拟代理 — 延迟加载 (最常用的代理类型)
// 知识点 2: 大资源"用到才加载", 不是"一开始就加载"
// ============================================================

/// <summary>
/// 知识点 3: ITextureLoader — 纹理加载接口
/// </summary>
public interface ITextureLoader
{
    void Display();
    string GetInfo();
}

/// <summary>
/// 知识点 4: HeavyTexture — 真正的大纹理 (模拟加载耗时)
/// 假设这张纹理有 4096×4096, 加载需要 2 秒
/// </summary>
public class HeavyTexture : ITextureLoader
{
    private string _path;
    private int _width;
    private int _height;
    private float _memoryMB;

    public HeavyTexture(string path, int width, int height)
    {
        _path = path;
        _width = width;
        _height = height;
        _memoryMB = (width * height * 4) / (1024f * 1024f); // RGBA
        LoadFromDisk();
    }

    private void LoadFromDisk()
    {
        // 模拟耗时加载
        Debug.Log($"[真实纹理] ⏳ 正在从磁盘加载 {_path}... ({_width}×{_height})");
        Debug.Log($"[真实纹理] ✅ 加载完成! 占用内存: {_memoryMB:F1} MB");
    }

    public void Display()
    {
        Debug.Log($"[真实纹理] 🖼️ 显示: {_path} ({_width}×{_height})");
    }

    public string GetInfo()
    {
        return $"{_path} | {_width}x{_height} | {_memoryMB:F1}MB";
    }
}

/// <summary>
/// 知识点 5: TextureProxy — 虚拟代理, "用到才加载"
/// 关键: 构造函数不加载真实纹理, Display() 时才加载
/// </summary>
public class TextureProxy : ITextureLoader
{
    private string _path;
    private int _width;
    private int _height;
    private HeavyTexture _realTexture; // null until needed

    public TextureProxy(string path, int width, int height)
    {
        _path = path;
        _width = width;
        _height = height;
        Debug.Log($"[纹理代理] 📋 注册纹理: {path} (尚未加载)");
    }

    public void Display()
    {
        // 知识点 6: 延迟初始化 — 第一次调用时才创建真正的对象
        if (_realTexture == null)
        {
            Debug.Log($"[纹理代理] 🔄 首次使用, 开始加载...");
            _realTexture = new HeavyTexture(_path, _width, _height);
        }
        _realTexture.Display();
    }

    public string GetInfo()
    {
        // 不碰真实对象也能返回基本信息
        if (_realTexture == null)
            return $"{_path} | {_width}x{_height} | (未加载)";
        return _realTexture.GetInfo();
    }
}

// ============================================================
// Part 2: 保护代理 — 权限控制
// 知识点 7: 在某些操作前检查权限/条件
// ============================================================

/// <summary>
/// 知识点 8: IGameSave — 存档操作接口
/// </summary>
public interface IGameSave
{
    bool SaveData(string slot, string data);
    string LoadData(string slot);
    bool DeleteData(string slot);
}

/// <summary>
/// 知识点 9: RealGameSave — 真实的存档系统
/// </summary>
public class RealGameSave : IGameSave
{
    public bool SaveData(string slot, string data)
    {
        Debug.Log($"[真实存档] 💾 写入槽位 {slot}: {data}");
        return true;
    }

    public string LoadData(string slot)
    {
        Debug.Log($"[真实存档] 📂 读取槽位 {slot}");
        return $"data_from_slot_{slot}";
    }

    public bool DeleteData(string slot)
    {
        Debug.Log($"[真实存档] 🗑️ 删除槽位 {slot}");
        return true;
    }
}

/// <summary>
/// 知识点 10: SaveProxy — 保护代理, 检查权限后才放行
/// 防止: 战斗中途存档 / 删别人存档 / 未登录就操作
/// </summary>
public class SaveProxy : IGameSave
{
    private RealGameSave _realSave = new RealGameSave();
    private bool _isLoggedIn;
    private bool _isInCombat;

    public SaveProxy(bool isLoggedIn, bool isInCombat)
    {
        _isLoggedIn = isLoggedIn;
        _isInCombat = isInCombat;
    }

    public void SetLoginStatus(bool loggedIn) => _isLoggedIn = loggedIn;
    public void SetCombatStatus(bool inCombat) => _isInCombat = inCombat;

    public bool SaveData(string slot, string data)
    {
        // 知识点 11: 代理的"前置检查" — 核心价值
        if (!_isLoggedIn)
        {
            Debug.Log("[存档代理] ❌ 拒绝存档: 未登录!");
            return false;
        }
        if (_isInCombat)
        {
            Debug.Log("[存档代理] ❌ 拒绝存档: 战斗中无法存档!");
            return false;
        }
        Debug.Log("[存档代理] ✅ 权限检查通过, 转发存档请求...");
        return _realSave.SaveData(slot, data);
    }

    public string LoadData(string slot)
    {
        if (!_isLoggedIn)
        {
            Debug.Log("[存档代理] ❌ 拒绝读档: 未登录!");
            return null;
        }
        Debug.Log("[存档代理] ✅ 权限通过, 转发读档请求...");
        return _realSave.LoadData(slot);
    }

    public bool DeleteData(string slot)
    {
        // 删除需要额外权限
        if (!_isLoggedIn)
        {
            Debug.Log("[存档代理] ❌ 拒绝删除: 未登录!");
            return false;
        }
        Debug.Log($"[存档代理] ⚠️ 删除确认: 槽位 {slot}");
        return _realSave.DeleteData(slot);
    }
}

// ============================================================
// Part 3: 日志/监控代理 — 记录调用 (不改变原功能)
// 知识点 12: 对已有的类加日志, 但不想改原有代码
// ============================================================

/// <summary>
/// 知识点 13: IDamageable — 可受伤害接口
/// </summary>
public interface IDamageable
{
    void TakeDamage(float amount);
    void Heal(float amount);
    float GetHP();
}

/// <summary>
/// 知识点 14: RealPlayer — 真实的玩家类 (假设是核心代码, 不想乱改)
/// </summary>
public class RealPlayer : IDamageable
{
    private string _name;
    private float _hp;
    private float _maxHp;

    public RealPlayer(string name, float maxHp)
    {
        _name = name;
        _maxHp = maxHp;
        _hp = maxHp;
    }

    public void TakeDamage(float amount)
    {
        _hp = Mathf.Max(0, _hp - amount);
        Debug.Log($"[真实玩家] {_name} 受到 {amount} 伤害, 剩余 HP: {_hp}/{_maxHp}");
    }

    public void Heal(float amount)
    {
        _hp = Mathf.Min(_maxHp, _hp + amount);
        Debug.Log($"[真实玩家] {_name} 回复 {amount} HP, 剩余 HP: {_hp}/{_maxHp}");
    }

    public float GetHP() => _hp;
}

/// <summary>
/// 知识点 15: LoggingProxy — 给 TakeDamage/Heal 加上日志
/// 不改 RealPlayer 一行代码!
/// </summary>
public class LoggingProxy : IDamageable
{
    private RealPlayer _player;
    private int _damageCount;
    private float _totalDamageReceived;
    private List<string> _eventLog = new List<string>();

    public LoggingProxy(RealPlayer player)
    {
        _player = player;
    }

    public void TakeDamage(float amount)
    {
        // 前置: 记录
        float hpBefore = _player.GetHP();
        Debug.Log($"[日志代理] 📝 [前] HP={hpBefore} | 即将受 {amount} 伤害");

        // 转发给真实对象
        _player.TakeDamage(amount);

        // 后置: 记录
        _damageCount++;
        _totalDamageReceived += amount;
        float hpAfter = _player.GetHP();
        string log = $"第{_damageCount}次受伤: -{amount}HP (HP:{hpBefore}→{hpAfter})";
        _eventLog.Add(log);
        Debug.Log($"[日志代理] 📝 [后] HP={hpAfter} | 累计受伤{_damageCount}次, 总伤害{_totalDamageReceived}");
    }

    public void Heal(float amount)
    {
        float hpBefore = _player.GetHP();
        _player.Heal(amount);
        float hpAfter = _player.GetHP();
        _eventLog.Add($"治疗: +{amount}HP (HP:{hpBefore}→{hpAfter})");
    }

    public float GetHP() => _player.GetHP();

    public void PrintDamageReport()
    {
        Debug.Log("\n=== 📊 战斗伤害报告 ===");
        Debug.Log($"受伤次数: {_damageCount}");
        Debug.Log($"总承伤: {_totalDamageReceived}");
        Debug.Log("事件时间线:");
        foreach (var log in _eventLog)
            Debug.Log($"  {log}");
        Debug.Log("=========================\n");
    }
}

// ============================================================
// Part 4: 缓存代理 — 避免重复请求
// 知识点 16: 网络数据第一次拉取, 后续直接返回缓存
// ============================================================

/// <summary>
/// 知识点 17: IConfigLoader — 配置加载接口
/// </summary>
public interface IConfigLoader
{
    string GetConfig(string key);
}

/// <summary>
/// 知识点 18: RemoteConfigLoader — 从"服务器"加载配置 (慢)
/// </summary>
public class RemoteConfigLoader : IConfigLoader
{
    private Dictionary<string, string> _serverData = new Dictionary<string, string>
    {
        { "max_hp", "100" },
        { "move_speed", "5.5" },
        { "bgm_volume", "0.8" },
        { "difficulty", "normal" }
    };

    public string GetConfig(string key)
    {
        // 模拟网络延迟
        Debug.Log($"[远程配置] 🌐 请求服务器: {key} ... (模拟延迟 200ms)");
        System.Threading.Thread.Sleep(200); // 模拟网络延迟
        if (_serverData.TryGetValue(key, out string value))
        {
            Debug.Log($"[远程配置] ✅ 返回: {key} = {value}");
            return value;
        }
        Debug.Log($"[远程配置] ⚠️ 未找到: {key}");
        return null;
    }
}

/// <summary>
/// 知识点 19: CacheProxy — 缓存代理
/// 第一次从服务器拉, 之后直接从缓存返回
/// </summary>
public class CacheProxy : IConfigLoader
{
    private RemoteConfigLoader _remote;
    private Dictionary<string, string> _cache = new Dictionary<string, string>();

    public CacheProxy(RemoteConfigLoader remote)
    {
        _remote = remote;
    }

    public string GetConfig(string key)
    {
        // 命中缓存 → 秒回
        if (_cache.TryGetValue(key, out string cached))
        {
            Debug.Log($"[缓存代理] ⚡ 缓存命中: {key} = {cached} (无需网络请求)");
            return cached;
        }

        // 未命中 → 请求服务器并缓存
        string value = _remote.GetConfig(key);
        if (value != null)
        {
            _cache[key] = value;
            Debug.Log($"[缓存代理] 💾 已缓存: {key}");
        }
        return value;
    }

    /// <summary>
    /// 知识点 20: 预加载 — 一次性缓存所有配置
    /// </summary>
    public void PreloadAll(string[] keys)
    {
        Debug.Log($"[缓存代理] 📦 预加载 {keys.Length} 个配置...");
        foreach (var key in keys)
        {
            GetConfig(key); // 每个只请求一次, 后续都命中缓存
        }
        Debug.Log("[缓存代理] ✅ 预加载完成");
    }

    public void ClearCache()
    {
        _cache.Clear();
        Debug.Log("[缓存代理] 🧹 缓存已清空");
    }
}

// ============================================================
// Part 5: 智能引用代理 — Unity 中最实用的代理
// 知识点 21: 自动处理引用计数 / 资源生命周期
// ============================================================

/// <summary>
/// 知识点 22: IResource — 资源接口
/// </summary>
public interface IResource
{
    void Use();
    void Release();
    string GetName();
    int GetRefCount();
}

/// <summary>
/// 知识点 23: RealResource — 真实资源 (模拟 Prefab / Texture / AudioClip)
/// </summary>
public class RealResource : IResource
{
    private string _name;
    private int _refCount;

    public RealResource(string name)
    {
        _name = name;
        _refCount = 0;
        Debug.Log($"[真实资源] 📦 创建资源: {name}");
    }

    public void Use()
    {
        _refCount++;
        Debug.Log($"[真实资源] ➕ {_name} 被引用 (引用数:{_refCount})");
    }

    public void Release()
    {
        _refCount = Mathf.Max(0, _refCount - 1);
        Debug.Log($"[真实资源] ➖ {_name} 释放 (引用数:{_refCount})");
        if (_refCount <= 0)
        {
            Debug.Log($"[真实资源] 🗑️ {_name} 无人引用, 卸载资源");
        }
    }

    public string GetName() => _name;
    public int GetRefCount() => _refCount;
}

/// <summary>
/// 知识点 24: ResourceProxy — 智能引用代理
/// 自动管理引用计数, 引用为0时自动释放
/// </summary>
public class ResourceProxy : IResource
{
    private RealResource _realResource;
    private string _resourceName;

    public ResourceProxy(string name)
    {
        _resourceName = name;
        Debug.Log($"[资源代理] 🔗 创建代理: {name} (资源尚未加载)");
    }

    public void Use()
    {
        // 懒加载: 第一次 Use 时才真正创建资源
        if (_realResource == null)
        {
            Debug.Log($"[资源代理] 🔄 首次引用, 加载资源...");
            _realResource = new RealResource(_resourceName);
        }
        _realResource.Use();
    }

    public void Release()
    {
        if (_realResource == null)
        {
            Debug.Log($"[资源代理] ⚠️ {_resourceName} 尚未加载, 无需释放");
            return;
        }
        _realResource.Release();

        // 引用归零 → 真正卸载
        if (_realResource.GetRefCount() <= 0)
        {
            Debug.Log($"[资源代理] 🗑️ 引用归零, 卸载: {_resourceName}");
            _realResource = null;
        }
    }

    public string GetName() => _resourceName;
    public int GetRefCount() => _realResource?.GetRefCount() ?? 0;
}

// ============================================================
// Part 6: 代理 vs 装饰器 — 最容易混淆的两个模式
// 知识点 25: 一图看懂区别
// ============================================================
//
// 代理 (Proxy):
//   控制访问 — "你能用吗? 现在需要加载吗? 有权限吗?"
//   代理替身一个具体的 RealSubject
//   客户端不知自己在和代理说话
//
// 装饰器 (Decorator):
//   增强功能 — "在原有功能上加一层新功能"
//   可以多层嵌套 A(B(C(subject)))
//   客户端主动用装饰器包装
//
// 💡 一句话:
//   代理 = 门卫 (检查/延迟/缓存) — 控制你能不能进去
//   装饰器 = 衣服 (加层/加功能) — 让你更强更好看

// ============================================================
// Part 7: 演示脚本
// ============================================================

public class ProxyDemo : MonoBehaviour
{
    private void Start()
    {
        StartCoroutine(RunAllDemos());
    }

    private IEnumerator RunAllDemos()
    {
        Debug.Log("╔══════════════════════════════════════════════╗");
        Debug.Log("║  每日脚本学习 Day 13 — 代理模式             ║");
        Debug.Log("║  Proxy Pattern Demo                         ║");
        Debug.Log("╚══════════════════════════════════════════════╝\n");

        DemoVirtualProxy();
        yield return null;

        Debug.Log("\n━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        Debug.Log("🛡️ 演示 2: 保护代理 — 权限控制");
        Debug.Log("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        DemoProtectionProxy();
        yield return null;

        Debug.Log("\n━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        Debug.Log("📝 演示 3: 日志代理 — 无侵入加日志");
        Debug.Log("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        DemoLoggingProxy();
        yield return null;

        Debug.Log("\n━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        Debug.Log("⚡ 演示 4: 缓存代理 — 避免重复请求");
        Debug.Log("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        DemoCacheProxy();

        Debug.Log("\n━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        Debug.Log("🔗 演示 5: 智能引用代理 — 资源生命周期");
        Debug.Log("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        DemoResourceProxy();

        Debug.Log("\n╔══════════════════════════════════════════════╗");
        Debug.Log("║  🎉 所有演示完成!                          ║");
        Debug.Log("╚══════════════════════════════════════════════╝");
        PrintSummary();
    }

    private void DemoVirtualProxy()
    {
        Debug.Log("--- 场景: 加载界面显示100张缩略图 ---");

        // 创建代理 (不加载真实图片!)
        ITextureLoader[] images = new ITextureLoader[]
        {
            new TextureProxy("boss_dragon.png", 4096, 4096),
            new TextureProxy("map_world.png", 2048, 2048),
            new TextureProxy("icon_sword.png", 512, 512),
        };

        Debug.Log($"已注册 {images.Length} 张图片, 内存占用: ~0 MB (都还没加载)");

        // 只看信息, 不加载
        foreach (var img in images)
            Debug.Log($"  📋 {img.GetInfo()}");

        // 只有真正显示时才加载
        Debug.Log("\n玩家点开第一张图片...");
        images[0].Display();  // ← 这时才真正加载 4096×4096 的图!

        Debug.Log("\n其他图片仍不加载 (玩家没点开)");
        Debug.Log($"  {images[1].GetInfo()}");
        Debug.Log($"  {images[2].GetInfo()}");

        Debug.Log("\n💡 关键点:");
        Debug.Log("  100张图的列表页 → 不用全部加载");
        Debug.Log("  玩家点哪张才加载哪张 → 内存省了99%");
    }

    private void DemoProtectionProxy()
    {
        // 未登录 → 无法存档
        var saveProxy = new SaveProxy(isLoggedIn: false, isInCombat: false);
        saveProxy.SaveData("slot1", "勇者进度");
        saveProxy.LoadData("slot1");

        // 登录成功
        Debug.Log("\n--- 玩家登录 ---");
        saveProxy.SetLoginStatus(true);
        saveProxy.SaveData("slot1", "勇者进度-Lv15");
        saveProxy.LoadData("slot1");

        // 战斗中 → 拒绝存档
        Debug.Log("\n--- 进入战斗 ---");
        saveProxy.SetCombatStatus(true);
        saveProxy.SaveData("slot1", "Boss战前");

        Debug.Log("\n💡 关键点:");
        Debug.Log("  保护代理 = 不修改存档代码, 加一层权限过滤");
        Debug.Log("  可以随时调整规则 (战斗禁止 / 需要登录 / VIP限制)");
    }

    private void DemoLoggingProxy()
    {
        var player = new RealPlayer("勇者太郎", 100f);
        var loggedPlayer = new LoggingProxy(player);

        // 战斗模拟
        loggedPlayer.TakeDamage(15f);
        loggedPlayer.TakeDamage(23f);
        loggedPlayer.Heal(10f);
        loggedPlayer.TakeDamage(30f);

        // 查看报告 (日志代理的额外功能)
        loggedPlayer.PrintDamageReport();

        Debug.Log("💡 关键点:");
        Debug.Log("  不改 RealPlayer 一行代码 → 就加上了完整日志");
        Debug.Log("  代理 = 无侵入式的功能增强");
    }

    private void DemoCacheProxy()
    {
        var remote = new RemoteConfigLoader();
        var cachedConfig = new CacheProxy(remote);

        // 第一次: 走网络 (慢)
        Debug.Log("--- 第一次请求 ---");
        cachedConfig.GetConfig("max_hp");
        cachedConfig.GetConfig("move_speed");

        // 第二次: 命中缓存 (快!)
        Debug.Log("\n--- 第二次请求 (命中缓存) ---");
        cachedConfig.GetConfig("max_hp");   // 缓存命中
        cachedConfig.GetConfig("move_speed"); // 缓存命中

        // 清除缓存后重新请求
        Debug.Log("\n--- 清空缓存后 ---");
        cachedConfig.ClearCache();
        cachedConfig.GetConfig("max_hp");   // 重新走网络

        Debug.Log("\n💡 关键点:");
        Debug.Log("  第一次慢, 第二次秒回 → 典型缓存代理");
        Debug.Log("  实际项目: 配置表/翻译文本/排行榜数据 都可以用");
    }

    private void DemoResourceProxy()
    {
        // 创建代理 (不加载资源)
        var swordProxy = new ResourceProxy("LegendarySword.prefab");

        // 第一次 Use → 真正加载
        Debug.Log("--- 第一次引用 ---");
        swordProxy.Use();  // 加载 + 引用数=1
        Debug.Log($"当前引用数: {swordProxy.GetRefCount()}");

        // 第二个地方也用了
        Debug.Log("\n--- 第二次引用 ---");
        swordProxy.Use();  // 引用数=2
        Debug.Log($"当前引用数: {swordProxy.GetRefCount()}");

        // 释放
        Debug.Log("\n--- 释放引用 ---");
        swordProxy.Release(); // 引用数=1
        swordProxy.Release(); // 引用数=0 → 卸载!

        Debug.Log("\n💡 关键点:");
        Debug.Log("  代理 = 自动管理'谁在用这个资源'");
        Debug.Log("  引用归零自动卸载 → 防止内存泄漏");
    }

    private void PrintSummary()
    {
        Debug.Log("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        Debug.Log("📋 代理模式 — 知识点总结");
        Debug.Log("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");

        Debug.Log("\n🧩 代理的四种类型:");
        Debug.Log("  1. 虚拟代理: 用到才创建 (懒加载图片/模型/AudioClip)");
        Debug.Log("  2. 保护代理: 检查权限后放行 (战斗存档/删除确认/VIP功能)");
        Debug.Log("  3. 日志代理: 记录调用日志 (战斗统计/API调用/性能监控)");
        Debug.Log("  4. 缓存代理: 第一次拉取, 后续用缓存 (配置/翻译/排行榜)");

        Debug.Log("\n🎮 Unity 中的典型应用:");
        Debug.Log("  1. Addressables / AssetBundle 的懒加载 → 虚拟代理");
        Debug.Log("  2. 付费墙检查 → 保护代理");
        Debug.Log("  3. 成就/统计追踪 → 日志代理");
        Debug.Log("  4. 网络请求缓存 → 缓存代理");
        Debug.Log("  5. 资源引用计数 → 智能代理");

        Debug.Log("\n🆚 代理 vs 装饰器 (常见混淆):");
        Debug.Log("  代理:  控制访问 — '你能加载吗? 你能存档吗?'");
        Debug.Log("  装饰器: 增强功能 — '加火焰伤害, 加吸血效果'");
        Debug.Log("  代理的意图是控制, 装饰器的意图是增强");

        Debug.Log("\n📐 与已学模式的关系:");
        Debug.Log("  - 装饰器 (Day7): 两者都包装对象, 但目的不同 (控制 vs 增强)");
        Debug.Log("  - 适配器 (Day10): 适配器换接口, 代理保持接口不变");
        Debug.Log("  - 外观   (Day11): 外观简化多个子系统, 代理代理一个对象");
        Debug.Log("  - 工厂   (Day9): 工厂创建对象, 代理控制对象的使用");
        Debug.Log("  - 建造者 (Day12): 建造者构建对象, 代理管理对象访问");

        Debug.Log("\n💡 一句话总结:");
        Debug.Log("  \"代理就是给你的对象雇一个秘书 —");
        Debug.Log("   秘书说: '老板在吗? 不在(懒加载) / 有事预约(权限) /");
        Debug.Log("   记录访客(日志) / 上次问过了(缓存)'。");
        Debug.Log("   老板本人(RealSubject)只在该出场时才出场。\"");
    }
}

// ============================================================
// 知识点总结 — 代理模式的精髓
// ============================================================
//
// ✅ 什么时候用代理?
//   1. 大资源不想一开始就加载 → 虚拟代理 (用到才加载)
//   2. 需要加权限验证但不想改原代码 → 保护代理
//   3. 需要加日志/统计但不想改原代码 → 日志代理
//   4. 网络请求想避免重复 → 缓存代理
//   5. 需要管理资源生命周期 → 智能引用代理
//
// ❌ 什么时候不用?
//   1. 对象本来就很轻量 → 虚拟代理多余
//   2. 不需要额外控制 → 直接访问更简单
//   3. 接口和实现完全一致且无额外逻辑 → 代理 = 多余的一层
//
// 🎮 游戏开发中最实用的场景:
//   1. 图集/模型预览: 列表页用缩略图(代理), 点开才加载高清(真实)
//   2. 付费功能门控: 免费玩家点功能 → 代理拦截 → 弹付费弹窗
//   3. 战斗统计: 代理包一层, 所有伤害/治疗自动记录
//   4. 配置热更新: 代理缓存 → 定时刷新 → 无感切换
//
// 💡 一句话总结:
//   "代理 = 给对象加一个'智能中间层' —
//    用得着才创建(懒), 有权限才放行(安),
//    自动做记录(查), 重复的跳过(快)。
//    不改原有代码, 所有额外逻辑都在代理里。"
// ============================================================
