# CTX - Release 1.0.7

发布日期：2026-04-17

版本：

- `1.0.7`

摘要：

- CTX 1.0 的稳定 hotfix 补丁版本。
- 恢复已发布 release metadata 与安装后二进制所报告产品版本之间的端到端一致性。

亮点：

- `ctx version`、viewer version surfaces 以及 runtime product-version checks 现在都会报告 `1.0.7`，而不再是错误残留在 `1.0.6` 中的 `1.0.4` 常量。
- 公共 `win-x64` portable asset 已从 release branch 重新构建，并带有修正后的 product version 常量。
- 公共 README、changelog、live-demo landing copy 与 release references 现在都指向这个修正后的 hotfix baseline。
