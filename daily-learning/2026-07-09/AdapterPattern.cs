// ============================================================
// 每日脚本学习 Day 10 — 2026-07-09
// 主题: C# 适配器模式 (Adapter Pattern)
// 适用: Unity 游戏开发 · 输入系统 · 存档系统 · SDK 对接 · 音频系统
// ============================================================

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ============================================================
// 知识点 1: 适配器模式核心思想 — "把一个接口变成另一个接口"
//
//   客户端 ──► ITarget (期望的接口)
//                  ▲
//             [适配器 Adapter] ──► Adaptee (已有的、不兼容的类)
//
// 现实类比: 电源适配器 — 中国插座(220V/扁孔) ← 适配器 → 美规插头(110V/圆孔)
// 适配器不改变原有类，只是包一层让它能适配新接口
// ============================================================

// ============================================================
// Part 1: 适配器模式的核心结构
// 知识点 2: 定义目标接口(客户端期望的) 和 被适配者(已有的旧代码)
// ============================================================

/// <summary>
/// 知识点 3: ITarget — 客户端期望的标准接口
/// 这是我们希望所有"游戏服务"都能遵守的统一接口
/// </summary>
public interface IGameService
{
    void Initialize();
    void Shutdown();
    string GetServiceName();
}

// --- 模拟两个"已有的、不兼容的"第三方库 ---

/// <summary>
/// 知识点 4: Adaptee A — 第三方成就系统 (接口和我们的 IGameService 不兼容)
/// 假设这是从 Asset Store 买的插件，不能改源码
/// </summary>
public class ThirdPartyAchievementSystem
{
    public void ConnectToServer(string apiKey)
    {
        Debug.Log($"[第三方成就系统] 连接到服务器, API Key: {apiKey}");
    }

    public void Disconnect()
    {
        Debug.Log("[第三方成就系统] 断开服务器连接");
    }

    public void SubmitAchievement(string achievementId)
    {
        Debug.Log($"[第三方成就系统] 提交成就: {achievementId}");
    }

    public string GetLibraryName()
    {
        return "SuperAchieveLib v3.2";
    }
}

/// <summary>
/// 知识点 5: Adaptee B — 第三方排行榜系统 (另一个不兼容的接口)
/// </summary>
public class ThirdPartyLeaderboardSystem
{
    public bool Login(string username, string password)
    {
        Debug.Log($"[第三方排行榜] 用户 {username} 登录成功");
        return true;
    }

    public void Logout()
    {
        Debug.Log("[第三方排行榜] 用户登出");
    }

    public void UploadScore(string boardId, int score)
    {
        Debug.Log($"[第三方排行榜] 上传分数到 {boardId}: {score} 分");
    }

    public string GetSDKVersion()
    {
        return "LeaderX v1.8";
    }
}

/// <summary>
/// 知识点 6: AchievementAdapter — 对象适配器(组合方式)
/// 把 ThirdPartyAchievementSystem 适配成 IGameService 接口
/// 这是最常用的适配器形式 — 持有 Adaptee 的引用
/// </summary>
public class AchievementAdapter : IGameService
{
    private ThirdPartyAchievementSystem _adaptee;  // 被适配的对象
    private string _apiKey;

    public AchievementAdapter(ThirdPartyAchievementSystem adaptee, string apiKey)
    {
        _adaptee = adaptee;
        _apiKey = apiKey;
    }

    // 把 Initialize() 翻译成 ConnectToServer()
    public void Initialize()
    {
        Debug.Log($"[成就适配器] 开始初始化...");
        _adaptee.ConnectToServer(_apiKey);
    }

    // 把 Shutdown() 翻译成 Disconnect()
    public void Shutdown()
    {
        Debug.Log($"[成就适配器] 开始关闭...");
        _adaptee.Disconnect();
    }

    public string GetServiceName()
    {
        return _adaptee.GetLibraryName();
    }
}

/// <summary>
/// 知识点 7: LeaderboardAdapter — 另一个对象适配器
/// 同样的 IGameService 接口，适配不同的第三方库
/// </summary>
public class LeaderboardAdapter : IGameService
{
    private ThirdPartyLeaderboardSystem _adaptee;
    private string _username;
    private string _password;

    public LeaderboardAdapter(ThirdPartyLeaderboardSystem adaptee, string username, string password)
    {
        _adaptee = adaptee;
        _username = username;
        _password = password;
    }

    public void Initialize()
    {
        Debug.Log($"[排行榜适配器] 开始初始化...");
        _adaptee.Login(_username, _password);
    }

    public void Shutdown()
    {
        Debug.Log($"[排行榜适配器] 开始关闭...");
        _adaptee.Logout();
    }

    public string GetServiceName()
    {
        return _adaptee.GetSDKVersion();
    }
}

// ============================================================
// Part 2: 输入系统适配器 — 新旧 Input System 的桥接
// 知识点 8: 将 Unity 旧版 Input Manager 适配成统一的 IInputProvider
// ============================================================

/// <summary>
/// 知识点 9: 统一的输入接口 — 游戏代码只依赖这个，不关心底层是哪种输入
/// </summary>
public interface IInputProvider
{
    float GetHorizontal();
    float GetVertical();
    bool IsJumpPressed();
    bool IsAttackPressed();
    string GetInputSourceName();
}

/// <summary>
/// 知识点 10: Adaptee — Unity 旧版 Input Manager (Input.GetAxis / Input.GetKey)
/// </summary>
public class LegacyUnityInput
{
    public float ReadHorizontal()
    {
        return Input.GetAxis("Horizontal");
    }

    public float ReadVertical()
    {
        return Input.GetAxis("Vertical");
    }

    public bool ReadJump()
    {
        return Input.GetKeyDown(KeyCode.Space);
    }

    public bool ReadAttack()
    {
        return Input.GetKeyDown(KeyCode.J);
    }
}

/// <summary>
/// 知识点 11: LegacyInputAdapter — 把旧版 Input 适配成 IInputProvider
/// </summary>
public class LegacyInputAdapter : IInputProvider
{
    private LegacyUnityInput _legacyInput;

    public LegacyInputAdapter(LegacyUnityInput legacyInput)
    {
        _legacyInput = legacyInput;
    }

    public float GetHorizontal() => _legacyInput.ReadHorizontal();
    public float GetVertical() => _legacyInput.ReadVertical();
    public bool IsJumpPressed() => _legacyInput.ReadJump();
    public bool IsAttackPressed() => _legacyInput.ReadAttack();
    public string GetInputSourceName() => "旧版 Input Manager";
}

/// <summary>
/// 知识点 12: 模拟新版 Input System 的适配器 (实际项目中换成真正的 Input System 包)
/// 同样的接口，不同的实现 — 游戏代码完全不用改!
/// </summary>
public class SimulatedNewInputAdapter : IInputProvider
{
    // 实际项目中使用 UnityEngine.InputSystem 的 Gamepad/Keyboard 类
    // 这里用模拟数据演示适配器的"互换性"

    public float GetHorizontal()
    {
        // 实际: Gamepad.current.leftStick.x.ReadValue()
        float value = Input.GetAxis("Horizontal");
        if (Mathf.Abs(value) < 0.1f) value = 0; // 新系统有更精细的死区
        return value;
    }

    public float GetVertical()
    {
        float value = Input.GetAxis("Vertical");
        if (Mathf.Abs(value) < 0.1f) value = 0;
        return value;
    }

    public bool IsJumpPressed()
    {
        // 实际: Gamepad.current.buttonSouth.wasPressedThisFrame
        return Input.GetKeyDown(KeyCode.Space);
    }

    public bool IsAttackPressed()
    {
        return Input.GetKeyDown(KeyCode.J);
    }

    public string GetInputSourceName() => "新版 Input System (模拟)";
}

// ============================================================
// Part 3: 存档系统适配器 — 多种存储后端统一接口
// 知识点 13: 将 PlayerPrefs / JSON 文件 / 远程云存档 统一为一个 ISaveSystem
// ============================================================

/// <summary>
/// 知识点 14: ISaveSystem — 存档系统的统一接口
/// </summary>
public interface ISaveSystem
{
    void Save(string key, string data);
    string Load(string key);
    void Delete(string key);
    bool Exists(string key);
    string GetBackendName();
}

/// <summary>
/// 知识点 15: Adaptee — PlayerPrefs (Unity 内置的简单存档)
/// 它的接口和我们想要的 ISaveSystem 不完全一样
/// </summary>
public class PlayerPrefsStorage
{
    public void Set(string key, string value) => PlayerPrefs.SetString(key, value);
    public string Get(string key) => PlayerPrefs.GetString(key, "");
    public void Remove(string key) => PlayerPrefs.DeleteKey(key);
    public bool Has(string key) => PlayerPrefs.HasKey(key);
}

/// <summary>
/// 知识点 16: PlayerPrefsAdapter — 把 PlayerPrefs 适配成 ISaveSystem
/// </summary>
public class PlayerPrefsAdapter : ISaveSystem
{
    private PlayerPrefsStorage _storage;

    public PlayerPrefsAdapter(PlayerPrefsStorage storage)
    {
        _storage = storage;
    }

    public void Save(string key, string data)
    {
        Debug.Log($"[PlayerPrefs存档] 保存: {key} = {data}");
        _storage.Set(key, data);
        PlayerPrefs.Save(); // 确保写入磁盘
    }

    public string Load(string key)
    {
        string data = _storage.Get(key);
        Debug.Log($"[PlayerPrefs存档] 读取: {key} = {(string.IsNullOrEmpty(data) ? "(空)" : data)}");
        return data;
    }

    public void Delete(string key)
    {
        Debug.Log($"[PlayerPrefs存档] 删除: {key}");
        _storage.Remove(key);
    }

    public bool Exists(string key)
    {
        return _storage.Has(key);
    }

    public string GetBackendName() => "PlayerPrefs (本地)";
}

/// <summary>
/// 知识点 17: JsonFileAdapter — 把 JSON 文件存储也适配成 ISaveSystem
/// 同一个接口，不同的后端 — 游戏逻辑完全不用改!
/// </summary>
public class JsonFileAdapter : ISaveSystem
{
    private Dictionary<string, string> _cache = new Dictionary<string, string>();
    private string _filePath;

    public JsonFileAdapter(string filePath)
    {
        _filePath = filePath;
        // 模拟: 读取已有文件
        Debug.Log($"[JSON存档] 初始化, 文件路径: {_filePath}");
    }

    public void Save(string key, string data)
    {
        Debug.Log($"[JSON存档] 保存: {key} = {data} → 写入 {_filePath}");
        _cache[key] = data;
        // 实际项目: File.WriteAllText(_filePath, JsonUtility.ToJson(_cache));
    }

    public string Load(string key)
    {
        _cache.TryGetValue(key, out string data);
        Debug.Log($"[JSON存档] 读取: {key} = {(string.IsNullOrEmpty(data) ? "(空)" : data)}");
        return data ?? "";
    }

    public void Delete(string key)
    {
        Debug.Log($"[JSON存档] 删除: {key}");
        _cache.Remove(key);
    }

    public bool Exists(string key) => _cache.ContainsKey(key);
    public string GetBackendName() => $"JSON 文件 ({_filePath})";
}

/// <summary>
/// 知识点 18: CloudSaveAdapter — 模拟云存档适配器
/// 即使云存档 API 完全不同，适配器也让它的使用方式和本地存档一模一样
/// </summary>
public class CloudSaveAdapter : ISaveSystem
{
    private string _userId;
    private bool _isConnected;

    public CloudSaveAdapter(string userId)
    {
        _userId = userId;
        Debug.Log($"[云存档] 初始化用户: {_userId}");
    }

    public void Save(string key, string data)
    {
        // 实际项目: 调用 REST API → POST /api/save
        Debug.Log($"[云存档] 🌐 上传: {key} = {data} (用户: {_userId})");
        _isConnected = true;
    }

    public string Load(string key)
    {
        // 实际项目: HTTP GET → /api/load/{key}
        Debug.Log($"[云存档] 🌐 下载: {key} (用户: {_userId})");
        return $"cloud_data_for_{key}"; // 模拟返回
    }

    public void Delete(string key)
    {
        Debug.Log($"[云存档] 🌐 删除云端: {key}");
    }

    public bool Exists(string key)
    {
        // 实际项目: HTTP HEAD 请求检查
        return true;
    }

    public string GetBackendName() => $"云存档 (用户: {_userId})";
}

// ============================================================
// Part 4: 双向适配器 — 新旧代码互相适配
// 知识点 19: 有时候需要让新代码也能被旧代码调用 (双向适配)
// ============================================================

/// <summary>
/// 知识点 20: 旧版音效管理器 (Adaptee — 已有的旧代码)
/// </summary>
public class LegacyAudioManager
{
    public void PlaySound(string clipName)
    {
        Debug.Log($"[旧版音频] 🔊 播放音效: {clipName}");
    }

    public void SetVolume(int volumePercent) // 0-100 的整数
    {
        Debug.Log($"[旧版音频] 🔊 音量设为: {volumePercent}%");
    }

    public void StopAll()
    {
        Debug.Log($"[旧版音频] 🔇 停止所有音效");
    }
}

/// <summary>
/// 知识点 21: 新版音频接口 (Target — 我们期望的现代接口)
/// </summary>
public interface IAudioService
{
    void Play(string audioId, float volume); // volume: 0.0-1.0
    void Stop(string audioId);
    void StopAll();
    void SetMasterVolume(float volume); // 0.0-1.0
}

/// <summary>
/// 知识点 22: AudioAdapter — 把 LegacyAudioManager 适配成 IAudioService
/// 关键: 适配器负责数据转换 (0-100 int ↔ 0.0-1.0 float)
/// </summary>
public class AudioAdapter : IAudioService
{
    private LegacyAudioManager _legacyAudio;

    public AudioAdapter(LegacyAudioManager legacyAudio)
    {
        _legacyAudio = legacyAudio;
    }

    public void Play(string audioId, float volume)
    {
        // 转换: float 0.0-1.0 → int 0-100
        int percentVolume = Mathf.RoundToInt(volume * 100);
        _legacyAudio.SetVolume(percentVolume);
        _legacyAudio.PlaySound(audioId);
    }

    public void Stop(string audioId)
    {
        Debug.Log($"[音频适配器] 停止: {audioId} (旧版不支持单首停止，停止全部)");
        _legacyAudio.StopAll();
    }

    public void StopAll()
    {
        _legacyAudio.StopAll();
    }

    public void SetMasterVolume(float volume)
    {
        int percentVolume = Mathf.RoundToInt(Mathf.Clamp01(volume) * 100);
        _legacyAudio.SetVolume(percentVolume);
    }
}

// ============================================================
// Part 5: 类适配器 vs 对象适配器
// 知识点 23: C# 中两种适配器实现方式的对比
// ============================================================

// --- 对象适配器 (Object Adapter) — 使用组合 ---
// 前面所有的例子都是对象适配器: 适配器持有 Adaptee 的引用
// 优点: 灵活，一个适配器可以适配 Adaptee 的多个子类
// C# 推荐: 组合优于继承

/// <summary>
/// 知识点 24: 对象适配器示例 — 适配器持有 Adaptee
/// </summary>
public class ObjectAdapterExample : IGameService
{
    private ThirdPartyAchievementSystem _adaptee; // 组合

    public ObjectAdapterExample(ThirdPartyAchievementSystem adaptee)
    {
        _adaptee = adaptee;
    }

    public void Initialize() => _adaptee.ConnectToServer("default-key");
    public void Shutdown() => _adaptee.Disconnect();
    public string GetServiceName() => _adaptee.GetLibraryName();
}

// --- 类适配器 (Class Adapter) — 使用继承 ---
// 在 C# 中因为不能多继承类，只能通过接口+继承实现

/// <summary>
/// 知识点 25: 类适配器示例 — 通过继承 Adaptee 来实现 Target 接口
/// 缺点: 强耦合到具体的 Adaptee，无法适配其子类
/// 优点: 可以重写 Adaptee 的方法
/// </summary>
public class ClassAdapterExample : ThirdPartyAchievementSystem, IGameService
{
    public void Initialize()
    {
        // 直接调用继承来的方法
        ConnectToServer("inherited-key");
    }

    public void Shutdown()
    {
        Disconnect();
    }

    public string GetServiceName()
    {
        return GetLibraryName() + " (类适配器版本)";
    }
}

// ============================================================
// Part 6: 实际应用 — GameManager 通过适配器统一管理所有服务
// 知识点 26: 所有服务都实现了 IGameService，管理器不需要知道它们的具体实现
// ============================================================

/// <summary>
/// 知识点 27: ServiceManager — 初始化所有游戏服务
/// 它只知道 IGameService 接口，不关心背后的第三方库是什么
/// 这就是适配器模式带来的好处 — 彻底解耦
/// </summary>
public class ServiceManager
{
    private List<IGameService> _services = new List<IGameService>();

    public void RegisterService(IGameService service)
    {
        _services.Add(service);
        Debug.Log($"[ServiceManager] 注册服务: {service.GetServiceName()}");
    }

    public void InitializeAll()
    {
        Debug.Log("═══════════════════════════════════");
        Debug.Log("[ServiceManager] 🚀 初始化所有服务...");
        Debug.Log("═══════════════════════════════════");
        foreach (var service in _services)
        {
            service.Initialize();
        }
    }

    public void ShutdownAll()
    {
        Debug.Log("═══════════════════════════════════");
        Debug.Log("[ServiceManager] 🔚 关闭所有服务...");
        Debug.Log("═══════════════════════════════════");
        foreach (var service in _services)
        {
            service.Shutdown();
        }
    }
}

// ============================================================
// Part 7: 可互换的存档管理器 — 演示适配器的最大优势
// 知识点 28: 换一个 ISaveSystem 实现，游戏代码一行都不用改
// ============================================================

/// <summary>
/// 知识点 29: GameDataManager — 使用 ISaveSystem，不关心底层是什么
/// </summary>
public class GameDataManager
{
    private ISaveSystem _saveSystem;

    public GameDataManager(ISaveSystem saveSystem)
    {
        _saveSystem = saveSystem;
        Debug.Log($"[GameDataManager] 当前存档后端: {_saveSystem.GetBackendName()}");
    }

    // 知识点 30: 运行时切换存档后端 — 适配器换一个就行
    public void SwitchBackend(ISaveSystem newSaveSystem)
    {
        Debug.Log($"[GameDataManager] 切换存档后端: {_saveSystem.GetBackendName()} → {newSaveSystem.GetBackendName()}");
        _saveSystem = newSaveSystem;
    }

    public void SavePlayerData(string playerName, int level, int score)
    {
        string jsonData = $"{playerName}|{level}|{score}";
        _saveSystem.Save("PlayerData", jsonData);
    }

    public string LoadPlayerData()
    {
        return _saveSystem.Load("PlayerData");
    }

    public void DeleteSave()
    {
        _saveSystem.Delete("PlayerData");
    }

    public bool HasSaveData()
    {
        return _saveSystem.Exists("PlayerData");
    }
}

// ============================================================
// Part 8: 演示脚本 — 挂载到 Unity GameObject 运行
// ============================================================

public class AdapterDemo : MonoBehaviour
{
    private void Start()
    {
        StartCoroutine(RunAllDemos());
    }

    private IEnumerator RunAllDemos()
    {
        Debug.Log("╔══════════════════════════════════════════════╗");
        Debug.Log("║  每日脚本学习 Day 10 — 适配器模式           ║");
        Debug.Log("║  Adapter Pattern Demo                       ║");
        Debug.Log("╚══════════════════════════════════════════════╝\n");

        // --- 演示 1: 核心适配器 ---
        Debug.Log("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        Debug.Log("📦 演示 1: 核心适配器 — 第三方SDK统一接入");
        Debug.Log("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        DemoCoreAdapter();
        yield return null;

        // --- 演示 2: 输入系统适配 ---
        Debug.Log("\n━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        Debug.Log("🎮 演示 2: 输入系统适配器 — 新旧Input无缝切换");
        Debug.Log("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        DemoInputAdapter();
        yield return null;

        // --- 演示 3: 存档系统适配 ---
        Debug.Log("\n━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        Debug.Log("💾 演示 3: 存档系统适配器 — 本地/JSON/云存档互换");
        Debug.Log("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        DemoArchiveAdapter();
        yield return null;

        // --- 演示 4: 双向适配 ---
        Debug.Log("\n━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        Debug.Log("🔊 演示 4: 音频系统适配 — 旧版→新版接口");
        Debug.Log("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        DemoAudioAdapter();
        yield return null;

        // --- 演示 5: 类适配器 vs 对象适配器 ---
        Debug.Log("\n━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        Debug.Log("📐 演示 5: 对象适配器 vs 类适配器");
        Debug.Log("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        DemoAdapterComparison();
        yield return null;

        // --- 总结 ---
        Debug.Log("\n╔══════════════════════════════════════════════╗");
        Debug.Log("║  🎉 所有演示完成!                          ║");
        Debug.Log("╚══════════════════════════════════════════════╝");
        PrintSummary();
    }

    private void DemoCoreAdapter()
    {
        // 实例化第三方库 (模拟从 Asset Store 导入的插件)
        var achievementPlugin = new ThirdPartyAchievementSystem();
        var leaderboardPlugin = new ThirdPartyLeaderboardSystem();

        // 用适配器包装它们 → 全部变成 IGameService
        IGameService achievementService = new AchievementAdapter(achievementPlugin, "api-key-12345");
        IGameService leaderboardService = new LeaderboardAdapter(leaderboardPlugin, "player01", "pass123");

        // ServiceManager 只认识 IGameService，不关心底层是谁
        var manager = new ServiceManager();
        manager.RegisterService(achievementService);
        manager.RegisterService(leaderboardService);

        manager.InitializeAll();
        manager.ShutdownAll();

        Debug.Log("\n💡 关键点:");
        Debug.Log("  两个完全不同接口的第三方库 (成就/排行榜)");
        Debug.Log("  通过适配器统一成了 IGameService");
        Debug.Log("  ServiceManager 不需要知道底层是什么库");
    }

    private void DemoInputAdapter()
    {
        // 旧版输入系统
        var legacyInput = new LegacyUnityInput();
        IInputProvider inputAdapter = new LegacyInputAdapter(legacyInput);

        Debug.Log($"当前输入源: {inputAdapter.GetInputSourceName()}");
        Debug.Log($"水平轴: {inputAdapter.GetHorizontal():F2}");
        Debug.Log($"垂直轴: {inputAdapter.GetVertical():F2}");

        // 切换到新版输入系统 → 游戏代码完全不用动!
        inputAdapter = new SimulatedNewInputAdapter();
        Debug.Log($"\n切换输入源: {inputAdapter.GetInputSourceName()}");
        Debug.Log($"水平轴: {inputAdapter.GetHorizontal():F2}");
        Debug.Log($"垂直轴: {inputAdapter.GetVertical():F2}");

        Debug.Log("\n💡 关键点:");
        Debug.Log("  游戏逻辑只依赖 IInputProvider 接口");
        Debug.Log("  换输入系统只需换一个适配器实例");
        Debug.Log("  这就是为什么 Unity 可以用适配器思维做 Input System 迁移");
    }

    private void DemoArchiveAdapter()
    {
        // 创建三个不同的存档后端
        ISaveSystem playerPrefsSave = new PlayerPrefsAdapter(new PlayerPrefsStorage());
        ISaveSystem jsonSave = new JsonFileAdapter("/save/savegame.json");
        ISaveSystem cloudSave = new CloudSaveAdapter("user_007");

        // GameDataManager 可以使用任意一个
        var dataManager = new GameDataManager(playerPrefsSave);
        dataManager.SavePlayerData("勇者小亮", 15, 9800);
        Debug.Log($"读取: {dataManager.LoadPlayerData()}");

        // 运行时切换到 JSON 存档
        dataManager.SwitchBackend(jsonSave);
        dataManager.SavePlayerData("勇者小亮", 16, 10200);

        // 运行时切换到云存档
        dataManager.SwitchBackend(cloudSave);
        dataManager.SavePlayerData("勇者小亮", 17, 11500);

        Debug.Log("\n💡 关键点:");
        Debug.Log("  同一个 ISaveSystem 接口，三种不同的后端");
        Debug.Log("  游戏存档逻辑完全不依赖具体存储方式");
        Debug.Log("  可以随时切换后端而不影响游戏代码");
    }

    private void DemoAudioAdapter()
    {
        var legacyAudio = new LegacyAudioManager();
        IAudioService audioService = new AudioAdapter(legacyAudio);

        // 新代码用新接口 (float 0.0-1.0)
        audioService.SetMasterVolume(0.75f);
        audioService.Play("bgm_battle", 0.8f);
        audioService.Play("sfx_sword_hit", 1.0f);
        audioService.StopAll();

        Debug.Log("\n💡 关键点:");
        Debug.Log("  旧版接口: SetVolume(int 0-100) — 整数值");
        Debug.Log("  新版接口: SetMasterVolume(float 0.0-1.0) — 浮点值");
        Debug.Log("  适配器负责数据格式转换 (×100 或 ÷100)");
    }

    private void DemoAdapterComparison()
    {
        // 对象适配器 — 组合方式
        var adaptee = new ThirdPartyAchievementSystem();
        IGameService objectAdapter = new ObjectAdapterExample(adaptee);
        Debug.Log("[对象适配器] " + objectAdapter.GetServiceName());
        objectAdapter.Initialize();
        objectAdapter.Shutdown();

        // 类适配器 — 继承方式
        IGameService classAdapter = new ClassAdapterExample();
        Debug.Log("[类适配器] " + classAdapter.GetServiceName());
        classAdapter.Initialize();
        classAdapter.Shutdown();

        Debug.Log("\n💡 对比:");
        Debug.Log("  对象适配器: 组合 Adaptee → 灵活,可适配子类,推荐");
        Debug.Log("  类适配器: 继承 Adaptee → C#不支持多继承,不够灵活");
        Debug.Log("  结论: 在 C# 中优先使用对象适配器(组合)");
    }

    private void PrintSummary()
    {
        Debug.Log("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        Debug.Log("📋 适配器模式 — 知识点总结");
        Debug.Log("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");

        Debug.Log("\n🧩 适配器三要素:");
        Debug.Log("  1. Target  (目标接口) — 客户端期望的接口 (IGameService)");
        Debug.Log("  2. Adaptee (被适配者) — 已有但不兼容的类 (第三方SDK)");
        Debug.Log("  3. Adapter (适配器)   — 把 Adaptee 翻译成 Target");

        Debug.Log("\n📐 两种实现方式:");
        Debug.Log("  对象适配器: Adapter 持有 Adaptee 引用 → 灵活, C# 首选");
        Debug.Log("  类适配器:   Adapter 继承 Adaptee        → 耦合, 少用");

        Debug.Log("\n🎮 Unity 中的典型应用:");
        Debug.Log("  1. 第三方 SDK 统一接入 (广告/统计/支付/社交)");
        Debug.Log("  2. 新旧 Input System 迁移过渡");
        Debug.Log("  3. 多平台存档 (PlayerPrefs/JSON/云端)");
        Debug.Log("  4. 音频中间层 (Unity Audio / FMOD / Wwise)");
        Debug.Log("  5. 不同平台的本地通知 (iOS/Android)");
        Debug.Log("  6. 网络层适配 (Photon/Mirror/自建服务器)");
        Debug.Log("  7. 不同数据格式 (XML/JSON/Protobuf) 的统一读写");

        Debug.Log("\n✅ 什么时候用适配器?");
        Debug.Log("  1. 想用已有类，但接口不匹配 → 适配它，不要改它");
        Debug.Log("  2. 需要对接多个第三方库 → 统一接口，一行不改业务代码");
        Debug.Log("  3. 需要新旧系统共存过渡 → 适配器做桥梁");
        Debug.Log("  4. 想做可替换的模块 → 定义接口，适配各种实现");

        Debug.Log("\n❌ 什么时候不用?");
        Debug.Log("  1. 能直接改源码 → 直接改比加适配器更简单");
        Debug.Log("  2. 接口差异太大 → 适配器写得很痛苦，可能架构设计有问题");
        Debug.Log("  3. 只需要对接一个库且不会换 → 直接依赖也没关系");

        Debug.Log("\n📐 与已学模式的关系:");
        Debug.Log("  - 工厂方法 (Day9): 工厂创建对象 → 适配器包装对象");
        Debug.Log("  - 装饰器   (Day7): 装饰器增强功能 → 适配器转换接口");
        Debug.Log("  - 策略模式 (Day6): 策略定义算法 → 适配器统一接口");
        Debug.Log("  - 模板方法 (Day8): 模板定义流程 → 适配器翻译调用");
        Debug.Log("  - 命令模式 (Day5): 命令封装请求 → 适配器转换参数");
        Debug.Log("  - 外观模式 (未学): 外观简化接口 → 适配器转换接口 (不同的意图)");
        Debug.Log("    外观: 让复杂接口变简单");
        Debug.Log("    适配器: 让不兼容接口变兼容");

        Debug.Log("\n💡 一句话总结:");
        Debug.Log("  \"适配器就像出国旅行用的电源转换插头 —");
        Debug.Log("   你的手机 (客户端) 需要的是 USB-C (ITarget),");
        Debug.Log("   当地的插座 (Adaptee) 是圆孔的,");
        Debug.Log("   插上转换头 (Adapter) — 完美充电，谁也不改。\"");
    }
}

// ============================================================
// 知识点总结 — 适配器模式的精髓
// ============================================================
//
// ✅ 适配器解决了什么问题?
//   把"已有的但不能改的代码"适配成"我们想要的接口"
//   - 第三方库的 API 不符合我们的规范 → 适配它
//   - 老代码接口过时了，但不能重写 → 适配它
//   - 多个实现各有各的调用方式 → 统一适配到一个接口
//
// 🧠 核心思想:
//   不修改原有代码 (开闭原则), 而是在外面包一层
//   就像你不会给美规插头换个形状,而是用转换插头
//
// 🎮 游戏开发中的真实场景:
//   1. 接入多个广告 SDK (AdMob / Unity Ads / 穿山甲)
//      → IAdProvider 接口 + 各SDK的适配器
//   2. 多平台登录 (Google / Apple / Facebook / Steam)
//      → ILoginService 接口 + 各平台的适配器
//   3. Unity 旧 Input → 新 Input System 过渡
//      → IInputProvider + 两个适配器
//   4. 本地存储 → 云端存储平滑升级
//      → ISaveSystem + 两种适配器
//
// ⚠️ 注意事项:
//   - 适配器增加了一层间接调用，有轻微性能开销
//   - 不要过度使用 → 如果接口本身就兼容，不需要适配器
//   - 适配器太多说明接口设计可能有问题
//
// 🔗 外观模式 vs 适配器模式 (常见混淆):
//   外观 (Facade):  简化复杂接口 → 让调用更简单
//   适配器 (Adapter): 转换不兼容接口 → 让两个接口能对接
//   举例:
//     外观:   Unity 的 Input 类本身就是对底层输入API的外观 (简单易用)
//     适配器: 把旧 Input 包装成新 Input System 的调用方式 (接口转换)
//
// 💡 一句话总结:
//   "适配器让两个不兼容的接口能一起工作 —
//    不改旧代码，不破坏新规范，就加一层翻译。
//    这是对接第三方库、系统迁移时最实用的模式之一。"
// ============================================================
