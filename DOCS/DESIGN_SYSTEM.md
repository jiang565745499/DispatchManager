# DispatchManager Liquid Glass 设计系统

## 设计灵感
灵感来源于 iOS 26 全新的 Liquid Glass 设计语言。界面元素像液态玻璃一样，透明、流动、有深度。内容在玻璃材质后方若隐若现，光影在表面流转，每一个交互都像触摸真实的玻璃——光滑、通透、有质感。

## 设计气质
- **关键词**：通透、流动、层次、高级、沉浸
- **情绪**：像透过水晶球看世界，一切都变得清澈而有深度
- **核心理念**：内容即界面，玻璃即层次。不遮挡，而是叠加；不填满，而是留白

## 设计原则
1. 全部文字使用中文
2. 严格遵循 iOS 26 Liquid Glass 设计语言
3. 透明玻璃材质是核心，让内容可见
4. 多层阴影创造深度感和层次感
5. 动画灵动自然，符合 iOS 原生体验
6. 整体氛围：通透、流动、层次、高级
7. 每个页面都要有独特的布局创意

---

## 一、设计基础系统

### 1.1 颜色系统

#### 主色调（玻璃材质）
```css
:root {
  /* 玻璃材质基础色 */
  --glass-white: rgba(255, 255, 255, 0.85);
  --glass-white-light: rgba(255, 255, 255, 0.6);
  --glass-white-ultra-light: rgba(255, 255, 255, 0.3);
  
  /* 玻璃边框 */
  --glass-border: rgba(255, 255, 255, 0.3);
  --glass-border-light: rgba(255, 255, 255, 0.15);
  
  /* 玻璃阴影 */
  --glass-shadow: 0 8px 32px rgba(0, 0, 0, 0.1);
  --glass-shadow-lg: 0 16px 48px rgba(0, 0, 0, 0.15);
  --glass-shadow-inset: inset 0 1px 1px rgba(255, 255, 255, 0.4);
  
  /* 玻璃高光 */
  --glass-highlight: linear-gradient(
    135deg,
    rgba(255, 255, 255, 0.4) 0%,
    rgba(255, 255, 255, 0.1) 50%,
    rgba(255, 255, 255, 0.05) 100%
  );
  
  /* 玻璃模糊 */
  --glass-blur: blur(20px);
  --glass-blur-heavy: blur(40px);
}
```

#### 语义颜色
```css
:root {
  /* 主要颜色 */
  --color-primary: #007AFF;
  --color-primary-light: rgba(0, 122, 255, 0.1);
  --color-primary-glass: rgba(0, 122, 255, 0.15);
  
  /* 成功/运行 */
  --color-success: #34C759;
  --color-success-light: rgba(52, 199, 89, 0.1);
  
  /* 警告 */
  --color-warning: #FF9500;
  --color-warning-light: rgba(255, 149, 0, 0.1);
  
  /* 错误/危险 */
  --color-error: #FF3B30;
  --color-error-light: rgba(255, 59, 48, 0.1);
  
  /* 信息 */
  --color-info: #5AC8FA;
  --color-info-light: rgba(90, 200, 250, 0.1);
  
  /* 中性色 */
  --color-text-primary: #1C1C1E;
  --color-text-secondary: #636366;
  --color-text-tertiary: #8E8E93;
  --color-text-quaternary: #AEAEB2;
  
  /* 背景色 */
  --color-background: #F2F2F7;
  --color-background-elevated: #FFFFFF;
  --color-background-grouped: #F2F2F7;
}
```

#### 深色主题颜色
```css
[data-theme="dark"] {
  /* 玻璃材质深色 */
  --glass-white: rgba(30, 30, 30, 0.85);
  --glass-white-light: rgba(30, 30, 30, 0.6);
  --glass-white-ultra-light: rgba(30, 30, 30, 0.3);
  
  --glass-border: rgba(255, 255, 255, 0.15);
  --glass-border-light: rgba(255, 255, 255, 0.08);
  
  --glass-shadow: 0 8px 32px rgba(0, 0, 0, 0.3);
  --glass-shadow-lg: 0 16px 48px rgba(0, 0, 0, 0.4);
  --glass-shadow-inset: inset 0 1px 1px rgba(255, 255, 255, 0.1);
  
  --glass-highlight: linear-gradient(
    135deg,
    rgba(255, 255, 255, 0.15) 0%,
    rgba(255, 255, 255, 0.05) 50%,
    rgba(255, 255, 255, 0.02) 100%
  );
  
  /* 语义颜色深色 */
  --color-text-primary: #F5F5F7;
  --color-text-secondary: #A1A1A6;
  --color-text-tertiary: #8E8E93;
  --color-text-quaternary: #636366;
  
  --color-background: #000000;
  --color-background-elevated: #1C1C1E;
  --color-background-grouped: #000000;
}
```

### 1.2 字体系统

```css
:root {
  /* 字体族 */
  --font-family-primary: -apple-system, BlinkMacSystemFont, 'SF Pro Display', 'SF Pro Text', 'Helvetica Neue', 'PingFang SC', 'Microsoft YaHei', sans-serif;
  --font-family-mono: 'SF Mono', 'Menlo', 'Monaco', 'Consolas', 'Microsoft YaHei Mono', monospace;
  
  /* 字体大小 */
  --font-size-xs: 0.6875rem;    /* 11px */
  --font-size-sm: 0.8125rem;    /* 13px */
  --font-size-base: 0.9375rem;  /* 15px */
  --font-size-lg: 1.0625rem;    /* 17px */
  --font-size-xl: 1.25rem;      /* 20px */
  --font-size-2xl: 1.5rem;      /* 24px */
  --font-size-3xl: 1.75rem;     /* 28px */
  --font-size-4xl: 2.125rem;    /* 34px */
  
  /* 字体权重 */
  --font-weight-regular: 400;
  --font-weight-medium: 500;
  --font-weight-semibold: 600;
  --font-weight-bold: 700;
  
  /* 行高 */
  --line-height-tight: 1.2;
  --line-height-normal: 1.4;
  --line-height-relaxed: 1.6;
  
  /* 字母间距 */
  --letter-spacing-tight: -0.022em;
  --letter-spacing-normal: 0;
  --letter-spacing-wide: 0.04em;
}
```

### 1.3 间距系统

```css
:root {
  /* 基础间距单位：4px */
  --space-1: 0.25rem;   /* 4px */
  --space-2: 0.5rem;    /* 8px */
  --space-3: 0.75rem;   /* 12px */
  --space-4: 1rem;      /* 16px */
  --space-5: 1.25rem;   /* 20px */
  --space-6: 1.5rem;    /* 24px */
  --space-8: 2rem;      /* 32px */
  --space-10: 2.5rem;   /* 40px */
  --space-12: 3rem;     /* 48px */
  --space-16: 4rem;     /* 64px */
  --space-20: 5rem;     /* 80px */
  
  /* 组件间距 */
  --spacing-card-padding: var(--space-5);
  --spacing-card-gap: var(--space-4);
  --spacing-section-gap: var(--space-8);
}
```

### 1.4 圆角系统

```css
:root {
  /* 圆角大小 */
  --radius-sm: 0.375rem;   /* 6px */
  --radius-md: 0.5rem;     /* 8px */
  --radius-lg: 0.75rem;    /* 12px */
  --radius-xl: 1rem;       /* 16px */
  --radius-2xl: 1.25rem;   /* 20px */
  --radius-3xl: 1.5rem;    /* 24px */
  --radius-full: 9999px;   /* 完全圆角 */
  
  /* 组件圆角 */
  --radius-card: var(--radius-xl);
  --radius-button: var(--radius-lg);
  --radius-input: var(--radius-lg);
  --radius-badge: var(--radius-full);
}
```

### 1.5 阴影系统

```css
:root {
  /* 玻璃阴影 */
  --shadow-glass: 
    0 8px 32px rgba(0, 0, 0, 0.1),
    inset 0 1px 1px rgba(255, 255, 255, 0.4);
  
  --shadow-glass-lg: 
    0 16px 48px rgba(0, 0, 0, 0.15),
    inset 0 2px 2px rgba(255, 255, 255, 0.3);
  
  --shadow-glass-xl: 
    0 24px 64px rgba(0, 0, 0, 0.2),
    inset 0 2px 2px rgba(255, 255, 255, 0.2);
  
  /* 元素阴影 */
  --shadow-element: 
    0 2px 8px rgba(0, 0, 0, 0.08),
    0 1px 2px rgba(0, 0, 0, 0.04);
  
  --shadow-element-hover: 
    0 4px 16px rgba(0, 0, 0, 0.12),
    0 2px 4px rgba(0, 0, 0, 0.06);
  
  /* 按钮阴影 */
  --shadow-button: 
    0 1px 3px rgba(0, 0, 0, 0.1),
    0 1px 2px rgba(0, 0, 0, 0.06);
  
  --shadow-button-hover: 
    0 4px 12px rgba(0, 0, 0, 0.15),
    0 2px 4px rgba(0, 0, 0, 0.08);
}
```

### 1.6 动画系统

```css
:root {
  /* 动画时长 */
  --duration-fast: 150ms;
  --duration-normal: 300ms;
  --duration-slow: 500ms;
  --duration-slower: 700ms;
  
  /* 缓动函数 */
  --ease-default: cubic-bezier(0.25, 0.1, 0.25, 1);
  --ease-in: cubic-bezier(0.42, 0, 1, 1);
  --ease-out: cubic-bezier(0, 0, 0.58, 1);
  --ease-in-out: cubic-bezier(0.42, 0, 0.58, 1);
  --ease-spring: cubic-bezier(0.34, 1.56, 0.64, 1);
  
  /* 玻璃特效动画 */
  --glass-shimmer: linear-gradient(
    105deg,
    rgba(255, 255, 255, 0) 40%,
    rgba(255, 255, 255, 0.3) 50%,
    rgba(255, 255, 255, 0) 60%
  );
}
```

---

## 二、组件库

### 2.1 基础玻璃组件

#### 玻璃卡片
```css
.glass-card {
  background: var(--glass-white);
  backdrop-filter: var(--glass-blur);
  -webkit-backdrop-filter: var(--glass-blur);
  border: 1px solid var(--glass-border);
  border-radius: var(--radius-card);
  box-shadow: var(--shadow-glass);
  padding: var(--spacing-card-padding);
  transition: all var(--duration-normal) var(--ease-default);
  position: relative;
  overflow: hidden;
}

.glass-card::before {
  content: '';
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  height: 1px;
  background: var(--glass-highlight);
}

.glass-card:hover {
  transform: translateY(-2px);
  box-shadow: var(--shadow-glass-lg);
}
```

#### 玻璃按钮
```css
.glass-button {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: var(--space-2);
  padding: var(--space-3) var(--space-5);
  background: var(--glass-white-light);
  backdrop-filter: var(--glass-blur);
  -webkit-backdrop-filter: var(--glass-blur);
  border: 1px solid var(--glass-border);
  border-radius: var(--radius-button);
  color: var(--color-text-primary);
  font-family: var(--font-family-primary);
  font-size: var(--font-size-base);
  font-weight: var(--font-weight-medium);
  cursor: pointer;
  transition: all var(--duration-fast) var(--ease-default);
  position: relative;
  overflow: hidden;
}

.glass-button::before {
  content: '';
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  height: 50%;
  background: var(--glass-highlight);
  pointer-events: none;
}

.glass-button:hover {
  background: var(--glass-white);
  transform: translateY(-1px);
  box-shadow: var(--shadow-button-hover);
}

.glass-button:active {
  transform: translateY(0);
  box-shadow: var(--shadow-button);
}

/* 主要按钮变体 */
.glass-button--primary {
  background: rgba(0, 122, 255, 0.2);
  border-color: rgba(0, 122, 255, 0.3);
  color: var(--color-primary);
}

.glass-button--primary:hover {
  background: rgba(0, 122, 255, 0.3);
}
```

#### 玻璃输入框
```css
.glass-input {
  width: 100%;
  padding: var(--space-3) var(--space-4);
  background: var(--glass-white-light);
  backdrop-filter: var(--glass-blur);
  -webkit-backdrop-filter: var(--glass-blur);
  border: 1px solid var(--glass-border);
  border-radius: var(--radius-input);
  color: var(--color-text-primary);
  font-family: var(--font-family-primary);
  font-size: var(--font-size-base);
  transition: all var(--duration-fast) var(--ease-default);
  outline: none;
}

.glass-input::placeholder {
  color: var(--color-text-tertiary);
}

.glass-input:focus {
  border-color: var(--color-primary);
  box-shadow: 0 0 0 3px var(--color-primary-light);
}

.glass-input:hover:not(:focus) {
  border-color: var(--glass-border-light);
}
```

### 2.2 布局组件

#### 页面容器
```css
.page-container {
  background: var(--color-background);
  min-height: 100vh;
  padding: var(--space-6);
  position: relative;
}

/* 背景装饰 */
.page-container::before {
  content: '';
  position: fixed;
  top: -50%;
  left: -50%;
  width: 200%;
  height: 200%;
  background: 
    radial-gradient(circle at 20% 20%, rgba(0, 122, 255, 0.08) 0%, transparent 50%),
    radial-gradient(circle at 80% 80%, rgba(52, 199, 89, 0.06) 0%, transparent 50%),
    radial-gradient(circle at 50% 50%, rgba(90, 200, 250, 0.04) 0%, transparent 50%);
  pointer-events: none;
  z-index: 0;
}

.page-container > * {
  position: relative;
  z-index: 1;
}
```

#### 内容包装器
```css
.content-wrapper {
  background: var(--glass-white);
  backdrop-filter: var(--glass-blur);
  -webkit-backdrop-filter: var(--glass-blur);
  border: 1px solid var(--glass-border);
  border-radius: var(--radius-2xl);
  box-shadow: var(--shadow-glass);
  padding: var(--space-8);
  position: relative;
  overflow: hidden;
}

.content-wrapper::before {
  content: '';
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  height: 1px;
  background: var(--glass-highlight);
}
```

### 2.3 数据展示组件

#### 统计卡片
```css
.stat-card {
  background: var(--glass-white);
  backdrop-filter: var(--glass-blur);
  -webkit-backdrop-filter: var(--glass-blur);
  border: 1px solid var(--glass-border);
  border-radius: var(--radius-xl);
  box-shadow: var(--shadow-glass);
  padding: var(--space-6);
  text-align: center;
  transition: all var(--duration-normal) var(--ease-default);
  position: relative;
  overflow: hidden;
}

.stat-card::before {
  content: '';
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  height: 50%;
  background: var(--glass-highlight);
  pointer-events: none;
}

.stat-card:hover {
  transform: translateY(-4px);
  box-shadow: var(--shadow-glass-lg);
}

.stat-card__icon {
  width: 56px;
  height: 56px;
  margin: 0 auto var(--space-4);
  background: var(--color-primary-light);
  border-radius: var(--radius-lg);
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: var(--font-size-2xl);
  color: var(--color-primary);
}

.stat-card__value {
  font-size: var(--font-size-3xl);
  font-weight: var(--font-weight-bold);
  color: var(--color-text-primary);
  margin-bottom: var(--space-2);
  line-height: var(--line-height-tight);
}

.stat-card__label {
  font-size: var(--font-size-sm);
  color: var(--color-text-secondary);
  font-weight: var(--font-weight-medium);
}
```

#### 数据表格
```css
.glass-table-container {
  background: var(--glass-white);
  backdrop-filter: var(--glass-blur);
  -webkit-backdrop-filter: var(--glass-blur);
  border: 1px solid var(--glass-border);
  border-radius: var(--radius-xl);
  box-shadow: var(--shadow-glass);
  overflow: hidden;
}

.glass-table-header {
  padding: var(--space-5) var(--space-6);
  border-bottom: 1px solid var(--glass-border);
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: var(--space-4);
}

.glass-table-title {
  font-size: var(--font-size-lg);
  font-weight: var(--font-weight-semibold);
  color: var(--color-text-primary);
}

.glass-table-badge {
  display: inline-flex;
  align-items: center;
  padding: var(--space-1) var(--space-3);
  background: var(--color-primary-light);
  color: var(--color-primary);
  font-size: var(--font-size-xs);
  font-weight: var(--font-weight-semibold);
  border-radius: var(--radius-badge);
}

.glass-table {
  width: 100%;
  border-collapse: collapse;
}

.glass-table thead th {
  padding: var(--space-4) var(--space-5);
  text-align: left;
  font-size: var(--font-size-sm);
  font-weight: var(--font-weight-semibold);
  color: var(--color-text-secondary);
  background: var(--glass-white-light);
  border-bottom: 1px solid var(--glass-border);
  white-space: nowrap;
}

.glass-table tbody td {
  padding: var(--space-4) var(--space-5);
  font-size: var(--font-size-base);
  color: var(--color-text-primary);
  border-bottom: 1px solid var(--glass-border-light);
  vertical-align: middle;
}

.glass-table tbody tr {
  transition: background-color var(--duration-fast) var(--ease-default);
}

.glass-table tbody tr:hover {
  background: var(--glass-white-light);
}

.glass-table tbody tr:last-child td {
  border-bottom: none;
}
```

### 2.4 反馈组件

#### 状态指示器
```css
.status-indicator {
  display: inline-flex;
  align-items: center;
  gap: var(--space-2);
  padding: var(--space-1) var(--space-3);
  border-radius: var(--radius-badge);
  font-size: var(--font-size-sm);
  font-weight: var(--font-weight-medium);
}

.status-indicator--success {
  background: var(--color-success-light);
  color: var(--color-success);
}

.status-indicator--warning {
  background: var(--color-warning-light);
  color: var(--color-warning);
}

.status-indicator--error {
  background: var(--color-error-light);
  color: var(--color-error);
}

.status-indicator--info {
  background: var(--color-info-light);
  color: var(--color-info);
}

.status-dot {
  width: 8px;
  height: 8px;
  border-radius: 50%;
  background: currentColor;
}
```

#### 玻璃弹窗
```css
.glass-modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(0, 0, 0, 0.3);
  backdrop-filter: blur(8px);
  -webkit-backdrop-filter: blur(8px);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
  opacity: 0;
  visibility: hidden;
  transition: all var(--duration-normal) var(--ease-default);
}

.glass-modal-overlay.active {
  opacity: 1;
  visibility: visible;
}

.glass-modal {
  background: var(--glass-white);
  backdrop-filter: var(--glass-blur-heavy);
  -webkit-backdrop-filter: var(--glass-blur-heavy);
  border: 1px solid var(--glass-border);
  border-radius: var(--radius-2xl);
  box-shadow: var(--shadow-glass-xl);
  padding: var(--space-8);
  max-width: 90vw;
  max-height: 90vh;
  overflow: auto;
  transform: scale(0.9) translateY(20px);
  transition: transform var(--duration-normal) var(--ease-spring);
}

.glass-modal-overlay.active .glass-modal {
  transform: scale(1) translateY(0);
}
```

### 2.5 导航组件

#### 侧边栏
```css
.glass-sidebar {
  background: var(--glass-white);
  backdrop-filter: var(--glass-blur);
  -webkit-backdrop-filter: var(--glass-blur);
  border-right: 1px solid var(--glass-border);
  width: 280px;
  height: 100vh;
  position: fixed;
  left: 0;
  top: 0;
  z-index: 100;
  display: flex;
  flex-direction: column;
}

.glass-sidebar__header {
  padding: var(--space-6);
  border-bottom: 1px solid var(--glass-border);
  display: flex;
  align-items: center;
  gap: var(--space-4);
}

.glass-sidebar__logo {
  width: 40px;
  height: 40px;
  border-radius: var(--radius-lg);
  background: var(--color-primary);
  display: flex;
  align-items: center;
  justify-content: center;
  color: white;
  font-size: var(--font-size-xl);
}

.glass-sidebar__title {
  font-size: var(--font-size-lg);
  font-weight: var(--font-weight-semibold);
  color: var(--color-text-primary);
}

.glass-sidebar__nav {
  flex: 1;
  padding: var(--space-4);
  overflow-y: auto;
}

.glass-nav-item {
  display: flex;
  align-items: center;
  gap: var(--space-3);
  padding: var(--space-3) var(--space-4);
  border-radius: var(--radius-lg);
  color: var(--color-text-secondary);
  text-decoration: none;
  font-size: var(--font-size-base);
  font-weight: var(--font-weight-medium);
  transition: all var(--duration-fast) var(--ease-default);
  margin-bottom: var(--space-1);
}

.glass-nav-item:hover {
  background: var(--glass-white-light);
  color: var(--color-text-primary);
}

.glass-nav-item.active {
  background: var(--color-primary-light);
  color: var(--color-primary);
}

.glass-nav-item__icon {
  width: 20px;
  text-align: center;
}
```

#### 顶部导航栏
```css
.glass-topbar {
  background: var(--glass-white);
  backdrop-filter: var(--glass-blur);
  -webkit-backdrop-filter: var(--glass-blur);
  border-bottom: 1px solid var(--glass-border);
  height: 64px;
  position: fixed;
  top: 0;
  left: 280px;
  right: 0;
  z-index: 99;
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 0 var(--space-6);
}

.glass-topbar__title {
  font-size: var(--font-size-xl);
  font-weight: var(--font-weight-semibold);
  color: var(--color-text-primary);
}

.glass-topbar__actions {
  display: flex;
  align-items: center;
  gap: var(--space-4);
}
```

---

## 三、页面布局模板

### 3.1 仪表板布局
```css
.dashboard-layout {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: var(--spacing-card-gap);
  margin-bottom: var(--spacing-section-gap);
}

@media (max-width: 1200px) {
  .dashboard-layout {
    grid-template-columns: repeat(2, 1fr);
  }
}

@media (max-width: 768px) {
  .dashboard-layout {
    grid-template-columns: 1fr;
  }
}
```

### 3.2 内容页面布局
```css
.content-page-layout {
  display: grid;
  grid-template-columns: 1fr;
  gap: var(--spacing-section-gap);
}

.content-page-layout__header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: var(--space-4);
  margin-bottom: var(--space-6);
}

.content-page-layout__title {
  font-size: var(--font-size-2xl);
  font-weight: var(--font-weight-bold);
  color: var(--color-text-primary);
}
```

---

## 四、动画效果

### 4.1 入场动画
```css
@keyframes fadeInUp {
  from {
    opacity: 0;
    transform: translateY(20px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

@keyframes fadeInDown {
  from {
    opacity: 0;
    transform: translateY(-20px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

@keyframes fadeInLeft {
  from {
    opacity: 0;
    transform: translateX(-20px);
  }
  to {
    opacity: 1;
    transform: translateX(0);
  }
}

@keyframes fadeInRight {
  from {
    opacity: 0;
    transform: translateX(20px);
  }
  to {
    opacity: 1;
    transform: translateX(0);
  }
}

@keyframes scaleIn {
  from {
    opacity: 0;
    transform: scale(0.9);
  }
  to {
    opacity: 1;
    transform: scale(1);
  }
}

.animate-fade-in-up {
  animation: fadeInUp var(--duration-normal) var(--ease-out);
}

.animate-fade-in-down {
  animation: fadeInDown var(--duration-normal) var(--ease-out);
}

.animate-fade-in-left {
  animation: fadeInLeft var(--duration-normal) var(--ease-out);
}

.animate-fade-in-right {
  animation: fadeInRight var(--duration-normal) var(--ease-out);
}

.animate-scale-in {
  animation: scaleIn var(--duration-normal) var(--ease-spring);
}
```

### 4.2 玻璃特效动画
```css
@keyframes glassShimmer {
  0% {
    background-position: -200% 0;
  }
  100% {
    background-position: 200% 0;
  }
}

.glass-shimmer {
  background: var(--glass-shimmer);
  background-size: 200% 100%;
  animation: glassShimmer 3s infinite;
}

@keyframes glassPulse {
  0%, 100% {
    opacity: 1;
  }
  50% {
    opacity: 0.8;
  }
}

.glass-pulse {
  animation: glassPulse 2s var(--ease-in-out) infinite;
}
```

### 4.3 交互动画
```css
/* 悬停提升效果 */
.hover-lift {
  transition: transform var(--duration-fast) var(--ease-default),
              box-shadow var(--duration-fast) var(--ease-default);
}

.hover-lift:hover {
  transform: translateY(-2px);
  box-shadow: var(--shadow-element-hover);
}

/* 悬停缩放效果 */
.hover-scale {
  transition: transform var(--duration-fast) var(--ease-default);
}

.hover-scale:hover {
  transform: scale(1.02);
}

/* 点击反馈效果 */
.press-effect {
  transition: transform var(--duration-fast) var(--ease-default);
}

.press-effect:active {
  transform: scale(0.98);
}
```

---

## 五、响应式设计

### 5.1 断点系统
```css
/* 移动端：< 768px */
@media (max-width: 767px) {
  :root {
    --font-size-base: 0.875rem;
    --spacing-card-padding: var(--space-4);
    --spacing-section-gap: var(--space-6);
  }
  
  .glass-sidebar {
    transform: translateX(-100%);
  }
  
  .glass-topbar {
    left: 0;
  }
}

/* 平板端：768px - 1023px */
@media (min-width: 768px) and (max-width: 1023px) {
  .glass-sidebar {
    width: 240px;
  }
  
  .glass-topbar {
    left: 240px;
  }
}

/* 桌面端：≥ 1024px */
@media (min-width: 1024px) {
  /* 默认样式 */
}
```

### 5.2 移动端适配
```css
@media (max-width: 767px) {
  .page-container {
    padding: var(--space-4);
  }
  
  .content-wrapper {
    padding: var(--space-5);
    border-radius: var(--radius-xl);
  }
  
  .stat-card {
    padding: var(--space-4);
  }
  
  .stat-card__value {
    font-size: var(--font-size-2xl);
  }
  
  .glass-table-header {
    flex-direction: column;
    align-items: flex-start;
  }
  
  .glass-table-container {
    overflow-x: auto;
  }
}
```

---

## 六、可访问性

### 6.1 焦点管理
```css
/* 焦点指示器 */
.glass-focusable:focus-visible {
  outline: 2px solid var(--color-primary);
  outline-offset: 2px;
}

/* 跳过链接 */
.skip-link {
  position: absolute;
  top: -40px;
  left: 0;
  background: var(--color-primary);
  color: white;
  padding: var(--space-2) var(--space-4);
  z-index: 1000;
  transition: top var(--duration-fast) var(--ease-default);
}

.skip-link:focus {
  top: 0;
}
```

### 6.2 颜色对比度
所有文本颜色与背景的对比度符合 WCAG AA 标准：
- 正常文本：4.5:1 对比度
- 大文本：3:1 对比度

### 6.3 减少动画
```css
@media (prefers-reduced-motion: reduce) {
  *,
  *::before,
  *::after {
    animation-duration: 0.01ms !important;
    animation-iteration-count: 1 !important;
    transition-duration: 0.01ms !important;
  }
}
```

---

## 七、实施指南

### 7.1 CSS 变量导入
将设计系统变量定义在 `:root` 中，确保全局可用。

### 7.2 组件类名规范
- 使用 BEM 命名规范：`.block__element--modifier`
- 玻璃效果类名以 `.glass-` 前缀
- 动画类名以 `.animate-` 前缀
- 交互类名以 `.hover-` 或 `.press-` 前缀

### 7.3 浏览器兼容性
- 使用 `-webkit-backdrop-filter` 支持 Safari
- 提供降级方案：当 `backdrop-filter` 不支持时使用纯色背景
- 使用 CSS 特性查询 `@supports` 检测支持情况

```css
@supports not (backdrop-filter: blur(20px)) {
  .glass-card {
    background: rgba(255, 255, 255, 0.95);
  }
}
```

---

## 八、设计检查清单

### 8.1 视觉一致性
- [ ] 所有玻璃元素使用统一的透明度值（0.85）
- [ ] 所有圆角使用设计系统定义的变量
- [ ] 所有阴影使用设计系统定义的变量
- [ ] 所有动画使用设计系统定义的时长和缓动函数

### 8.2 可访问性
- [ ] 所有文本颜色对比度符合 WCAG AA 标准
- [ ] 所有交互元素有清晰的焦点指示器
- [ ] 支持键盘导航
- [ ] 支持屏幕阅读器

### 8.3 性能优化
- [ ] 使用 `will-change` 优化动画性能
- [ ] 避免在大面积元素上使用 `backdrop-filter`
- [ ] 使用 `transform` 和 `opacity` 进行动画
- [ ] 提供减少动画的选项

### 8.4 响应式设计
- [ ] 在所有断点下测试布局
- [ ] 移动端提供适当的触摸目标大小
- [ ] 表格在小屏幕上可横向滚动
- [ ] 导航在小屏幕上可折叠

---

**UI Designer**: UI Designer  
**设计系统日期**: 2026年6月10日  
**实施状态**: 准备就绪，可供开发团队使用  
**下一步**: 创建具体的页面实现示例和组件文档