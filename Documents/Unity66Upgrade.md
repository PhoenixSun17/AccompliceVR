# Unity 6.6 upgrade record

Date: 6 October 2026

The upgrade copy compiles and produces both Windows x64 players in Unity **6000.6.4f1**. Fresh-cache import, reopening, and an offline Direct3D 11 WebRTC native-peer check passed. Quest 3 PCVR operation, video encoding/decoding, two-PC collaboration, and mixed-version interoperability are **not yet verified**.

The protected `C:\AccompliceVR` project was never opened in Unity or modified. Source SHA-256 hashes and an inventory of file sizes/modification times were captured before implementation and checked afterward. The original already had a Render Streaming wizard-state change; it was preserved.

## Delivered builds

Extract an entire archive before running its executable; the accompanying data folders and DLLs are required.

| Role | Unity 6.6 archive |
| --- | --- |
| Pilot | `Builds/Unity66/AVRPilot-Unity66-Windows-x64.zip` |
| Copilot | `Builds/Unity66/AVRCoPilot-Unity66-Windows-x64.zip` |

Uncompressed players are also available in `Builds/Unity66/Pilot` and `Builds/Unity66/Copilot`. Separate Unity 2022.3.16f1 comparison archives are in `Builds/Unity2022`. They were built from `Builds/Unity2022BaselineProject`, a disposable copy of the original with an Editor-only verification helper. No runtime source changes were needed for those baseline builds.

Both sets of builds preserve the saved networking configuration. They have not been launched into a live room during this migration.

## Compatibility changes

| Area | Changes |
| --- | --- |
| OpenVR XR Plugin 1.1.4 | Embedded current package; removed unused experimental XR imports; replaced assembly enumeration on Unity 6.6. |
| Render Streaming 3.1.0-exp.6 | Embedded current package; replaced assembly enumeration on Unity 6.6; updated the Editor wizard's obsolete Android API-level constant. |
| WebRTC 3.0.0-pre.4 | Embedded current package; updated the Editor build processor's obsolete Android API-level constant for Unity 6.6. Native binaries unchanged. |
| SteamVR source | Version-guarded reflection, PhysicsMaterial, EntityId, native XR-state detection, and obsolete legacy VR settings. Full EntityId values are retained. Unity 6 uses the existing configured XR loader instead of the removed legacy forced device-switching path. |
| Application source | Replaced three obsolete `GameObject.active` setters in `LineController` with `SetActive`; removed the unused `UnityEditor` import from `RenderSkybox`. Network message fields are unchanged. |
| Ubiq | Pinned the already-resolved commit `6fd899cd2e9fc3990e6ef905322a7c92de2abf42`. The actual package identifies as 0.4.5; the old README's 1.0.0-pre.16 claim does not describe the locked dependency. |

The three embedded packages live under `Packages` and add approximately 210 MB. Their versions, licenses, asset GUIDs and native binaries are preserved. Changes are not stored in `Library/PackageCache`. Unity regenerated package-lock entries for the embedded packages.

Kept XRI 3.6.1 and the pre-existing namespace repair in its older Starter Assets sample. Scene/prefab contents, materials, shaders, XR loader selection, action files, input bindings, room IDs, codecs and stream dimensions were preserved.

Unity also migrated sprite and native-plugin importer metadata, added a SteamVR serialized class identifier, and materialized current OpenXR settings. Existing tracking/action values and feature-enable flags were retained; newly materialized OpenXR features are disabled. These generated format changes are included. No conversion to OpenXR, URP, Android, or new application features was performed.

## Verification results

| Check | Result |
| --- | --- |
| Unity 6.6 Editor compilation | Passed, zero compilation errors. |
| Unity 6.6 Pilot / Copilot Windows x64 builds | Both succeeded, zero build errors. |
| Unity 2022 Pilot / Copilot comparison builds | Both succeeded, zero build errors. |
| Main-scene script audit | No new missing scripts; the exact inherited disabled Portal component is an explicit exception. |
| Import with no copied Library/cache | Passed in `Builds/Unity66CleanImport`. |
| Reopen the fresh-import project | Passed. |
| WebRTC native peer on Direct3D 11 | Native context available; peer creation, initial-state query and disposal passed without signaling or ICE negotiation. |
| Original-folder integrity | Source hashes and complete file size/modification-time inventory unchanged. |
| Quest 3 tracking, capture, overlays and controls | Pending physical PCVR/SteamVR test. |
| Video/audio streaming, mixed versions and reconnects | Pending two-PC test. |
| Representative 15-minute sessions / performance comparison | Pending hardware tests. |

Warnings remain, including obsolete APIs and Unity 6 serialization diagnostics. They were not globally suppressed. Initial full build reports contain 99 Pilot / 1 Copilot warnings on Unity 6.6 and 16 / 2 on Unity 2022; counts include the first player-compilation pass and differ between cached builds.

Two inherited missing-script issues were reproduced in Unity 2022:

- Pilot `Overlayer/Portal`: disabled component file ID `855534233`, script GUID `ebbadd562653000418f6ad790262c683`, with no custom fields. No matching source exists in the project or inspected history. It remains unchanged. The optional audit exception checks its exact serialized block, hierarchy, and count; additional or changed missing components fail.
- Ubiq sample `Basketball.prefab`: a packaging warning in both engine versions. It is a referenced sample asset, outside the instantiated main-scene script count. No speculative component replacement was made.

## Reproduce the checks

Close any Editor using the project first. From the upgrade root, using the installed Python 3.13:

```powershell
python Tools\verify_unity_upgrade.py build
python Tools\verify_unity_upgrade.py native
python Tools\verify_unity_upgrade.py validate --project clean
python Tools\verify_unity_upgrade.py build --project baseline
```

The runner uses the installed Editor versions, refuses the protected original and junction aliases, and keeps output inside the upgrade copy. The `clean` and `baseline` commands use the disposable projects already prepared for this migration. Native validation forces D3D11 and does not enter Play mode or connect to a server.

The Editor-only helper also exposes `AccompliceVRUpgradeBuild.BuildAll`, `ValidateScenes`, and `NativeSmoke` as batch entry points. Scene validation is strict by default. The runner explicitly supplies `-accompliceAllowBaselineMissingPortal` for the documented baseline exception. Opening the helper's menu commands without that flag will report the inherited Portal issue rather than silently ignoring it.

Evidence is in `Logs/Unity66Upgrade`: build reports, native-smoke report, original-integrity report, change audit, archive hashes, and per-run logs. The pre-implementation checkpoint is `copy-before-implementation.zip` in the same directory. Baseline and clean-import scene reports are inside their respective disposable projects' `Logs/Unity66Upgrade` folders. Build outputs, caches, and local evidence are ignored by Git; source fixes, embedded packages, the helper, runner, and this record are intended project changes.

## Remaining Quest 3 and two-PC checks

The headset was not connected during implementation. Use the established Quest 3 PCVR connection with SteamVR. No standalone Quest/Android build is included.

Confirm the test network configuration before launching a live session. The effective saved video-signaling endpoint is `ws://128.16.11.75:9090`; the separate Ubiq server is `nexus.cs.ucl.ac.uk:8009`. The README's local-server example uses port 8080. None of these remote endpoints was tested here.

Pilot's existing `runOnAwake` path constructs signaling before applying CLI URL overrides. Do not rely on a Pilot `-signalingUrl` override: a different Pilot endpoint requires selecting the effective project default and rebuilding the copy. Copilot's explicit-start path parses the CLI before constructing signaling. This existing behavior was left unchanged. Endpoint changes should be agreed before creating test-specific builds; never change the protected original.

For each approved pairing, launch both players with `-force-d3d11` and capture separate `-logFile` outputs. Test 2022/2022 first, then 6.6/6.6, 2022 Pilot/6.6 Copilot, and 6.6 Pilot/2022 Copilot. Verify source-VR-app coexistence, right-eye capture, overlays, alignment, tracking, WASD, viewport lock, mouse rotation, pointing, synchronized laser visibility, reset, avatars and configured audio. Test both startup orders, disconnect/reconnect, Play/Stop and player restarts, followed by a 15-minute session per upgraded pairing. Compare responsiveness and frame pacing on the same hardware. Native-peer success does not establish video encoding, headset functionality, or mixed-version network compatibility.
