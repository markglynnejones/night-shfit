using UnityEngine;

// Quiet organising feedback. No XP, shift timer or prescribed workflow.
// Panels are separated so help text does not overlap the progress card.
public sealed class PrototypeOrganisationHud : MonoBehaviour
{
    private int total;
    private int correctlyOrganised;
    private int misfiled;
    private int offShelves;
    private float nextRefreshTime;
    private GUIStyle headingStyle;
    private GUIStyle detailStyle;
    private GUIStyle controlStyle;

    private void Update()
    {
        if (PrototypeSortingSetup.HasCompletedSetup
            && Time.unscaledTime >= nextRefreshTime)
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
        correctlyOrganised = 0;
        int shelved = 0;
        foreach (ShelfSlot shelf in shelves)
        {
            shelved += shelf.ItemCount;
            correctlyOrganised += shelf.CountCorrectlyOrganisedItems();
        }

        misfiled = Mathf.Max(0, shelved - correctlyOrganised);
        offShelves = Mathf.Max(0, total - shelved);
    }

    private void OnGUI()
    {
        if (!PrototypeSortingSetup.HasCompletedSetup || total <= 0)
        {
            return;
        }

        if (headingStyle == null)
        {
            headingStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 19,
                fontStyle = FontStyle.Bold,
                clipping = TextClipping.Clip,
                normal = { textColor = new Color(0.97f, 0.94f, 0.87f) }
            };
            detailStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 15,
                clipping = TextClipping.Clip,
                normal = { textColor = new Color(0.84f, 0.89f, 0.89f) }
            };
            controlStyle = new GUIStyle(detailStyle)
            {
                fontSize = 13,
                normal = { textColor = new Color(0.91f, 0.94f, 0.87f) }
            };
        }

        float panelWidth = Mathf.Min(355f, Screen.width * 0.47f);
        Color oldColor = GUI.color;

        GUI.color = new Color(0.075f, 0.13f, 0.17f, 0.96f);
        GUI.DrawTexture(new Rect(12f, 12f, panelWidth, 133f),
            Texture2D.whiteTexture);
        GUI.color = oldColor;

        GUI.Label(new Rect(23f, 19f, panelWidth - 22f, 28f),
            "NIGHT SHIFT  /  MUSIC", headingStyle);
        GUI.Label(new Rect(23f, 51f, panelWidth - 22f, 24f),
            correctlyOrganised + " / " + total + " in correct order", detailStyle);
        GUI.Label(new Rect(23f, 79f, panelWidth - 22f, 24f),
            offShelves + " off shelves   •   " + misfiled + " misplaced", detailStyle);

        float trackWidth = Mathf.Max(10f, panelWidth - 24f);
        GUI.color = new Color(0.31f, 0.37f, 0.39f);
        GUI.DrawTexture(new Rect(23f, 121f, trackWidth, 8f),
            Texture2D.whiteTexture);
        GUI.color = new Color(0.49f, 0.82f, 0.72f);
        GUI.DrawTexture(new Rect(23f, 121f,
            trackWidth * Mathf.Clamp01(correctlyOrganised / (float)total), 8f),
            Texture2D.whiteTexture);
        GUI.color = oldColor;

        float helpY = Mathf.Max(152f, Screen.height - 35f);
        float helpWidth = Mathf.Min(Screen.width * 0.53f, 595f);
        GUI.color = new Color(0.075f, 0.13f, 0.17f, 0.89f);
        GUI.DrawTexture(new Rect(12f, helpY, helpWidth, 29f),
            Texture2D.whiteTexture);
        GUI.color = oldColor;

        GUI.Label(new Rect(19f, helpY + 4f, helpWidth - 14f, 22f),
            "E  collect / shelve     Wheel  browse     G  drop     Shift+E  force shelve",
            controlStyle);
    }
}
