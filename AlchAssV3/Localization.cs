using HarmonyLib;
using Newtonsoft.Json;
using PotionCraft.Assemblies.DataBaseSystem.PreparedObjects;
using PotionCraft.LocalizationSystem;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace AlchAssV3
{
    public static class Localization
    {
        #region 本地化
        /// <summary>
        /// 读取本地化文件
        /// </summary>
        public static List<Variable.Localization> LoadLocalization(string path, Assembly assembly)
        {
            using var reader = new StreamReader(assembly.GetManifestResourceStream(path));
            return JsonConvert.DeserializeObject<List<Variable.Localization>>(reader.ReadToEnd());
        }

        /// <summary>
        /// 注册本地化文本
        /// </summary>
        public static void RegisterLocalization(string path, Assembly assembly)
        {
            var locs = LoadLocalization(path, assembly);
            var data = Traverse.Create(typeof(LocalizationManager)).Field("localizationData").GetValue<LocalizationData>();
            foreach (var loc in locs)
                foreach (var locale in Enum.GetValues(typeof(LocalizationManager.Locale)))
                    if (loc.values.ContainsKey($"{locale}"))
                        data.Add((int)locale, loc.key, loc.values[$"{locale}"]);
                    else
                        data.Add((int)locale, loc.key, loc.key);
        }
        #endregion


        #region 标签格式化
        /// <summary>
        /// 计算标签长度
        /// </summary>
        public static float GetLabelWidth(string[] labels, bool loc)
        {
            var width = 0f;
            foreach (var label in labels)
                if (loc)
                    width = Mathf.Max(width, Variable.LabelStyle.CalcSize(new GUIContent(LocalizationManager.GetText(label) + "0")).x);
                else
                    width = Mathf.Max(width, Variable.LabelStyle.CalcSize(new GUIContent(label + "0")).x);
            return width;
        }
        #endregion
    }
}
