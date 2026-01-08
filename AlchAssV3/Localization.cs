using System;
using HarmonyLib;
using PotionCraft.Assemblies.DataBaseSystem.PreparedObjects;
using PotionCraft.DebugObjects.DebugWindows;
using PotionCraft.LocalizationSystem;
using PotionCraft.ManagersSystem;
using PotionCraft.ManagersSystem.Ingredient;
using PotionCraft.ManagersSystem.TMP;
using PotionCraft.ObjectBased.RecipeMap.RecipeMapItem.IndicatorMapItem;
using PotionCraft.ObjectBased.RecipeMap.RecipeMapItem.VortexMapItem;
using PotionCraft.ObjectBased.UIElements.FloatingText;
using PotionCraft.Settings;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;


namespace AlchAssV3
{
    public static class Localization
    {
        #region Texts
        // Window Titles
        public static readonly LocEntry Loc_Unavailabele = new LocEntry("AlchAssV3_unavailable", new Dictionary<LocalizationManager.Locale, string>()
        {
            { LocalizationManager.Locale.en, "Unavailable" },
            { LocalizationManager.Locale.zh, "不可用" },
        });
        public static readonly LocEntry Loc_Enabled = new LocEntry("AlchAssV3_enabled", new Dictionary<LocalizationManager.Locale, string>()
        {
            { LocalizationManager.Locale.en, "Enabled" },
            { LocalizationManager.Locale.zh, "已启用" },
        });
        public static readonly LocEntry Loc_Disabled = new LocEntry("AlchAssV3_disabled", new Dictionary<LocalizationManager.Locale, string>()
        {
            { LocalizationManager.Locale.en, "Disabled" },
            { LocalizationManager.Locale.zh, "已禁用" },
        });
        private static readonly LocEntry Loc_Path_Information = new LocEntry("AlchAssV3_path_information", new Dictionary<LocalizationManager.Locale, string>()
        {
            { LocalizationManager.Locale.en, "Path Information" },
            { LocalizationManager.Locale.zh, "路径信息" },
        });
        private static readonly LocEntry Loc_Pouring_Information = new LocEntry("AlchAssV3_pouring_information", new Dictionary<LocalizationManager.Locale, string>()
        {
            { LocalizationManager.Locale.en, "Pouring Information" },
            { LocalizationManager.Locale.zh, "加水信息" },
        });
        private static readonly LocEntry Loc_Move_Information = new LocEntry("AlchAssV3_move_information", new Dictionary<LocalizationManager.Locale, string>()
        {
            { LocalizationManager.Locale.en, "Move Information" },
            { LocalizationManager.Locale.zh, "移动信息" },
        });
        private static readonly LocEntry Loc_Target_Information = new LocEntry("AlchAssV3_target_information", new Dictionary<LocalizationManager.Locale, string>()
        {
            { LocalizationManager.Locale.en, "Target Information" },
            { LocalizationManager.Locale.zh, "目标信息" },
        });
        private static readonly LocEntry Loc_Position_Information = new LocEntry("AlchAssV3_position_information", new Dictionary<LocalizationManager.Locale, string>()
        {
            { LocalizationManager.Locale.en, "Position Information" },
            { LocalizationManager.Locale.zh, "位置信息" },
        });
        private static readonly LocEntry Loc_Deviation_Information = new LocEntry("AlchAssV3_deviation_information", new Dictionary<LocalizationManager.Locale, string>()
        {
            { LocalizationManager.Locale.en, "Deviation Information" },
            { LocalizationManager.Locale.zh, "偏离信息" },
        });
        private static readonly LocEntry Loc_Vortex_Information = new LocEntry("AlchAssV3_vortex_information", new Dictionary<LocalizationManager.Locale, string>()
        {
            { LocalizationManager.Locale.en, "Vortex Information" },
            { LocalizationManager.Locale.zh, "漩涡信息" },
        });
        private static readonly LocEntry Loc_Health_Information = new LocEntry("AlchAssV3_health_information", new Dictionary<LocalizationManager.Locale, string>()
        {
            { LocalizationManager.Locale.en, "Health Information" },
            { LocalizationManager.Locale.zh, "生命信息" },
        });
        private static readonly LocEntry Loc_Grinding_Information = new LocEntry("AlchAssV3_grinding_information", new Dictionary<LocalizationManager.Locale, string>()
        {
            { LocalizationManager.Locale.en, "Grinding Information" },
            { LocalizationManager.Locale.zh, "研磨信息" },
        });
        private readonly static LocEntry Loc_Debug_Information = new LocEntry("AlchAssV3_debug_information", new Dictionary<LocalizationManager.Locale, string>()
        {
            { LocalizationManager.Locale.en, "Debug Information" },
            { LocalizationManager.Locale.zh, "调试信息" },
        });
        private readonly static LocEntry Loc_IO_Information = new LocEntry("AlchAssV3_I/O_information", new Dictionary<LocalizationManager.Locale, string>()
        {
            { LocalizationManager.Locale.en, "I/O Information" },
            { LocalizationManager.Locale.zh, "输入/输出信息" },
        });
        private readonly static LocEntry Loc_Hotkey_Information = new LocEntry("AlchAssV3_Hotkey_Information", new Dictionary<LocalizationManager.Locale, string>()
        {
            { LocalizationManager.Locale.en, "Hotkey Information" },
            { LocalizationManager.Locale.zh, "热键信息" },
        });

        // Switch Messages
        private static readonly LocEntry Loc_Message_Path_Line = new LocEntry("AlchAssV3_message_path_line", new Dictionary<LocalizationManager.Locale, string>()
        {
            { LocalizationManager.Locale.en, "Path Direction Indicator: " },
            { LocalizationManager.Locale.zh, "路径方向：" },
        });
        private static readonly LocEntry Loc_Message_Ladle_Line = new LocEntry("AlchAssV3_message_ladle_line", new Dictionary<LocalizationManager.Locale, string>()
        {
            { LocalizationManager.Locale.en, "Pour Direction Indicator: " },
            { LocalizationManager.Locale.zh, "加水方向：" },
        });
        private static readonly LocEntry Loc_Message_Target_Line = new LocEntry("AlchAssV3_message_target_line", new Dictionary<LocalizationManager.Locale, string>()
        {
            { LocalizationManager.Locale.en, "Target Direction Indicator: " },
            { LocalizationManager.Locale.zh, "目标方向：" },
        });
        private static readonly LocEntry Loc_Message_Vortex_Line = new LocEntry("AlchAssV3_message_vortex_line", new Dictionary<LocalizationManager.Locale, string>()
        {
            { LocalizationManager.Locale.en, "Vortex Direction Indicator: " },
            { LocalizationManager.Locale.zh, "漩涡方向：" },
        });
        private static readonly LocEntry Loc_Message_Path_Curve = new LocEntry("AlchAssV3_message_path_curve", new Dictionary<LocalizationManager.Locale, string>()
        {
            { LocalizationManager.Locale.en, "Path Curve Indicator: " },
            { LocalizationManager.Locale.zh, "路径曲线：" },
        });
        private static readonly LocEntry Loc_Message_Vortex_Curve = new LocEntry("AlchAssV3_message_vortex_curve", new Dictionary<LocalizationManager.Locale, string>()
        {
            { LocalizationManager.Locale.en, "Vortex Curve Indicator: " },
            { LocalizationManager.Locale.zh, "漩涡曲线：" },
        });
        private static readonly LocEntry Loc_Message_Target_Range = new LocEntry("AlchAssV3_message_target_range", new Dictionary<LocalizationManager.Locale, string>()
        {
            { LocalizationManager.Locale.en, "Target Range Indicator: " },
            { LocalizationManager.Locale.zh, "目标范围：" },
        });
        private static readonly LocEntry Loc_Message_Vortex_Range = new LocEntry("AlchAssV3_message_vortex_range", new Dictionary<LocalizationManager.Locale, string>()
        {
            { LocalizationManager.Locale.en, "Vortex Range Indicator: " },
            { LocalizationManager.Locale.zh, "漩涡范围：" },
        });
        private static readonly LocEntry Loc_Message_Area_Tracking = new LocEntry("AlchAssV3_message_area_tracking", new Dictionary<LocalizationManager.Locale, string>()
        {
            { LocalizationManager.Locale.en, "Danger Zone Detection: " },
            { LocalizationManager.Locale.zh, "骷髅检测：" },
        }); // The original function is reworked.
        private static readonly LocEntry Loc_Message_Swamp_Scaling = new LocEntry("AlchAssV3_message_swamp_scaling", new Dictionary<LocalizationManager.Locale, string>()
        {
            { LocalizationManager.Locale.en, "Swamp Retracting: " },
            { LocalizationManager.Locale.zh, "沼泽收缩：" },
        });
        private static readonly LocEntry Loc_Message_Transparency = new LocEntry("AlchAssV3_message_transparency", new Dictionary<LocalizationManager.Locale, string>()
        {
            { LocalizationManager.Locale.en, "Transparent Bottle: " },
            { LocalizationManager.Locale.zh, "透明药瓶：" },
        });
        private static readonly LocEntry Loc_Message_Polar_Mode = new LocEntry("AlchAssV3_message_polar_mode", new Dictionary<LocalizationManager.Locale, string>()
        {
            { LocalizationManager.Locale.en, "Polar mode: "},
            { LocalizationManager.Locale.zh, "极坐标模式：" }
        });
        private static readonly LocEntry Loc_Message_Salt_Degree_Mode = new LocEntry("AlchAssV3_message_salt_degree_mode", new Dictionary<LocalizationManager.Locale, string>()
        {
            { LocalizationManager.Locale.en, "Salt degree mode: "},
            { LocalizationManager.Locale.zh, "盐量度数模式：" }
        });
        // Window Sub-Titles

        private static readonly LocEntry Loc_Subtitle_Total_Deviation = new LocEntry("AlchAssV3_subtitle_total_deviation", new Dictionary<LocalizationManager.Locale, string>()
        {
            { LocalizationManager.Locale.en, "Total deviation:" },
            { LocalizationManager.Locale.zh, "总偏离：" },
        });
        private static readonly LocEntry Loc_Subtitle_Position_Deviation = new LocEntry("AlchAssV3_subtitle_position_deviation", new Dictionary<LocalizationManager.Locale, string>()
        {
            { LocalizationManager.Locale.en, "Position deviation:" },
            { LocalizationManager.Locale.zh, "位置偏离：" },
        });
        private static readonly LocEntry Loc_Subtitle_Angle_Deviation = new LocEntry("AlchAssV3_subtitle_angle_deviation", new Dictionary<LocalizationManager.Locale, string>()
        {
            { LocalizationManager.Locale.en, "Angle deviation:" },
            { LocalizationManager.Locale.zh, "角度偏离：" },
        });
        private static readonly LocEntry Loc_Subtitle_Current_Vortex_Distance = new LocEntry("AlchAssV3_subtitle_current_vortex_distance", new Dictionary<LocalizationManager.Locale, string>()
        {
            { LocalizationManager.Locale.en, "Current vortex distance:" },
            { LocalizationManager.Locale.zh, "当前漩涡距离：" },
        });
        private readonly static LocEntry Loc_Subtitle_Maximal_Vortex_Distance = new LocEntry("AlchAssV3_subtitle_maximal_vortex_distance", new Dictionary<LocalizationManager.Locale, string>()
        {
            { LocalizationManager.Locale.en, "Maximal vortex distance:" },
            { LocalizationManager.Locale.zh, "最大漩涡距离：" },
        });
        private readonly static LocEntry Loc_Subtitle_Vortex_Direction = new LocEntry("AlchAssV3_subtitle_vortex_direction", new Dictionary<LocalizationManager.Locale, string>()
        {
            { LocalizationManager.Locale.en, "Vortex direction:" },
            { LocalizationManager.Locale.zh, "漩涡方向：" },
        });
        private readonly static LocEntry Loc_Subtitle_Vortex_Heat_Move_Direction = new LocEntry("AlchAssV3_subtitle_heat_move_direction", new Dictionary<LocalizationManager.Locale, string>()
        {
            { LocalizationManager.Locale.en, "Heat move direction:" },
            { LocalizationManager.Locale.zh, "加热位移方向：" },
        });
        private static readonly LocEntry Loc_Subtitle_Closest_Point_direction = new LocEntry("AlchAssV3_subtitle_closest_point_direction", new Dictionary<LocalizationManager.Locale, string>()
        {
            { LocalizationManager.Locale.en, "Closest point direction:" },
            { LocalizationManager.Locale.zh, "最近点方向：" },
        });
        private static readonly LocEntry Loc_Subtitle_Delta_Anlge_To_Target = new LocEntry("AlchAssV3_subtitle_delta_angle_to_target", new Dictionary<LocalizationManager.Locale, string>()
        {
            { LocalizationManager.Locale.en, "Delta angle to target:" },
            { LocalizationManager.Locale.zh, "与目标夹角差：" },
        });
        private static readonly LocEntry Loc_Subtitle_Life_Salt_Needed = new LocEntry("AlchAssV3_subtitle_life_salt", new Dictionary<LocalizationManager.Locale, string>()
        {
            { LocalizationManager.Locale.en, "Life salt needed:" },
            { LocalizationManager.Locale.zh, "需要生命之盐：" },
        });
        private readonly static LocEntry Loc_Subtitle_Stir_Progression = new LocEntry("AlchAssV3_subtitle_stir_progression", new Dictionary<LocalizationManager.Locale, string>()
        {
            { LocalizationManager.Locale.en, "Stir progression:" },
            { LocalizationManager.Locale.zh, "搅拌进度：" },
        });
        private readonly static LocEntry Loc_Subtitle_Path_Direction = new LocEntry("AlchAssV3_subtitle_path_direction", new Dictionary<LocalizationManager.Locale, string>()
        {
            { LocalizationManager.Locale.en, "Path direction:" },
            { LocalizationManager.Locale.zh, "路径方向：" },
        });
        private readonly static LocEntry Loc_Subtitle_Pour_Direction = new LocEntry("AlchAssV3_subtitle_pour_direction", new Dictionary<LocalizationManager.Locale, string>()
        {
            { LocalizationManager.Locale.en, "Pour direction:" },
            { LocalizationManager.Locale.zh, "加水方向：" },
        });
        private readonly static LocEntry Loc_Subtitle_Target_Effect_ID = new LocEntry("AlchAssV3_subtitle_target_effect_id", new Dictionary<LocalizationManager.Locale, string>()
        {
            { LocalizationManager.Locale.en, "Target effect ID:" },
            { LocalizationManager.Locale.zh, "目标效果ID：" },
        });
        private readonly static LocEntry Loc_Subtitle_Relative_Rotation = new LocEntry("AlchAssV3_subtitle_relative_rotation", new Dictionary<LocalizationManager.Locale, string>()
        {
            { LocalizationManager.Locale.en, "Relative rotation:" },
            { LocalizationManager.Locale.zh, "相对旋转：" },
        });
        private readonly static LocEntry Loc_Subtitle_Target_Direction = new LocEntry("AlchAssV3_subtitle_target_direction", new Dictionary<LocalizationManager.Locale, string>()
        {
            { LocalizationManager.Locale.en, "Target direction:" },
            { LocalizationManager.Locale.zh, "目标方向：" },
        });
        private readonly static LocEntry Loc_Subtitle_Rotation = new LocEntry("AlchAssV3_subtitle_rotation_salt", new Dictionary<LocalizationManager.Locale, string>()
        {
            { LocalizationManager.Locale.en, "Rotation salt:" },
            { LocalizationManager.Locale.zh, "旋转盐量：" },
        });
        private readonly static LocEntry Loc_Subtitle_Current_Health = new LocEntry("AlchAssV3_subtitle_current_health", new Dictionary<LocalizationManager.Locale, string>()
        {
            { LocalizationManager.Locale.en, "Current health:" },
            { LocalizationManager.Locale.zh, "当前生命值：" },
        });
        private readonly static LocEntry Loc_Subtitle_Grinding_Progression = new LocEntry("AlchAssV3_subtitle_grinding_progression", new Dictionary<LocalizationManager.Locale, string>()
        {
            { LocalizationManager.Locale.en, "Grinding progression:" },
            { LocalizationManager.Locale.zh, "研磨进度：" },
        });
        // Pop-up Texts
        private readonly static LocEntry Loc_Popup_Selected_Effect = new LocEntry("AlchAssV3_popup_selected_effect", new Dictionary<LocalizationManager.Locale, string>()
        {
            { LocalizationManager.Locale.en, "Selected Effect: " },
            { LocalizationManager.Locale.zh, "已选择效果：" },
        });
        private readonly static LocEntry Loc_Popup_Unselected_Effort = new LocEntry("AlchAssV3_subtitle_unselected_effect", new Dictionary<LocalizationManager.Locale, string>()
        {
            { LocalizationManager.Locale.en, "Unselected Effect" },
            { LocalizationManager.Locale.zh, "已取消选择效果" },
        });
        private readonly static LocEntry Loc_Popup_Selected_Vortex = new LocEntry("AlchAssV3_popup_selected_vortex", new Dictionary<LocalizationManager.Locale, string>()
        {
            { LocalizationManager.Locale.en, "Selected {0}-th vortex." },
            { LocalizationManager.Locale.zh, "已选择第{0}个漩涡" },
        });
        private readonly static LocEntry Loc_Popup_Unselected_Vortex = new LocEntry("AlchAssV3_popup_unselected_vortex", new Dictionary<LocalizationManager.Locale, string>()
        {
            { LocalizationManager.Locale.en, "Unselected Vortex" },
            { LocalizationManager.Locale.zh, "已取消选择漩涡" },
        });
        // Hotkey Texts
        private readonly static LocEntry Loc_Hotkey_Switch_Key_Mode = new LocEntry("AlchAssV3_hotkey_switch_key_mode", new Dictionary<LocalizationManager.Locale, string>()
        {
            { LocalizationManager.Locale.en, "Switch Key Mode:" },
            { LocalizationManager.Locale.zh, "切换按键模式：" },
        });
        private readonly static LocEntry Loc_Hotkey_Select_Prev_Vortex = new LocEntry("AlchAssV3_hotkey_select_prev_vortex", new Dictionary<LocalizationManager.Locale, string>()
        {
            { LocalizationManager.Locale.en, "Select Previous Vortex:" },
            { LocalizationManager.Locale.zh, "选择上一个漩涡：" },
        });
        private readonly static LocEntry Loc_Hotkey_Select_Next_Vortex = new LocEntry("AlchAssV3_hotkey_select_next_vortex", new Dictionary<LocalizationManager.Locale, string>()
        {
            { LocalizationManager.Locale.en, "Select Next Vortex:" },
            { LocalizationManager.Locale.zh, "选择下一个漩涡：" },
        });
        private readonly static LocEntry Loc_Hotkey_Select_Near_Vortex = new LocEntry("AlchAssV3_hotkey_select_near_vortex", new Dictionary<LocalizationManager.Locale, string>()
        {
            { LocalizationManager.Locale.en, "Select Nearest Vortex:" },
            { LocalizationManager.Locale.zh, "选择最近的漩涡：" },
        });
        private readonly static LocEntry Loc_Hotkey_Select_No_Vortex = new LocEntry("AlchAssV3_hotkey_select_no_vortex", new Dictionary<LocalizationManager.Locale, string>()
        {
            { LocalizationManager.Locale.en, "Deselect Vortex:" },
            { LocalizationManager.Locale.zh, "取消选择漩涡：" },
        });
        private readonly static LocEntry Loc_Hotkey_Select_Effect = new LocEntry("AlchAssV3_hotkey_select_effect", new Dictionary<LocalizationManager.Locale, string>()
        {
            { LocalizationManager.Locale.en, "Select Effect:" },
            { LocalizationManager.Locale.zh, "选择效果：" },
        });
        private readonly static LocEntry Loc_Hotkey_Polar_Mode = new LocEntry("AlchAssV3_hotkey_polar_mode", new Dictionary<LocalizationManager.Locale, string>()
        {
            { LocalizationManager.Locale.en, "Toggle Polar Mode:" },
            { LocalizationManager.Locale.zh, "切换极坐标模式：" },
        });
        private readonly static LocEntry Loc_Hotkey_Salt_Degree_Mode = new LocEntry("AlchAssV3_hotkey_salt_degree_mode", new Dictionary<LocalizationManager.Locale, string>()
        {
            { LocalizationManager.Locale.en, "Toggle Salt Degree Mode:" },
            { LocalizationManager.Locale.zh, "切换盐量度数模式：" },
        });
        #endregion


        #region Functions
        public static void setLocalization(LocEntry locEntry)
        {
            var localizationData = AccessTools.StaticFieldRefAccess<LocalizationData>(typeof(LocalizationManager), "localizationData");
            for (var localeIndex = 0; localeIndex < 13; ++localeIndex)
            {
                var localizedText = locEntry.LocaleText[localeIndex] != null ? locEntry.LocaleText[localeIndex] : locEntry.LocaleText[0];
                localizationData.Add(localeIndex, locEntry.LocaleKey, localizedText);
            }
        }
        public static void SetAllLocalizations()
        {
            setLocalization(Loc_Unavailabele);
            setLocalization(Loc_Enabled);
            setLocalization(Loc_Disabled);
            setLocalization(Loc_Path_Information);
            setLocalization(Loc_Pouring_Information);
            setLocalization(Loc_Move_Information);
            setLocalization(Loc_Target_Information);
            setLocalization(Loc_Position_Information);
            setLocalization(Loc_Deviation_Information);
            setLocalization(Loc_Vortex_Information);
            setLocalization(Loc_Health_Information);
            setLocalization(Loc_Grinding_Information);
            setLocalization(Loc_Debug_Information);
            setLocalization(Loc_IO_Information);
            setLocalization(Loc_Hotkey_Information);
            setLocalization(Loc_Message_Path_Line);
            setLocalization(Loc_Message_Ladle_Line);
            setLocalization(Loc_Message_Target_Line);
            setLocalization(Loc_Message_Vortex_Line);
            setLocalization(Loc_Message_Path_Curve);
            setLocalization(Loc_Message_Vortex_Curve);
            setLocalization(Loc_Message_Target_Range);
            setLocalization(Loc_Message_Vortex_Range);
            setLocalization(Loc_Message_Area_Tracking);
            setLocalization(Loc_Message_Swamp_Scaling);
            setLocalization(Loc_Message_Transparency);
            setLocalization(Loc_Message_Polar_Mode);
            setLocalization(Loc_Message_Salt_Degree_Mode);
            setLocalization(Loc_Subtitle_Total_Deviation);
            setLocalization(Loc_Subtitle_Position_Deviation);
            setLocalization(Loc_Subtitle_Angle_Deviation);
            setLocalization(Loc_Subtitle_Current_Vortex_Distance);
            setLocalization(Loc_Subtitle_Maximal_Vortex_Distance);
            setLocalization(Loc_Subtitle_Vortex_Direction);
            setLocalization(Loc_Subtitle_Vortex_Heat_Move_Direction);
            setLocalization(Loc_Subtitle_Closest_Point_direction);
            setLocalization(Loc_Subtitle_Delta_Anlge_To_Target);
            setLocalization(Loc_Subtitle_Life_Salt_Needed);
            setLocalization(Loc_Subtitle_Stir_Progression);
            setLocalization(Loc_Subtitle_Path_Direction);
            setLocalization(Loc_Subtitle_Pour_Direction);
            setLocalization(Loc_Subtitle_Target_Effect_ID);
            setLocalization(Loc_Subtitle_Relative_Rotation);
            setLocalization(Loc_Subtitle_Target_Direction);
            setLocalization(Loc_Subtitle_Rotation);
            setLocalization(Loc_Subtitle_Current_Health);
            setLocalization(Loc_Subtitle_Grinding_Progression);
            setLocalization(Loc_Popup_Selected_Effect);
            setLocalization(Loc_Popup_Unselected_Effort);
            setLocalization(Loc_Popup_Selected_Vortex);
            setLocalization(Loc_Popup_Unselected_Vortex);
            setLocalization(Loc_Hotkey_Switch_Key_Mode);
            setLocalization(Loc_Hotkey_Select_Prev_Vortex);
            setLocalization(Loc_Hotkey_Select_Next_Vortex);
            setLocalization(Loc_Hotkey_Select_Near_Vortex);
            setLocalization(Loc_Hotkey_Select_No_Vortex);
            setLocalization(Loc_Hotkey_Select_Effect);
            setLocalization(Loc_Hotkey_Polar_Mode);
            setLocalization(Loc_Hotkey_Salt_Degree_Mode);
        }
        #endregion


        #region Structs
        public struct LocEntry
        {
            public string LocaleKey;
            public string[] LocaleText;

            public LocEntry()
            {
                this.LocaleKey = default;
                this.LocaleText = new string[13];
            }
            public LocEntry(string localeKey, Dictionary<LocalizationManager.Locale, string> localeText)
            {
                this.LocaleKey = localeKey;
                this.LocaleText = new string[13];
                foreach (var item in localeText)
                {
                    this.LocaleText[(int)item.Key] = item.Value;
                }
            }
        }
        #endregion

    }
}
