// IMDM327 Material
// Reads solar.csv into an array of BodyProperty structs. The CSV file is in Assets/Resources/solar.csv, so it can be read with Resources.Load().
using UnityEngine;
[System.Serializable]
public struct BodyProperty
{
    public float mass;              
    public float distance;         
    public float initial_velocity; 
    public float radius;
}

public class DataCSV : MonoBehaviour
{
    public BodyProperty[] bp;

    // Awake runs before Start methods, so other scripts can use bp in Start.
    void Awake()
    {
        LoadIntoArray();
    }

    void LoadIntoArray()
    {
        // Load Assets/Resources/solar.csv (omit extension)
        // https://docs.unity3d.com/6000.3/Documentation/ScriptReference/Resources.Load.html
        TextAsset csv = Resources.Load<TextAsset>("solar"); 
        if (csv == null) // - Safer
        {
            Debug.LogError("Resources/solar.csv not found.");
            bp = new BodyProperty[0];
            return;
        }
        // Ignore the empty row that can appear after the final line feed.
        string[] lines = csv.text.Split(new[] { '\n' }, System.StringSplitOptions.RemoveEmptyEntries);

        // Allocate array with read values
        bp = new BodyProperty[lines.Length];
        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i].Trim(); 
            // - Safer: Trim() is used to remove whitespace or specific characters
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }
            string[] cols = line.Split(',');
            if (cols.Length < 7)
            {
                Debug.LogWarning("Skipped a CSV row with fewer than 7 columns: " + line);
                continue;
            }

            if (float.TryParse(cols[2].Trim(), out float mass) &&
                float.TryParse(cols[3].Trim(), out float au) &&
                float.TryParse(cols[4].Trim(), out float dist) &&
                float.TryParse(cols[5].Trim(), out float vel) &&
                float.TryParse(cols[6].Trim(), out float radius))
            {
                // assignment into array
                // solar.csv already stores mass in kilograms.
                bp[i].mass = mass;
                bp[i].distance = dist;
                bp[i].initial_velocity = vel;
                bp[i].radius = radius;
            }
        }
    }
}
