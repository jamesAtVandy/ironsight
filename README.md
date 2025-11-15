# ironsight
construction AR / AI assisting agent

How this becomes aeusable Unity protocol
Unity-side mental model
Scene Loader (runs once):
Reads Scene JSON.
Builds all objects and stores them in a dictionary:
Dictionary<string, GameObject> objectsById.
Overlay Manager (polls frequently or listens via WebSocket):
Reads Overlay JSON.
Finds the current step (by stepId or index).
For each overlay in that step:
object = objectsById[overlay.objectId]
If kind == "outline":
Draws a line/mesh following points in the object’s local space.
If kind == "marker_group":
Spawns marker prefabs at each (u, v) on the object’s face.
Clears or updates old overlays each time.
Why this is replicable for future overlays
You can add more kinds later:
"label" → draw text label at (u, v).
"arrow" → an arrow from (u1, v1) to (u2, v2).
"dimension_line" → line with tet showing measurement.
"heatmap" → colors mapped over surface.


ALL OBJECTS follow the same structure:
{
  "id": "something",
  "objectId": "some_object",
  "kind": "new_kind",
  "space": "localUV",
  "style": { ... },
  "points": [ ... ]   // or extra fields defined for that kind
}
