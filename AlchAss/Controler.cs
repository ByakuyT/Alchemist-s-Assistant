using HarmonyLib;
using PotionCraft.LocalizationSystem;
using System;
using UnityEngine.InputSystem;

namespace AlchAss
{
    public static class Controler
    {
        #region 模式切换
        public static void EndMode()
        {
            if (Keyboard.current.backslashKey.wasPressedThisFrame)
            {
                Vars.endMode = !Vars.endMode;
                Helper.SpawnMessageText(LocalizationManager.GetText("aend") + LocalizationManager.GetText(Vars.endMode ? "aopen" : "aclose"));
            }
        }
        public static void PositionMode()
        {
            if (Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                Vars.xOy = !Vars.xOy;
                Helper.SpawnMessageText(LocalizationManager.GetText("axoy") + LocalizationManager.GetText(Vars.xOy ? "aopen" : "aclose"));
            }
        }
        public static void ZoneMode()
        {
            if (Keyboard.current.periodKey.wasPressedThisFrame)
            {
                Vars.zoneMode = (Vars.zoneMode + 1) % 4;
                Helper.SpawnMessageText(LocalizationManager.GetText("azone") + LocalizationManager.GetText(Vars.zoneModeName[Vars.zoneMode]));
            }
        }
        public static void DirectionLine()
        {
            if (Keyboard.current.slashKey.wasPressedThisFrame)
            {
                Vars.directionLine = !Vars.directionLine;
                Helper.SpawnMessageText(LocalizationManager.GetText("aline") + LocalizationManager.GetText(Vars.directionLine ? "aopen" : "aclose"));
                if (Vars.solventDirectionHint != null)
                    Traverse.Create(Vars.solventDirectionHint).Method("OnPositionOnMapChanged", Array.Empty<object>()).GetValue();
            }
        }
        #endregion
    }
}