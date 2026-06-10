# Liquid Glass 仪表板设计示例

## 页面概述
仪表板是用户进入系统后的第一个页面，展示系统状态概览和快速操作入口。采用 Liquid Glass 设计语言，创造通透、流动、有层次的视觉体验。

## 设计目标
1. **信息层次清晰**：通过玻璃材质的透明度和阴影创造视觉层次
2. **内容可读性好**：确保文字在玻璃背景上清晰可读
3. **交互反馈及时**：通过微动画提供即时的交互反馈
4. **视觉风格统一**：与整体设计系统保持一致

---

## 页面结构

### 1. 页面容器
```html
<div class="dashboard-page">
  <!-- 背景装饰 -->
  <div class="dashboard-background"></div>
  
  <!-- 主要内容 -->
  <div class="dashboard-content">
    <!-- 页面标题区 -->
    <div class="dashboard-header">
      <h1 class="dashboard-title">调度管理仪表板</h1>
      <p class="dashboard-subtitle">系统状态概览和快速操作</p>
    </div>
    
    <!-- 统计卡片区域 -->
    <div class="dashboard-stats">
      <div class="stat-card animate-fade-in-up" style="animation-delay: 0ms;">
        <!-- 统计卡片内容 -->
      </div>
      <div class="stat-card animate-fade-in-up" style="animation-delay: 100ms;">
        <!-- 统计卡片内容 -->
      </div>
      <div class="stat-card animate-fade-in-up" style="animation-delay: 200ms;">
        <!-- 统计卡片内容 -->
      </div>
      <div class="stat-card animate-fade-in-up" style="animation-delay: 300ms;">
        <!-- 统计卡片内容 -->
      </div>
    </div>
    
    <!-- 内容区域 -->
    <div class="dashboard-main">
      <!-- 左侧：最近任务 -->
      <div class="glass-card animate-fade-in-left" style="animation-delay: 400ms;">
        <!-- 最近任务内容 -->
      </div>
      
      <!-- 右侧：快速操作 -->
      <div class="glass-card animate-fade-in-right" style="animation-delay: 500ms;">
        <!-- 快速操作内容 -->
      </div>
    </div>
  </div>
</div>
```

### 2. CSS 样式实现

```css
/* 页面容器 */
.dashboard-page {
  position: relative;
  min-height: 100vh;
  padding: var(--space-6);
  overflow: hidden;
}

/* 背景装饰 */
.dashboard-background {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: 
    radial-gradient(circle at 20% 20%, rgba(0, 122, 255, 0.08) 0%, transparent 50%),
    radial-gradient(circle at 80% 80%, rgba(52, 199, 89, 0.06) 0%, transparent 50%),
    radial-gradient(circle at 50% 50%, rgba(90, 200, 250, 0.04) 0%, transparent 50%);
  pointer-events: none;
  z-index: 0;
}

/* 主要内容 */
.dashboard-content {
  position: relative;
  z-index: 1;
  max-width: 1400px;
  margin: 0 auto;
}

/* 页面标题区 */
.dashboard-header {
  margin-bottom: var(--space-8);
  text-align: center;
}

.dashboard-title {
  font-size: var(--font-size-4xl);
  font-weight: var(--font-weight-bold);
  color: var(--color-text-primary);
  margin-bottom: var(--space-2);
  background: linear-gradient(135deg, #007AFF 0%, #5AC8FA 100%);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
  background-clip: text;
}

.dashboard-subtitle {
  font-size: var(--font-size-lg);
  color: var(--color-text-secondary);
  font-weight: var(--font-weight-medium);
}

/* 统计卡片区域 */
.dashboard-stats {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: var(--space-6);
  margin-bottom: var(--space-8);
}

/* 内容区域 */
.dashboard-main {
  display: grid;
  grid-template-columns: 2fr 1fr;
  gap: var(--space-6);
}

/* 响应式设计 */
@media (max-width: 1200px) {
  .dashboard-stats {
    grid-template-columns: repeat(2, 1fr);
  }
  
  .dashboard-main {
    grid-template-columns: 1fr;
  }
}

@media (max-width: 768px) {
  .dashboard-page {
    padding: var(--space-4);
  }
  
  .dashboard-stats {
    grid-template-columns: 1fr;
    gap: var(--space-4);
  }
  
  .dashboard-title {
    font-size: var(--font-size-2xl);
  }
  
  .dashboard-subtitle {
    font-size: var(--font-size-base);
  }
}
```

### 3. 统计卡片实现

```html
<div class="stat-card">
  <div class="stat-card__icon">
    <i class="fas fa-tasks"></i>
  </div>
  <div class="stat-card__value">128</div>
  <div class="stat-card__label">总任务数</div>
  <div class="stat-card__trend">
    <span class="trend-up">↑ 12%</span>
    <span class="trend-label">较上周</span>
  </div>
</div>
```

```css
.stat-card__trend {
  margin-top: var(--space-3);
  display: flex;
  align-items: center;
  justify-content: center;
  gap: var(--space-2);
  font-size: var(--font-size-sm);
}

.trend-up {
  color: var(--color-success);
  font-weight: var(--font-weight-semibold);
}

.trend-down {
  color: var(--color-error);
  font-weight: var(--font-weight-semibold);
}

.trend-label {
  color: var(--color-text-tertiary);
}
```

### 4. 最近任务列表

```html
<div class="recent-tasks">
  <div class="glass-card-header">
    <h3 class="glass-card-title">最近任务</h3>
    <span class="glass-badge">最近 24 小时</span>
  </div>
  
  <div class="task-list">
    <div class="task-item">
      <div class="task-item__status">
        <span class="status-dot status-dot--success"></span>
      </div>
      <div class="task-item__info">
        <div class="task-item__name">数据同步任务</div>
        <div class="task-item__time">2 分钟前</div>
      </div>
      <div class="task-item__actions">
        <button class="glass-button glass-button--sm">查看</button>
      </div>
    </div>
    
    <div class="task-item">
      <div class="task-item__status">
        <span class="status-dot status-dot--running"></span>
      </div>
      <div class="task-item__info">
        <div class="task-item__name">报表生成任务</div>
        <div class="task-item__time">运行中</div>
      </div>
      <div class="task-item__actions">
        <button class="glass-button glass-button--sm">监控</button>
      </div>
    </div>
    
    <!-- 更多任务项... -->
  </div>
</div>
```

```css
.recent-tasks {
  height: 100%;
  display: flex;
  flex-direction: column;
}

.glass-card-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: var(--space-5);
  padding-bottom: var(--space-4);
  border-bottom: 1px solid var(--glass-border);
}

.glass-card-title {
  font-size: var(--font-size-lg);
  font-weight: var(--font-weight-semibold);
  color: var(--color-text-primary);
}

.task-list {
  flex: 1;
  overflow-y: auto;
}

.task-item {
  display: flex;
  align-items: center;
  gap: var(--space-4);
  padding: var(--space-4);
  border-radius: var(--radius-lg);
  transition: background-color var(--duration-fast) var(--ease-default);
  margin-bottom: var(--space-2);
}

.task-item:hover {
  background: var(--glass-white-light);
}

.task-item__status {
  flex-shrink: 0;
}

.task-item__info {
  flex: 1;
  min-width: 0;
}

.task-item__name {
  font-size: var(--font-size-base);
  font-weight: var(--font-weight-medium);
  color: var(--color-text-primary);
  margin-bottom: var(--space-1);
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.task-item__time {
  font-size: var(--font-size-sm);
  color: var(--color-text-tertiary);
}

.task-item__actions {
  flex-shrink: 0;
}

.glass-button--sm {
  padding: var(--space-2) var(--space-3);
  font-size: var(--font-size-sm);
}
```

### 5. 快速操作区域

```html
<div class="quick-actions">
  <div class="glass-card-header">
    <h3 class="glass-card-title">快速操作</h3>
  </div>
  
  <div class="action-grid">
    <button class="action-button">
      <div class="action-button__icon">
        <i class="fas fa-plus"></i>
      </div>
      <div class="action-button__label">新建任务</div>
    </button>
    
    <button class="action-button">
      <div class="action-button__icon">
        <i class="fas fa-play"></i>
      </div>
      <div class="action-button__label">批量运行</div>
    </button>
    
    <button class="action-button">
      <div class="action-button__icon">
        <i class="fas fa-chart-bar"></i>
      </div>
      <div class="action-button__label">查看报表</div>
    </button>
    
    <button class="action-button">
      <div class="action-button__icon">
        <i class="fas fa-cog"></i>
      </div>
      <div class="action-button__label">系统设置</div>
    </button>
  </div>
</div>
```

```css
.quick-actions {
  height: 100%;
  display: flex;
  flex-direction: column;
}

.action-grid {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: var(--space-4);
  flex: 1;
}

.action-button {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: var(--space-3);
  padding: var(--space-5);
  background: var(--glass-white-light);
  backdrop-filter: var(--glass-blur);
  -webkit-backdrop-filter: var(--glass-blur);
  border: 1px solid var(--glass-border);
  border-radius: var(--radius-xl);
  cursor: pointer;
  transition: all var(--duration-fast) var(--ease-default);
  position: relative;
  overflow: hidden;
}

.action-button::before {
  content: '';
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  height: 50%;
  background: var(--glass-highlight);
  pointer-events: none;
}

.action-button:hover {
  background: var(--glass-white);
  transform: translateY(-2px);
  box-shadow: var(--shadow-element-hover);
}

.action-button:active {
  transform: translateY(0);
}

.action-button__icon {
  width: 48px;
  height: 48px;
  background: var(--color-primary-light);
  border-radius: var(--radius-lg);
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: var(--font-size-xl);
  color: var(--color-primary);
}

.action-button__label {
  font-size: var(--font-size-sm);
  font-weight: var(--font-weight-medium);
  color: var(--color-text-primary);
}
```

---

## 交互效果

### 1. 统计卡片悬停效果
```css
.stat-card {
  transition: all var(--duration-normal) var(--ease-default);
}

.stat-card:hover {
  transform: translateY(-4px);
  box-shadow: var(--shadow-glass-lg);
}

.stat-card:hover .stat-card__icon {
  transform: scale(1.1);
  transition: transform var(--duration-fast) var(--ease-spring);
}
```

### 2. 任务项悬停效果
```css
.task-item {
  transition: all var(--duration-fast) var(--ease-default);
}

.task-item:hover {
  background: var(--glass-white-light);
  transform: translateX(4px);
}

.task-item:hover .task-item__name {
  color: var(--color-primary);
}
```

### 3. 快速操作按钮点击效果
```css
.action-button {
  transition: all var(--duration-fast) var(--ease-default);
}

.action-button:hover {
  transform: translateY(-2px);
  box-shadow: var(--shadow-element-hover);
}

.action-button:active {
  transform: translateY(0) scale(0.98);
  box-shadow: var(--shadow-element);
}
```

---

## 数据绑定示例

### 1. Blazor 组件实现
```razor
@page "/dashboard"
@using DispatchManager.Components.Shared

<div class="dashboard-page">
  <div class="dashboard-background"></div>
  
  <div class="dashboard-content">
    <div class="dashboard-header">
      <h1 class="dashboard-title">调度管理仪表板</h1>
      <p class="dashboard-subtitle">系统状态概览和快速操作</p>
    </div>
    
    <div class="dashboard-stats">
      @foreach (var stat in Statistics)
      {
        <div class="stat-card animate-fade-in-up" style="animation-delay: @(stat.Delay)ms">
          <div class="stat-card__icon">
            <i class="@stat.Icon"></i>
          </div>
          <div class="stat-card__value">@stat.Value</div>
          <div class="stat-card__label">@stat.Label</div>
          @if (stat.Trend != null)
          {
            <div class="stat-card__trend">
              <span class="@(stat.Trend.IsUp ? "trend-up" : "trend-down")">
                @(stat.Trend.IsUp ? "↑" : "↓") @stat.Trend.Percentage%
              </span>
              <span class="trend-label">较上周</span>
            </div>
          }
        </div>
      }
    </div>
    
    <div class="dashboard-main">
      <div class="glass-card animate-fade-in-left" style="animation-delay: 400ms">
        <RecentTasks Tasks="RecentTasks" />
      </div>
      
      <div class="glass-card animate-fade-in-right" style="animation-delay: 500ms">
        <QuickActions />
      </div>
    </div>
  </div>
</div>

@code {
  private List<StatisticItem> Statistics { get; set; } = new()
  {
    new StatisticItem 
    { 
      Icon = "fas fa-tasks", 
      Value = "128", 
      Label = "总任务数", 
      Delay = 0,
      Trend = new TrendInfo { IsUp = true, Percentage = 12 }
    },
    new StatisticItem 
    { 
      Icon = "fas fa-play-circle", 
      Value = "24", 
      Label = "运行中", 
      Delay = 100,
      Trend = new TrendInfo { IsUp = true, Percentage = 8 }
    },
    new StatisticItem 
    { 
      Icon = "fas fa-check-circle", 
      Value = "96", 
      Label = "已完成", 
      Delay = 200,
      Trend = new TrendInfo { IsUp = false, Percentage = 3 }
    },
    new StatisticItem 
    { 
      Icon = "fas fa-exclamation-triangle", 
      Value = "8", 
      Label = "异常任务", 
      Delay = 300,
      Trend = new TrendInfo { IsUp = true, Percentage = 15 }
    }
  };
  
  private List<TaskItem> RecentTasks { get; set; } = new()
  {
    new TaskItem { Name = "数据同步任务", Status = "success", Time = "2 分钟前" },
    new TaskItem { Name = "报表生成任务", Status = "running", Time = "运行中" },
    new TaskItem { Name = "用户数据导入", Status = "success", Time = "15 分钟前" },
    new TaskItem { Name = "系统备份任务", Status = "warning", Time = "30 分钟前" },
    new TaskItem { Name = "日志清理任务", Status = "success", Time = "1 小时前" }
  };
}

<style>
  /* 页面特定样式 */
  .dashboard-page {
    position: relative;
    min-height: 100vh;
    padding: var(--space-6);
    overflow: hidden;
  }
  
  .dashboard-background {
    position: fixed;
    top: 0;
    left: 0;
    right: 0;
    bottom: 0;
    background: 
      radial-gradient(circle at 20% 20%, rgba(0, 122, 255, 0.08) 0%, transparent 50%),
      radial-gradient(circle at 80% 80%, rgba(52, 199, 89, 0.06) 0%, transparent 50%),
      radial-gradient(circle at 50% 50%, rgba(90, 200, 250, 0.04) 0%, transparent 50%);
    pointer-events: none;
    z-index: 0;
  }
  
  .dashboard-content {
    position: relative;
    z-index: 1;
    max-width: 1400px;
    margin: 0 auto;
  }
  
  /* 其他样式... */
</style>
```

---

## 设计要点总结

### 1. 视觉层次
- 使用玻璃材质的透明度和阴影创造视觉层次
- 通过背景装饰增强空间感
- 使用渐变和微动画引导用户注意力

### 2. 信息架构
- 统计卡片提供关键指标概览
- 最近任务列表展示系统活动
- 快速操作提供常用功能入口

### 3. 交互设计
- 悬停效果提供即时反馈
- 点击效果增强操作确认
- 动画过渡使界面更流畅

### 4. 响应式设计
- 桌面端：四列统计卡片，两列内容区域
- 平板端：两列统计卡片，单列内容区域
- 移动端：单列布局，优化触摸目标

### 5. 性能优化
- 使用 CSS 变量减少重复代码
- 使用 `transform` 和 `opacity` 进行动画
- 提供减少动画的选项

---

**UI Designer**: UI Designer  
**设计日期**: 2026年6月10日  
**实施状态**: 设计完成，可供开发团队参考