# 颜文字资料来源

- `blue-island.txt`：用户提供的《蓝岛颜文字.txt》原文副本。SHA-256：`ee1cc67f056a9bb28ce923d5403287e9e5dd0721cb0df8840b881fb0ea917ca1`。
- `web-2026-10-25.json`：从 [网页端脚本](https://www.nmbxd1.com/Public/Js/h.desktop.js)及[综合版 1](https://www.nmbxd1.com/f/%E7%BB%BC%E5%90%88%E7%89%881)内嵌补丁提取的数据快照。SHA-256：`bab9bbf37fb8d083d7fca3c59f9fa393da5dc4702b656d8ea4115e33c5a40b6e`。

- `web-ex-2026-10-25.json`：从 X岛-EX 提取的扩展颜文字，来源和上游版本见[资料声明](../../licenses/EmoticonData-NOTICE.txt)。

保留资料的原始来源及版本说明；应用只内置提取后的颜文字数据。早期试验用的 Gboard 字典不再参与生成，也不纳入仓库。

在项目根目录运行 `python tools/Build-Emoticons.py` 可重建资源。当前方案见 [preview.17 实现文档](../../emoticons-preview17.md)。
