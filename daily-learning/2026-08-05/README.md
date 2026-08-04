# Day 17 — C# 委托与事件

> **日期:** 2026-08-05  
> **难度:** ★★☆☆☆ (进阶级)  
> **技术栈:** C# · Unity · Delegate · Action · Func · Event · 事件驱动 · 解耦

---

## 🎯 今日主题

今天学 C# 里最重要的解耦工具 — **委托与事件**。

### 📨 Delegate — 把方法当"变量"存

**委托 = 一个可以装方法的变量。** 普通变量存数据（`int hp = 100`），委托变量存方法（`Action callback = MyMethod`）。

有了委托，你可以把方法像参数一样传来传去 — "我不自己干活，我告诉你怎么干，你来干"。

### 📢 Event — 给委托加上"安全锁"

**event = 订阅/通知 模型。** 一个对象发出通知（"我受伤了！"），多个对象各自响应（成就系统记录数据、UI刷新血条、音效播放...），但它们彼此完全不知道对方的存在。

**这就是你项目里已经在用的架构！**

---

## 📖 核心知识点

| # | 知识点 | 说明 |
|---|--------|------|
| 1 | 委托是什么 | 存方法的变量 — 把方法当参数传递 |
| 2 | delegate 关键字 | 自定义委托类型 — `delegate void MyHandler(int x)` |
| 3 | Action 系列 | 内置无返回委托 — `Action` `Action<T>` `Action<T1,T2>`... |
| 4 | Func 系列 | 内置有返回委托 — `Func<TResult>` `Func<T,TResult>`... 最后一个泛型参数永远是返回类型 |
| 5 | event 关键字 | 给委托加锁 — 外部只能 `+=`/`-=`，不能直接调用或赋 null |
| 6 | ?.Invoke() | 安全触发 — 没人订阅就跳过，不报 NullReferenceException |
| 7 | += 订阅 / -= 取消 | 关注事件 / 取消关注 — 就像关注/取关 UP 主 |
| 8 | 生命周期管理 | OnEnable 里 +=，OnDisable 里 -= — 不取消会内存泄漏+报错 |
| 9 | 多播委托 | 一个委托绑多个方法，触发时依次执行 |
| 10 | Lambda 订阅 | `player.OnDied += () => Debug.Log("死了");` — 简洁方便 |
| 11 | 解耦架构 | 事件源不依赖订阅者 — Controller 不知道 Animator 的存在 |
| 12 | 回顾项目代码 | Player_Controller 的 `GroundedChanged`/`Jumped` 事件 → Player_Animator 订阅 |

---

## 🔗 与其他学习日的关系

- **Day 14 (观察者模式):** 事件就是 C# 原生的观察者模式实现 — `event` 关键字 = 语言级别的观察者
- **Day 3 (事件系统):** GameEventSystem 是全局事件总线 — 今天学的 `event` 是对象级别的点对点事件
- **Day 16 (Dictionary):** 事件总线内部用 Dictionary 存储事件名→订阅者列表
- **Day 15 (struct):** 委托的回调参数可以用 struct 减少 GC
- **Day 5 (策略模式):** 策略 = 委托的面向对象版本 — Func<T,bool> 就是一个单方法策略

---

## 💡 一句话总结

> **委托：** 方法也能当数据存，存进去的是一个方法，拿出来还能执行。  
> **事件：** "有人吗？我要通知一件事" — 谁订阅了谁响应，没订阅就安静路过。

---

## 🧪 运行方式

将 `DelegatesAndEvents.cs` 放入 Unity 项目的任意 `Scripts` 文件夹中，在场景中创建空 GameObject 并挂载 `DelegatesAndEventsDemo` 组件，运行后即可在 Console 中看到输出：

- **Section 1:** 基础委托 — 把方法赋值给变量、多播委托
- **Section 2:** Action 和 Func — C# 内置委托的用法
- **Section 3:** 事件驱动实战 — Player受伤 → 成就系统 + 血条UI 同时响应
- **Section 4:** 回顾你的项目代码 — Player_Controller 和 Player_Animator 就是事件驱动的！

---

## 🤔 延伸思考

1. **为什么 Player_Controller 用 `event` 而不是普通的 `Action`？**
   → 因为 event 阻止了外部代码"清空所有订阅者"。如果 Animator 辛苦订阅了跳跃事件，另一个脚本写了 `_controller.GroundedChanged = null`，Animator 就收不到通知了。event 关键字禁止了这种操作。

2. **为什么要在 OnDisable 里 `-=` 取消订阅？**
   → 如果你的 Animator 被 Destroy 了，但 Controller 还活着，下次跳跃时 Controller 尝试通知一个已经被销毁的 Animator → NullReferenceException。铁律：`+=` 和 `-=` 必须成对。

3. **事件和直接调用方法有什么区别？**
   → 直接调用：A 必须知道 B 的存在 → `b.DoSomething()`。事件通知：A 只知道"我有事件"，谁订阅谁响应 → 解耦。

4. **能不能在事件触发的一瞬间修改订阅列表？** (进阶)
   → 不能！这和 foreach 里修改集合一样会出问题。如果真的需要，用 `GetInvocationList()` 拿到快照再操作。

5. **静态事件 vs 实例事件？**
   → 静态事件 (`static event Action`) 属于类型本身，任何地方都能订阅 → 就是全局事件总线的基础。实例事件属于具体对象 → 只能订阅某个对象的。

---

## 🏷️ 标签

`C#基础` `委托` `Delegate` `Action` `Func` `事件` `Event` `事件驱动` `解耦` `观察者模式` `订阅通知` `Unity` `进阶级`
