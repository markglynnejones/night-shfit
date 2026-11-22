using UnityEngine;

// Album information is drawn directly into each shared procedural cover
// texture and on the readable focus/hand-stack HUD. The original prototype
// created four tiny TextMesh children per physical CD (5,000+ objects at store
// scale), producing overlapping labels. Keep this component as a lightweight
// compatibility hook for PhysicalInteractable while avoiding that overhead.
[RequireComponent(typeof(MediaItem))]
public sealed class AlbumCaseLabel : MonoBehaviour
{
    public void SetShelfMode(bool enabled)
    {
        // The illustrated front is visible both in a browse tray and in hand.
        // No 3D text is needed; detailed artist/title metadata is in the HUD.
    }
}
