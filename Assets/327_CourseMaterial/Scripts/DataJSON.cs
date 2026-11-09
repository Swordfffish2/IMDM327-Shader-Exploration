// IMDM327 Material
// JsonUtility reads one serializable object, so the JSON file has a "bodies" array.
using UnityEngine;
[System.Serializable]
public class SolarData
{
    public SolarBody[] bodies;
}

[System.Serializable]
public class SolarBody
{
    public string name;
    public string type;
    public float mass;
    public float orbitAU;
    public float distance;
    public float initial_velocity;
    public float radius;
}

public class DataJSON : MonoBehaviour
{
    // This one object contains the bodies array from solar.json.
    public SolarData solarData;

    void Awake()
    {
        // Loads Assets/Resources/JSON/solar.json. Do not include the .json extension.
        // The JSON is in its own folder so it does not share a name with solar.csv.
        TextAsset json = Resources.Load<TextAsset>("JSON/solar");

        // Turn the JSON text into a SolarData object.
        solarData = JsonUtility.FromJson<SolarData>(json.text);
    }
}
