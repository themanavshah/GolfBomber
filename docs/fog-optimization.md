# Fog Optimization — Pending Work

Currently disabled. Pick this back up when ready to re-enable border fog.

## The setup

- ~100 instances of the fog GLB (`fog_for_your_sketchfab_scenes`) placed around scene borders.
- All instances are prefab clones of the same source GLB.
- All share the same material (`Scene_-_Root`).
- They never move (static at scene start).

## The problem

100 separate transparent draw calls × 2 eyes for VR = ~200 fog draw calls per frame. Quest 3 budget is ~150 total draw calls for smooth 90 Hz. Without optimization, fog alone could tank the framerate.

## The fix: GPU Instancing

GPU Instancing renders all 100 instances in a single draw call. Designed exactly for this case ("many copies of same mesh + same material").

The toggle lives on the **material**, not the shader. But the GLB-embedded material `Scene_-_Root` is **read-only** (all fields greyed in Inspector), so we need to either extract it or replace it before we can enable instancing.

## Option A: Extract the material from the GLB

1. Click the **`fog_for_your_sketchfab_scenes`** GLB parent asset in Project window.
2. Inspector → **Materials** tab.
3. Look for **Extract Materials** button → click → choose destination folder (e.g., `Assets/Materials/`).
4. Unity creates editable `.mat` files outside the GLB, updates GLB to reference them.
5. Click the extracted material → tick **Enable GPU Instancing ✔** in Advanced Options.

If the Materials tab doesn't have an Extract button (depends on glTFast version), use Option B.

## Option B: Create a replacement material

1. Project → right-click → Create → **Material** → name `FogMat`.
2. Set **Shader** = `glTF/PbrMetallicRoughness` (same as original).
3. Drag the `image_0` texture (from the GLB sub-assets) into the Base Color Tex slot.
4. Mirror values from the read-only material:
   - **Blend Mode**: Fade
   - **Alpha Cutoff**: 0.5
   - **Roughness**: ~0.56
   - **Metallic**: 0
5. Tick **Enable GPU Instancing ✔** (in Advanced Options, scroll to bottom).
6. **Apply to the prefab**:
   - Double-click `fog_for_your_sketchfab_scenes` prefab in Project to enter Prefab Edit Mode.
   - Select the inner mesh GameObject (with Mesh Renderer).
   - Drag `FogMat` into the Materials slot, replacing the read-only one.
   - Save the prefab (exit Prefab Edit Mode).
7. All 100 instances now use the editable, instanced material.

## Bonus: flag fogs as Batching Static

While 100 fog GameObjects are selected in Hierarchy:
- Inspector → top-right → **Static** dropdown → tick **Batching Static**.
- Apply to children when prompted.

Static + GPU Instancing on the material = Unity picks the fastest path automatically.

## Verify it worked

1. Press Play in Unity.
2. Game view → **Stats** button (top-right).
3. Look at **Batches** count:
   - Before instancing: hundreds.
   - After instancing: should drop dramatically (single digits to low tens for fog).
4. **Saved by batching** should show a positive number.

## Alternative: Unity built-in fog (skip the mesh fog entirely)

If the mesh fog ends up being too much trouble or visually unnecessary:

- **Window → Rendering → Lighting → Environment** tab.
- Tick **Fog ✔** → **Mode = Linear**.
- **Start**: ~50m, **End**: ~150m (tune to taste).
- Pick fog color matching atmosphere (gray/white for mist).

Free, no draw calls, covers everything past your scene boundary. Works great in VR. The mesh fog provides a more specific visual look; built-in fog gives general atmospheric depth.

Consider built-in fog as the default unless the mesh fog has a specific aesthetic you need.

## Related performance tips

- Always check the **Stats** panel after adding many objects in VR.
- Keep total **Batches** under ~150-200 per frame for Quest 3 smoothness.
- For repeated objects (trees, rocks, fog), GPU Instancing or Static Batching is the first lever to pull.
- Transparent meshes are 2-4× more expensive than opaque due to overdraw — avoid stacking many transparent layers in view.
