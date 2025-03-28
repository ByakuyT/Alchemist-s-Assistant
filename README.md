# Alchemist's Assistant

所有功能都可配置，请在 BepInEx\config 中找到 AlchAss.cfg 并编辑其中内容。

注意本模组和 FasterChores 在变速功能上冲突。

如果您有希望实现的其他辅助功能，欢迎联系我。

[Nexus 链接](https://www.nexusmods.com/potioncraftalchemistsimulator/mods/47)

## 功能：
1. **减速控制**  
   - 按住 Z/X 键时降低操作速度至配置文件指定比例（`AlchAssSpeedBrewConfig.txt`）  
   ▶️ 研磨减速：`enableGrindSpeed`  
   ▶️ 搅拌减速：`enableStirSpeed`  
   ▶️ 加水减速：`enableLadleSpeed`  
   ▶️ 加热减速：`enableHeatSpeed`  

2. **批量炼药**  
   - 按住 Z/X 键时批量炼药数量提升至配置文件倍数（`AlchAssSpeedBrewConfig.txt`，材料不足时失效）  
   ▶️ 对应配置：`enableBrewMore`  

3. **精准温控**  
   - 右键点击风箱将热量设定为配置文件数值（`AlchAssGrindHeatConfig.txt`百分比）  
   ▶️ 对应配置：`enableShuttingDown`  

4. **定量研磨**  
   - 右键点击研杵将研磨度设定为配置文件数值（`AlchAssGrindHeatConfig.txt`百分比）  
   ▶️ 对应配置：`enableGrindAll`  

5. **状态监测**  
   ▶️ 实时血量显示：`enableHealthStatus`  
   ▶️ 实时研磨进度：`enableGrindStatus`  
   ▶️ 搅拌阶段/方向：`enableStirStatus`  
   ▶️ 沼泽/骷髅区移动距离：`enableZoneStatus`  

6. **动态示线**  
   - 按 `/` 键显示搅拌/加水/目标方向提示线  
   ▶️ 对应配置：`enableDirectionLine`  

7. **漩涡管理**  
   - 按 `'` 键启用漩涡边缘减速制动  
   ▶️ 对应配置：`enableVortexEdge`  
   ▶️ 漩涡中心距离/角度显示：`enableVortexStatus`  

8. **坐标系统**  
   ▶️ 实时直角/极坐标切换（空格键）：`enablePositionStatus`  
   ▶️ 盐量折算显示：`enablePositionStatus`  

9. **偏差分析**  
   ▶️ 目标接触时偏差显示：`enableTargetStatus`  
   ▶️ 总体偏差分级提示（III级≤100%，II级≤600%）：`enableDeviationStatus`  
   ▶️ 路径最近点偏差：`enablePathStatus`  
   ▶️ 加水路径偏差：`enableLadleStatus`  

10. **辅助界面**  
    ▶️ 按键提示表：`enableTooltip`  
    ▶️ 窗口位置记忆 & 缩放配置：`windowScale`  
    ▶️ 信息窗口自由开关（研磨/移动/区域等）：各`enable*Status`参数
