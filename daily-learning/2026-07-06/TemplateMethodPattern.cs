// ============================================================
// 每日脚本学习 Day 8 — 2026-07-06
// 主题: C# 模板方法模式 (Template Method Pattern)
// 适用: Unity 游戏开发 · 合成/制作系统 · AI 行为流程 · 任务系统 · UI 生命周期
// ============================================================

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ============================================================
// 知识点 1: 模板方法模式核心思想 — "父类定义算法骨架，子类填充具体步骤"
//
//   [抽象父类] ──► TemplateMethod()  ← 算法骨架 (通常 sealed，不允许覆写)
//                      ├── Step1()   ← 抽象方法 (子类必须实现)
//                      ├── Step2()   ← 抽象方法
//                      ├── Hook1()   ← 虚拟钩子 (子类可选覆写，有默认行为)
//                      └── Step3()   ← 具体方法 (所有子类共用)
//
// 好莱坞原则: "不要打电话给我们，我们会打电话给你"
// 父类控制流程，子类只负责填空 — 控制权在父类手里
// ============================================================

// ============================================================
// Part 1: 药水酿造系统 — 模板方法模式的经典应用
// 知识点 2: 所有药水遵循相同的酿造步骤，但每步的具体内容不同
// ============================================================

/// <summary>
/// 知识点 3: PotionRecipe — 模板方法抽象基类
///   定义了酿造药水的固定流程（算法骨架）
///   子类只需覆写每个步骤，不能改变流程顺序
/// </summary>
public abstract class PotionRecipe
{
    // 知识点 4: 模板方法 — 定义算法骨架，用 sealed 防止子类破坏流程
    // 所有药水都按这个顺序酿造：准备材料 → 混合 → 加热/冷却 → 装瓶 → 完成
    public sealed void Brew()
    {
        PrepareIngredients();   // 步骤1: 准备材料
        MixIngredients();       // 步骤2: 混合材料
        ApplyTemperature();     // 步骤3: 加热或冷却
        AddCatalyst();          // 步骤4: 添加催化剂 (钩子方法 — 可选)
        Bottle();               // 步骤5: 装瓶
        OnBrewComplete();       // 步骤6: 完成回调 (钩子方法 — 可选)
    }

    // 知识点 5: 抽象方法 — 子类必须实现，每个药水都不同
    protected abstract void PrepareIngredients();
    protected abstract void MixIngredients();
    protected abstract void ApplyTemperature();

    // 知识点 6: 具体方法 — 所有子类共用的逻辑，放在父类中避免重复
    protected virtual void Bottle()
    {
        Debug.Log("  🧴 将药水装入玻璃瓶，贴上标签");
    }

    // 知识点 7: 钩子方法 (Hook) — 子类可选覆写，有默认空实现
    // 不是所有药水都需要催化剂
    protected virtual void AddCatalyst()
    {
        // 默认: 不添加催化剂
    }

    // 知识点 8: 另一个钩子 — 酿造完成后的回调
    protected virtual void OnBrewComplete()
    {
        Debug.Log("  ✅ 酿造完成!");
    }
}

/// <summary>
/// 知识点 9: 生命药水 — 覆写抽象方法实现具体配方
/// </summary>
public class HealthPotion : PotionRecipe
{
    protected override void PrepareIngredients()
    {
        Debug.Log("  🌿 采集红色药材: 生命草 ×3 + 红色蘑菇 ×2");
    }

    protected override void MixIngredients()
    {
        Debug.Log("  🥣 顺时针搅拌 5 圈，直到液体变成深红色");
    }

    protected override void ApplyTemperature()
    {
        Debug.Log("  🔥 小火慢炖 3 分钟");
    }

    protected override void OnBrewComplete()
    {
        Debug.Log("  ❤️ 生命药水完成! 恢复 50 HP");
    }
}

/// <summary>
/// 知识点 10: 法力药水 — 相同的流程，不同的步骤实现
/// </summary>
public class ManaPotion : PotionRecipe
{
    protected override void PrepareIngredients()
    {
        Debug.Log("  💎 采集蓝色药材: 魔力水晶粉末 + 月光草 ×2");
    }

    protected override void MixIngredients()
    {
        Debug.Log("  🥣 逆时针搅拌 3 圈，加入魔力水晶粉末");
    }

    protected override void ApplyTemperature()
    {
        Debug.Log("  ❄️ 冰镇冷却 2 分钟");
    }

    // 知识点 11: 覆写钩子方法 — 法力药水需要催化剂
    protected override void AddCatalyst()
    {
        Debug.Log("  ✨ 滴入 3 滴独角兽眼泪作为催化剂");
    }

    protected override void OnBrewComplete()
    {
        Debug.Log("  💙 法力药水完成! 恢复 30 MP");
    }
}

/// <summary>
/// 知识点 12: 爆炸药水 — 证明即使是"攻击型"药水也走同样的流程
/// </summary>
public class ExplosivePotion : PotionRecipe
{
    protected override void PrepareIngredients()
    {
        Debug.Log("  💥 采集危险材料: 火硝石 ×2 + 硫磺粉 ×1 + 木炭粉 ×3");
    }

    protected override void MixIngredients()
    {
        Debug.Log("  🥣 小心混合干燥材料 (注意: 不能沾水!)");
    }

    protected override void ApplyTemperature()
    {
        Debug.Log("  🔥 高温烘烤至材料融合成黑色粉末");
    }

    protected override void AddCatalyst()
    {
        Debug.Log("  ⚡ 加入不稳定触媒 — 摇晃后 3 秒爆炸!");
    }

    // 知识点 13: 覆写装瓶方法 — 爆炸药水需要特殊容器
    protected override void Bottle()
    {
        Debug.Log("  🧪 装入加固陶瓷瓶 (标有💀危险标志)");
    }

    protected override void OnBrewComplete()
    {
        Debug.Log("  💣 爆炸药水完成! 投掷后造成 80 AOE 伤害");
    }
}

// ============================================================
// Part 2: 任务/关卡流程系统 — 所有任务走同样的生命周期
// 知识点 14: 模板方法不限于"制作" — 任何有固定流程的场景都适用
// ============================================================

/// <summary>
/// 知识点 15: QuestTemplate — 所有任务的统一流程骨架
///   接受 → 进行中 → 完成条件检查 → 发奖励 → 记录日志
/// </summary>
public abstract class QuestTemplate
{
    public string QuestName { get; protected set; }
    public bool IsCompleted { get; private set; }

    protected QuestTemplate(string name)
    {
        QuestName = name;
    }

    // 知识点 16: 任务模板方法 — 任务的标准生命周期
    public sealed void ExecuteQuestFlow()
    {
        OnQuestAccepted();          // 1. 接受任务
        OnQuestInProgress();        // 2. 任务进行中
        if (CheckCompletion())      // 3. 检查是否完成
        {
            GrantReward();          // 4. 发放奖励
            IsCompleted = true;
        }
        OnQuestLogged();            // 5. 记录日志
    }

    protected abstract void OnQuestAccepted();
    protected abstract void OnQuestInProgress();
    protected abstract bool CheckCompletion();
    protected abstract void GrantReward();

    // 知识点 17: 钩子 — 日志记录可选覆写
    protected virtual void OnQuestLogged()
    {
        Debug.Log($"📝 任务记录已更新: {QuestName}");
    }
}

/// <summary>
/// 知识点 18: 击杀任务 — 覆写任务流程的具体步骤
/// </summary>
public class SlayMonsterQuest : QuestTemplate
{
    private readonly int _requiredKills = 5;
    private int _currentKills;

    public SlayMonsterQuest() : base("击杀史莱姆 ×5") { }

    protected override void OnQuestAccepted()
    {
        _currentKills = 0;
        Debug.Log($"📋 接受任务: {QuestName}");
    }

    protected override void OnQuestInProgress()
    {
        // 模拟击杀
        _currentKills = 5;
        Debug.Log($"⚔️ 已击杀史莱姆: {_currentKills}/{_requiredKills}");
    }

    protected override bool CheckCompletion()
    {
        return _currentKills >= _requiredKills;
    }

    protected override void GrantReward()
    {
        Debug.Log("🎁 奖励: 金币 ×100 + 经验 ×50 + 铁剑 ×1");
    }
}

/// <summary>
/// 知识点 19: 收集任务 — 不同的内容，相同的流程
/// </summary>
public class CollectHerbQuest : QuestTemplate
{
    private readonly int _requiredHerbs = 10;
    private int _collectedHerbs;

    public CollectHerbQuest() : base("采集月光草 ×10") { }

    protected override void OnQuestAccepted()
    {
        Debug.Log($"📋 接受任务: {QuestName}");
    }

    protected override void OnQuestInProgress()
    {
        _collectedHerbs = 10;
        Debug.Log($"🌿 已采集月光草: {_collectedHerbs}/{_requiredHerbs}");
    }

    protected override bool CheckCompletion()
    {
        return _collectedHerbs >= _requiredHerbs;
    }

    protected override void GrantReward()
    {
        Debug.Log("🎁 奖励: 金币 ×50 + 生命药水 ×3");
    }
}

/// <summary>
/// 知识点 20: 护送任务 — 又一个不同的任务类型
/// </summary>
public class EscortNPCQuest : QuestTemplate
{
    private bool _npcArrived;

    public EscortNPCQuest() : base("护送商人到城镇") { }

    protected override void OnQuestAccepted()
    {
        Debug.Log($"📋 接受任务: {QuestName} — 保护商人，小心沿途的强盗!");
    }

    protected override void OnQuestInProgress()
    {
        _npcArrived = true;
        Debug.Log("🚶 商人已安全到达城镇大门");
    }

    protected override bool CheckCompletion()
    {
        return _npcArrived;
    }

    protected override void GrantReward()
    {
        Debug.Log("🎁 奖励: 金币 ×200 + 商店 8 折优惠卡");
    }
}

// ============================================================
// Part 3: 敌人回合 AI 流程 — 模板方法在战斗系统中的应用
// 知识点 21: 回合制战斗中，所有敌人遵循相同的决策流程
// ============================================================

/// <summary>
/// 知识点 22: EnemyTurnAI — 敌人回合行为的模板
///   评估战场 → 选择行动 → 执行行动 → 结束回合
/// </summary>
public abstract class EnemyTurnAI
{
    public string EnemyName { get; protected set; }
    public float HP { get; protected set; } = 100f;

    protected EnemyTurnAI(string name)
    {
        EnemyName = name;
    }

    // 知识点 23: 回合模板 — 每回合的标准流程
    public sealed void ExecuteTurn()
    {
        Debug.Log($"\n--- [{EnemyName}] 的回合开始 (HP: {HP}) ---");

        AssessBattlefield();    // 1. 评估战场局势
        string action = DecideAction();  // 2. 决定行动
        ExecuteAction(action);  // 3. 执行行动

        if (ShouldUseItem())    // 4. 钩子: 是否使用道具
        {
            UseItem();
        }

        EndTurn();              // 5. 结束回合
    }

    protected abstract void AssessBattlefield();
    protected abstract string DecideAction();
    protected abstract void ExecuteAction(string action);

    // 知识点 24: 钩子 — 低血量时使用恢复道具
    protected virtual bool ShouldUseItem()
    {
        return HP < 30f;  // 默认: 血量低于30%时使用道具
    }

    protected virtual void UseItem()
    {
        HP += 30f;
        Debug.Log($"  🧪 {EnemyName} 使用了恢复药水! HP 恢复到 {HP}");
    }

    protected virtual void EndTurn()
    {
        Debug.Log($"--- [{EnemyName}] 的回合结束 ---");
    }
}

/// <summary>
/// 知识点 25: 史莱姆 AI — 简单的攻击模式
/// </summary>
public class SlimeAI : EnemyTurnAI
{
    public SlimeAI() : base("绿色史莱姆") { HP = 60f; }

    protected override void AssessBattlefield()
    {
        Debug.Log("  👀 史莱姆环顾四周... 发现了最近的敌人!");
    }

    protected override string DecideAction()
    {
        return (HP > 30) ? "撞击" : "分裂";
    }

    protected override void ExecuteAction(string action)
    {
        Debug.Log($"  🟢 史莱姆使用 [{action}]!");
    }

    // 知识点 26: 史莱姆覆写钩子 — 分裂比吃药更有用
    protected override bool ShouldUseItem()
    {
        return false;
    }
}

/// <summary>
/// 知识点 27: 龙 AI — 复杂的策略选择
/// </summary>
public class DragonAI : EnemyTurnAI
{
    public DragonAI() : base("远古火龙") { HP = 500f; }

    protected override void AssessBattlefield()
    {
        Debug.Log("  🔥 火龙扫视战场，计算最优攻击角度...");
    }

    protected override string DecideAction()
    {
        if (HP > 350) return "龙息";
        if (HP > 150) return "尾击";
        return "狂暴龙息";
    }

    protected override void ExecuteAction(string action)
    {
        Debug.Log($"  🐉 火龙释放 [{action}]! 造成巨额伤害!");
    }

    protected override bool ShouldUseItem()
    {
        return HP < 100f;
    }

    protected override void UseItem()
    {
        HP += 100f;
        Debug.Log($"  💎 火龙吞噬了一颗魔法宝石! HP 恢复到 {HP}");
    }
}

/// <summary>
/// 知识点 28: 哥布林 AI — 狡猾的逃跑策略
/// </summary>
public class GoblinAI : EnemyTurnAI
{
    public GoblinAI() : base("狡猾哥布林") { HP = 40f; }

    protected override void AssessBattlefield()
    {
        Debug.Log("  👁️ 哥布林快速扫了一眼局势...");
    }

    protected override string DecideAction()
    {
        if (HP < 15) return "逃跑";
        return "偷袭";
    }

    protected override void ExecuteAction(string action)
    {
        if (action == "逃跑")
        {
            Debug.Log("  🏃 哥布林扔下烟雾弹，试图逃跑!");
        }
        else
        {
            Debug.Log("  🗡️ 哥布林从背后偷袭! 暴击率翻倍!");
        }
    }

    // 知识点 29: 覆写结束回合 — 哥布林可能有额外动作
    protected override void EndTurn()
    {
        Debug.Log($"  😈 哥布林发出嘲弄的笑声...");
        base.EndTurn();
    }
}

// ============================================================
// Part 4: UI 面板生命周期 — 所有弹窗都走相同的生命周期
// 知识点 30: 模板方法确保所有 UI 面板行为一致
// ============================================================

/// <summary>
/// 知识点 31: UIPanel — 所有 UI 面板的模板基类
///   显示 → 等待交互 → 验证 → 关闭
///   这确保了项目中所有弹窗的行为一致
/// </summary>
public abstract class UIPanel
{
    public bool IsShowing { get; private set; }

    // 知识点 32: UI 模板方法 — 标准弹窗生命周期
    public sealed void Show()
    {
        OnPreShow();        // 1. 显示前准备 (加载数据)
        PlayShowAnimation(); // 2. 播放入场动画
        IsShowing = true;
        OnShown();          // 3. 显示完成回调
    }

    public sealed void Hide()
    {
        if (!IsShowing) return;

        if (CanClose())     // 4. 检查是否可以关闭 (可能有未保存的更改)
        {
            PlayHideAnimation(); // 5. 播放出场动画
            IsShowing = false;
            OnHidden();          // 6. 关闭完成回调
        }
    }

    protected abstract void OnPreShow();
    protected abstract void OnShown();
    protected abstract void OnHidden();

    protected virtual void PlayShowAnimation()
    {
        Debug.Log("  ✨ 播放默认淡入动画 (0.3s)");
    }

    protected virtual void PlayHideAnimation()
    {
        Debug.Log("  🌑 播放默认淡出动画 (0.2s)");
    }

    // 知识点 33: 钩子 — 检查是否有未保存的数据
    protected virtual bool CanClose()
    {
        return true;  // 默认允许关闭
    }
}

/// <summary>
/// 知识点 34: 设置面板 — 使用模板确保行为一致
/// </summary>
public class SettingsPanel : UIPanel
{
    private bool _hasUnsavedChanges;

    protected override void OnPreShow()
    {
        Debug.Log("⚙️ 设置面板: 加载当前设置...");
        _hasUnsavedChanges = false;
    }

    protected override void OnShown()
    {
        Debug.Log("⚙️ 设置面板已显示 — 等待用户操作");
    }

    protected override void OnHidden()
    {
        Debug.Log("⚙️ 设置面板已关闭");
    }

    // 知识点 35: 覆写钩子 — 设置面板需要检查未保存更改
    protected override bool CanClose()
    {
        if (_hasUnsavedChanges)
        {
            Debug.Log("⚠️ 有未保存的更改，弹出确认对话框");
            return false;
        }
        return true;
    }
}

/// <summary>
/// 知识点 36: 背包面板 — 同样的模板，不同的内容
/// </summary>
public class InventoryPanel : UIPanel
{
    protected override void OnPreShow()
    {
        Debug.Log("🎒 背包面板: 加载物品列表...");
    }

    protected override void OnShown()
    {
        Debug.Log("🎒 背包面板已显示 — 共 42 个物品");
    }

    protected override void OnHidden()
    {
        Debug.Log("🎒 背包面板已关闭 — 内存已释放");
    }
}

// ============================================================
// Part 5: 进阶 — 模板方法 + 协程 (Unity Coroutine)
// 知识点 37: 模板方法可以结合协程处理异步操作
// ============================================================

/// <summary>
/// 知识点 38: 异步酿造 — 支持协程的模板方法
/// 酿造过程加入等待时间，更接近真实游戏场景
/// </summary>
public abstract class AsyncPotionRecipe : MonoBehaviour
{
    // 知识点 39: 协程版模板方法 — 不能用 sealed，因为 yield 需要子类参与
    // 使用 IEnumerator 替代 sealed void
    public IEnumerator BrewAsync()
    {
        Debug.Log($"🍳 开始酿造...");
        yield return PrepareAsync();    // 异步准备
        yield return MixAsync();        // 异步混合
        yield return HeatAsync();       // 异步加热
        yield return BottleAsync();     // 异步装瓶
        OnComplete();
    }

    protected abstract IEnumerator PrepareAsync();
    protected abstract IEnumerator MixAsync();
    protected abstract IEnumerator HeatAsync();

    protected virtual IEnumerator BottleAsync()
    {
        Debug.Log("  ⏳ 装瓶中...");
        yield return new WaitForSeconds(0.5f);
        Debug.Log("  ✅ 装瓶完成");
    }

    protected virtual void OnComplete()
    {
        Debug.Log("🎉 酿造成功!");
    }
}

/// <summary>
/// 知识点 40: 隐形药水 — Unity MonoBehaviour + 协程模板方法
/// </summary>
public class InvisibilityPotion : AsyncPotionRecipe
{
    protected override IEnumerator PrepareAsync()
    {
        Debug.Log("  🌿 研磨隐形菇粉末...");
        yield return new WaitForSeconds(1f);
        Debug.Log("  ✅ 粉末准备完成");
    }

    protected override IEnumerator MixAsync()
    {
        Debug.Log("  🥄 将粉末溶入幽灵精华...");
        yield return new WaitForSeconds(1.5f);
        Debug.Log("  ✅ 混合完成，液体变成透明");
    }

    protected override IEnumerator HeatAsync()
    {
        Debug.Log("  ❄️ 必须在低温下完成 — 放入冰窖...");
        yield return new WaitForSeconds(2f);
        Debug.Log("  ✅ 冷却完成");
    }

    protected override void OnComplete()
    {
        Debug.Log("👻 隐形药水完成! 使用后隐身 30 秒");
    }
}

// ============================================================
// Part 6: 演示入口 — MonoBehaviour 启动器
// 知识点 41: 在 Unity 场景中运行查看所有模板方法示例
// ============================================================

/// <summary>
/// 知识点 42: TemplateMethodDemo — 一键运行所有演示
/// </summary>
public class TemplateMethodDemo : MonoBehaviour
{
    private void Start()
    {
        Debug.Log("========== Day 8: 模板方法模式 演示开始 ==========");

        DemonstratePotionBrewing();
        DemonstrateQuestSystem();
        DemonstrateEnemyAI();
        DemonstrateUIPanels();

        // 启动协程版演示
        StartCoroutine(DemonstrateAsyncBrewing());
    }

    /// <summary>
    /// 演示 1: 药水酿造 — 相同的流程，不同的药水
    /// </summary>
    private void DemonstratePotionBrewing()
    {
        Debug.Log("\n========== 演示 1: 药水酿造系统 ==========");

        PotionRecipe[] potions =
        {
            new HealthPotion(),
            new ManaPotion(),
            new ExplosivePotion(),
        };

        foreach (var potion in potions)
        {
            Debug.Log($"\n--- 酿造 {potion.GetType().Name} ---");
            potion.Brew();  // 知识点 43: 统一调用模板方法，子类自动执行各自逻辑
        }
    }

    /// <summary>
    /// 演示 2: 任务系统 — 不同的任务，相同的生命周期
    /// </summary>
    private void DemonstrateQuestSystem()
    {
        Debug.Log("\n========== 演示 2: 任务流程系统 ==========");

        QuestTemplate[] quests =
        {
            new SlayMonsterQuest(),
            new CollectHerbQuest(),
            new EscortNPCQuest(),
        };

        foreach (var quest in quests)
        {
            Debug.Log($"\n--- {quest.QuestName} ---");
            quest.ExecuteQuestFlow();
            Debug.Log($"任务完成状态: {quest.IsCompleted}");
        }
    }

    /// <summary>
    /// 演示 3: 敌人 AI — 相同的回合流程，不同的策略
    /// </summary>
    private void DemonstrateEnemyAI()
    {
        Debug.Log("\n========== 演示 3: 敌人回合 AI ==========");

        EnemyTurnAI[] enemies =
        {
            new SlimeAI(),
            new DragonAI(),
            new GoblinAI(),
        };

        foreach (var enemy in enemies)
        {
            enemy.ExecuteTurn();
        }
    }

    /// <summary>
    /// 演示 4: UI 面板 — 保证所有弹窗行为一致
    /// </summary>
    private void DemonstrateUIPanels()
    {
        Debug.Log("\n========== 演示 4: UI 面板生命周期 ==========");

        UIPanel[] panels =
        {
            new SettingsPanel(),
            new InventoryPanel(),
        };

        foreach (var panel in panels)
        {
            Debug.Log($"\n--- {panel.GetType().Name} ---");
            panel.Show();
            panel.Hide();
        }
    }

    /// <summary>
    /// 演示 5: 协程版异步酿造
    /// </summary>
    private IEnumerator DemonstrateAsyncBrewing()
    {
        Debug.Log("\n========== 演示 5: 协程版异步酿造 ==========");

        // 因为 AsyncPotionRecipe 继承 MonoBehaviour，这里用子类实例演示
        var invisPotion = gameObject.AddComponent<InvisibilityPotion>();
        yield return invisPotion.BrewAsync();

        Debug.Log("\n========== Day 8: 所有演示完成 ==========");
    }
}

// ============================================================
// Part 7: 实战技巧 — 模板方法模式的最佳实践
// 知识点 44: 几个关键设计原则
// ============================================================

/// <summary>
/// 知识点 45: 模板方法最佳实践总结
/// </summary>
public static class TemplateMethodBestPractices
{
    public static void PrintGuidelines()
    {
        Debug.Log("\n========== 模板方法模式 最佳实践 ==========");

        // 原则 1: 模板方法加 sealed
        Debug.Log("✅ 原则 1: 模板方法加 sealed 关键字 — 防止子类破坏流程");
        Debug.Log("   public sealed void Brew() { ... }  ← 子类不能覆写");

        // 原则 2: 尽量少的抽象方法
        Debug.Log("✅ 原则 2: 抽象方法 ≤ 5 个 — 太多步骤说明类职责过大");

        // 原则 3: 善用钩子
        Debug.Log("✅ 原则 3: 钩子方法提供默认实现 — 子类按需覆写，不强制");
        Debug.Log("   protected virtual void AddCatalyst() { }  ← 空实现钩子");

        // 原则 4: 具体方法放在父类
        Debug.Log("✅ 原则 4: 共用逻辑放父类 — 避免代码重复");
        Debug.Log("   protected virtual void Bottle() → 所有药水装瓶逻辑一样");

        // 原则 5: 好莱坞原则
        Debug.Log("✅ 原则 5: 好莱坞原则 — 父类控制一切，子类只填空");
        Debug.Log("   'Don't call us, we'll call you.'");
    }
}

// ============================================================
// 知识点总结 — 模板方法模式的精髓
// ============================================================
//
// ✅ 什么时候用?
//   - 多个类有相同的算法流程，但具体步骤不同
//   - 需要控制算法的结构，确保子类不破坏流程
//   - 想在父类中集中管理共用逻辑，减少重复代码
//   - 希望"框架"控制流程，"用户"填充细节
//
// ❌ 什么时候不用?
//   - 流程本身不稳定、经常变化 → 模板方法不易修改
//   - 子类之间差异太大，没有统一的流程骨架
//   - 需要的步骤数量不固定 → 考虑策略模式
//
// 🎮 游戏开发中的典型应用:
//   1. 制作/合成系统 (如本文件: 药水酿造)
//   2. 任务/成就系统 (接受→进行→完成→奖励)
//   3. 回合制 AI 行为 (评估→决策→执行→结束)
//   4. UI 面板生命周期 (显示→交互→验证→关闭)
//   5. 场景加载流程 (加载→初始化→淡入→可操作)
//   6. 存档系统 (序列化→压缩→加密→写入)
//   7. 技能释放流程 (蓄力→吟唱→释放→冷却)
//
// 📐 与已学模式的关系:
//   - 策略模式 (Day6): 策略替换整个算法 | 模板方法只替换算法中的步骤
//   - 工厂方法   : 工厂方法是模板方法的一种特殊形式
//   - 装饰器模式 (Day7): 装饰器从外部叠加 | 模板方法从内部填充
//   - 状态机 (Day4): 状态机管理状态转换 | 模板方法管理步骤顺序
//
// 💡 一句话总结:
//   "模板方法就像填空题 — 父类出好试卷，
//    子类只需要填上每个空格的答案，
//    答题的顺序和规则已经由父类定好了，谁也改不了。"
// ============================================================
