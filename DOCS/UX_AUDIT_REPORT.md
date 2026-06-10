# DispatchManager UX 审查报告

> 审查人: Senior Developer (高级开发工程师)
> 审查日期: 2026-06-10
> 审查范围: 全部 Razor 页面、CSS 样式、交互逻辑、可访问性

---

## 一、项目技术栈

| 项目 | 技术选型 |
|------|---------|
| 框架 | ASP.NET Core 8.0 Blazor Server (Interactive Server) |
| UI 库 | BootstrapBlazor |
| 图标 | Font Awesome |
| 主题 | Liquid Glass (自定义玻璃态设计系统) |
| 图表 | Chart.js (CDN) |
| 布局 | 侧边栏 + 顶栏 + TabSet 标签页 |

---

## 二、项目优点

1. **完整的 Liquid Glass 设计系统** -- `liquid-glass-theme.css` (1366行) 定义了完整的 CSS 变量体系，包括颜色、间距、圆角、动画、字体，并支持暗色主题
2. **动画效果出色** -- 渐入动画、悬停提升、玻璃光泽等效果丰富且流畅，使用了 `prefers-reduced-motion` 媒体查询
3. **响应式设计有基础** -- 定义了移动端/平板端/桌面端三档断点
4. **可访问性有基础** -- 已有焦点指示器、跳过链接 CSS 定义、`prefers-reduced-motion` 支持
5. **任务日志页面** -- 搜索无结果有友好提示（`TaskLog.razor:195-203`），是做得好的范例
6. **深色主题支持** -- 通过 `.dark` 类和 `data-theme="dark"` 双重机制支持主题切换

---

## 三、导航结构分析

### 当前菜单项 (MainLayout.razor.cs:40-57)

1. 任务系统 `/DispatchClass`
2. 任务调度 `/DispatchTasks`
3. 金蝶对接 `/DispatchTasksKingDee`
4. 任务日志 `/DispatchLog`

### 问题

| # | 严重程度 | 问题 |
|---|---------|------|
| 1 | **严重** | 主页 (`/`) 不在导航菜单中，用户无法从侧边栏返回主页 |
| 2 | **中等** | 3个隐藏页面（日志仪表板、日志分析报告、性能指标）已开发但被注释掉 |
| 3 | **轻微** | 路由命名不统一 -- 有的用 `/DispatchClass`，有的用 `/DispatchLog` |

---

## 四、P0 必须修复 (5项)

### 4.1 表单缺少 Label

**影响**: 屏幕阅读器无法识别输入框用途，WCAG 2.1 AA 不合规

**文件**:
- `TaskEditor.razor:5-62` -- 所有 `BootstrapInput` 缺少 `DisplayText`
- `TaskClassEditor.razor:4-11` -- 所有 `BootstrapInput` 缺少 `DisplayText`
- `TaskKingDeeEditor.razor:5-37` -- 所有 `BootstrapInput` 缺少 `DisplayText`

**修复方案**: 为每个 `BootstrapInput` 添加 `DisplayText` 属性

```razor
<!-- 修复前 -->
<BootstrapInput @bind-Value="@Value.FNo" />

<!-- 修复后 -->
<BootstrapInput @bind-Value="@Value.FNo" DisplayText="序号" />
```

### 4.2 删除失败返回 true

**影响**: 删除操作失败时，用户看到成功提示，数据可能不一致

**文件**:
- `Tasks.razor.cs:202-205` -- catch 块返回 `Task.FromResult(true)`
- `TasksKingDee.razor.cs:211-214` -- 同上
- `TaskClass.razor.cs:146-149` -- 同上

**修复方案**: catch 块返回 `Task.FromResult(false)`

```csharp
// 修复前
catch (Exception ex) {
    LogHelperUtil.WriteError(...);
    return Task.FromResult(true);  // BUG!
}

// 修复后
catch (Exception ex) {
    LogHelperUtil.WriteError(...);
    return Task.FromResult(false);
}
```

### 4.3 HTML lang 属性错误

**影响**: 屏幕阅读器使用错误的语言引擎朗读中文内容

**文件**: `App.razor:5`

**修复方案**: `<html lang="en">` 改为 `<html lang="zh-CN">`

### 4.4 主页数据硬编码

**影响**: 仪表板显示虚假数据，误导用户

**文件**: `Index.razor:24-63` -- 统计卡片 `128`/`24`/`96`/`8` 全部是静态值

**修复方案**: 从数据库动态获取统计数据，或在未连接时显示 "暂无数据"

### 4.5 Switch 双重切换 Bug

**影响**: 点击开关后值被反转两次，等于没有变化

**文件**:
- `TaskEditor.razor.cs:114-117` -- `OnValueChanged` 手动 `!model.IsLog`
- `TaskEditor.razor.cs:119-122` -- `OnFISRetryChanged` 手动 `!model.FISRetry`
- `TaskKingDeeEditor.razor.cs:88-91` -- 同上

**原因**: `Switch` 组件的 `ValueChanged` 回调在值已经切换之后触发，手动取反导致双重切换

**修复方案**: 移除手动取反逻辑

```csharp
// 修复前
private void OnValueChanged(DispatchTask model)
{
    model.IsLog = !model.IsLog;  // BUG: Switch 已经切换了值
}

// 修复后
private void OnValueChanged(DispatchTask model)
{
    // Switch 已经自动切换了绑定值，无需手动操作
    // 只需触发 UI 更新
    InvokeAsync(StateHasChanged);
}
```

---

## 五、P1 建议修复 (10项)

### 5.1 表单缺少验证

**问题**: 编辑器未使用 `<ValidateForm>` 包裹，用户可以提交空值或无效数据

**修复方案**: 使用 BootstrapBlazor 的 `ValidateForm` 组件包裹表单

### 5.2 查询异常被静默吞掉

**问题**: `OnQueryAsync` 的 catch 块只记录日志，用户看到空表格无任何提示

**文件**: `Tasks.razor.cs:118-122`, `TasksKingDee.razor.cs:122-126`, `TaskClass.razor.cs:89-93`

**修复方案**: 添加 Toast 错误提示

```csharp
catch (Exception ex)
{
    LogHelperUtil.WriteError(...);
    toastService.Show(new ToastOption()
    {
        Category = ToastCategory.Error,
        Title = "查询异常",
        Content = "数据加载失败，请稍后重试",
    });
    return Task.FromResult(new QueryData<DispatchTask>() { });
}
```

### 5.3 删除操作无二次确认

**问题**: 删除任务/分类时没有确认对话框，误操作无法挽回

**修复方案**: 使用 BootstrapBlazor 的 `DialogService.ShowDeleteConfirm()` 或 `Swal` 组件

### 5.4 Tasks 与 TasksKingDee 代码重复

**问题**: 两个页面约 90% 代码重复（查询、保存、删除、状态格式化等逻辑完全相同）

**修复方案**: 提取基类 `TaskPageBase<T>` 或使用共享组件

### 5.5 Razor 文件内嵌业务逻辑

**问题**: `Tasks.razor:11` 内嵌约 300 字符的单行业务逻辑

**修复方案**: 将数据查询逻辑移到 code-behind 的 `OnInitialized` 或 `OnParametersSet` 中

### 5.6 移动端侧边栏隐藏但无汉堡菜单

**问题**: `liquid-glass-theme.css:826-828` 在移动端将侧边栏隐藏（`transform: translateX(-100%)`），但没有提供汉堡菜单按钮让用户重新打开

**修复方案**: 添加移动端汉堡菜单按钮，使用 BootstrapBlazor 的 `Drawer` 组件或自定义 toggle 逻辑

### 5.7 Skip Link 未实现

**问题**: CSS 中定义了 `.skip-link` 样式（`liquid-glass-theme.css:862-875`），但 HTML 中没有对应的元素

**修复方案**: 在 `App.razor` 的 `<body>` 开头添加

```html
<a class="skip-link" href="#main-content">跳转到主要内容</a>
```

### 5.8 Error.razor 全英文

**问题**: 错误页面全部英文，与中文界面不匹配

**文件**: `Error.razor:1-16`

**修复方案**: 中文化错误页面内容

### 5.9 暂停操作仅修改内存

**问题**: `OnPause` 方法只修改内存中的任务状态，不持久化到数据库，重启后状态丢失

**文件**: `Tasks.razor.cs:269-279`, `TasksKingDee.razor.cs:278-288`

**修复方案**: 参考 `OnDisable` 和 `OnRun` 方法，添加数据库持久化

### 5.10 "导出Excel" 和 "导出CSV" 功能未区分

**问题**: `TaskLog.razor:58-63` 有两个导出按钮，但实际功能可能生成相同格式的文件

**修复方案**: 确保两个按钮分别生成 `.xlsx` 和 `.csv` 格式

---

## 六、P2 可以改进 (8项)

### 6.1 导航菜单顺序和命名

**建议**: 将 "任务日志" 移到最后，主页应加入导航菜单

### 6.2 隐藏页面处理

**建议**: 启用已开发的页面（日志仪表板、日志分析报告、性能指标），或彻底移除代码

### 6.3 CSS 重复定义

**问题**: `MyStyleSheet.css` 和 `liquid-glass-theme.css` 中存在重复的 `.stat-card`、`.stat-card-item` 定义

**建议**: 统一使用 `liquid-glass-theme.css` 中的定义，清理重复代码

### 6.4 空状态引导

**问题**: 表格无数据时只显示空表格，没有友好的空状态提示

**建议**: 使用 BootstrapBlazor 的 `Empty` 组件或自定义空状态模板

### 6.5 骨架屏/加载状态

**问题**: 数据加载时没有骨架屏或加载指示器

**建议**: 使用 BootstrapBlazor 的 `Skeleton` 组件

### 6.6 颜色选择器中文化

**问题**: `TaskClassEditor.razor.cs:35-45` 颜色选项使用英文（"Primary"、"Success" 等）

**修复方案**: 替换为中文

```csharp
new(Color.Primary.ToString(), "主色"),
new(Color.Success.ToString(), "成功绿"),
new(Color.Danger.ToString(), "危险红"),
new(Color.Warning.ToString(), "警告黄"),
new(Color.Info.ToString(), "信息蓝"),
```

### 6.7 NotFound 页面中文化

**问题**: `MainLayout.razor:45` 的 404 提示是英文 "Sorry, there's nothing at this address."

**修复方案**: 改为中文

### 6.8 blazor-error-ui 中文化

**问题**: `MainLayout.razor:126-129` 的错误 UI 全部英文

**修复方案**: 中文化错误提示

---

## 七、UX 流程分析

### 7.1 用户首次访问流程

```
主页(/) → 看到硬编码的统计数据 → 点击功能卡片 → 进入具体页面
```

**问题**: 
- 主页不在导航菜单中，用户无法快速返回
- 统计数据是假的，建立错误的心智模型

### 7.2 创建任务流程

```
任务调度页面 → 点击"新建" → 弹出编辑器 → 填写表单 → 保存
```

**问题**:
- 表单没有 Label，用户不知道每个字段的含义
- 没有表单验证，可以提交空值
- Cron 表达式需要手动输入或通过外部网站获取

### 7.3 删除任务流程

```
任务调度页面 → 点击"删除" → 直接删除（无确认）
```

**问题**:
- 没有二次确认，误操作无法挽回
- 删除失败时显示成功（P0 bug）

---

## 八、可访问性审查

| 检查项 | 状态 | 说明 |
|--------|------|------|
| HTML lang 属性 | ❌ | `lang="en"` 应为 `lang="zh-CN"` |
| 表单 Label | ❌ | 所有输入框缺少 `DisplayText` |
| 颜色对比度 | ⚠️ | 部分 `text-muted` 在浅色背景上对比度不足 |
| 键盘导航 | ⚠️ | Switch 组件可通过键盘操作，但表格行操作按钮未测试 |
| 焦点指示器 | ✅ | `liquid-glass-theme.css` 中有 `.glass-focusable:focus-visible` |
| 跳过链接 | ❌ | CSS 已定义但 HTML 未实现 |
| 减少动画 | ✅ | `prefers-reduced-motion: reduce` 已实现 |
| 触摸目标 | ⚠️ | 移动端按钮尺寸未统一验证 (应 >= 44px) |

---

## 九、性能审查

| 检查项 | 状态 | 说明 |
|--------|------|------|
| CSS 文件数量 | ⚠️ | 4个 CSS 文件可考虑合并减少请求 |
| backdrop-filter | ✅ | 有 `@supports` 降级方案 |
| 动画性能 | ✅ | 使用 `transform` 和 `opacity`，避免布局抖动 |
| 重复样式定义 | ⚠️ | 多个文件重复定义 `.stat-card` 等样式 |
| 移动端优化 | ⚠️ | 移动端隐藏侧边栏但未减少其他资源加载 |

---

## 十、修复优先级总结

```
P0 (必须修复 - 影响功能正确性):
├── 1. 删除失败返回 true          -- 3个文件
├── 2. Switch 双重切换 Bug        -- 2个文件
├── 3. HTML lang="en" → "zh-CN"  -- 1个文件
├── 4. 表单缺少 Label             -- 3个编辑器文件
└── 5. 主页数据硬编码             -- 1个文件

P1 (建议修复 - 影响用户体验):
├── 6.  删除操作无二次确认
├── 7.  查询异常静默吞掉
├── 8.  移动端缺少汉堡菜单
├── 9.  Skip Link 未实现
├── 10. Error.razor 中文化
├── 11. 暂停操作不持久化
├── 12. 表单缺少验证
├── 13. Tasks/TasksKingDee 代码重复
├── 14. Razor 内嵌业务逻辑
└── 15. 导出功能区分

P2 (可以改进 - 提升整体品质):
├── 16. 导航菜单优化
├── 17. 隐藏页面处理
├── 18. CSS 重复清理
├── 19. 空状态引导
├── 20. 骨架屏加载
├── 21. 颜色选择器中文化
├── 22. NotFound 中文化
└── 23. blazor-error-ui 中文化
```

---

**审查完成**