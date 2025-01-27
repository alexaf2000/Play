﻿using System;
using System.IO;
using UnityEngine;

public class SettingsManager : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void Init()
    {
        settingsPath = null;
        settings = null;
        initializedResolution = false;
    }

    public static SettingsManager Instance
    {
        get
        {
            return GameObjectUtils.FindComponentWithTag<SettingsManager>("SettingsManager");
        }
    }

    // The settings must be written to the same path they have been loaded from.
    // This field stores the path from where settings have been loaded / will be saved.
    private static string settingsPath;

    // The settings field is static to persist it across scene changes.
    // The SettingsManager is meant to be used as a singleton, such that this static field should not be a problem.
    private static Settings settings;
    public Settings Settings
    {
        get
        {
            if (settings == null)
            {
                Reload();
            }
            return settings;
        }
    }

    private static bool initializedResolution;

    // Non-static settings field for debugging of the settings in the Unity Inspector.
    public Settings nonStaticSettings;

    void Start()
    {
        // Load reference from last scene if needed
        nonStaticSettings = settings;
        if (!initializedResolution)
        {
            initializedResolution = true;
            // GetCurrentAppResolution may only be called from Start() and Awake(). This is why it is done here.
            Settings.GraphicSettings.resolution = ApplicationUtils.GetScreenResolution();
        }
    }

    void OnDisable()
    {
        Save();
    }

    public void Save()
    {
        string json = JsonConverter.ToJson(Settings, true);
        File.WriteAllText(GetSettingsPath(), json);
    }

    public void Reload()
    {
        using (new DisposableStopwatch("Loading the settings took <millis> ms"))
        {
            string loadedSettingsPath = GetSettingsPath();
            if (!File.Exists(loadedSettingsPath))
            {
                UnityEngine.Debug.LogWarning($"Settings file not found. Creating default settings at {loadedSettingsPath}.");
                settings = new Settings();
                Save();
                return;
            }
            string fileContent = File.ReadAllText(loadedSettingsPath);
            settings = JsonConverter.FromJson<Settings>(fileContent);
            nonStaticSettings = settings;
            OverwriteSettingsWithCommandLineArguments();
        }
    }

    private void OverwriteSettingsWithCommandLineArguments()
    {
        string settingsOverwriteJson = ApplicationManager.Instance.GetCommandLineArgument("--settingsOverwriteJson");
        if (!settingsOverwriteJson.IsNullOrEmpty())
        {
            settingsOverwriteJson = settingsOverwriteJson.Strip("\"", "\"");
            settingsOverwriteJson = settingsOverwriteJson.Strip("'", "'");
            try
            {
                JsonConverter.FillFromJson(settingsOverwriteJson, settings);
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError("OverwriteSettingsWithCommandLineArguments failed");
                UnityEngine.Debug.LogException(e);
            }
        }
    }

    public string GetSettingsPath()
    {
        if (settingsPath.IsNullOrEmpty())
        {
            string commandLineSettingsPath = ApplicationManager.Instance.GetCommandLineArgument("--settingsPath");
            commandLineSettingsPath = commandLineSettingsPath.Strip("\"", "\"");
            commandLineSettingsPath = commandLineSettingsPath.Strip("'", "'");
            if (!commandLineSettingsPath.IsNullOrEmpty())
            {
                settingsPath = commandLineSettingsPath;
            }
            else
            {
                settingsPath = Path.Combine(Application.persistentDataPath, "Settings.json");
            }
        }
        return settingsPath;
    }
}
