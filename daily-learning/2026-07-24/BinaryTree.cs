// ============================================================
// 每日脚本学习 Day 15 — 2026-07-24
// 主题: 树形结构入门 — 二叉树 (Binary Tree)
// 适用: Unity 游戏开发 · 对话系统 · 技能树 · AI决策 · 场景层级
// 难度: ★★☆☆☆ (入门级)
// ============================================================

using System;
using System.Collections.Generic;
using UnityEngine;

// ============================================================
// 知识点 1: 什么是树? — 生活中的树形结构
//
// 树 = 一个根节点, 下面分叉出子节点, 子节点再分叉...
//
// 现实类比:
//   🏢 公司组织架构: CEO → 总监 → 经理 → 员工
//   📁 电脑文件夹:   C盘 → 用户 → 文档 → 图片
//   🎮 游戏技能树:   基础攻击 → 强化攻击 → 终极技能
//   📱 微信朋友圈:   你发的帖子 → 评论 → 回复 → 回复的回复
//
// 树的术语 (记住这 5 个就够了):
//   🌳 根节点 (Root)    — 最顶层的那个节点, 一棵树只有一个根
//   🍃 叶子节点 (Leaf)  — 没有子节点的节点 (最末端的)
//   👆 父节点 (Parent)  — 上层节点
//   👇 子节点 (Child)   — 下层节点
//   📏 深度 (Depth)     — 从根到某个节点经过的边数
//
// 例如这棵树:
//        A          ← 根节点 (Root)
//       / \
//      B   C        ← B和C是A的子节点, A是B和C的父节点
//     / \   \
//    D   E   F      ← D,E,F是叶子节点 (Leaf)
//
// ============================================================

// ============================================================
// Part 1: 二叉树节点 — 树的最小单元
// 知识点 2: 二叉树 = 每个节点最多有 2 个子节点 (左和右)
// ============================================================

/// <summary>
/// 二叉树节点类
/// 每个节点包含: 自己的数据 + 左子节点引用 + 右子节点引用
/// 就像文件夹: 文件夹有自己的名字, 里面可以有左子文件夹和右子文件夹
/// </summary>
/// <typeparam name="T">节点存储的数据类型 (int, string, GameObject...都可以)</typeparam>
public class BinaryTreeNode<T>
{
    /// <summary>节点存储的数据</summary>
    public T Data;

    /// <summary>左子节点 (可以理解为"左边的分支")</summary>
    public BinaryTreeNode<T> Left;

    /// <summary>右子节点 (可以理解为"右边的分支")</summary>
    public BinaryTreeNode<T> Right;

    /// <summary>
    /// 构造函数 — 创建一个新节点
    /// </summary>
    public BinaryTreeNode(T data)
    {
        Data = data;
        Left = null;   // 刚创建时没有子节点
        Right = null;
    }
}

// ============================================================
// Part 2: 二叉树类 — 管理整棵树
// 知识点 3: 树只需要记住根节点, 从根出发可以找到所有节点
// ============================================================

/// <summary>
/// 二叉树 — 提供插入、遍历等基础操作
/// </summary>
public class BinaryTree<T> where T : IComparable<T>
{
    /// <summary>根节点 — 树的入口</summary>
    public BinaryTreeNode<T> Root;

    // ============================================================
    // 2.1 插入节点 (二叉搜索树规则)
    // 知识点 4: 二叉搜索树 (BST) 规则 —
    //   左边放比当前小的, 右边放比当前大的
    //
    // 类比: 图书馆书架 — 比当前书编号小的放左边, 大的放右边
    //       这样找书时不用从头翻到尾, 每次都可以排除一半!
    // ============================================================

    /// <summary>
    /// 插入一个新数据到树中
    /// </summary>
    public void Insert(T data)
    {
        Root = InsertRecursive(Root, data);
    }

    /// <summary>
    /// 递归插入 — 树的经典操作模式: 递归!
    /// 知识点 5: 树和递归是好朋友 —
    ///   "把大树的问题拆成小树的问题, 直到最简单的情况"
    ///
    /// 插入逻辑:
    ///   1. 如果当前位置是空的 → 直接放这里
    ///   2. 如果新数据比当前节点小 → 去左边找位置
    ///   3. 如果新数据比当前节点大 → 去右边找位置
    /// </summary>
    private BinaryTreeNode<T> InsertRecursive(BinaryTreeNode<T> node, T data)
    {
        // 递归终止条件: 找到空位了, 就在这里创建新节点
        if (node == null)
        {
            return new BinaryTreeNode<T>(data);
        }

        // CompareTo: 比较大小
        //   < 0  → data 小于 node.Data → 去左边
        //   > 0  → data 大于 node.Data → 去右边
        //   == 0 → 相等 (这里简单跳过重复值)
        int compareResult = data.CompareTo(node.Data);

        if (compareResult < 0)
        {
            // 新数据比较小, 放到左子树
            node.Left = InsertRecursive(node.Left, data);
        }
        else if (compareResult > 0)
        {
            // 新数据比较大, 放到右子树
            node.Right = InsertRecursive(node.Right, data);
        }
        // 相等则忽略 (不插入重复值)

        return node;
    }

    // ============================================================
    // 2.2 查找节点
    // 知识点 6: 在 BST 中查找非常快!
    //   每次比较后排除一半的数据 → O(log n) 时间复杂度
    //   100万个数据, 最多只需要比较 20 次!
    // ============================================================

    /// <summary>
    /// 在树中搜索指定数据
    /// </summary>
    /// <returns>找到返回 true, 否则返回 false</returns>
    public bool Search(T data)
    {
        return SearchRecursive(Root, data);
    }

    private bool SearchRecursive(BinaryTreeNode<T> node, T data)
    {
        // 找到空节点了, 说明没有这个数据
        if (node == null) return false;

        // 找到了!
        if (data.CompareTo(node.Data) == 0) return true;

        // 比当前小 → 去左边找
        if (data.CompareTo(node.Data) < 0)
            return SearchRecursive(node.Left, data);

        // 比当前大 → 去右边找
        return SearchRecursive(node.Right, data);
    }

    // ============================================================
    // Part 3: 树的三种遍历方式 — 访问所有节点的顺序
    // 知识点 7: 遍历 = 按某种顺序"拜访"树中的每个节点
    //
    // 三种遍历的区别 → 只看"访问自己"的时机:
    //   🌿 前序 (Preorder) : 自己 → 左 → 右  (先处理自己)
    //   🌿 中序 (Inorder)  : 左 → 自己 → 右  (中间处理自己)
    //   🌿 后序 (Postorder): 左 → 右 → 自己  (最后处理自己)
    //
    // 记忆口诀:
    //   "前中后"说的是"自己"在哪个位置
    //   左永远在右之前
    //
    // 例如这棵树:    5
    //               / \
    //              3   8
    //             / \   \
    //            1   4   9
    //
    // 前序: 5 → 3 → 1 → 4 → 8 → 9  (先处理根, 从上往下)
    // 中序: 1 → 3 → 4 → 5 → 8 → 9  (从小到大! BST中序 = 排序)
    // 后序: 1 → 4 → 3 → 9 → 8 → 5  (先处理孩子, 从下往上)
    // ============================================================

    // ---------- 前序遍历: 根 → 左 → 右 ----------
    // 游戏应用: 保存场景树 (先存父节点, 再存子节点)

    /// <summary>
    /// 前序遍历 (Preorder) — 自己 → 左子树 → 右子树
    /// </summary>
    public List<T> PreorderTraversal()
    {
        List<T> result = new List<T>();
        PreorderRecursive(Root, result);
        return result;
    }

    private void PreorderRecursive(BinaryTreeNode<T> node, List<T> result)
    {
        if (node == null) return;

        result.Add(node.Data);                     // 1. 访问自己
        PreorderRecursive(node.Left, result);      // 2. 遍历左子树
        PreorderRecursive(node.Right, result);     // 3. 遍历右子树
    }

    // ---------- 中序遍历: 左 → 根 → 右 ----------
    // 游戏应用: 按顺序显示排行榜 (从小到大)

    /// <summary>
    /// 中序遍历 (Inorder) — 左子树 → 自己 → 右子树
    /// 在 BST 中, 中序遍历 = 从小到大排序!
    /// </summary>
    public List<T> InorderTraversal()
    {
        List<T> result = new List<T>();
        InorderRecursive(Root, result);
        return result;
    }

    private void InorderRecursive(BinaryTreeNode<T> node, List<T> result)
    {
        if (node == null) return;

        InorderRecursive(node.Left, result);       // 1. 遍历左子树
        result.Add(node.Data);                     // 2. 访问自己
        InorderRecursive(node.Right, result);      // 3. 遍历右子树
    }

    // ---------- 后序遍历: 左 → 右 → 根 ----------
    // 游戏应用: 删除场景树 (先删子节点, 再删父节点, 防止"删了父节点找不到子节点")

    /// <summary>
    /// 后序遍历 (Postorder) — 左子树 → 右子树 → 自己
    /// </summary>
    public List<T> PostorderTraversal()
    {
        List<T> result = new List<T>();
        PostorderRecursive(Root, result);
        return result;
    }

    private void PostorderRecursive(BinaryTreeNode<T> node, List<T> result)
    {
        if (node == null) return;

        PostorderRecursive(node.Left, result);     // 1. 遍历左子树
        PostorderRecursive(node.Right, result);    // 2. 遍历右子树
        result.Add(node.Data);                     // 3. 访问自己
    }

    // ============================================================
    // 2.4 获取树的信息
    // ============================================================

    /// <summary>
    /// 获取树的节点总数
    /// </summary>
    public int GetNodeCount()
    {
        return CountRecursive(Root);
    }

    private int CountRecursive(BinaryTreeNode<T> node)
    {
        if (node == null) return 0;
        // 当前节点(1) + 左子树的节点数 + 右子树的节点数
        return 1 + CountRecursive(node.Left) + CountRecursive(node.Right);
    }

    /// <summary>
    /// 获取树的高度 (从根到最深叶子的距离)
    /// </summary>
    public int GetHeight()
    {
        return HeightRecursive(Root);
    }

    private int HeightRecursive(BinaryTreeNode<T> node)
    {
        if (node == null) return 0;
        // 树的高度 = 左右子树中较高的那个 + 1
        return 1 + Mathf.Max(HeightRecursive(node.Left), HeightRecursive(node.Right));
    }

    // ============================================================
    // 2.5 打印树的结构 (可视化)
    // ============================================================

    /// <summary>
    /// 打印树的结构 (用缩进表示层级)
    /// </summary>
    public void PrintTree()
    {
        PrintRecursive(Root, "", true);
    }

    private void PrintRecursive(BinaryTreeNode<T> node, string indent, bool isLast)
    {
        if (node == null) return;

        // 先打印右子树 (这样打印出来右子树在上面, 像一棵横过来的树)
        PrintRecursive(node.Right, indent + (isLast ? "    " : "│   "), false);

        // 打印当前节点
        Debug.Log(indent + (isLast ? "└── " : "├── ") + node.Data);

        // 再打印左子树
        PrintRecursive(node.Left, indent + (isLast ? "    " : "│   "), true);
    }
}

// ============================================================
// Part 4: 游戏实战 — 技能树系统 (简易版)
// 知识点 8: 技能树本质就是一棵树!
//
// 例如 RPG 游戏中的技能树:
//          基础攻击 (Lv1)
//         /          \
//    强化拳击(Lv2)   火焰拳(Lv2)
//       /    \           \
//   铁拳(Lv3) 冰拳(Lv3)  陨石拳(Lv3)
//
// 每个技能有: 名字, 等级, 是否解锁, 前置技能(父节点)
// ============================================================

/// <summary>
/// 技能节点 — 存储技能信息
/// </summary>
[System.Serializable]
public class SkillNode
{
    public string SkillName;    // 技能名称
    public int Level;           // 技能等级
    public bool IsUnlocked;     // 是否已解锁

    public SkillNode(string name, int level)
    {
        SkillName = name;
        Level = level;
        IsUnlocked = false;
    }
}

/// <summary>
/// 简易技能树 — 基于二叉树结构
/// 知识点 9: 把通用二叉树改成具体应用 — 这就是"举一反三"
/// </summary>
public class SkillTree
{
    public BinaryTreeNode<SkillNode> Root;

    /// <summary>
    /// 解锁一棵技能树 (前序遍历: 从根开始, 依次向下解锁)
    /// 前序遍历适合这个场景: 先解锁父技能, 再解锁子技能
    /// </summary>
    public void UnlockAllPreorder(BinaryTreeNode<SkillNode> node)
    {
        if (node == null) return;

        // 1. 先解锁自己 (必须先解锁父技能才能学子技能)
        node.Data.IsUnlocked = true;
        Debug.Log($"🔓 解锁技能: {node.Data.SkillName} (Lv.{node.Data.Level})");

        // 2. 解锁左子树
        UnlockAllPreorder(node.Left);

        // 3. 解锁右子树
        UnlockAllPreorder(node.Right);
    }

    /// <summary>
    /// 统计已解锁的技能数量 (后序遍历: 从下往上统计)
    /// </summary>
    public int CountUnlocked(BinaryTreeNode<SkillNode> node)
    {
        if (node == null) return 0;

        // 先统计子节点
        int leftCount = CountUnlocked(node.Left);
        int rightCount = CountUnlocked(node.Right);

        // 再算上自己
        int selfCount = node.Data.IsUnlocked ? 1 : 0;

        return leftCount + rightCount + selfCount;
    }

    /// <summary>
    /// 按等级顺序列出技能 (中序遍历: 可以做到有序输出)
    /// </summary>
    public void ListByLevel(BinaryTreeNode<SkillNode> node, List<string> result)
    {
        if (node == null) return;

        ListByLevel(node.Left, result);
        result.Add($"{node.Data.SkillName} (Lv.{node.Data.Level})");
        ListByLevel(node.Right, result);
    }
}

// ============================================================
// Part 5: Unity 演示组件
// 在场景中创建空 GameObject, 挂载此脚本, 运行即可看到效果
// ============================================================

/// <summary>
/// 二叉树学习演示 — 挂载到 GameObject 上运行
/// </summary>
public class BinaryTreeDemo : MonoBehaviour
{
    void Start()
    {
        Debug.Log("═══════════════════════════════════════");
        Debug.Log("🌳 Day 15: 二叉树入门学习演示");
        Debug.Log("═══════════════════════════════════════\n");

        Demo1_BasicTree();
        Demo2_Traversals();
        Demo3_SkillTree();
    }

    /// <summary>
    /// 演示 1: 创建一棵二叉搜索树, 演示插入和查找
    /// </summary>
    void Demo1_BasicTree()
    {
        Debug.Log("━━━ 演示 1: 创建二叉搜索树 ━━━");

        // 创建树并插入数据
        BinaryTree<int> tree = new BinaryTree<int>();

        // 按这个顺序插入: 5, 2, 8, 1, 3, 7, 9
        // 建出来的树长这样:
        //       5
        //     /   \
        //    2     8
        //   / \   / \
        //  1   3 7   9
        int[] values = { 5, 2, 8, 1, 3, 7, 9 };
        foreach (int v in values)
        {
            tree.Insert(v);
            Debug.Log($"📥 插入节点: {v}");
        }

        Debug.Log($"\n📊 树的信息:");
        Debug.Log($"   节点总数: {tree.GetNodeCount()}");
        Debug.Log($"   树的高度: {tree.GetHeight()}");

        // 查找测试
        Debug.Log($"\n🔍 查找测试:");
        Debug.Log($"   找 7: {(tree.Search(7) ? "✅ 找到了!" : "❌ 没找到")}");
        Debug.Log($"   找 3: {(tree.Search(3) ? "✅ 找到了!" : "❌ 没找到")}");
        Debug.Log($"   找 10: {(tree.Search(10) ? "✅ 找到了!" : "❌ 没找到 (树里没有10)")}");

        // 打印树的结构
        Debug.Log($"\n📐 树的结构 (横过来看):");
        tree.PrintTree();
    }

    /// <summary>
    /// 演示 2: 三种遍历方式的对比
    /// </summary>
    void Demo2_Traversals()
    {
        Debug.Log("\n━━━ 演示 2: 三种遍历方式对比 ━━━");

        BinaryTree<int> tree = new BinaryTree<int>();
        int[] values = { 5, 2, 8, 1, 3, 7, 9 };
        foreach (int v in values) tree.Insert(v);

        // 前序遍历
        var preorder = tree.PreorderTraversal();
        Debug.Log($"🌿 前序 (根→左→右): [{string.Join(", ", preorder)}]");
        Debug.Log($"   💡 用途: 复制树、序列化保存");

        // 中序遍历
        var inorder = tree.InorderTraversal();
        Debug.Log($"🌿 中序 (左→根→右): [{string.Join(", ", inorder)}]");
        Debug.Log($"   💡 用途: BST 中序遍历 = 从小到大排序!");

        // 后序遍历
        var postorder = tree.PostorderTraversal();
        Debug.Log($"🌿 后序 (左→右→根): [{string.Join(", ", postorder)}]");
        Debug.Log($"   💡 用途: 删除树 (先删子节点, 再删父节点)");
    }

    /// <summary>
    /// 演示 3: 游戏技能树实战
    /// </summary>
    void Demo3_SkillTree()
    {
        Debug.Log("\n━━━ 演示 3: 游戏技能树 ━━━");

        // 手工构建一棵技能树
        //           基础攻击(Lv1)
        //          /            \
        //    强化拳击(Lv2)    火焰拳(Lv2)
        //       /    \            \
        //   铁拳(Lv3) 冰拳(Lv3)  陨石拳(Lv3)

        BinaryTreeNode<SkillNode> skillRoot = new BinaryTreeNode<SkillNode>(
            new SkillNode("基础攻击", 1));

        skillRoot.Left = new BinaryTreeNode<SkillNode>(
            new SkillNode("强化拳击", 2));
        skillRoot.Right = new BinaryTreeNode<SkillNode>(
            new SkillNode("火焰拳", 2));

        skillRoot.Left.Left = new BinaryTreeNode<SkillNode>(
            new SkillNode("铁拳", 3));
        skillRoot.Left.Right = new BinaryTreeNode<SkillNode>(
            new SkillNode("冰拳", 3));
        skillRoot.Right.Right = new BinaryTreeNode<SkillNode>(
            new SkillNode("陨石拳", 3));

        SkillTree skillTree = new SkillTree();
        skillTree.Root = skillRoot;

        // 前序遍历解锁所有技能
        Debug.Log("🔑 开始解锁技能树 (前序遍历):");
        skillTree.UnlockAllPreorder(skillTree.Root);

        // 统计解锁情况
        int unlockedCount = skillTree.CountUnlocked(skillTree.Root);
        Debug.Log($"\n📊 解锁统计: {unlockedCount} 个技能已解锁");

        // 按等级列出
        List<string> skillList = new List<string>();
        skillTree.ListByLevel(skillTree.Root, skillList);
        Debug.Log($"📋 技能列表: [{string.Join(" | ", skillList)}]");
    }
}

// ============================================================
// 📝 今日要点总结:
//
// ✅ 树 = 节点 + 边, 由根开始分叉
// ✅ 二叉树 = 每个节点最多 2 个子节点 (左和右)
// ✅ BST (二叉搜索树) = 左小右大, 查找非常快 O(log n)
// ✅ 树的很多操作"天生适合递归" — 把大问题拆成小问题
// ✅ 三种遍历: 前序(自己→左→右) 中序(左→自己→右) 后序(左→右→自己)
// ✅ 游戏中的应用: 技能树、对话树、AI行为树、场景层级...
//
// 🔗 与之前学习的关系:
//   - Day 4 (状态机): 行为树是状态机的高级版
//   - Day 9 (工厂模式): 可用树形结构组织工厂层级
//   - Day 8 (模板方法): 树遍历就是一个模板方法
// ============================================================
