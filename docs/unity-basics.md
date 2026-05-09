# Unity Basics — GolfBomber Reference

A practical glossary tailored to this project. Skim it once, refer back when something is fuzzy. Examples use GolfBomber concepts (boat, golf club, bomb-balls, buildings).

---

## 1. The mental model

Unity has **three layers** that you constantly switch between:

1. **Assets** — files on disk (scripts, models, textures, sounds, prefabs, scenes).
2. **Scene** — what's currently loaded in memory: a tree of GameObjects with components attached.
3. **Play mode** — the scene running like a real game. Changes here **do not persist** when you stop playing.

> **Gotcha:** Editing values in Play mode and then stopping = everything resets. This is the #1 source of "where did my changes go?!" frustration.

---

## 2. The Unity Editor windows

| Window | What it shows |
|---|---|
| **Hierarchy** | The tree of GameObjects in the currently-open scene. |
| **Scene** | 3D editor view — you fly around, place objects, gizmos. |
| **Game** | What the camera actually sees (what the player will see). |
| **Inspector** | Components on whatever is selected in the Hierarchy or Project. |
| **Project** | All assets on disk (scripts, prefabs, models, etc.). |
| **Console** | Logs, warnings, errors from running scripts. Watch this constantly. |

---

## 3. Core concepts

### GameObject
Everything in a scene is a GameObject — the boat, the golf club, a building, the player camera, even an empty "container" used for organization. By itself a GameObject does nothing.

### Component
A behavior attached to a GameObject. GameObjects are containers; **Components** are the actual functionality.

Examples we'll use:
- `Transform` — position/rotation/scale (every GameObject has one, can't remove).
- `MeshRenderer` + `MeshFilter` — makes it visible.
- `Collider` (BoxCollider, SphereCollider, MeshCollider) — physical shape for collisions.
- `Rigidbody` — adds physics (gravity, forces, velocity).
- `AudioSource` — plays sound.
- A C# script (`MonoBehaviour`) — your custom behavior.

> **Mantra:** "GameObjects don't *do* — Components do."

### Transform
Position, rotation, scale. **Local** (relative to parent) vs **World** (relative to the scene origin). Parenting one GameObject under another means the child's Transform is relative to the parent.

### Prefab
A reusable GameObject template saved as an asset. You build a GameObject in the scene with all its components, drag it into the Project window → it becomes a `.prefab` file. Now you can spawn many copies. Editing the prefab updates every instance.

For GolfBomber: `BombBall.prefab`, `GolfClub.prefab`, `Building.prefab`, `ExplosionVFX.prefab`.

### Scene
A `.unity` file holding a saved hierarchy of GameObjects. We currently have `SampleScene.unity`. A game can have many scenes (main menu, level 1, level 2…).

### Asset
Anything in the `Assets/` folder. Unity creates a `.meta` file next to each one storing its **GUID** (a unique ID). The GUID is how scripts and scenes reference assets, so **never delete or rename `.meta` files outside Unity.**

---

## 4. Scripts (MonoBehaviour)

A C# script that inherits from `MonoBehaviour` can be attached to a GameObject as a component.

```csharp
using UnityEngine;

public class BombBall : MonoBehaviour
{
    [SerializeField] private float explosionRadius = 5f;
    [SerializeField] private GameObject explosionVfx;

    void Awake()  { /* called when object is created */ }
    void Start()  { /* called once before first frame */ }
    void Update() { /* called every frame */ }
    void FixedUpdate() { /* called at fixed physics rate (default 50Hz) */ }
    void OnCollisionEnter(Collision c) { /* physics collision */ }
    void OnTriggerEnter(Collider c)    { /* trigger overlap */ }
    void OnDestroy() { /* called when destroyed */ }
}
```

### Lifecycle order to remember
`Awake → OnEnable → Start → Update (every frame) → FixedUpdate (every physics tick) → OnDestroy`

### Inspector exposure
- `public` fields → shown in Inspector.
- `private` fields → hidden, **unless** you add `[SerializeField]`.
- Best practice: keep fields `private` and use `[SerializeField]` for things you want to tweak from the Inspector. Cleaner API, same designer experience.

```csharp
[SerializeField] private float swingForce = 10f;   // editable in Inspector, hidden from other scripts
public int score;                                  // editable in Inspector AND readable from other scripts
private bool isLoaded;                             // hidden, internal only
```

### Common attributes
- `[SerializeField]` — show a private field in Inspector.
- `[Range(0, 100)]` — slider in Inspector.
- `[Tooltip("...")]` — hover text.
- `[Header("Section name")]` — group fields with a header.
- `[RequireComponent(typeof(Rigidbody))]` — auto-add a Rigidbody if missing.

---

## 5. Physics

### Rigidbody
Add this to a GameObject and physics takes over: gravity, forces, collisions. The bomb-ball will have a Rigidbody. The boat won't (it's static).

Key properties:
- `mass` — heavier = harder to push.
- `useGravity` — gravity on/off.
- `isKinematic` — physics-driven (false) vs script-driven (true).
- `drag` / `angularDrag` — air resistance.

### Collider
Defines the **shape** for collision. Smaller = faster. Prefer `BoxCollider` / `SphereCollider` / `CapsuleCollider` over `MeshCollider` (mesh colliders are expensive).

### Trigger
A Collider with `isTrigger = true` doesn't block movement but fires `OnTriggerEnter/Stay/Exit`. Good for "did the ball enter this zone?" without bouncing.

### Layers
A bitmask label on each GameObject. Used to filter:
- What physics layers collide with what (`Edit → Project Settings → Physics → Layer Collision Matrix`).
- What the camera renders.
- What an XR ray hits.

For GolfBomber we'll likely have layers like `Default`, `Player`, `Ball`, `Building`, `Boat`.

### Tag
A simple string label on a GameObject (`"Building"`, `"Ball"`). Useful for `if (other.CompareTag("Building"))`. Layers are for filtering at the engine level; tags are for ad-hoc identification.

---

## 6. Materials, Meshes, Rendering

- **Mesh** — the geometry (vertices, triangles).
- **Material** — how the surface looks: a **Shader** + parameters (color, texture, smoothness…).
- **MeshRenderer** — the component that draws the mesh using a material.

When importing an FBX (like `golf_club.fbx`), Unity creates a model with sub-meshes and auto-generates default materials. You usually want to extract or replace those.

---

## 7. ScriptableObject (data assets)

Like a MonoBehaviour, but lives as an asset on disk instead of attached to a GameObject. Use for:
- Game config (e.g. `LevelConfig` with target count, time limit).
- Shared data referenced by many scripts.

Cleaner than putting config on random GameObjects.

---

## 8. Coroutines

Functions that can pause and resume across frames — Unity's lightweight version of async.

```csharp
IEnumerator SpawnBallAfterDelay()
{
    yield return new WaitForSeconds(1.5f);
    Instantiate(ballPrefab, teePosition, Quaternion.identity);
}
// kick it off:
StartCoroutine(SpawnBallAfterDelay());
```

---

## 9. UI (Canvas)

- **Canvas** — root for all UI. Three render modes: `Screen Space - Overlay`, `Screen Space - Camera`, **`World Space`** (← we'll use this for VR — UI lives in 3D space, e.g. a scoreboard floating above the boat).
- **Text / TextMeshPro** — text rendering. Always prefer **TextMeshPro** over the legacy Text.
- **Button, Image, Slider** — standard widgets.

---

## 10. XR specifics (relevant to this project)

### 10.1 Quick reference table

| Term | Meaning |
|---|---|
| **XR Origin** (a.k.a. XR Rig) | The root GameObject representing the player's tracking space. Contains the camera and controllers. |
| **XR Interaction Manager** | Singleton that mediates between Interactors and Interactables. |
| **Interactor** | A component on a controller that can interact (XR Direct Interactor, XR Ray Interactor, XR Poke Interactor, Teleport Interactor). |
| **Interactable** | A component on an object that *can be interacted with* (XR Grab Interactable for grabbing, XR Simple Interactable for select/hover). |
| **XR Grab Interactable** | Makes an object grabbable. The golf club will have this. |
| **Locomotion Provider** | Move/turn/teleport providers attached to the XR Origin. |
| **Action-based input** | The modern input style (vs device-based). Uses an `InputActionAsset` like `XRI Default Input Actions`. We use this. |

### 10.2 XR Origin (XR Rig) — in detail

This is the **player root**. Everything that represents the human in VR lives under this single GameObject.

A typical XR Origin hierarchy looks like:

```
XR Origin
├── Camera Offset                ← child used to offset the camera height
│   ├── Main Camera              ← the HMD; tracks the headset's pose
│   ├── LeftHand Controller      ← tracks the left controller
│   │   └── (Direct/Ray/Poke Interactors as children)
│   └── RightHand Controller     ← tracks the right controller
│       └── (Direct/Ray/Poke Interactors as children)
└── Locomotion System            ← (sometimes a sibling, sometimes a child)
```

Components on the **XR Origin** GameObject itself:
- **`XROrigin`** — the brain. Knows the *Tracking Origin Mode*:
  - `Floor` — `Y=0` in Unity = the real-world floor. Player height comes from headset tracking.
  - `Device` — `Y=0` = wherever the headset was at start. Less common.
  - For Quest, **always use `Floor`**.
- **`Character Controller`** — collision capsule so the player can't walk through walls.
- **`Character Controller Driver`** — keeps the capsule positioned under the headset as the player physically moves.

**Mental model:** the XR Origin is "the tracking space". The camera moves *inside* the XR Origin as the player physically moves their head. To teleport the player, you move the XR Origin GameObject — **never** move the camera directly.

For GolfBomber: the player stands on a boat. We'll likely **parent the XR Origin to the boat** (or place it on the boat's deck) so they're standing on the deck. Locomotion can be turned off later if we don't want them roaming.

---

### 10.3 XR Interaction Manager — in detail

A small but mandatory component. **One per scene.** Usually lives on its own empty GameObject named `XR Interaction Manager`.

It's a switchboard: every Interactor and every Interactable in the scene **registers itself with the Manager** at startup. When you move your controller near a grabbable object, the Manager is what figures out "this Direct Interactor is overlapping with that Grab Interactable — emit a hover event."

You almost never write code that talks to it directly. You just need to make sure one exists in the scene. (The XRI Starter Assets prefab `XR Interaction Setup` already includes one.)

> **Gotcha:** Two managers in a scene = duplicate events fire and weird bugs. One scene, one manager.

---

### 10.4 Interactors — in detail

An Interactor is a component you put on a controller (or its child). It's "the thing doing the interacting" — the active half of the system.

| Interactor | What it does | When to use |
|---|---|---|
| **XR Direct Interactor** | Grabs anything its trigger collider overlaps. | Hand-touch grabbing (picking up the golf club). **Primary one for GolfBomber.** |
| **XR Ray Interactor** | Shoots a ray from the controller; can grab or click whatever the ray hits. | Distant grabbing, UI buttons in world space, menus. |
| **XR Poke Interactor** | A small finger-tip collider for "pushing" UI buttons. | Pressing buttons on a wrist menu or scoreboard. |
| **XR Gaze Interactor** | Aims with the head/eye direction. | Look-to-select on devices without controllers. |
| **XR Socket Interactor** | A fixed slot/socket that holds an interactable when you drop it in. | A holster on the boat for the club, or a tee that holds the next ball. |
| **Teleport Interactor** | A specialized ray that requests teleports to Teleport Areas/Anchors. | Teleport-style locomotion. |

A controller usually has multiple Interactors as children (Direct + Ray + Poke). Only one is "active" at a time based on what's around — XRI handles the priority. The XRI Starter `XR Controller Left/Right.prefab` files are pre-wired this way.

**Events on every Interactor:**
- `Select Entered` — just grabbed something.
- `Select Exited` — just let go.
- `Hover Entered` / `Hover Exited` — currently pointing at / overlapping with something but not grabbing.
- `Activate` / `Deactivate` — trigger pressed while holding something (we'll likely use this for any "fire" or "use" action on the club, if needed).

---

### 10.5 Interactables — in detail

An Interactable is a component on the **object being interacted with** — the passive half. It needs:
- A **Collider** (so the Interactor can detect it).
- A **Rigidbody** (for `XR Grab Interactable`, since grabbing manipulates physics).

| Interactable | What it does |
|---|---|
| **XR Grab Interactable** | Makes the object pickup-able. **The golf club will have this.** |
| **XR Simple Interactable** | Fires hover/select events without parenting/grabbing. Good for "tap to activate" objects. |
| **XR Socket Interactable** | (Combo) marks an object as one that *belongs* in a socket. |

It also exposes the same events Interactors do, mirrored — `OnSelectEntered`, `OnSelectExited`, etc. You can wire UnityEvents to them in the Inspector or subscribe from code:

```csharp
grabInteractable.selectEntered.AddListener(OnGrabbed);
grabInteractable.selectExited.AddListener(OnReleased);
```

---

### 10.6 XR Grab Interactable — deep dive (this matters for the golf club)

The golf club's grab settings determine **how the club behaves while held**, which determines whether your swing actually transfers force to the ball. Read this carefully.

**Movement Type** (the most important setting):

| Mode | Behavior | When to use |
|---|---|---|
| `Velocity Tracking` | The held object stays a real physics body — it gets velocity applied to follow your hand. **It will collide with other objects and push them.** | **Use this for the golf club.** A swung club must actually hit the ball with measurable velocity. |
| `Kinematic` | The object snaps to your hand position each frame, ignoring physics. Cheap and stable but it won't physically push other objects (it'll just clip through them). | A held tool you don't swing — a flashlight, a clipboard. |
| `Instantaneous` | Teleports the object to the hand each frame. Even less physical than Kinematic. | Rarely. UI-like grab targets. |

**Other key fields:**
- **Throw On Detach** — when released, keep linear/angular velocity. Fine to leave on; doesn't matter much for a club we're not throwing.
- **Smooth Position / Rotation** — softens jitter. Default is fine.
- **Track Position / Rotation** — both should be on for a held tool.
- **Attach Transform** — an empty child GameObject defining "where the hand holds it". For the club, you'd put the Attach Transform on the grip area. Without one, the hand snaps to the pivot of the GameObject (often the model origin, often wrong).
- **Use Dynamic Attach** — let XRI compute the attach pose from where you actually grabbed. Useful for a flexible grip on the club shaft.
- **Select Mode**: `Single` (one hand at a time) vs `Multiple` (both hands simultaneously).
  - For GolfBomber, **two-handed** feels much better. Set to `Multiple` and configure secondary hand pose.

**Required components on the same GameObject:**
- `Rigidbody` (must — the script enforces this).
- `Collider` (one or more on the object or children).

---

### 10.7 Locomotion Provider — in detail

Locomotion = how the player moves through the world. XRI ships with several Provider components, each handling one input type. They're added to a `Locomotion System` GameObject which references the XR Origin.

| Provider | What it does | Comfort |
|---|---|---|
| **Continuous Move Provider** | Smooth walking via thumbstick. | Can cause motion sickness. |
| **Continuous Turn Provider** | Smooth rotation. | Worst for nausea. |
| **Snap Turn Provider** | Rotates in fixed increments (e.g. 30°/click). | Much more comfortable. |
| **Teleportation Provider** | Works with Teleport Interactor + `Teleport Area` / `Teleport Anchor` interactables. | Very comfortable. |
| **Climb Provider** | Lets the player pull themselves up by grabbing climb points. | n/a |
| **Grab Move Provider** | Pull-to-move locomotion (grab the world and pull). | Comfortable, niche. |

For GolfBomber: the player stands on a boat. Realistically we want:
- **No move** (player doesn't walk around the deck) — or limited walk on deck only.
- **Snap turn** for comfort (so they can face different parts of the city without physically turning).

We can disable/remove what we don't need from the existing rig.

---

### 10.8 Action-based input — in detail

There are two input styles in XRI; we use the **action-based** one (modern, recommended):

- **Old (device-based):** scripts read directly from a controller — `InputDevice.GetButtonDown(...)`. Tightly coupled to specific hardware. Avoid.
- **New (action-based):** scripts read **InputActions** (e.g. "Select", "Activate", "Move") that are *mapped* to controller buttons by an asset, not in code.

The pieces:
- **`InputActionAsset`** — a `.inputactions` asset listing actions and their bindings. We use the bundled `XRI Default Input Actions.inputactions`.
- **Action** — a named input ("Select", "Activate", "Turn", "Teleport Mode Activate"…).
- **Binding** — what controller button feeds that action (e.g. Right Hand Trigger → Activate).
- **`InputActionReference`** — a typed reference you drop into a script's `[SerializeField]` slot pointing at a specific action.

How the controllers consume it: the `ActionBasedController` component on each hand has slots for Position, Rotation, Select, Activate, etc. — each filled by an `InputActionReference`.

**Why this matters for GolfBomber:**
- We want "trigger to grab the club" → bound to `Select`.
- We don't really need a separate "use" action since swinging is physical.
- If we add a wrist menu, we'd add a "Menu" action.
- Cross-platform (if we ever ship beyond Quest), the same action map can re-bind to a different controller without code changes.

You usually don't author the asset from scratch — you open `XRI Default Input Actions` in the Inspector and tweak bindings if needed.

> **Gotcha:** if a controller "doesn't grab anything," the most common cause is the `Select Action` slot on the `ActionBasedController` component is empty. Drag in `XRI Default Input Actions/XRI RightHand Interaction/Select` (and matching for left).

---

## 11. Inspector wiring (the "drag-and-drop" part)

When a script has:
```csharp
[SerializeField] private GameObject ballPrefab;
[SerializeField] private Transform teeTransform;
[SerializeField] private TMP_Text scoreText;
```
…the Inspector shows three empty slots. You **drag** the corresponding asset/GameObject into each slot. This is the part I'll usually ask you to do.

> **Gotcha:** If the script reference shows "**Missing (Mono Script)**", the script file moved or its class was renamed. Re-add the script.

---

## 12. Build settings

- `File → Build Settings…` — pick platform (Android for Quest), tick the scenes to include, hit Build.
- The first scene in the list = the one that loads on app start.
- For Quest: switch platform to Android, set XR Plug-in Provider = Oculus.

---

## 12b. Mac + Quest 3 development workflow

Important: **on macOS, Unity Play mode does not stream to the Quest.** Meta's Quest Link / Air Link are Windows-only. So the loop is different from what tutorials assume.

### The three iteration loops (use the right one for the right task)

| Loop | Speed | Use it for |
|---|---|---|
| Editor Play + XR Device Simulator | instant | logic, scoring, swing math, UI, spawning |
| Build & Run to Quest 3 (USB/Wi-Fi) | ~30s–2 min | swing feel, real performance, hand/grip ergonomics |
| Full clean build | 5–15 min | after package changes, settings changes, or weird breakage |

### Setup checklist (do these once)

1. **Enable Developer Mode on Quest 3** — Meta mobile app → your headset → Developer Mode → On.
2. **Install Meta Quest Developer Hub (MQDH) for macOS** from `developer.oculus.com → Tools`. (MQDH *does* support Mac including Apple Silicon — common myth that it's Windows-only.) Use it for screen casting, deploy, logs, recording, wireless ADB.
3. **In Unity Hub** → install Android Build Support module (with OpenJDK + Android SDK & NDK Tools) for your Editor version.
4. **Switch build target to Android**: `File → Build Settings → Android → Switch Platform`.
5. **XR Plug-in Management** (`Edit → Project Settings → XR Plug-in Management`) → Android tab → tick **Oculus**.
6. **Install the XR Device Simulator** sample so you can test in the Editor without the headset:
   - `Window → Package Manager → XR Interaction Toolkit → Samples → XR Device Simulator → Import`.
7. Plug Quest in via USB-C. First time, the headset asks "Allow USB debugging?" — say Always Allow.
8. `Build And Run` from Build Settings — produces an APK and pushes it to the headset.

### MQDH — what's actually useful for development

- **Casting** — mirrors the headset view to your Mac so you can see what's happening in VR while I'm watching the chat. Huge for debugging.
- **APK deploy** — drag a `.apk` onto MQDH and it installs it.
- **Wireless ADB pairing** — pair once over USB, then untether and continue dev wirelessly.
- **Performance HUD** — overlay FPS/GPU usage in the headset.
- **Log streaming** — see `adb logcat` output without using the terminal.
- **Recording** — capture clips/screenshots for sharing.

### When something goes wrong inside the headset

You won't see Console output in Unity once the build is running on Quest. To see logs:
- MQDH → Devices → your Quest → Logs, **or**
- Terminal: `adb logcat -s Unity` (filters Unity-only messages).

### Tips that save real time

- **Tick "Development Build"** in Build Settings while iterating — gives you stack traces and lets you attach the Unity Profiler to the running app on the headset.
- **"Patch" / "Patch And Run"** (Build Settings → small dropdown next to Build) is faster than full Build for code-only changes after the first build.
- **Don't bake lighting** until close to release — baked lighting is slow and we don't need it for prototyping.
- **First build of a session is the slow one** (5–10 min, shader compilation). Subsequent ones are 30s–2 min.
- Keep the **headset plugged in via USB-C with a quality cable** for builds — Wi-Fi deploys exist but USB is more reliable while developing.

### What about ALVR / streaming Editor Play to Quest on Mac?

ALVR (open-source) has experimental macOS builds that can sometimes stream PCVR-style. Apple Silicon support is uneven and performance varies a lot. Not recommended as your primary dev loop — the XR Device Simulator + Build & Run combo is more reliable. Worth knowing it exists if you want to experiment later.

---

## 13. Common gotchas

1. **Play mode changes don't save.** Make changes in Edit mode.
2. **Forgetting to assign Inspector fields.** `NullReferenceException` on Start = 90% of the time you forgot to drag a reference in.
3. **MeshCollider on moving objects.** Costly and often broken — use Box/Sphere/Capsule.
4. **Two Audio Listeners.** Only one allowed per scene; usually it's on the player's camera.
5. **Saving the scene.** `Cmd+S` saves the scene. Unsaved scene + crash = lost work.
6. **Modifying a prefab instance vs the prefab itself.** Click "Open Prefab" or use "Apply Overrides" carefully — otherwise edits stay local to that instance.
7. **`.meta` files.** Always commit them with their pair file. Never rename/delete from outside Unity.
8. **FBX models can import a stowaway Camera or Light.** When 3D artists author a model, sometimes they include a reference camera or scene light. Unity imports these by default — and a stowaway Camera tagged MainCamera will steal rendering from your XR rig and produce flat 2D output in VR. **Always check Import Cameras / Import Lights are unticked** for non-camera/non-light assets. Inspector → Model tab on the FBX → uncheck → Apply. (We learned this the hard way with `golf_club.fbx` on 2026-05-09.)
9. **Two cameras tagged MainCamera.** Even if you fix #8, if a previous instance dragged the rogue camera into the scene, it's still there. Search Hierarchy for `Camera`, confirm only one exists (under `XR Origin → Camera Offset → Main Camera`).

---

## 14. Workflow we'll use for GolfBomber

When we add a feature, the steps are usually:

1. **Me:** write or update C# scripts in `Assets/Scripts/`.
2. **You (in Unity):**
   - Create / open the relevant GameObject or prefab.
   - Add the component (`Add Component → <ScriptName>` or drag the script onto it).
   - Drag the required references into the script's Inspector slots.
   - Hit Play, test, check the Console.
3. **You:** report what happened. If there's an error, paste the first line — it almost always points to the broken file:line.
4. **Me:** fix the script, you re-test.

The faster we get into that loop, the faster the game shapes up.

---

## 15. Quick C# reminders

- `Instantiate(prefab, position, rotation)` — spawn a copy.
- `Destroy(gameObject)` — remove the GameObject.
- `GetComponent<T>()` — fetch another component on the same GameObject.
- `transform.position` / `transform.rotation` — world transform.
- `Vector3` for positions/directions, `Quaternion` for rotations.
- `Time.deltaTime` — seconds since last frame (use to make movement frame-rate independent).
- `Time.fixedDeltaTime` — physics tick (used in `FixedUpdate`).
- `Mathf.Clamp(x, min, max)` — clamp a value.
- `Random.Range(0, 10)` — random number.

---

That's the working glossary. Add to it whenever a new term shows up in our conversation that you want pinned down.
