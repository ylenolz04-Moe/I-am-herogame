// ============================================================
// 每日脚本学习 Day 11 — 2026-07-18
// 主题: C# 外观模式 (Facade Pattern)
// 适用: Unity 游戏开发 · GameManager · 子系统整合 · 场景加载 · 战斗入口
// ============================================================

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ============================================================
// 知识点 1: 外观模式核心思想 — "给复杂子系统提供一个简单入口"
//
//   [客户端] ──► [外观 Facade] ──┬── [子系统A]
//                                ├── [子系统B]
//                                └── [子系统C]
//
// 现实类比: 一键启动汽车 —
//   你只需按"启动键"(Facade)
//   背后: 供电→点火→喷油→发动机转→自检→仪表亮 (一堆子系统协作)
//   外观让你不需要知道内部有多复杂
// ============================================================

// ============================================================
// Part 1: 复杂子系统 — 模拟游戏中分散的各种模块
// 知识点 2: 外观简化的是"一群互相依赖的子系统"
// ============================================================

/// <summary>
/// 知识点 3: 音频子系统 — 管理所有声音
/// </summary>
public class AudioSubsystem
{
    private float _masterVolume = 1f;
    private float _sfxVolume = 1f;
    private float _bgmVolume = 0.8f;

    public void LoadAudioBank(string bankName)
    {
        Debug.Log($"[音频] 加载音频库: {bankName}");
    }

    public void PlayBGM(string trackName)
    {
        Debug.Log($"[音频] 🎵 播放背景音乐: {trackName} (音量:{_bgmVolume})");
    }

    public void PlaySFX(string clipName)
    {
        Debug.Log($"[音频] 🔊 播放音效: {clipName} (音量:{_sfxVolume})");
    }

    public void StopBGM()
    {
        Debug.Log($"[音频] ⏸️ 停止背景音乐");
    }

    public void SetMasterVolume(float vol)
    {
        _masterVolume = Mathf.Clamp01(vol);
        Debug.Log($"[音频] 主音量设为: {_masterVolume:F1}");
    }

    public bool IsMusicPlaying() => true;
}

/// <summary>
/// 知识点 4: 输入子系统 — 处理玩家操作
/// </summary>
public class InputSubsystem
{
    private bool _enabled = true;

    public void Enable()
    {
        _enabled = true;
        Debug.Log("[输入] ✅ 玩家输入已启用");
    }

    public void Disable()
    {
        _enabled = false;
        Debug.Log("[输入] ❌ 玩家输入已禁用");
    }

    public Vector2 GetMoveDirection()
    {
        if (!_enabled) return Vector2.zero;
        return new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
    }

    public bool IsActionPressed(string action)
    {
        if (!_enabled) return false;
        switch (action)
        {
            case "Jump": return Input.GetKeyDown(KeyCode.Space);
            case "Attack": return Input.GetKeyDown(KeyCode.J);
            case "Interact": return Input.GetKeyDown(KeyCode.E);
            default: return false;
        }
    }
}

/// <summary>
/// 知识点 5: UI 子系统 — 管理界面显示
/// </summary>
public class UISubsystem
{
    private Dictionary<string, GameObject> _panels = new Dictionary<string, GameObject>();

    public void ShowPanel(string panelName)
    {
        Debug.Log($"[UI] 📋 显示面板: {panelName}");
    }

    public void HidePanel(string panelName)
    {
        Debug.Log($"[UI] 🙈 隐藏面板: {panelName}");
    }

    public void ShowDamageNumber(Vector3 position, float damage)
    {
        Debug.Log($"[UI] 💥 伤害数字: {damage} @ ({position.x:F1}, {position.y:F1})");
    }

    public void ShowDialog(string text, Action onConfirm = null)
    {
        Debug.Log($"[UI] 💬 对话框: \"{text}\"");
        onConfirm?.Invoke();
    }

    public void UpdateHUD(int hp, int maxHp, int mp, int maxMp)
    {
        Debug.Log($"[UI] 📊 HUD: HP {hp}/{maxHp} | MP {mp}/{maxMp}");
    }

    public void FadeScreen(float duration, bool fadeIn, Action onComplete = null)
    {
        Debug.Log($"[UI] 🌑 淡{(fadeIn ? "入" : "出")}屏幕 ({duration}秒)");
        onComplete?.Invoke();
    }
}

/// <summary>
/// 知识点 6: 存档子系统 — 读写游戏进度
/// </summary>
public class SaveSubsystem
{
    public void SaveGame(int slotIndex, string data)
    {
        Debug.Log($"[存档] 💾 保存到槽位 {slotIndex}: {data}");
    }

    public string LoadGame(int slotIndex)
    {
        Debug.Log($"[存档] 📂 从槽位 {slotIndex} 加载");
        return $"save_data_slot_{slotIndex}";
    }

    public bool HasSave(int slotIndex)
    {
        return slotIndex <= 3; // 模拟: 前3个槽位有存档
    }

    public void DeleteSave(int slotIndex)
    {
        Debug.Log($"[存档] 🗑️ 删除槽位 {slotIndex}");
    }
}

/// <summary>
/// 知识点 7: 场景子系统 — 加载/切换场景
/// </summary>
public class SceneSubsystem
{
    public void LoadScene(string sceneName)
    {
        Debug.Log($"[场景] 🌍 加载场景: {sceneName}");
    }

    public string GetCurrentScene()
    {
        return "Level_01";
    }

    public void PreloadScene(string sceneName)
    {
        Debug.Log($"[场景] ⏳ 预加载场景: {sceneName}");
    }
}

/// <summary>
/// 知识点 8: 粒子特效子系统
/// </summary>
public class VFXSubsystem
{
    public void PlayEffect(string effectName, Vector3 position)
    {
        Debug.Log($"[特效] ✨ 播放特效: {effectName} @ ({position.x:F1}, {position.y:F1})");
    }

    public void StopAllEffects()
    {
        Debug.Log("[特效] 🛑 停止所有特效");
    }

    public void SetQualityLevel(int level)
    {
        Debug.Log($"[特效] 画质等级: {level}");
    }
}

/// <summary>
/// 知识点 9: 敌人 AI 子系统 (战斗相关)
/// </summary>
public class EnemySubsystem
{
    private List<string> _activeEnemies = new List<string>();

    public void SpawnEnemies(string waveConfig)
    {
        Debug.Log($"[敌人] 👾 根据配置生成敌人: {waveConfig}");
        _activeEnemies.Add("Slime_x2");
        _activeEnemies.Add("Skeleton_x1");
    }

    public void ClearAllEnemies()
    {
        Debug.Log($"[敌人] 💀 清除所有敌人");
        _activeEnemies.Clear();
    }

    public int GetEnemyCount() => _activeEnemies.Count;

    public void SetDifficulty(float difficulty)
    {
        Debug.Log($"[敌人] ⚡ 难度设为: {difficulty:F1}");
    }
}

// ============================================================
// Part 2: 外观类 — 把复杂子系统打包成简单入口
// 知识点 10: GameFacade — 客户端只需要和这一个类打交道
// ============================================================

/// <summary>
/// 知识点 11: GameFacade — 一站式游戏管理外观
///
/// 想象没有外观的情况:
///   客户端要自己调 AudioSubsystem.PlayBGM()
///   再调 UISubsystem.ShowPanel()
///   再调 InputSubsystem.Disable()
///   再调 SceneSubsystem.LoadScene()
///   ... 每个地方都要记住这堆子系统，耦合严重
///
/// 有了外观:
///   客户端只需: gameFacade.StartLevel("Level_01");
///   外观内部帮你搞定所有子系统的协调
/// </summary>
public class GameFacade
{
    // 知识点 12: 外观持有所有子系统的引用
    private AudioSubsystem _audio;
    private InputSubsystem _input;
    private UISubsystem _ui;
    private SaveSubsystem _save;
    private SceneSubsystem _scene;
    private VFXSubsystem _vfx;
    private EnemySubsystem _enemy;

    public GameFacade()
    {
        _audio = new AudioSubsystem();
        _input = new InputSubsystem();
        _ui = new UISubsystem();
        _save = new SaveSubsystem();
        _scene = new SceneSubsystem();
        _vfx = new VFXSubsystem();
        _enemy = new EnemySubsystem();
    }

    // ============================================================
    // 知识点 13: 高级 API — 一个方法搞定一整件事
    // 客户端不需要知道内部有多少子系统在协作
    // ============================================================

    /// <summary>
    /// 知识点 14: 启动游戏 — 一行调用，背后协调6个子系统
    /// </summary>
    public void StartGame()
    {
        Debug.Log("═══════════════════════════════════");
        Debug.Log("[GameFacade] 🚀 启动游戏...");
        Debug.Log("═══════════════════════════════════");

        _audio.LoadAudioBank("main_audio_bank");
        _audio.PlayBGM("title_theme");
        _ui.ShowPanel("MainMenu");
        _ui.FadeScreen(1f, true);
        _input.Enable();
    }

    /// <summary>
    /// 知识点 15: 开始关卡 — 一个调用完成加载、音乐、UI、敌人生成
    /// </summary>
    public void StartLevel(string levelName)
    {
        Debug.Log("═══════════════════════════════════");
        Debug.Log($"[GameFacade] 🎮 开始关卡: {levelName}");
        Debug.Log("═══════════════════════════════════");

        _input.Disable();                     // 1. 先禁用输入
        _ui.FadeScreen(0.5f, false, () => {   // 2. 黑屏过渡
            Debug.Log("  (屏幕已黑，切换场景中...)");
        });
        _scene.LoadScene(levelName);          // 3. 加载场景
        _audio.StopBGM();                     // 4. 停掉旧音乐
        _audio.PlayBGM($"bgm_{levelName}");   // 5. 播放关卡音乐
        _enemy.SpawnEnemies("wave_01");       // 6. 生成敌人
        _ui.UpdateHUD(100, 100, 50, 50);     // 7. 刷新 HUD
        _ui.FadeScreen(0.5f, true);           // 8. 淡入
        _input.Enable();                      // 9. 恢复输入
    }

    /// <summary>
    /// 知识点 16: 暂停游戏 — 统一控制所有需要暂停的子系统
    /// </summary>
    public void PauseGame()
    {
        Debug.Log("[GameFacade] ⏸️ 暂停游戏");
        _input.Disable();
        _audio.SetMasterVolume(0.3f);  // 降低音量
        _ui.ShowPanel("PauseMenu");
        Time.timeScale = 0f;           // Unity 时间暂停
    }

    /// <summary>
    /// 知识点 17: 恢复游戏
    /// </summary>
    public void ResumeGame()
    {
        Debug.Log("[GameFacade] ▶️ 恢复游戏");
        Time.timeScale = 1f;
        _ui.HidePanel("PauseMenu");
        _audio.SetMasterVolume(1f);
        _input.Enable();
    }

    /// <summary>
    /// 知识点 18: 保存并退出 — 存档→过渡→返回主菜单
    /// </summary>
    public void SaveAndQuit(int slotIndex)
    {
        Debug.Log("[GameFacade] 💾 保存并退出...");
        _input.Disable();
        _save.SaveGame(slotIndex, "player_progress_data");
        _vfx.StopAllEffects();
        _enemy.ClearAllEnemies();
        _ui.FadeScreen(1f, false, () => {
            _scene.LoadScene("MainMenu");
            _audio.StopBGM();
            _audio.PlayBGM("title_theme");
            _ui.ShowPanel("MainMenu");
            _ui.FadeScreen(1f, true);
            _input.Enable();
        });
    }

    /// <summary>
    /// 知识点 19: 玩家死亡处理 — 播放特效→显示UI→禁用输入→清怪
    /// </summary>
    public void OnPlayerDeath(Vector3 deathPosition)
    {
        Debug.Log("[GameFacade] 💀 玩家死亡!");
        _input.Disable();
        _vfx.PlayEffect("death_explosion", deathPosition);
        _audio.PlaySFX("player_death");
        _ui.ShowDialog("你阵亡了...", () => {
            _enemy.ClearAllEnemies();
            _scene.LoadScene(_scene.GetCurrentScene()); // 重新加载
            _input.Enable();
        });
    }

    /// <summary>
    /// 知识点 20: 播放过场动画 — 禁用输入+黑屏+音乐切换+特效+恢复
    /// </summary>
    public void PlayCutscene(string cutsceneId)
    {
        Debug.Log($"[GameFacade] 🎬 播放过场: {cutsceneId}");
        _input.Disable();
        _ui.FadeScreen(0.5f, false);
        _audio.StopBGM();
        _ui.HidePanel("HUD");
        // 实际项目: 触发 Timeline / Cinemachine 过场
        Debug.Log($"[GameFacade] → 过场 {cutsceneId} 播放完毕");
        _audio.PlayBGM("bgm_after_cutscene");
        _ui.ShowPanel("HUD");
        _ui.FadeScreen(0.5f, true);
        _input.Enable();
    }

    // ============================================================
    // 知识点 21: 也提供子系统级别的访问 (可选，便于需要细粒度控制时使用)
    // ============================================================

    public AudioSubsystem Audio => _audio;
    public InputSubsystem Input => _input;
    public UISubsystem UI => _ui;
    public SaveSubsystem Save => _save;

    /// <summary>
    /// 知识点 22: 检查是否有存档 — 对外观自己来说只是转发
    /// </summary>
    public bool HasAnySave()
    {
        for (int i = 1; i <= 5; i++)
        {
            if (_save.HasSave(i)) return true;
        }
        return false;
    }
}

// ============================================================
// Part 3: 另一组子系统 — 演示外观模式不限于一个外观
// 知识点 23: 战斗系统外观 — 专门负责战斗相关的子系统协调
// ============================================================

/// <summary>
/// 知识点 24: 战斗相关子系统 — 技能系统
/// </summary>
public class SkillSubsystem
{
    public void CastSkill(string skillId, Vector3 target)
    {
        Debug.Log($"[技能] ⚔️ 释放技能 {skillId} → ({target.x:F1}, {target.y:F1})");
    }

    public bool IsSkillReady(string skillId)
    {
        return true; // 模拟: 冷却好了
    }

    public float GetCooldownRemaining(string skillId)
    {
        return 0f;
    }
}

/// <summary>
/// 知识点 25: 战斗子系统 — 伤害计算
/// </summary>
public class DamageSubsystem
{
    public float CalculateDamage(float baseAttack, float defense, float critChance)
    {
        bool isCrit = UnityEngine.Random.value < critChance;
        float damage = Mathf.Max(1, baseAttack - defense * 0.5f);
        if (isCrit) damage *= 2f;
        return damage;
    }

    public void ApplyDamage(GameObject target, float damage)
    {
        Debug.Log($"[伤害] 🔴 对 {target.name} 造成 {damage:F0} 点伤害");
    }
}

/// <summary>
/// 知识点 26: 战斗外观 — 玩家只需要: "攻击那个敌人"
/// 不需要关心技能检查 / 伤害计算 / 特效播放 / 音效 / UI 更新
/// </summary>
public class CombatFacade
{
    private SkillSubsystem _skill;
    private DamageSubsystem _damage;
    private VFXSubsystem _vfx;
    private AudioSubsystem _audio;
    private UISubsystem _ui;

    public CombatFacade(VFXSubsystem vfx, AudioSubsystem audio, UISubsystem ui)
    {
        _skill = new SkillSubsystem();
        _damage = new DamageSubsystem();
        _vfx = vfx;
        _audio = audio;
        _ui = ui;
    }

    /// <summary>
    /// 知识点 27: 执行一次攻击 — 外观封装了5个步骤
    /// </summary>
    public void PerformAttack(string skillId, GameObject attacker, GameObject target)
    {
        Debug.Log("--- [战斗外观] 执行攻击 ---");

        // 步骤1: 检查技能冷却
        if (!_skill.IsSkillReady(skillId))
        {
            Debug.Log($"[战斗外观] {skillId} 还在冷却中! ({_skill.GetCooldownRemaining(skillId):F1}秒)");
            return;
        }

        // 步骤2: 计算伤害
        float damage = _damage.CalculateDamage(
            baseAttack: 50f,
            defense: 20f,
            critChance: 0.15f
        );

        // 步骤3: 释放技能特效
        _skill.CastSkill(skillId, target.transform.position);
        _vfx.PlayEffect(skillId, target.transform.position);

        // 步骤4: 应用伤害+音效
        _damage.ApplyDamage(target, damage);
        _audio.PlaySFX($"hit_{skillId}");

        // 步骤5: 显示伤害数字
        _ui.ShowDamageNumber(target.transform.position, damage);

        Debug.Log($"[战斗外观] ✅ 攻击完成 — 造成 {damage:F0} 伤害");
    }

    /// <summary>
    /// 知识点 28: 简单攻击 — 普攻的快捷方式
    /// </summary>
    public void BasicAttack(GameObject attacker, GameObject target)
    {
        PerformAttack("basic_slash", attacker, target);
    }
}

// ============================================================
// Part 4: 进阶 — 可配置的外观
// 知识点 29: 通过构造函数注入子系统，让外观更灵活
// ============================================================

/// <summary>
/// 知识点 30: 不同平台的 GameFacade 可以有不同配置
/// </summary>
public class PlatformGameFacade : GameFacade
{
    private bool _isMobile;

    public PlatformGameFacade(bool isMobile) : base()
    {
        _isMobile = isMobile;
    }

    public void SetupForPlatform()
    {
        if (_isMobile)
        {
            Debug.Log("[PlatformFacade] 📱 配置为移动端模式");
            // 移动端: 降低画质、启用触屏UI
            // 实际项目: VFX.SetQualityLevel(1); UI.ShowPanel("TouchControls");
        }
        else
        {
            Debug.Log("[PlatformFacade] 🖥️ 配置为PC端模式");
            // PC端: 高画质、显示键盘提示
        }
    }
}

// ============================================================
// Part 5: 外观模式的注意事项
// 知识点 31: 外观不是万能的 — 什么时候用，什么时候不用
// ============================================================

// ✅ 用外观的时机:
//   1. 多个子系统需要按顺序协作 → 外观封装顺序
//   2. 客户端不需要细粒度控制 → 外观提供简洁入口
//   3. 子系统经常一起出现 → 外观减少重复代码
//   4. 需要隔离复杂度 → 客户端只知道外观
//
// ❌ 不用的时机:
//   1. 子系统本身已经很简单 → 加外观是多余
//   2. 客户端需要直接访问子系统 → 别挡路 (可以提供属性暴露)
//   3. 外观变得过于庞大 (God Object) → 考虑拆分成多个外观
//
// ⚠️ 外观 ≠ 单例:
//   外观只是"提供一个简单入口"，不一定要是单例
//   你可以创建多个外观实例,对应不同场景(战斗外观/菜单外观)

// ============================================================
// Part 6: 演示脚本
// ============================================================

public class FacadeDemo : MonoBehaviour
{
    private void Start()
    {
        StartCoroutine(RunAllDemos());
    }

    private IEnumerator RunAllDemos()
    {
        Debug.Log("╔══════════════════════════════════════════════╗");
        Debug.Log("║  每日脚本学习 Day 11 — 外观模式             ║");
        Debug.Log("║  Facade Pattern Demo                        ║");
        Debug.Log("╚══════════════════════════════════════════════╝\n");

        // --- 演示 1: 基础外观 ---
        Debug.Log("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        Debug.Log("📦 演示 1: GameFacade — 一站式游戏管理");
        Debug.Log("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        yield return StartCoroutine(DemoGameFacade());

        // --- 演示 2: 战斗外观 ---
        Debug.Log("\n━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        Debug.Log("⚔️ 演示 2: CombatFacade — 战斗系统一键操作");
        Debug.Log("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        yield return StartCoroutine(DemoCombatFacade());

        // --- 演示 3: 暂停恢复 ---
        Debug.Log("\n━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        Debug.Log("⏯️ 演示 3: 暂停/恢复 — 统一控制");
        Debug.Log("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        yield return StartCoroutine(DemoPauseResume());

        // --- 总结 ---
        Debug.Log("\n╔══════════════════════════════════════════════╗");
        Debug.Log("║  🎉 所有演示完成!                          ║");
        Debug.Log("╚══════════════════════════════════════════════╝");
        PrintSummary();
    }

    private IEnumerator DemoGameFacade()
    {
        var game = new GameFacade();

        // 对比: 没有外观时，你需要这样写:
        //   audio.LoadAudioBank("main");
        //   audio.PlayBGM("title");
        //   ui.ShowPanel("MainMenu");
        //   input.Enable();
        //   ... 每次都写一堆, 而且到处复制粘贴

        // 有了外观:
        game.StartGame();  // 一行搞定!
        yield return new WaitForSeconds(0.2f);

        // 开始关卡
        game.StartLevel("Forest_01");
        yield return new WaitForSeconds(0.2f);

        Debug.Log("\n💡 关键点:");
        Debug.Log("  没有外观: 客户端需要知道并调用 6+ 个子系统");
        Debug.Log("  有了外观: 客户端只调 game.StartLevel('Forest_01')");
        Debug.Log("  外观内部按正确顺序协调所有子系统");
    }

    private IEnumerator DemoCombatFacade()
    {
        var audio = new AudioSubsystem();
        var vfx = new VFXSubsystem();
        var ui = new UISubsystem();
        var combat = new CombatFacade(vfx, audio, ui);

        // 模拟两个游戏对象
        var player = new GameObject("Player");
        var enemy = new GameObject("Slime");

        // 普攻 — 外观内部完成: 检查冷却→计算伤害→特效→音效→UI
        combat.BasicAttack(player, enemy);
        yield return new WaitForSeconds(0.2f);

        // 技能攻击
        combat.PerformAttack("fire_blast", player, enemy);
        yield return new WaitForSeconds(0.2f);

        Debug.Log("\n💡 关键点:");
        Debug.Log("  一次攻击背后: 技能检查 + 伤害计算 + 特效 + 音效 + UI");
        Debug.Log("  客户端只需: combat.BasicAttack(player, enemy);");
        Debug.Log("  战斗逻辑改了也只改外观内部,客户端不受影响");
    }

    private IEnumerator DemoPauseResume()
    {
        var game = new GameFacade();
        game.StartGame();
        yield return new WaitForSeconds(0.1f);

        // 暂停
        game.PauseGame();
        yield return new WaitForSeconds(0.2f);

        // 恢复
        game.ResumeGame();
        yield return new WaitForSeconds(0.1f);

        Debug.Log("\n💡 关键点:");
        Debug.Log("  暂停 = 禁用输入 + 降低音量 + 显示暂停UI + 冻结时间");
        Debug.Log("  恢复 = 逆操作全自动完成");
        Debug.Log("  如果手动管理,很容易忘记恢复某个子系统 → Bug");
    }

    private void PrintSummary()
    {
        Debug.Log("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        Debug.Log("📋 外观模式 — 知识点总结");
        Debug.Log("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");

        Debug.Log("\n🧩 核心公式:");
        Debug.Log("  复杂 (子系统A + B + C + D + E)");
        Debug.Log("  → 简单 (Facade.一键操作())");

        Debug.Log("\n🎮 Unity 中的典型应用:");
        Debug.Log("  1. GameManager — 游戏启动/暂停/退出的总入口");
        Debug.Log("  2. CombatManager — 战斗流程 (受击/技能/死亡/掉落)");
        Debug.Log("  3. SceneLoader — 场景加载 (加载画面/进度条/预加载)");
        Debug.Log("  4. SaveManager — 存档 (序列化/加密/多槽位/云同步)");
        Debug.Log("  5. AudioManager — 音频 (BGM/SFX/音量/静音/淡入淡出)");
        Debug.Log("  6. NetworkManager — 网络 (连接/重连/消息/心跳)");
        Debug.Log("  7. UIManager — UI (面板栈/弹窗/转场/输入导航)");

        Debug.Log("\n📐 外观 vs 适配器 vs 装饰器:");
        Debug.Log("  外观   (Facade):   简化复杂接口 → 让调用更简单");
        Debug.Log("  适配器 (Adapter):  转换不兼容接口 → 让两个接口能对接");
        Debug.Log("  装饰器 (Decorator): 增强已有功能 → 不改原类加新功能");
        Debug.Log("  三者都是\"包装\"，但目的完全不同");

        Debug.Log("\n📐 与已学模式的关系:");
        Debug.Log("  - 适配器 (Day10): 适配器转换接口, 外观简化接口");
        Debug.Log("  - 工厂方法 (Day9): 工厂创建子系统对象, 外观整合它们");
        Debug.Log("  - 模板方法 (Day8): 模板定义流程骨架, 外观封装流程调用");
        Debug.Log("  - 命令模式 (Day5): 外观的一个方法可以封装多个命令");
        Debug.Log("  - 观察者 (Day3): 子系统间通过事件通信, 外观统一对外");
        Debug.Log("  - 单例模式: 外观通常是单例 (全局只有一个 GameManager)");

        Debug.Log("\n💡 一句话总结:");
        Debug.Log("  \"外观就像饭店的前台服务员 —");
        Debug.Log("   你说'来一桌年夜饭'(Facade.StartGame()),");
        Debug.Log("   服务员帮你协调后厨(音频)、传菜(UI)、");
        Debug.Log("   收银(存档)、保洁(场景) — 一群人在忙,");
        Debug.Log("   但你只需要对着一个服务员说话。");
        Debug.Log("   外观让复杂系统变得简单易用。\"");
    }
}

// ============================================================
// 知识点总结 — 外观模式的精髓
// ============================================================
//
// ✅ 外观解决了什么问题?
//   把"一堆复杂的子系统调用"变成"一个简单的方法调用"
//   - 客户端不需要知道内部有几个子系统
//   - 客户端不需要知道子系统之间的调用顺序
//   - 子系统内部变化时, 只影响外观, 不影响客户端
//
// 🧠 核心思想:
//   提供高层接口, 隐藏底层复杂度
//   就像汽车的一键启动 — 你按一下, 背后几十个零件在协作
//
// 🎮 游戏开发中的真实场景:
//   1. GameManager.StartLevel("boss_fight")
//      → 加载场景 → 切换BGM → 生成Boss → 关小怪 → 更新HUD → 播放过场
//
//   2. SaveManager.AutoSave()
//      → 序列化状态 → 压缩 → 加密 → 写入文件 → 同步云端 → 更新UI时间戳
//
//   3. CombatManager.ExecuteAttack(attacker, target, skill)
//      → 检查冷却 → 计算伤害 → 播放动画 → 特效 → 音效 → 伤害数字 → 检查死亡
//
// ⚠️ 注意事项:
//   - 外观不要太庞大 (God Object 反模式)
//   - 如果外观方法太多, 考虑拆成多个外观 (GameFacade / CombatFacade / MenuFacade)
//   - 外观是简化入口, 不是限制 — 高级用户可以直接访问子系统
//
// 🔗 外观 vs 单例:
//   很多 Unity 项目中的 GameManager 既是外观又是单例
//   外观 = 设计模式 (提供简单接口)
//   单例 = 实现细节 (保证只有一个实例)
//   两者可以组合, 但不是同一个概念
//
// 💡 一句话总结:
//   "外观模式就是给复杂系统装一个'一键操作'按钮 —
//    按下去, 所有脏活累活自动完成。
//    这是游戏开发中最常用的模式之一, 你的 GameManager 很可能已经是一个外观!"
// ============================================================
