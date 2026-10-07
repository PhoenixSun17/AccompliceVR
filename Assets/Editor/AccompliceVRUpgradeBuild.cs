using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// Editor-only migration verification. Does not save scenes or change PlayerSettings.
public static class AccompliceVRUpgradeBuild
{
    private const string ProtectedProject = @"C:\AccompliceVR";
    private static readonly string[] Scenes =
    {
        "Assets/Scenes/AVRPilot.unity",
        "Assets/Scenes/AVRCoPilot.unity"
    };
    private static readonly string[] Roles = { "Pilot", "Copilot" };
    private static readonly string[] Executables = { "AVRPilot.exe", "AVRCoPilot.exe" };

    [Serializable]
    private class MissingScript
    {
        public string hierarchy;
        public int count;
    }

    [Serializable]
    private class SceneAudit
    {
        public string scene;
        public int gameObjects;
        public int missingScripts;
        public int baselineKnownMissing;
        public int unexpectedMissing;
        public string error;
        public List<MissingScript> objects = new List<MissingScript>();
    }

    [Serializable]
    private class AuditReport
    {
        public string utc;
        public string unityVersion;
        public string project;
        public bool passed;
        public bool allowBaselineMissingPortal;
        public bool passedWithBaselineException;
        public List<SceneAudit> scenes = new List<SceneAudit>();
    }

    [Serializable]
    private class BuildMessage
    {
        public string step;
        public string type;
        public string message;
    }

    [Serializable]
    private class PlayerReport
    {
        public string utc;
        public string unityVersion;
        public string scene;
        public string output;
        public string result;
        public int errors;
        public int warnings;
        public ulong bytes;
        public double seconds;
        public string exception;
        public List<BuildMessage> messages = new List<BuildMessage>();
    }

    [Serializable]
    private class NativeSmokeReport
    {
        public string utc, unityVersion, graphicsBackend, error, cleanupError;
        public bool passed;
        public List<string> steps = new List<string>();
    }

    // Offline native ABI smoke only: no scene loading, signaling, SDP, ICE, or media encoding.
    public static void NativeSmoke()
    {
        string project = SafeProject();
        var report = new NativeSmokeReport { utc = DateTime.UtcNow.ToString("o"),
            unityVersion = Application.unityVersion, graphicsBackend = SystemInfo.graphicsDeviceType.ToString() };
        Unity.WebRTC.RTCPeerConnection peer = null;
        try
        {
            if (!Application.isBatchMode || EditorApplication.isPlaying || EditorUtility.scriptCompilationFailed)
                throw new InvalidOperationException("Run in a fresh, compiled batch Editor outside Play mode.");
            if (SystemInfo.graphicsDeviceType != UnityEngine.Rendering.GraphicsDeviceType.Direct3D11)
                throw new InvalidOperationException("Native smoke requires -force-d3d11 without -nographics.");
            // 3.0.0-pre.4 initializes its native context through Editor assembly reload hooks.
            bool limitTextureSize = Unity.WebRTC.WebRTC.enableLimitTextureSize;
            report.steps.Add("Package-owned native context available; limitTextureSize=" + limitTextureSize);
            // Outside Play mode its callback dispatcher needs the package's runtime initializer.
            typeof(Unity.WebRTC.WebRTC).GetMethod("RuntimeInitializeOnLoadMethod",
                System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic).Invoke(null, null);
            report.steps.Add("Package callback dispatcher initialized");
            peer = new Unity.WebRTC.RTCPeerConnection();
            report.steps.Add("Native RTCPeerConnection created");
            if (peer.ConnectionState != Unity.WebRTC.RTCPeerConnectionState.New)
                throw new InvalidOperationException("Unexpected initial native peer state: " + peer.ConnectionState);
            report.steps.Add("Native peer state queried: New");
        }
        catch (Exception exception) { report.error = exception.ToString(); }
        finally
        {
            try
            {
                if (peer != null) { peer.Dispose(); report.steps.Add("Native peer disposed"); }
                Unity.WebRTC.WebRTC.ExecutePendingTasks(100);
            }
            catch (Exception exception) { report.cleanupError = exception.ToString(); }
            report.passed = string.IsNullOrEmpty(report.error) && string.IsNullOrEmpty(report.cleanupError);
            WriteReport(project, "native-smoke.json", report);
        }
        if (!report.passed) throw new InvalidOperationException("Native smoke failed; see Logs/Unity66Upgrade/native-smoke.json.");
        Debug.Log("WebRTC native peer smoke passed; media streaming and headset behavior remain untested.");
    }

    [MenuItem("AccompliceVR/Upgrade/Validate Pilot and Copilot scenes")]
    public static void ValidateScenes()
    {
        string project = SafeProject();
        if (EditorApplication.isCompiling)
            throw new InvalidOperationException("Wait for script compilation before validating.");
        if (EditorUtility.scriptCompilationFailed)
            throw new InvalidOperationException("Script compilation failed; fix compiler errors before validating/building.");
        for (int index = 0; index < SceneManager.sceneCount; index++)
        {
            if (SceneManager.GetSceneAt(index).isDirty)
                throw new InvalidOperationException("Save or discard scene edits before migration validation; the helper never saves scenes.");
        }

        var report = new AuditReport
        {
            utc = DateTime.UtcNow.ToString("o"),
            unityVersion = Application.unityVersion,
            project = project,
            passed = true,
            allowBaselineMissingPortal = Array.IndexOf(Environment.GetCommandLineArgs(),
                "-accompliceAllowBaselineMissingPortal") >= 0
        };
        foreach (string path in Scenes)
        {
            var audit = new SceneAudit { scene = path };
            report.scenes.Add(audit);
            try
            {
                if (!File.Exists(Path.Combine(project, path)))
                    throw new FileNotFoundException("Required player scene is missing.", path);
                bool knownPortalUnchanged = path == Scenes[0] && HasKnownDisabledPortalComponent(Path.Combine(project, path));
                var previousActiveScene = SceneManager.GetActiveScene();
                var scene = SceneManager.GetSceneByPath(path);
                bool wasPresent = scene.IsValid();
                bool openedForAudit = !wasPresent || !scene.isLoaded;
                if (openedForAudit)
                    scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
                try
                {
                    foreach (GameObject root in scene.GetRootGameObjects())
                    {
                        foreach (Transform item in root.GetComponentsInChildren<Transform>(true))
                        {
                            audit.gameObjects++;
                            int count = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(item.gameObject);
                            if (count == 0)
                                continue;
                            string hierarchy = Hierarchy(item);
                            audit.missingScripts += count;
                            audit.objects.Add(new MissingScript { hierarchy = hierarchy, count = count });
                            // Reproduced in the untouched Unity 2022 baseline: one Portal component.
                            if (path == "Assets/Scenes/AVRPilot.unity" && hierarchy == "Overlayer/Portal" &&
                                count == 1 && audit.baselineKnownMissing == 0 && knownPortalUnchanged)
                                audit.baselineKnownMissing = 1;
                            else
                                audit.unexpectedMissing += count;
                        }
                    }
                }
                finally
                {
                    if (openedForAudit)
                        EditorSceneManager.CloseScene(scene, !wasPresent);
                    if (previousActiveScene.IsValid() && previousActiveScene.isLoaded)
                        SceneManager.SetActiveScene(previousActiveScene);
                }
            }
            catch (Exception exception)
            {
                audit.error = exception.ToString();
            }
            report.passed &= (report.allowBaselineMissingPortal ? audit.unexpectedMissing == 0 : audit.missingScripts == 0)
                && string.IsNullOrEmpty(audit.error);
            report.passedWithBaselineException |= report.allowBaselineMissingPortal && audit.baselineKnownMissing > 0;
        }
        report.passedWithBaselineException &= report.passed;
        WriteReport(project, "scene-audit.json", report);
        if (!report.passed)
            throw new InvalidOperationException("Scene validation failed; see Logs/Unity66Upgrade/scene-audit.json.");
        Debug.Log(report.passedWithBaselineException
            ? "AccompliceVR scene audit passed under the explicit baseline exception: Pilot Overlayer/Portal has one known missing script; no new missing scripts."
            : "AccompliceVR scene audit passed: Pilot and Copilot contain no missing MonoBehaviour scripts.");
    }

    [MenuItem("AccompliceVR/Upgrade/Build Windows Pilot and Copilot")]
    public static void BuildAll()
    {
        string project = SafeProject();
        string outputRoot = SafeChild(project, GetArgument("-accompliceBuildRoot") ?? "Builds/Unity66");
        ValidateScenes();

        bool allPassed = true;
        for (int index = 0; index < Scenes.Length; index++)
        {
            string output = SafeChild(project, Path.Combine(outputRoot, Roles[index], Executables[index]));
            var details = new PlayerReport
            {
                utc = DateTime.UtcNow.ToString("o"),
                unityVersion = Application.unityVersion,
                scene = Scenes[index],
                output = output,
                result = "Failed"
            };
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(output));
                BuildReport report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
                {
                    scenes = new[] { Scenes[index] },
                    locationPathName = output,
                    target = BuildTarget.StandaloneWindows64,
                    options = BuildOptions.None
                });
                details.result = report.summary.result.ToString();
                details.errors = Convert.ToInt32(report.summary.totalErrors);
                details.warnings = Convert.ToInt32(report.summary.totalWarnings);
                details.bytes = report.summary.totalSize;
                details.seconds = report.summary.totalTime.TotalSeconds;
                foreach (BuildStep step in report.steps)
                {
                    foreach (UnityEditor.Build.Reporting.BuildStepMessage message in step.messages)
                    {
                        if (message.type == LogType.Log)
                            continue;
                        details.messages.Add(new BuildMessage
                        {
                            step = step.name,
                            type = message.type.ToString(),
                            message = message.content
                        });
                    }
                }
                allPassed &= report.summary.result == BuildResult.Succeeded && details.errors == 0;
            }
            catch (Exception exception)
            {
                allPassed = false;
                details.exception = exception.ToString();
            }
            WriteReport(project, Roles[index].ToLowerInvariant() + "-build.json", details);
        }
        if (!allPassed)
            throw new InvalidOperationException("A Windows player build failed; see Logs/Unity66Upgrade/*-build.json.");
        Debug.Log("AccompliceVR Windows Pilot and Copilot builds succeeded: " + outputRoot);
    }

    private static bool HasKnownDisabledPortalComponent(string scenePath)
    {
        const string expected = @"--- !u!114 &855534233
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 855534228}
  m_Enabled: 0
  m_EditorHideFlags: 0
  m_Script: {fileID: 11500000, guid: ebbadd562653000418f6ad790262c683, type: 3}
  m_Name:
  m_EditorClassIdentifier:";
        string yaml = File.ReadAllText(scenePath).Replace("\r\n", "\n");
        int start = yaml.IndexOf("--- !u!114 &855534233\n", StringComparison.Ordinal);
        if (start < 0) return false;
        int end = yaml.IndexOf("\n--- ", start + 1, StringComparison.Ordinal);
        string block = end < 0 ? yaml.Substring(start) : yaml.Substring(start, end - start);
        block = System.Text.RegularExpressions.Regex.Replace(block, @"[ \t]+(?=\n|$)", "");
        return block.TrimEnd() == expected.Replace("\r\n", "\n").TrimEnd();
    }

    private static string SafeProject()
    {
        string project = Path.GetFullPath(Path.Combine(Application.dataPath, "..")).TrimEnd(Path.DirectorySeparatorChar);
        string original = Path.GetFullPath(ProtectedProject).TrimEnd(Path.DirectorySeparatorChar);
        if (string.Equals(project, original, StringComparison.OrdinalIgnoreCase) ||
            project.StartsWith(original + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("The original AccompliceVR project is protected; use an independent copy.");
        RejectReparsePoints(project);
        return project;
    }

    private static string SafeChild(string project, string path)
    {
        string fullPath = Path.GetFullPath(Path.Combine(project, path));
        if (!fullPath.StartsWith(project + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Build/report outputs must stay inside the current project: " + fullPath);
        RejectReparsePoints(fullPath);
        return fullPath;
    }

    private static void RejectReparsePoints(string path)
    {
        // Refuse junction/symlink aliases that could redirect writes into the protected project.
        for (string item = path; !string.IsNullOrEmpty(item); item = Path.GetDirectoryName(item))
        {
            if ((Directory.Exists(item) || File.Exists(item)) &&
                (File.GetAttributes(item) & FileAttributes.ReparsePoint) != 0)
                throw new InvalidOperationException("Migration verification refuses symbolic links/junctions: " + item);
        }
    }

    private static void WriteReport(string project, string name, object report)
    {
        string path = SafeChild(project, Path.Combine("Logs/Unity66Upgrade", name));
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        File.WriteAllText(path, JsonUtility.ToJson(report, true));
    }

    private static string GetArgument(string name)
    {
        string[] args = Environment.GetCommandLineArgs();
        for (int index = 0; index < args.Length; index++)
        {
            if (!string.Equals(args[index], name, StringComparison.Ordinal))
                continue;
            if (index + 1 >= args.Length || args[index + 1].StartsWith("-", StringComparison.Ordinal))
                throw new ArgumentException("Missing value for " + name);
            return args[index + 1];
        }
        return null;
    }

    private static string Hierarchy(Transform item)
    {
        string path = item.name;
        while (item.parent != null)
        {
            item = item.parent;
            path = item.name + "/" + path;
        }
        return path;
    }
}
