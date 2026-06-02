using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using SpaceCraft;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using TMPro;
using UnityEngine;

namespace PlanetCrafterTranslationMod;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public sealed class Plugin : BaseUnityPlugin
{
    public const string PluginGuid = "actepukc.theplanetcrafter.uitranslationbulgarian";
    public const string PluginName = "(UI) Bulgarian Translation";
    public const string PluginVersion = "0.1.0";

    internal static readonly Dictionary<string, string> Labels = new Dictionary<string, string>(StringComparer.Ordinal);

    private static string targetLanguage = "bulgarian";
    private static string currentLanguage = string.Empty;
    private static string pluginDir = string.Empty;
    private static string translationsDir = string.Empty;
    private static string dumpsDir = string.Empty;
    private static string languageFilePath = string.Empty;
    private static string dumpFilePath = string.Empty;
    private static ManualLogSource log;
    private static bool localizationLoaded;
    private static readonly HashSet<string> dumpedLines = new HashSet<string>(StringComparer.Ordinal);
    private static ConfigEntry<bool> dumpTranslations;
    private static ConfigEntry<bool> checkMissingKeys;
    private static ConfigEntry<bool> enableTranslationOverrides;
    private static ConfigEntry<string> labelsFileName;
    private static ConfigEntry<string> targetLanguageName;
    private static ConfigEntry<bool> assumeTargetLanguageOnStartup;

    private void Awake()
    {
        log = Logger;

        pluginDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? Paths.PluginPath;
        translationsDir = Path.Combine(pluginDir, "translations");
        dumpsDir = Path.Combine(pluginDir, "dumps");
        Directory.CreateDirectory(translationsDir);
        Directory.CreateDirectory(dumpsDir);

        enableTranslationOverrides = Config.Bind("General", "EnableTranslationOverrides", true, "Load labels.txt and inject it into the configured game language slot.");
        targetLanguageName = Config.Bind("General", "TargetLanguageName", "bulgarian", "Language key to replace or add inside the game's localization dictionary.");
        assumeTargetLanguageOnStartup = Config.Bind("General", "AssumeTargetLanguageOnStartup", false, "Treat the target language slot as active before the game reports the selected language.");
        labelsFileName = Config.Bind("General", "LabelsFileName", "labels.txt", "Translation file name inside the translations folder.");
        dumpTranslations = Config.Bind("Diagnostics", "DumpTranslations", false, "Dump observed localization entries to dumps/translation-dump.txt.");
        checkMissingKeys = Config.Bind("Diagnostics", "CheckMissingKeys", false, "Log english keys that are missing from labels.txt.");

        targetLanguage = NormalizeLanguageKey(targetLanguageName.Value);
        if (assumeTargetLanguageOnStartup.Value)
        {
            currentLanguage = targetLanguage;
        }

        languageFilePath = Path.Combine(translationsDir, labelsFileName.Value);
        dumpFilePath = Path.Combine(dumpsDir, "translation-dump.txt");
        LoadLabels(languageFilePath);

        Harmony.CreateAndPatchAll(typeof(Plugin));
        StartCoroutine(WaitForLocalizationReady());

        log.LogInfo($"{PluginName} {PluginVersion} loaded");
        log.LogInfo($"target language slot: {targetLanguage}");
        log.LogInfo($"translation entries: {Labels.Count}");
        log.LogInfo($"labels path: {languageFilePath}");
        log.LogInfo($"dump path: {dumpFilePath}");
    }

    private static void LoadLabels(string path)
    {
        Labels.Clear();

        if (!File.Exists(path))
        {
            log.LogWarning($"Translation file not found: {path}");
            return;
        }

        foreach (var rawLine in File.ReadAllLines(path, Encoding.UTF8))
        {
            var line = rawLine.Trim();
            if (line.Length == 0 || line.StartsWith("#") || line.StartsWith("//"))
            {
                continue;
            }

            var separatorIndex = FindSeparator(line, '=', '.', '@', '~', '*');
            if (separatorIndex <= 0)
            {
                continue;
            }

            var key = line.Substring(0, separatorIndex).Trim();
            var value = DecodeLabelValue(line.Substring(separatorIndex + 1));

            if (key.Length != 0)
            {
                Labels[key] = value;
            }
        }
    }

    private static int FindSeparator(string line, params char[] separators)
    {
        var result = -1;

        foreach (var separator in separators)
        {
            var index = line.IndexOf(separator);
            if (index < 0)
            {
                continue;
            }

            if (result < 0 || index < result)
            {
                result = index;
            }
        }

        return result;
    }

    private static string DecodeLabelValue(string value)
    {
        return value
            .Replace("\\r\\n", "\n")
            .Replace("\\n", "\n")
            .Replace("\\r", "\n")
            .Replace("\\t", "\t");
    }

    private static IEnumerator WaitForLocalizationReady()
    {
        while (!localizationLoaded)
        {
            yield return new WaitForSecondsRealtime(0.1f);
        }

        RefreshVisibleTexts();
    }

    private static void RefreshVisibleTexts()
    {
        AdjustNewsletterButton();

        foreach (var localizedText in UnityEngine.Object.FindObjectsByType<LocalizedText>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            localizedText.UpdateTranslation();
        }
    }

    private static void AdjustNewsletterButton()
    {
        foreach (var localizedText in UnityEngine.Object.FindObjectsByType<LocalizedText>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (localizedText == null || localizedText.textId != "Newsletter_Button")
            {
                continue;
            }

            var tmp = localizedText.GetComponent<TextMeshProUGUI>();
            if (tmp == null)
            {
                continue;
            }

            tmp.textWrappingMode = TextWrappingModes.NoWrap;
            tmp.enableAutoSizing = true;
            tmp.margin = new Vector4(5f, 5f, 5f, 5f);
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Localization), nameof(Localization.SetLangage))]
    private static void Localization_SetLangage_Prefix(string langage)
    {
        currentLanguage = NormalizeLanguageKey(langage);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Localization), "LoadLocalization")]
    private static void Localization_LoadLocalization_Postfix(
        Dictionary<string, Dictionary<string, string>> ___localizationDictionary,
        List<string> ___availableLanguages,
        bool ___hasLoadedSuccesfully)
    {
        localizationLoaded = ___hasLoadedSuccesfully;

        DumpLocalizationDictionary(___localizationDictionary);

        if (!enableTranslationOverrides.Value)
        {
            return;
        }

        if (!___availableLanguages.Contains(targetLanguage))
        {
            ___availableLanguages.Add(targetLanguage);
        }

        if (!GameConfig.TranslatedLangages.Contains(targetLanguage))
        {
            GameConfig.TranslatedLangages.Add(targetLanguage);
        }

        ___localizationDictionary[targetLanguage] = Labels;

        if (checkMissingKeys.Value && ___localizationDictionary.TryGetValue("english", out var english))
        {
            var missing = 0;
            foreach (var key in english.Keys)
            {
                if (Labels.ContainsKey(key))
                {
                    continue;
                }

                missing++;
                log.LogWarning($"Missing translation key: {key}");
            }

            log.LogInfo($"Missing translation keys: {missing}");
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Intro), "Start")]
    private static void Intro_Start_Postfix()
    {
        if (IsTargetLanguageSelected())
        {
            RefreshVisibleTexts();
        }
        else
        {
            AdjustNewsletterButton();
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(UnlockablesGraph), "OnEnable")]
    private static void UnlockablesGraph_OnEnable_Prefix(UnlockablesGraph __instance)
    {
        if (!IsTargetLanguageSelected())
        {
            return;
        }

        if (__instance.worldUnitType == DataConfig.WorldUnitType.Terraformation)
        {
            __instance.unitLabel.enableAutoSizing = true;
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(ToxicCountDisplayer), "OnEnable")]
    private static void ToxicCountDisplayer_OnEnable_Prefix(ToxicCountDisplayer __instance)
    {
        var toxicElements = __instance.transform.Find("ToxicArea/ToxicElements");
        if (toxicElements == null)
        {
            return;
        }

        var tmp = toxicElements.GetComponent<TextMeshProUGUI>();
        if (tmp == null)
        {
            return;
        }

        tmp.autoSizeTextContainer = true;
        tmp.textWrappingMode = TextWrappingModes.NoWrap;
        tmp.enabled = false;
        tmp.enabled = true;
    }

    private static bool IsTargetLanguageSelected()
    {
        return string.Equals(currentLanguage, targetLanguage, StringComparison.OrdinalIgnoreCase);
    }

    private static string NormalizeLanguageKey(string value)
    {
        return (value ?? string.Empty).Trim().ToLowerInvariant();
    }

    private static void DumpLocalizationDictionary(Dictionary<string, Dictionary<string, string>> localizationDictionary)
    {
        if (!dumpTranslations.Value || localizationDictionary == null)
        {
            return;
        }

        foreach (var languageEntry in localizationDictionary)
        {
            foreach (var pair in languageEntry.Value)
            {
                DumpLine(languageEntry.Key, pair.Key, pair.Value);
            }
        }
    }

    private static void DumpLine(string languageName, string key, string value)
    {
        var normalized = $"{languageName}\t{key}\t{value}";
        lock (dumpedLines)
        {
            if (!dumpedLines.Add(normalized))
            {
                return;
            }
        }

        try
        {
            File.AppendAllText(dumpFilePath, normalized + Environment.NewLine, Encoding.UTF8);
        }
        catch (Exception ex)
        {
            log.LogWarning($"Failed to append translation dump: {ex.Message}");
        }
    }
}
