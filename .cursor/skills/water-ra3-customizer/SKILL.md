---
name: water-ra3-customizer
description: >-
  在「搬运工」产出的独立水面 UPM 包基础上，按《命令与征服：红色警戒 3》风格逐步定制画面
  （目标一致但允许分阶段实现）。在用户要求 RA3 水面、红警3 水面、水面风格化对接 RTS、或引用「定制工」技能时使用。
disable-model-invocation: true
---

# 定制工（红色警戒 3 水面表现框架）

## 必读上下文

1. 用 Read 工具打开 **`docs/WaterRenderingSystem.md`**，明确当前着色器入口（`WaterCommon.hlsl`、`WaterLighting.hlsl`、`GerstnerWaves.hlsl`）、全局贴图与 `WaterSystemFeature` 的职责。
2. 假设水面已按 **`water-system-porter`（搬运工）** 技能剥离为独立包；本技能只在该包（或其后继 fork）上改 `Shaders/`、材质参数、`WaterSurfaceData` / `WaterSettingsData` 及必要的 C# 钩子。

## 任务目标（总目标）

将剥离后的水面系统在 **画面 read 上与大方向对齐《红色警戒 3》水面**：偏 RTS 俯视可读性、岸线清晰、反射与环境光简化、波纹尺度与颜色梯队可辨认。  
**不要求**一步复刻专有引擎细节；采用 **迭代清单**，用户后续在对话中追加条目时，把新需求落入下文对应章节。

## 工作方式

- **先读文档与现状**，再改 shader / 资源；尽量单点突破（颜色/菲涅尔/泡沫/深度岸带/反射源）避免一次大改。
- 每轮用户补充需求时：**更新本 SKILL 或包内 `docs/RA3-Water-Targets.md`**（推荐在包中维护目标清单，本 SKILL 只保留流程与章节索引）。

## RA3 表现框架（待填满 — Phase 0）

以下小节为 **占位**。用户追加需求时，把具体条目记到「可验收标准」子 bullet，并标注优先级 P0/P1/P2。

### Phase A — 色彩与体积感

- **参考基调**：RA3 近岸偏饱和、远水略压暗；团队色/地图色调由宿主后处理决定时，水面 **避免抢对比**。
- **待填**：主色、吸收/散射 ramp、`WaterSurfaceData` Gradient 目标截图或取色。
- **可验收标准**：（待添加）

### Phase B — 岸线、浅水与泡沫带

- **现状锚点**：`WaterCommon.hlsl` 中深度、`WaterDepth`、泡沫 blend；岸线深度来自 `_WaterDepthMap` 或烘焙。
- **待填**：岸带宽、泡沫硬度、是否需额外 shore noise。
- **可验收标准**：（待添加）

### Phase C — 波纹尺度与法线（俯视可读）

- **现状锚点**：Gerstner 参数、`SurfaceMap` 细节、远距离 normal blend。
- **待填**：主导波长、俯视角下「棋盘格感」抑制策略。
- **可验收标准**：（待添加）

### Phase D — 反射与环境

- **现状锚点**：`WaterLighting.hlsl` `SampleReflections`；平面反射 / 探针 / Cubemap。
- **RA3 取向**：RTS 常弱化动态镜面，偏天空与环境块反射。
- **待填**：默认反射源与模糊/LOD。
- **可验收标准**：（待添加）

### Phase E — 特效与合成

- **现状锚点**：`_WaterFXMap`、焦散 pass（可关）。
- **待填**：尾迹、爆炸倒影、是否需要 RA3 式高光条带（若与 WaterFX 分工）。
- **可验收标准**：（待添加）

### Phase F — 性能与 WebGL

- **与搬运工对齐**：烘焙深度、可选关焦散、反射档位。
- **待填**：目标帧率与降级策略。
- **可验收标准**：（待添加）

## 执行流程（Agent）

1. Read `docs/WaterRenderingSystem.md`。
2. 确认修改范围仅在 **剥离后的水面包**（shader / hlsl / ScriptableObject / 可选 Feature）。
3. 查阅包内 **RA3 目标清单**（若不存在则在本轮对话中根据用户口述创建 `docs/RA3-Water-Targets.md` 骨架）。
4. 从当前最高优先级的 Phase 选 **最少 diff** 实现；避免改动与 RA3 无关的公共 API。
5. 建议用同一俯视场景截图对比 Before/After（用户侧验收）。

## 法律与资产说明

红色警戒 3 为 EA 发行作品；定制时 **仅做原创实现与风格参考**，不复制其受版权保护的贴图、模型或提取资源。目标为 **类似观感** 的技术与艺术方向对齐。
