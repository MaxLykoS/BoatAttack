---
name: water-system-porter
description: >-
  将 BoatAttack URP 水面系统剥离为可搬运的独立 Unity URP 包，并针对上帝俯视、WebGL
  场景做默认取舍与验收清单。在用户要求搬运水面、剥离水面包、做 WebGL 俯视海水、或引用「搬运工」技能时使用。
disable-model-invocation: true
---

# 搬运工（水面系统剥离）

## 必读上下文

开始前用 Read 工具打开 **`docs/WaterRenderingSystem.md`**，对照其中的包路径、管线、`Water` / `WaterSystemFeature` / `PlanarReflections` 职责与清单后再改代码。

## 任务目标

在 **不修改** `Packages/com.verasl.water-system` 的前提下，维护 **`Packages/com.boatattack.water-system-portable`**（完整副本：独立 `package.json`、GUID、`WaterSystemPortable` 命名空间、`PortableWater/*` 着色器），使其成为 **可独立拷贝或通过 git submodule / openupm 本地路径引用的 UPM 包**，使目标工程只需：

- 在 `Packages/manifest.json` 中加入对该包的依赖（`file:` 或 registry）
- 使用兼容的 Unity + URP 大版本
- 按本文档配置 Renderer Feature、可选的第二套 Renderer、Layer、资源

副本与原版可同仓并存；**游戏默认玩法仍使用原版 `WaterSystem`**，除非宿主工程已切换到副本 API。

剥离/裁剪副本时 **默认适合「上帝俯视 + WebGL」**：性能优先、尽量少实时全屏附加 pass、反射与深度路径在 WebGL 上可预期。

## 剥离产物结构（建议）

在仓库或旁路目录中形成类似布局：

```text
<PackageRoot>/
  package.json          # 唯一包名，例如 com.<your-org>.water-system-webgl
  README.md             # 安装步骤、URP 版本说明、WebGL 注意事项
  Runtime/              # 原 Scripts + 必要资源引用说明
  Shaders/
  Resources/            # WaterResources 等（若保留 Resources.Load）
  Editor/               # 原 Editor（可选）
  *.asmdef
```

包名与 asmdef 与 BoatAttack 解耦；避免保留仅 Demo 用的 GUID 硬引用。

## 上帝俯视 + WebGL 的默认取向

实施剥离时 **优先** 做下列取舍（若与宿主工程冲突，在 README 说明如何改回）：

1. **岸线深度**：WebGL 上实时 `CaptureDepthMap` 已知有风险；默认文档化并优先 **烘焙 `_WaterDepthMap`**（`Water.bakedDepthTex`），或提供 Editor 菜单烘焙到贴图。
2. **反射**：平面反射在 WebGL + 俯视 RTS 上成本高；默认建议 **`ReflectionType.ReflectionProbe` 或 `Cubemap`**，将平面反射列为「高质量可选」并在 README 标明额外 Renderer、第二轮 `RenderSingleCamera` 成本。
3. **焦散**：`WaterCausticsPass` 对 WebGL 为可选；提供 **Feature 开关或 stripping**，俯视角收益有限时默认关或降低频率/画幅。
4. **细分**：俯视远处网格密集度低；默认 **VertexOffset** 几何即可，`WaterTessellated` 可作为可选子模块或 `#ifdef` 档位。
5. **水面网格跟随**：保留 `Graphics.DrawMesh` 量化跟随逻辑；俯视相机下量化步长可在包内集中常量，便于调小闪烁与裁剪。
6. **第三层依赖**：`package.json` 仅声明 **URP、Mathematics、Burst**（与现包一致）；不要把 BoatAttack 的 `Assets/Scripts`、Addressables 等拉入包。

## 执行流程（Agent）

1. **Read** `docs/WaterRenderingSystem.md`。
2. 盘点 `Packages/com.boatattack.water-system-portable` 下全部必须文件（脚本、shader、Resources、Editor、Textures、Meshes、Materials 预置若包内自带）。
3. 在新包目录中复制并 **修正**：
   - `package.json`（`name`、`displayName`、`dependencies`、`samples` 可选）
   - 所有 `PortableWater/`、`com.boatattack.water-system-portable` 路径：shader 名称、命名空间可保留或统一重命名（一次改全）
   - `Resources.Load("WaterResources")` 路径与资源位置一致
4. 从宿主工程提取 **最小集成样板**（可选 `Samples~/`）：一个场景、URP Renderer 上挂 `WaterSystemFeature`、一层用于深度的 Layer 说明、一份 `WaterSettingsData` / `WaterSurfaceData` 实例说明。
5. **验收清单**（剥离完成后自检）：
   - [ ] 空工程仅加本包 + URP 能编译
   - [ ] WebGL Player Settings 下无仅 Editor 的 API 裸奔（用 `#if UNITY_EDITOR` 包裹 AssetDatabase 等）
   - [ ] 主水面 + Gerstner +（可选）WaterFX 能跑；深度与反射路径与 README 一致
   - [ ] 无对 `BoatAttack` 命名空间或 Assets 下 Demo 脚本的硬引用

## 与宿主工程的切割

- 包内 **不包含仅 Demo 用的船、引擎采样**；若需 API 示例，放在 `Samples~`。
- `PlanarReflections` 依赖 **第二 Renderer**：在 README 写明宿主必须有的 Renderer asset（或提供包内 `PlanarReflectionRenderer` 样板）。

## 文档同步

若剥离过程中改变了目录、包名、默认反射/深度策略，在 **`docs/WaterRenderingSystem.md`** 或 **包内 `README.md`** 二选一为主更新事实来源，避免双处矛盾（以包内 README 为对外的权威更佳）。
