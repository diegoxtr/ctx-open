# CTX - V1 功能规格
如果语言模型及其代理丢失了上下文，这就是你需要的工具。

## 1. 文档目标

本文定义 CTX V1 的目标功能规格。

其作用是：

- 定义产品必须做什么
- 划定 V1 的范围与排除项
- 建立验收标准
- 作为技术执行与验证参考

## 2. 产品概述

CTX 是认知版本控制系统，用于记录、版本化、比较并复用结构化推理。

产品围绕显式认知工件运作：

- context
- goals
- tasks
- hypotheses
- decisions
- evidence
- conclusions
- runs
- packets
- commits

## 3. V1 功能目标

V1 必须让技术用户能够：

- 初始化认知仓库
- 使用结构化工件建模问题
- 构建相关上下文
- 执行 AI 迭代
- 保存可复现的认知快照
- 比较变更
- 使用分支
- 检测认知冲突
- 在操作层面检查仓库状态

## 4. V1 功能范围

## 4.1 V1 包含

- 本地 `.ctx/` 仓库
- 文件系统持久化
- CLI 作为主界面
- 可替换 providers
- 结构化上下文
- 认知提交
- 认知 diff
- branching 与 merge
- 基础指标
- 核心自动化测试
- 基础操作文档

## 4.2 V1 排除

- Web UI
- 远程同步
- 真正多用户协作
- 高级认证与授权
- 可视化 dashboard
- 深度 IDE 集成
- 云服务
- workspace 级访问控制
- 高级语义搜索

## 5. 功能模块

## 模块 A - 认知仓库

### 目标

初始化并维护结构可复现的本地 CTX 仓库。

### 功能需求

- 创建具有已知结构的 `.ctx/`
- 持久化 `version.json`、`config.json`、`project.json`、`HEAD`
- 创建仓库功能目录
- 加载与保存工作状态
- 允许仓库格式未来演进

### 验收标准

- `ctx init` 创建完整结构
- `ctx status` 可无错误读取仓库
- 状态在多次运行间持久
- 仓库格式有版本标识

## 模块 B - 认知领域模型

### 目标

用可追溯实体表示结构化推理。

### 功能需求

- 支持 `Project`
- 支持 `Goal`
- 支持 `Task`
- 支持 `Hypothesis`
- 支持 `Decision`
- 支持 `Evidence`
- 支持 `Conclusion`
- 支持 `Run`
- 支持 `ContextCommit`
- 支持 `ContextPacket`
- 支持 `WorkingContext`

### 验收标准

- 所有实体都有强标识
- 所有实体都有相关状态或追溯信息
- 实体关系可持久化与恢复
- 模型可重建完整认知状态

## 模块 C - 认知工件管理

### 目标

在 CLI 中创建、列出与查询认知工件。

### 功能需求

- 创建 goals
- 创建 tasks
- 创建 hypotheses
- 创建 evidence
- 创建 decisions
- 创建 conclusions
- 按类型列出工件
- 按 ID 显示单个工件
- 校验交叉引用

### 验收标准

- 每个工件都有对应创建命令
- 每个工件可通过 `list/show` 检查
- 缺失引用的错误清晰
- hypothesis、evidence、decision、conclusion 的关系可追溯

## 模块 D - ContextBuilder

### 目标

构建用于 AI 迭代的优化上下文包。

### 功能需求

- 按目标或任务选择相关工件
- 避免空或无关信息
- 生成 context fingerprint
- 估算 tokens
- 持久化 packets

### 验收标准

- `ctx context` 返回可用 packet
- packet 各章节结构一致
- 相关内容变化时 fingerprint 改变
- packet 可从仓库中取回

## 模块 E - Runs 与 providers

### 目标

用可替换 provider 执行 AI runs，并记录结果。

### 功能需求

- 定义 `IAIProvider` 接口
- 至少支持 OpenAI
- 至少支持 Anthropic
- 基于 `ContextPacket` 执行 `Run`
- 记录 run artifacts
- 记录使用量、成本与时长
- 允许离线回退以便受控测试

### 验收标准

- `ctx run` 可执行且不破坏流程
- run 被持久化
- `run list/show` 可用
- `metrics show` 反映 run 影响
- 无密钥时系统仍可测试

## 模块 F - 认知提交

### 目标

持久化仓库状态的可复现认知快照。

### 功能需求

- 从 `WorkingContext` 构建提交
- 计算快照哈希
- 记录 parent commit
- 提交时清除 dirty 状态
- 在 `.ctx/commits` 中持久化提交

### 验收标准

- `ctx commit` 生成稳定提交
- 快照被持久化
- `HEAD` 更新
- 活跃分支指向创建的提交

## 模块 G - 认知 diff

### 目标

比较认知状态并高亮相关变化。

### 功能需求

- 检测 tasks 的变化
- 检测 hypotheses 的变化
- 检测 decisions 的变化
- 检测 evidence 的变化
- 检测 conclusions 的变化
- 比较 working state 或 commits

### 验收标准

- `ctx diff` 返回可理解的变化
- 变化区分新增、修改与删除
- diff 摘要对用户有价值

## 模块 H - Branching 与 merge

### 目标

探索替代性的推理路线。

### 功能需求

- 创建分支
- 切换分支
- 合并分支
- 检测分歧认知冲突
- 返回可解释的 merge 结果

### 验收标准

- `ctx branch` 创建可用分支
- `ctx checkout` 切换活跃上下文
- `ctx merge` 产生清晰结果
- 冲突被明确列出

## 模块 I - 操作可观测性

### 目标

为测试与分析提供系统使用可见性。

### 功能需求

- 列出 providers
- 列出 runs
- 列出 packets
- 显示 metrics
- 显示仓库状态
- 显示提交历史

### 验收标准

- 用户可以从 CLI 审计工作
- 可以回顾成本与迭代次数
- 无需手动检查文件也能理解操作状态

## 模块 J - 使用文档

### 目标

让用户在不依赖开发者直接协助的情况下完成 onboarding 与产品测试。

### 功能需求

- 记录安装
- 记录首次使用
- 记录 V1 plan
- 记录 pilot
- 记录内部 release

### 验收标准

- 技术测试者可从书面文档开始
- 存在 pilot guide
- 存在 installation guide
- 存在 V1 scope 参考

## 6. 非功能需求

V1 必须满足：

- .NET 8 兼容
- 可复现的本地持久化
- 可理解的错误
- 模块化架构
- 层间低耦合
- 无需大改即可添加 providers
- 自动化核心测试
- 结构化 CLI 输出

## 7. 质量要求

V1 可接受的条件：

- build 通过
- 核心测试通过
- 主流程不需要手动编辑 JSON
- CLI 能覆盖一个简单真实案例的端到端流程
- 产品至少支持一个受控技术 pilot
