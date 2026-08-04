// ============================================================
// 每日脚本学习 Day 17 — 2026-08-05
// 主题: C# 委托与事件 — 从订阅通知到解耦架构
// 适用: Unity 游戏开发 · 事件驱动 · 观察者模式 · 成就系统
// 难度: ★★☆☆☆ (进阶级)
// ============================================================

using System;
using System.Collections.Generic;
using UnityEngine;

// ============================================================
// 知识点 1: 委托 (Delegate) — 把方法当作"包裹"传递
//
// 类比: 现实中的委托
//   你委托朋友帮你取快递 — 你告诉朋友"取快递的方法"
//   朋友不需要知道怎么取, 只需要执行你给的方法
//
// 代码里的委托:
//   delegate 返回值类型 委托名(参数列表);
//   → 声明一种"方法签名" — 什么参数进、什么类型出
//   → 然后把符合签名的方法"装"进委托变量里
//
// 最简单的理解:
//   变量存数据:    int hp = 100;
//   委托存方法:    Action onDie = DieMethod;  ← 把方法当数据存!
// ============================================================

// ----- 自定义委托 (最传统的写法) -----
// 知识点 2: delegate 关键字 — 声明一种"方法类型"
// 格式: delegate 返回值 委托名(参数);
// 这行意思是: "HealthChangedHandler 是一种方法类型,
//             这种方法的签名是: 接收 float 参数, 返回 void"
public delegate void HealthChangedHandler(float currentHealth, float maxHealth);

// ============================================================
// 知识点 3: Action 和 Func — C# 内置委托, 不需要自己声明!
//
// Action        = 无返回值的方法   (void)
// Action<T>     = 有1个参数, 无返回值
// Action<T1,T2> = 有2个参数, 无返回值 (最多16个参数!)
//
// Func<TResult>       = 无参数, 有返回值
// Func<T, TResult>    = 1个参数, 有返回值
// Func<T1,T2,TResult> = 2个参数, 有返回值
//
// 对比:
//   自己写: delegate void HealthChangedHandler(float hp, float max);
//   用内置: Action<float, float>  ← 一模一样! 省一行声明
// ============================================================


// ============================================================
// Part 1: 玩家类 — 发出事件的一方 (事件源 / Publisher)
//
// 知识点 4: event 关键字 — 给委托加上"安全锁"
//
// 普通委托的问题:
//   public Action OnDie;  ← 外部代码可以写 OnDie = null; (清空所有订阅!)
//                          ← 外部代码可以写 OnDie(); (外部也能触发!)
//
// event 委托的好处:
//   public event Action OnDie;  ← 外部只能用 += 订阅 / -= 取消
//                               ← 外部不能直接调用 (只有本类能 Invoke)
//                               ← 外部不能赋值为 null (更安全)
//
// 规则: 事件的触发权属于声明它的类, 订阅权属于所有人
// ============================================================
public class Player
{
    // ----- 属性 -----
    public string Name { get; private set; }
    public float MaxHealth { get; private set; }
    public float CurrentHealth { get; private set; }

    // ----- 事件声明 -----
    // 知识点 5: 事件命名惯例 — 用"发生了什么事"命名
    // On/Before + 名词 + 动词过去式/现在式
    //
    // 游戏中常见的事件命名:
    //   OnDamaged     — 受伤时
    //   OnHealed      — 回血时
    //   OnDied        — 死亡时
    //   OnJumped      — 跳跃时
    //   OnItemPicked  — 捡到物品时
    //   OnLevelUp     — 升级时
    //   OnHealthChanged — 血量变化时 (你看的 Player_Controller 里就有类似事件!)

    // 用自定义委托 — 语义更清晰, 看名字就知道是血量变化
    public event HealthChangedHandler OnHealthChanged;

    // 用内置 Action — 简单省事, 不需要单独声明委托类型
    public event Action OnDied;
    public event Action<float> OnDamaged;       // float = 伤害值
    public event Action<float> OnHealed;        // float = 回复值

    // 知识点 6: 也支持 Func (有返回值的事件), 但游戏里很少用
    // Func<bool> 意思是: 返回 bool 的方法
    public event Func<bool> OnBeforeDie;  // 死前检查: 返回 false 可阻止死亡

    public Player(string name, float maxHealth)
    {
        Name = name;
        MaxHealth = maxHealth;
        CurrentHealth = maxHealth;
    }

    // ----- 受伤 -----
    public void TakeDamage(float damage)
    {
        if (CurrentHealth <= 0) return;  // 已经死了, 不能再受伤

        CurrentHealth -= damage;
        OnDamaged?.Invoke(damage);                    // 通知: 受伤了!
        OnHealthChanged?.Invoke(CurrentHealth, MaxHealth); // 通知: 血量变了!

        Debug.Log($"[Player] {Name} 受到 {damage} 点伤害, 剩余血量 {CurrentHealth}/{MaxHealth}");

        if (CurrentHealth <= 0)
        {
            CurrentHealth = 0;
            Die();
        }
    }

    // ----- 回血 -----
    public void Heal(float amount)
    {
        if (CurrentHealth <= 0) return;

        CurrentHealth = Mathf.Min(CurrentHealth + amount, MaxHealth);
        OnHealed?.Invoke(amount);                      // 通知: 回血了!
        OnHealthChanged?.Invoke(CurrentHealth, MaxHealth); // 通知: 血量变了!

        Debug.Log($"[Player] {Name} 回复 {amount} 点生命, 当前血量 {CurrentHealth}/{MaxHealth}");
    }

    // ----- 死亡 -----
    private void Die()
    {
        // 知识点 7: ?.Invoke() — 安全调用
        // 如果事件没有任何订阅者 (null), 跳过不报错
        // 等价于: if (OnBeforeDie != null) { ... }

        // 死前检查: 如果有人订阅了 OnBeforeDie 并返回 false, 阻止死亡
        bool? canDie = OnBeforeDie?.Invoke();
        if (canDie.HasValue && !canDie.Value)
        {
            Debug.Log($"[Player] {Name} 被阻止死亡! (OnBeforeDie 返回 false)");
            CurrentHealth = 1;  // 留1点血
            return;
        }

        OnDied?.Invoke();  // 通知: 玩家死了!
        Debug.Log($"[Player] {Name} 已死亡!");
    }
}


// ============================================================
// Part 2: 成就系统 — 接收事件的一方 (订阅者 / Subscriber)
//
// 知识点 8: 订阅事件 — += 和 -=
//
//   订阅:   player.OnDied += ShowDeathScreen;
//   取消:   player.OnDied -= ShowDeathScreen;
//
//   就像你关注了一个UP主:
//     += 关注 → 有新视频就通知你
//     -= 取关 → 不再通知你
//
//   重要! 一定要在合适的时机取消订阅 (OnDisable/OnDestroy)
//   否则: 被销毁的对象还订阅着事件 → 调用时 NullReferenceException!
// ============================================================
public class AchievementSystem
{
    private int _killCount = 0;
    private float _totalDamageDealt = 0f;

    // 知识点 9: 订阅方法签名必须和事件委托匹配!
    //
    // 事件: event Action<float> OnDamaged;    ← 需要 void 方法(float 参数)
    // 匹配: void TrackDamage(float damage)    ← void 方法(float 参数) ✓
    //
    // 事件: event Action OnDied;               ← 需要 void 方法(无参数)
    // 匹配: void OnPlayerKilled()             ← void 方法(无参数)  ✓
    //
    // 事件: event Func<bool> OnBeforeDie;      ← 需要 bool 方法(无参数)
    // 匹配: bool PreventDeath()               ← bool 方法(无参数)  ✓

    public void TrackDamage(float damage)
    {
        _totalDamageDealt += damage;
    }

    public void OnPlayerKilled()
    {
        _killCount++;
        Debug.Log($"[成就] 击杀数: {_killCount}, 累计伤害: {_totalDamageDealt}");

        // 检查成就
        if (_killCount >= 3)
            Debug.Log("[成就解锁] 🏆 三杀! — 击杀3个敌人");
        if (_totalDamageDealt >= 200)
            Debug.Log("[成就解锁] 🏆 重拳出击! — 累计造成200点伤害");
    }

    public bool PreventDeath()
    {
        // 例如: 如果有复活道具, 返回 false 阻止死亡
        // 这里简单演示, 永远允许死亡
        return true;
    }
}


// ============================================================
// Part 3: UI 系统 — 也来订阅事件!
// 知识点 10: 一个事件可以被多个订阅者同时订阅
//
// 当玩家受伤时:
//   → AchievementSystem.TrackDamage() 被调用 (记录数据)
//   → HealthBarUI.UpdateHealthBar()   被调用 (刷新血条)
//   → ScreenShakeEffect.Shake()       被调用 (屏幕震动)
//
// 这三者完全不认识彼此, 只通过事件通信 → 解耦!
// ============================================================
public class HealthBarUI
{
    public void UpdateHealthBar(float current, float max)
    {
        float percent = current / max;
        string bar = new string('█', Mathf.RoundToInt(percent * 20));
        string empty = new string('░', 20 - Mathf.RoundToInt(percent * 20));
        Debug.Log($"[血条UI] [{bar}{empty}] {current}/{max} ({percent:P0})");
    }

    public void OnPlayerDied()
    {
        Debug.Log("[血条UI] 显示死亡画面...");
    }
}


// ============================================================
// Part 4: Unity 演示入口
// ============================================================
public class DelegatesAndEventsDemo : MonoBehaviour
{
    void Start()
    {
        Debug.Log("========== Day 17: C# 委托与事件 ==========\n");

        // ============================================================
        // Section 1: 基础委托 — 把方法当变量用
        // ============================================================
        Demo_BasicDelegate();

        // ============================================================
        // Section 2: Action 和 Func — 内置委托
        // ============================================================
        Demo_ActionAndFunc();

        // ============================================================
        // Section 3: 事件驱动实战 — 玩家 + 成就 + UI
        // ============================================================
        Demo_EventSystem();

        // ============================================================
        // Section 4: 回顾你的代码 — Player_Controller 里的事件
        // ============================================================
        Demo_RealWorldConnection();
    }

    // ============================================================
    // Section 1: 基础委托
    //
    // 知识点 11: 委托 = 方法变量
    //
    //   普通变量: int hp = 100;        // hp 存数字
    //   委托变量: Action callback = Foo; // callback 存方法!
    //
    //   然后可以像调用方法一样调用委托:
    //   callback();  ← 执行 Foo()
    // ============================================================
    void Demo_BasicDelegate()
    {
        Debug.Log("--- Section 1: 基础委托 — 把方法当参数传递 ---");

        // ----- 知识点 11.1: 委托变量赋值 -----
        // 这里 Action<string> 是一个委托类型, 可以存"接收string, 返回void"的方法
        Action<string> printMethod;

        // 把方法赋值给委托变量 (就像 int x = 5;)
        printMethod = SimplePrint;
        printMethod("你好, 委托!");  // ← 和调用方法一模一样!

        // ----- 知识点 11.2: 委托可以重新赋值 -----
        printMethod = FancyPrint;
        printMethod("你好, 又是委托!"); // ← 这次调用不同的方法!

        // ----- 知识点 11.3: 多播委托 — 一个委托绑多个方法 -----
        // 用 += 添加、-= 移除
        printMethod = null;
        printMethod += SimplePrint;
        printMethod += FancyPrint;
        printMethod += SimplePrint;   // 甚至可以同一个方法加两次
        printMethod("多播!");         // ← 3个方法依次执行!
        Debug.Log("(上面一行触发了3次打印 — 因为绑了3个方法)\n");
    }

    void SimplePrint(string msg)
    {
        Debug.Log("  [简单模式] " + msg);
    }

    void FancyPrint(string msg)
    {
        Debug.Log("  ★ [花哨模式] " + msg + " ★");
    }

    // ============================================================
    // Section 2: Action 和 Func
    //
    // 知识点 12: 内置委托速查表
    //
    //   需求                        用什么
    //   ─────────────────────────────────────────
    //   无参无返                     Action
    //   1个参数无返                  Action<T>
    //   2个参数无返                  Action<T1,T2>
    //   无参有返                     Func<TResult>
    //   1个参数有返                  Func<T, TResult>
    //   2个参数有返                  Func<T1,T2,TResult>
    //
    //   最后一个泛型参数永远是返回值类型!
    //   Func<int, string, bool> = 接收 int 和 string, 返回 bool
    // ============================================================
    void Demo_ActionAndFunc()
    {
        Debug.Log("--- Section 2: Action 和 Func — C# 内置委托 ---");

        // ----- Action: 无返回值 -----
        Action simpleAction = () => Debug.Log("  Action: 无参无返");
        simpleAction();

        Action<int, string> complexAction = (num, text) =>
            Debug.Log($"  Action: 收到数字 {num} 和文字 \"{text}\"");
        complexAction(42, "宇宙的答案");

        // ----- Func: 有返回值 (最后一个泛型参数是返回类型!) -----
        Func<int, int, int> add = (a, b) => a + b;  // ← 注意! 第3个int是返回类型
        int result = add(3, 7);
        Debug.Log($"  Func: 3 + 7 = {result}");

        Func<string, bool> isLong = s => s.Length > 5;  // string进 → bool出
        Debug.Log($"  Func: \"Hello\" 长度>5? {isLong("Hello")}");
        Debug.Log($"  Func: \"HelloWorld\" 长度>5? {isLong("HelloWorld")}\n");
    }

    // ============================================================
    // Section 3: 事件驱动实战
    //
    // 知识点 13: 事件完整流程
    //
    //   ① 声明事件    → public event Action OnDied;
    //   ② 触发事件    → OnDied?.Invoke();
    //   ③ 订阅事件    → player.OnDied += MyHandler;
    //   ④ 处理事件    → void MyHandler() { ... }
    //   ⑤ 取消订阅    → player.OnDied -= MyHandler;  (非常重要!)
    //
    //   注意: event 关键字让外部只能 += 和 -=, 不能直接调用!
    //   这就是 event 和普通 delegate 变量最核心的区别
    // ============================================================
    void Demo_EventSystem()
    {
        Debug.Log("--- Section 3: 事件驱动实战 — 玩家受伤→成就+UI响应 ---");

        // ----- 创建玩家和订阅者 -----
        Player hero = new Player("勇者", 100f);
        AchievementSystem achievements = new AchievementSystem();
        HealthBarUI healthBar = new HealthBarUI();

        // ----- 知识点 13.1: += 订阅事件 -----
        // 多个系统订阅同一个事件, 互不依赖!
        hero.OnDamaged += achievements.TrackDamage;     // 成就系统: 记录伤害
        hero.OnHealed += amount => Debug.Log($"[音效] 播放回血音效 (回了{amount}点)");  // Lambda也OK!
        hero.OnHealthChanged += healthBar.UpdateHealthBar; // UI系统: 刷新血条
        hero.OnDied += achievements.OnPlayerKilled;     // 成就系统: 记录击杀
        hero.OnDied += healthBar.OnPlayerDied;          // UI系统: 死亡画面
        hero.OnBeforeDie += achievements.PreventDeath;  // 死前检查

        // ----- 知识点 13.2: 触发→订阅者自动响应 -----
        hero.TakeDamage(30);   // → 触发 OnDamaged + OnHealthChanged
        hero.TakeDamage(25);   // → 再次触发
        hero.Heal(15);         // → 触发 OnHealed + OnHealthChanged
        hero.TakeDamage(60);   // → 血量归零! 触发 OnBeforeDie + OnDied

        // ----- 知识点 13.3: -= 取消订阅 (防止内存泄漏!) -----
        hero.OnDamaged -= achievements.TrackDamage;
        hero.OnHealthChanged -= healthBar.UpdateHealthBar;
        hero.OnDied -= achievements.OnPlayerKilled;
        hero.OnDied -= healthBar.OnPlayerDied;
        hero.OnBeforeDie -= achievements.PreventDeath;
        Debug.Log("[清理] 所有事件订阅已取消\n");
    }

    // ============================================================
    // Section 4: 回顾你的项目代码
    //
    // 知识点 14: 你的 Player_Controller 和 Player_Animator 就用到了今天学的一切!
    //
    // 在 Player_Controller.cs 里:
    //   public event Action<bool, float> GroundedChanged;  ← 声明事件
    //   public event Action Jumped;                        ← 声明事件
    //   GroundedChanged?.Invoke(true, ...);                ← 触发事件
    //
    // 在 Player_Animator.cs 里:
    //   _player.Jumped += OnJumped;                 ← 订阅事件 (OnEnable)
    //   _player.GroundedChanged += OnGroundedChanged; ← 订阅事件 (OnEnable)
    //   _player.Jumped -= OnJumped;                 ← 取消订阅 (OnDisable)
    //   _player.GroundedChanged -= OnGroundedChanged; ← 取消订阅 (OnDisable)
    //
    // 这就是事件驱动架构! Controller 只管物理+输入,
    // Animator 只管动画+特效, 两者通过事件通信, 完全解耦!
    // ============================================================
    void Demo_RealWorldConnection()
    {
        Debug.Log("--- Section 4: 回顾你的项目代码 ---");
        Debug.Log("你的 Player_Controller.cs 就是一个事件源 (Publisher):");
        Debug.Log("  public event Action<bool, float> GroundedChanged;");
        Debug.Log("  public event Action Jumped;");
        Debug.Log("");
        Debug.Log("你的 Player_Animator.cs 就是一个订阅者 (Subscriber):");
        Debug.Log("  _player.Jumped += OnJumped;           ← 订阅跳跃事件");
        Debug.Log("  _player.GroundedChanged += OnGroundedChanged; ← 订阅落地事件");
        Debug.Log("  _player.Jumped -= OnJumped;           ← OnDisable里取消订阅");
        Debug.Log("");
        Debug.Log("这就是今天学的委托和事件! 你已经用上了, 只是今天才认识它们的名字 :)");
    }
}

// ============================================================
// 📌 今日速查表 (Cheat Sheet)
//
//   需求                               代码
//   ───────────────────────────────────────────────────────────
//   声明自定义委托                      delegate void MyHandler(int x);
//   声明事件(无参)                      public event Action OnDied;
//   声明事件(有参)                      public event Action<float> OnDamaged;
//   声明事件(有返回值)                  public event Func<bool> OnCheck;
//   触发事件                           OnDied?.Invoke();
//   订阅事件                           obj.OnDied += MyMethod;
//   取消订阅                           obj.OnDied -= MyMethod;
//   Lambda订阅                         obj.OnDied += () => Debug.Log("死了");
//   安全检查后触发                     OnDamaged?.Invoke(10f);
//
// ============================================================

// ============================================================
// 🤔 常见问题 FAQ
//
// Q1: 委托和事件有什么区别?
// A1: 委托是类型, 事件是成员。
//     委托 = 方法模板 (像 int 是数据类型)
//     事件  = 加了安全锁的委托变量
//     没有 event 关键字: 外部可以直接 = null 清空所有订阅者!
//     有 event 关键字: 外部只能 += 和 -=, 更安全
//
// Q2: 什么时候用 Action, 什么时候用自定义委托?
// A2: 简单情况用 Action/Func (99%的情况都够用)
//     需要语义清晰时用自定义委托:
//       delegate void HealthChangedHandler(float current, float max);
//       → 一看就知道是血量变化, 比 Action<float,float> 更有表达力
//
// Q3: 为什么要在 OnDisable/OnDestroy 里 -= 取消订阅?
// A3: 如果对象销毁了但还订阅着事件, 事件触发时会访问已销毁对象
//     → NullReferenceException! 这是新手最常见的 Bug 之一
//     铁律: 在哪 +=, 就在对应的生命周期 -= (OnEnable←→OnDisable)
//
// Q4: ?.Invoke() 和 .Invoke() 有什么区别?
// A4: ?. 是空检查语法糖。
//     OnDied?.Invoke();   ← 如果 OnDied 是 null (没人订阅), 安全跳过
//     OnDied.Invoke();    ← 如果 OnDied 是 null, 直接报 NullReferenceException!
//     永远用 ?.Invoke(), 这是个好习惯!
//
// Q5: 事件和广播有什么区别?
// A5: 事件就是"精准广播" — 只有订阅了的人能收到!
//     没有订阅者 → 触发时什么也不会发生 (?.Invoke() 跳过)
//     多个订阅者 → 每个人都会收到通知 (多播)
//
// Q6: 我的 Player_Controller 里那些 event 现在能看懂了吗?
// A6: 当然!
//     public event Action<bool, float> GroundedChanged;
//     拆解开:
//       event              → 加了安全锁的委托
//       Action<bool,float> → 接收bool(是否落地)和float(冲击力), 无返回值
//       GroundedChanged    → 事件名: 落地状态变化了
//
//     GroundedChanged?.Invoke(true, MathF.Abs(_frameVelocity.y));
//     拆解开:
//       ?.Invoke           → 安全调用 (没人订阅就跳过)
//       (true, 落地冲击力)  → 传递参数: 落地=true, 冲击力=下落速度的绝对值
// ============================================================
