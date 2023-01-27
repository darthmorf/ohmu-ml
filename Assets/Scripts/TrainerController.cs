using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using UnityEditor;
using UnityEditor.PackageManager.UI;
using UnityEditor.VersionControl;
using UnityEngine;


public class TrainingController : EditorWindow
{
    // State
    static Process process;
    string modelName;
    string configFile = "trainer-config.yaml";

    [MenuItem("Window/Manage Training")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(TrainingController));
    }

    void OnGUI()
    {
        modelName = EditorGUILayout.TextField("Model Name", modelName);
        configFile = EditorGUILayout.TextField("Config File", configFile);

        EditorGUILayout.Space();

        if (GUILayout.Button("Start Training"))
        {
            StartTraining(false);
        }

        if (GUILayout.Button("Resume Training"))
        {
            StartTraining(false);
        }
    }

    public void StartTraining(bool resume)
    {
        process = new Process();

        string forceResume = "force";

        if (resume)
        {
            forceResume = "resume";
        }

        process.StartInfo.FileName = "cmd.exe";
        process.StartInfo.Arguments = $"/C mlagents-learn {configFile} --run-id={modelName} --{forceResume} --results-dir=\"./Assets/Training-Results\" & pause";
        process.StartInfo.UseShellExecute = true;
        process.StartInfo.CreateNoWindow = false;

        process.Start();
    }
}