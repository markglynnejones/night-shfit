using UnityEngine;

// Quiet playtest feedback; deliberately no score, timer or prescribed workflow.
public sealed class PrototypeOrganisationHud : MonoBehaviour
{
    private int total;
    private int correctlyOrganised;
    private int misfiled;
    private int loose;
    private float nextRefreshTime;
    private GUIStyle titleStyle;
    private GUIStyle detailsStyle;

    private void Update()
    {
        if (Time.unscaledTime >= nextRefreshTime && PrototypeSortingSetup.HasCompletedSetup)
        {
            Refresh();
            nextRefreshTime = Time.unscaledTime + 0.5f;
        }
    }

    private void Refresh()
    {
        MediaItem[] stock = FindObjectsByType<MediaItem>(FindObjectsInactive.Exclude);
        ShelfSlot[] shelves = FindObjectsByType<ShelfSlot>(FindObjectsInactive.Exclude);

        total = stock.Length;
        int shelved = 0;
        correctlyOrganised = 0;
        foreach (ShelfSlot shelf in shelves)
        {
            shelved += shelf.ItemCount;
            correctlyOrganised += shelf.CountCorrectlyOrganisedItems();
        }

        misfiled = Mathf.Max(0, shelved - correctlyOrganised);
        loose = Mathf.Max(0, total - shelved);
    }

    private void OnGUI()
    {
        if (!PrototypeSortingSetup.HasCompletedSetup || total == 0)
        {
            return;
        }

        if (titleStyle == null)
        {
            titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 17,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white }
            };
            detailsStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 13,
                normal = { textColor = new Color(0.86f, 0.90f, 0.91f) }
            };
        }

        Rect panel = new(18f, 16f, 345f, 112f);
        Color oldColor = GUI.color;
        GUI.color = new Color(0.08f, 0.11f, 0.15f, 0.94f);
        GUI.DrawTexture(panel, Texture2D.whiteTexture);
        GUI.color = oldColor;

        GUI.Label(new Rect(30f, 23f, 320f, 28f), "ROCK / INDIE  •  NIGHT SHIFT", titleStyle);
        GUI.Label(new Rect(30f, 50f, 310f, 25f),
            correctlyOrganised + " / " + total + " correctly shelved", detailsStyle);
        GUI.Label(new Rect(30f, 74f, 310f, 24f),
            loose + " loose  •  " + misfiled + " misplaced", detailsStyle);

        Rect track = new(30f, 104f, 318f, 7f);
        GUI.color = new Color(0.28f, 0.34f, 0.37f);
        GUI.DrawTexture(track, Texture2D.whiteTexture);
        GUI.color = new Color(0.54f, 0.82f, 0.72f);
        GUI.DrawTexture(new Rect(track.x, track.y,
            track.width * (correctlyOrganised / (float)total), track.height),
            Texture2D.whiteTexture);
        GUI.color = oldColor;

        GUI.Label(new Rect(19f, 132f, 580f, 20f),
            "E: pick up / place / drop     F5: save     F9: reload     Esc: cursor",
            detailsStyle);
    }
}
