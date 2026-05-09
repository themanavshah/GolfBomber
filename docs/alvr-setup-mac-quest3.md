# ALVR Setup — Mac + Quest 3

> ## ⚠️ STATUS: SHELVED (as of 2026-05-09)
>
> ALVR's official GitHub releases ship **Windows** and **Linux** streamer binaries plus an **Android** client APK — **no macOS streamer build**. Verified at the July 2025 release: assets are `alvr_streamer_linux.tar.gz`, `alvr_streamer_windows.zip`, `alvr_launcher_linux.tar.gz`, `alvr_launcher_windows.zip`, `alvr_client_android.apk`, and a Flatpak. No `_macos` artifact.
>
> Using ALVR on macOS would require **building the streamer from source** (a Rust + C++ project with VideoToolbox bindings to wire up). That's a real engineering project, separate from GolfBomber, and the project has explicitly opted out of source-compiling third-party tools for now.
>
> **Current dev workflow on Mac instead:** XR Device Simulator in Editor for logic, Build & Run / Patch & Run to Quest 3 for swing feel. See `unity-basics.md` section 12b.
>
> The setup steps below are kept for reference if we ever revisit. They assume a Mac streamer binary exists, which it currently does not.

---

Goal (if/when we revisit): hit Play in Unity on macOS, see the scene in the Quest 3 headset live, like Windows users get with Quest Link.

> Reality check: ALVR is community-built and macOS support is the least-mature of its targets. Expect some fiddling on first run. We're using it as a tool — when it breaks, we triage; we don't dive into ALVR's source unless something blocks us hard.

---

## 0. Prerequisites (one-time)

- **Quest 3 in Developer Mode** — Meta mobile app → Headset Settings → Developer Mode → On.
- **USB-C cable** that supports data (not charge-only) — we use it for sideloading the ALVR client and for stable USB streaming if Wi-Fi is flaky.
- **Same Wi-Fi network** for Mac and Quest (5 GHz strongly preferred — 2.4 GHz is too slow for streaming).
- **Optional but recommended:** [SideQuest](https://sidequestvr.com/setup-howto) installed on Mac for easy APK sideloading. (`adb install` works fine too if you prefer terminal.)

---

## 1. Install ALVR Server on Mac

1. Go to ALVR releases: <https://github.com/alvr-org/ALVR/releases>
2. Download the latest **macOS** build. Filename pattern looks like:
   - `alvr_streamer_macos.tar.gz` (or similar)
3. Extract somewhere stable like `~/Applications/ALVR/`. Don't put it in a cloud-synced folder — it has runtime state.
4. First launch: macOS will gatekeep an unsigned binary. Right-click the `ALVR Dashboard` app → **Open** → confirm the warning. After that it'll launch normally.

The Dashboard is a desktop app that runs the streamer and exposes a config UI.

---

## 2. Install ALVR Client on Quest 3

Same release page — grab the **client APK**. Filename pattern:
- `alvr_client_quest.apk`

Sideload it onto the Quest:

**Option A — SideQuest:**
1. Plug Quest in via USB-C, accept the "Allow USB debugging" prompt inside the headset.
2. Open SideQuest, drag the APK into the SideQuest window.

**Option B — adb (if you already have ADB installed via Unity's Android SDK):**
```bash
~/Library/Android/sdk/platform-tools/adb install -r ~/Downloads/alvr_client_quest.apk
```
Path may differ — search for `adb` if needed:
```bash
mdfind -name adb | head
```

Verify install: in the Quest, go to **Apps → Unknown Sources** dropdown → ALVR should be listed.

---

## 3. First connection

1. **Mac:** launch the ALVR Dashboard. The first-launch wizard will walk you through:
   - Choose the OpenXR runtime → set ALVR as the active runtime (it'll write a JSON manifest to the standard OpenXR location).
   - Network adapter selection — pick the one connected to your Wi-Fi (not VPN or virtual interfaces).
   - Encoder preset — pick **VideoToolbox** (Apple's hardware H.264/HEVC encoder).
2. **Quest:** put on the headset, launch ALVR from **Apps → Unknown Sources → ALVR**. It'll show a "waiting for streamer" screen with the headset's hostname.
3. **Mac dashboard:** the headset should appear under the **Devices** tab. Click **Trust**.
4. After trusting, the Quest screen should switch to a SteamVR-style void (or whatever ALVR's idle scene is). You're connected.

If anything is unhappy at this stage, the Dashboard's **Logs** tab is where to look first.

---

## 4. Unity-side configuration (do this *after* the Unity 6.3 upgrade is healthy)

ALVR exposes itself to Unity as an **OpenXR runtime**, not as an Oculus runtime. Two changes in the project:

### 4a. Install the OpenXR plugin
Unity → `Window → Package Manager` → `+` → **Add package by name** → `com.unity.xr.openxr` → Add.

### 4b. Switch active XR plugin (Editor only)
`Edit → Project Settings → XR Plug-in Management`:
- **Standalone (Mac/Windows/Linux)** tab — this controls Editor Play mode:
  - **Untick** Oculus.
  - **Tick** OpenXR.
- **Android** tab — this controls Quest builds. Leave Oculus ticked here. We want builds to use Oculus's native runtime on the headset; ALVR is only for Editor streaming.

### 4c. Configure OpenXR for Quest controllers
Under XR Plug-in Management → **OpenXR**:
- Add **Oculus Touch Controller Profile** to the Interaction Profiles list (so OpenXR knows how to map Quest controllers).
- Optionally add **Meta Quest Touch Plus Controller Profile** for Quest 3 specifically.

### 4d. Test it
- Make sure ALVR is connected (Quest in headset showing the void, Dashboard shows connected).
- Open `Assets/Scenes/SampleScene.unity`.
- Hit Play.
- Editor Play should appear in the Quest headset. Move your head — view should track. Press grip — you should see the controllers.

---

## 5. Daily-driver workflow

Once it's working:

1. Put on Quest, launch ALVR client.
2. On Mac, ALVR Dashboard auto-connects.
3. In Unity, hit Play → headset shows the scene.
4. Stop, edit, hit Play again — fast iteration.

Tips:
- **Start ALVR before Unity Play.** If Unity initializes XR before ALVR is connected, it'll fall back to no-VR mode and you'll have to stop/start.
- **Watch the Dashboard's Performance tab** for frame timing. Latency under ~50 ms feels fine; over ~80 ms is noticeable.
- **Wired USB streaming** is more stable than Wi-Fi but requires extra setup (ADB reverse-tunneling on the connection settings tab). Try Wi-Fi first.

---

## 6. Common failure modes (we'll add to this as we hit them)

| Symptom | Likely cause | First check |
|---|---|---|
| ALVR client says "waiting for streamer" forever | Mac firewall blocking, or wrong network adapter selected | Mac → System Settings → Network → Firewall: allow ALVR. Dashboard → Connection tab → re-pick adapter. |
| Connects, but Unity Play shows no VR | OpenXR runtime not registered, or Oculus plugin still active for Standalone | Project Settings → XR Plug-in Management → Standalone tab → Oculus off, OpenXR on. Re-launch Editor. |
| Streaming connects but image is black/frozen | Encoder mismatch | Dashboard → Video → switch encoder (VideoToolbox H.264 ↔ HEVC). |
| Massive latency / stutter | Wi-Fi too slow or interference | Switch to 5 GHz, or use USB tethered streaming. |
| Controllers don't show / can't grab | OpenXR Interaction Profiles missing | Project Settings → XR Plug-in Management → OpenXR → add Oculus Touch + Quest Touch Plus profiles. |
| Crash on Play with `XR_ERROR_RUNTIME_FAILURE` | Active OpenXR runtime is wrong (e.g. SteamVR ghost) | Dashboard → "Set ALVR as active OpenXR runtime" again. |

---

## 7. When to give up and use Build & Run

If ALVR is stable enough to be useful, great — make it your default. If it's flaky enough that you spend more time fighting it than coding, fall back to:

- **XR Device Simulator** in Editor for logic iteration.
- **Build And Run** (or Patch And Run) to Quest for swing-feel testing.

That's a fine workflow. ALVR is a luxury, not a requirement.

---

## 8. What we explicitly are *not* doing yet

- Reading ALVR's source code.
- Building ALVR from source.
- Submitting upstream patches.

If/when something blocks us hard enough to justify it, we revisit. Until then: it's a black-box tool.
