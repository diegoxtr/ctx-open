# CTX 技术索引
如果语言模型及其代理丢失了上下文，这就是你需要的工具。

本文档汇总仓库中的技术与操作文档。

它是以下用途的入口：

- 开发
- 架构
- 运维
- 测试
- 技术 onboarding

## 快速阅读顺序

推荐顺序：

1. [README.md](../../README.md)
2. [V1_PLAN.md](../V1_PLAN.md)
3. [V1_FUNCTIONAL_SPEC.md](../V1_FUNCTIONAL_SPEC.md)
4. [TECHNICAL_ARCHITECTURE.md](../TECHNICAL_ARCHITECTURE.md)
5. [DOMAIN_MODEL.md](../DOMAIN_MODEL.md)
6. [CTX_STRUCTURE.md](../CTX_STRUCTURE.md)
7. [CLI_COMMANDS.md](../CLI_COMMANDS.md)
8. [BOOTSTRAP_COGNITIVE_INDEXING.md](../BOOTSTRAP_COGNITIVE_INDEXING.md)
9. [BOOTSTRAP_TEST_DEVELOPMENT.md](../BOOTSTRAP_TEST_DEVELOPMENT.md)
10. [HYPOTHESIS_BRANCH_SEMANTICS.md](../HYPOTHESIS_BRANCH_SEMANTICS.md)

## 按类别划分的文档

### 产品与范围

- [V1_PLAN.md](../V1_PLAN.md)
  总结目标、范围、阶段、backlog 与 V1 路径。

- [V1_FUNCTIONAL_SPEC.md](../V1_FUNCTIONAL_SPEC.md)
  定义模块、需求、验收标准与 V1 done 标准。

- [RELEASE_1_0_12.md](RELEASE_1_0_12.md)
  总结当前稳定 release baseline。

### 架构与设计

- [TECHNICAL_ARCHITECTURE.md](../TECHNICAL_ARCHITECTURE.md)
  分层、职责、依赖与端到端流程。

- [DOMAIN_MODEL.md](../DOMAIN_MODEL.md)
  实体、强 ID、状态、关系与领域规则。

- [CTX_STRUCTURE.md](../CTX_STRUCTURE.md)
  `.ctx/` 的结构、基础文件、目录与不变量。

- [CTX_SPECIFICATION_V1.md](../CTX_SPECIFICATION_V1.md)
  最小 CTX v1 规格：上下文如何存储、版本化与结构化。

- [COGNITIVE_GRAPH_AND_LINEAGE.md](../COGNITIVE_GRAPH_AND_LINEAGE.md)
  定义知识的关系投影及其可视化路线图。

- [COGNITIVE_THREAD_RECONSTRUCTION.md](../COGNITIVE_THREAD_RECONSTRUCTION.md)
  从结构化工件、commits 与 branches 重建认知线程的规范模型。

- [COGNITIVE_TRIGGERS.md](../COGNITIVE_TRIGGERS.md)
  认知线的持久 origin 模型、紧凑 trigger summary 与 packet 集成。

- [WORK_MODEL_AND_PRIORITIZATION.md](../WORK_MODEL_AND_PRIORITIZATION.md)
  issue / gap / task / subtask / duplicate / blocker 的规范分类与 proximity-based prioritization。

- [OPERATIONAL_RUNBOOKS.md](../OPERATIONAL_RUNBOOKS.md)
  重复性操作知识、packet 注入与 overflow 处理的紧凑设计。

- [CTX_GOAL_FLOW_DIAGRAM.md](../CTX_GOAL_FLOW_DIAGRAM.md)
  用 CTX 命令解决一个 goal 并构建 `.ctx` 图谱的示例流程。

### 运维与使用

- [CLI_COMMANDS.md](../CLI_COMMANDS.md)
  已实现 CLI 命令的完整参考。

- [BOOTSTRAP_COGNITIVE_INDEXING.md](../BOOTSTRAP_COGNITIVE_INDEXING.md)
  定义从外部材料重建 provisional cognitive threads 的 idea-first bootstrap map/apply 表面。

- [BOOTSTRAP_TEST_DEVELOPMENT.md](../BOOTSTRAP_TEST_DEVELOPMENT.md)
  记录 bootstrap indexing 在实践中如何被测试，包括 regression case、failure mode 与 product conclusion。

- [HYPOTHESIS_BRANCH_SEMANTICS.md](../HYPOTHESIS_BRANCH_SEMANTICS.md)
  定义 competing hypotheses 的 branch-like lifecycle、relations 与 evidence 行为，而不直接耦合到 repo branches。

- [COMMAND_ADOPTION_AND_COVERAGE.md](../COMMAND_ADOPTION_AND_COVERAGE.md)
  哪些命令常用、哪些较冷，以及推荐采用顺序。

- [INSTALLATION_AND_USAGE_GUIDE.md](../INSTALLATION_AND_USAGE_GUIDE.md)
  安装、运行与首次使用的操作 onboarding。

- [PILOT_TESTING_GUIDE.md](../PILOT_TESTING_GUIDE.md)
  受控 pilot 的执行指南。

- [CTX_VIEWER_GUIDE.md](../CTX_VIEWER_GUIDE.md)
  如何理解 viewer、它的 timeline、branches 与 panels。

- [LOCAL_CTX_INSTALLATION.md](../LOCAL_CTX_INSTALLATION.md)
  `C:\ctx`、`ctx` 与 `ctx-viewer` 的规范本地发布 / 安装流程。

- [INSTALLER_AND_DISTRIBUTION.md](../INSTALLER_AND_DISTRIBUTION.md)
  packaging model、portable archives 与 distribution output policy。

### 操作 prompts

- [CTX_HELPER_PROMPT.md](../../prompts/CTX_HELPER_PROMPT.md)
  helper/bootstrap prompt，会在开始工作前把代理与操作员重新锚定到 active repo、核心 docs、viewer 与 public boundary。

- [CTX_BASE_PROMPT.md](../../prompts/CTX_BASE_PROMPT.md)
  用新工具操作 CTX 的基础模板。

- [CTX_AGENT_PROMPT.md](../../prompts/CTX_AGENT_PROMPT.md)
  面向代理的 prompt，包含 continuity、evidence 与 cognitive closeout 规则。

- [CTX_AUTONOMOUS_OPERATOR_PROMPT.md](../../prompts/CTX_AUTONOMOUS_OPERATOR_PROMPT.md)
  面向 autonomous operator 的 prompt，包含严格的 inspection / execution / closeout 顺序。

### 仓库根目录

- [README.md](../../README.md)
  项目入口。

- [CHANGELOG.md](../../CHANGELOG.md)
  产品变更历史摘要。

- [LICENSE](../../LICENSE)
  source-available license。

- [COPYRIGHT.md](../../COPYRIGHT.md)
  版权声明。

- [TRADEMARK.md](../../TRADEMARK.md)
  商标使用规则。

- [CONTRIBUTOR_ASSIGNMENT.md](../../CONTRIBUTOR_ASSIGNMENT.md)
  贡献分配条款。

- [NOTICE](../../NOTICE)
  补充仓库声明。

### 验证脚本

- [run-smoke-test.ps1](../../scripts/run-smoke-test.ps1)
  可复现的功能验证流程。

- [run-merge-conflict-demo.ps1](../../scripts/run-merge-conflict-demo.ps1)
  可复现的 branch / merge / conflict 演示。

- [publish-local.ps1](../../scripts/publish-local.ps1)
  发布本地安装到 `C:\ctx`，同时保留版本化工作区。

- [build-distribution.ps1](../../scripts/build-distribution.ps1)
  根据 `distribution/targets.json` 构建跨平台 portable CTX bundles。

## 按角色推荐阅读

### Developer

1. [README.md](../../README.md)
2. [TECHNICAL_ARCHITECTURE.md](../TECHNICAL_ARCHITECTURE.md)
3. [DOMAIN_MODEL.md](../DOMAIN_MODEL.md)
4. [CTX_STRUCTURE.md](../CTX_STRUCTURE.md)
5. [CLI_COMMANDS.md](../CLI_COMMANDS.md)

### Technical tester

1. [README.md](../../README.md)
2. [INSTALLATION_AND_USAGE_GUIDE.md](../INSTALLATION_AND_USAGE_GUIDE.md)
3. [CLI_COMMANDS.md](../CLI_COMMANDS.md)
4. [PILOT_TESTING_GUIDE.md](../PILOT_TESTING_GUIDE.md)

### Business / V1 scope

1. [V1_PLAN.md](../V1_PLAN.md)
2. [V1_FUNCTIONAL_SPEC.md](../V1_FUNCTIONAL_SPEC.md)
3. [RELEASE_1_0_0.md](../RELEASE_1_0_0.md)

## 文档覆盖状态

当前已覆盖：

- 产品目标
- V1 范围
- 领域模型
- 技术架构
- 持久化结构
- CLI 命令
- 安装
- viewer
- pilot
- 稳定 release
- 知识产权与贡献
- 认知图谱
- 正式线程重建
- cognitive triggers
- 本地安装与 distribution
- bootstrap regression development
- hypothesis branch-like semantics

## 未来可能补充的文档

可考虑新增：

- technical decision ADRs
- 手工认知冲突解决指南
- provider 集成指南
- 运维故障排查指南
- post-V1 roadmap
