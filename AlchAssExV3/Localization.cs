using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PotionCraft.LocalizationSystem;

namespace AlchAssExV3
{
    public static class Localization
    {
        #region Texts
        private readonly static AlchAssV3.Localization.LocEntry Loc_Ex_Vortex_Edge_Control = new AlchAssV3.Localization.LocEntry("AlchAssV3Ex_vortex_edge_control", new Dictionary<PotionCraft.LocalizationSystem.LocalizationManager.Locale, string>()
        {
            { LocalizationManager.Locale.en, "Vortex edge control: " },
            { LocalizationManager.Locale.zh, "漩涡边界控制："  },
        });
        private readonly static AlchAssV3.Localization.LocEntry Loc_Ex_Closest_Point_Control = new AlchAssV3.Localization.LocEntry("AlchAssV3Ex_closest_point_control", new Dictionary<PotionCraft.LocalizationSystem.LocalizationManager.Locale, string>()
        {
            { LocalizationManager.Locale.en, "Closest point control: " },
            { LocalizationManager.Locale.zh, "最近点控制："  },
        });
        private readonly static AlchAssV3.Localization.LocEntry Loc_Ex_Target_Proximity_Control = new AlchAssV3.Localization.LocEntry("AlchAssV3Ex_target_proximity_control", new Dictionary<PotionCraft.LocalizationSystem.LocalizationManager.Locale, string>()
        {
            { LocalizationManager.Locale.en, "Target proximity control: " },
            { LocalizationManager.Locale.zh, "目标接近控制："  },
        });
        #endregion

        #region Functions
        public static void SetAllLocalizations()
        {
            AlchAssV3.Localization.setLocalization(Loc_Ex_Vortex_Edge_Control);
            AlchAssV3.Localization.setLocalization(Loc_Ex_Closest_Point_Control);
            AlchAssV3.Localization.setLocalization(Loc_Ex_Target_Proximity_Control);
        }
        #endregion
    }

}
