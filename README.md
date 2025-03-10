# Alchemist's Assistant

所有功能都可配置，请在 BepInEx\config 中找到 AlchAss.cfg 并编辑其中内容。

注意本模组和 FasterChores 在变速功能上冲突。

如果您有希望实现的其他辅助功能，欢迎联系我。

[Nexus 链接](https://www.nexusmods.com/potioncraftalchemistsimulator/mods/47)

## 功能：
1. 按住 Z 或 X 键时研磨、搅拌、加水和漩涡移动速度减少至 BepInEx\plugins\AlchAssSpeedBrewConfig.txt 中第一行或第二行的数值分之一；
2. 右键点击风箱，会使药剂热量值改变为 BepInEx\plugins\AlchAssGrindHeatConfig.txt 中的数值（按百分比计）；
3. 右键点击研杵，会使药材研磨度改变为 BepInEx\plugins\AlchAssGrindHeatConfig.txt 中的数值（按百分比计）；
4. 实时显示药水的生命值；
5. 实时显示药水的搅拌进度和搅拌方向，同时显示药水在沼泽和骷髅区内移动的累计距离；
6. 按下 / 键后会打开实时渲染搅拌方向提示线；
7. 按下 ' 键后，当药水到达漩涡时自动立即停止加水和搅拌，当药水即将离开漩涡时自动逐渐减速直至停止加水和搅拌；
8. 当药水瓶与目标接触时，实时显示药水与目标之间的旋转夹角和坐标偏差；不与目标接触时，实时显示药水的旋转和坐标；并将旋转量折算为盐量显示；
9. 按下空格键后，将药水的坐标显示在直角坐标系和极坐标系之间切换；
10. 当药水瓶与目标接触时，实时显示药水与目标之间的总体、位置和旋转偏差程度，总体偏差程度 <= 600% 时可获得 II 级效果，<= 100% 时可获得 III 级效果；
11. 实时显示鼠标右键点击的效果与药水搅拌移动路径上最接近时的偏差程度，按下 \ 键后改为显示药水搅拌终点的偏差程度；
12. 实时显示鼠标右键点击的效果与药水加水移动路径上最接近时的偏差程度；
13. 当药水瓶在漩涡中时，实时显示药水瓶加水方向与漩涡中心方向的夹角和药水瓶与漩涡中心距离，并提示漩涡半径和加水方向最大角度点；
14. 实时显示研磨程度；
15. 按住 Z 或 X 键时一次性批量炼药数量增加至 BepInEx\plugins\AlchAssSpeedBrewConfig.txt 中第一行或第二行的数值倍，如果材料不足不会增加；
16. 保存游戏时自动保存信息窗口的位置，退出并再次进入游戏时窗口位置不会改变。
