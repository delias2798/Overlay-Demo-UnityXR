using UnityEngine;
using System.Collections.Generic;
using System.IO;

[System.Serializable]
public class ObjectPosition
{
    public string label;
    public float x;
    public float y;
    public float z;

    public ObjectPosition(string label, Vector3 position)
    {
        this.label = label;
        this.x = position.x;
        this.y = position.y;
        this.z = position.z;
    }
}

[System.Serializable]
public class ObjectPositionList
{
    public List<ObjectPosition> positions;
}

public class GenerateJSONPositions : MonoBehaviour
{
    public List<GameObject> gameObjectsToExport;

    public void GenerateJSON()
    {
        ObjectPositionList positionList = new ObjectPositionList();
        positionList.positions = new List<ObjectPosition>();

        foreach (var obj in gameObjectsToExport)
        {
            ObjectPosition pos = new ObjectPosition(obj.name, obj.transform.position);
            positionList.positions.Add(pos);
        }

        string json = JsonUtility.ToJson(positionList, true);

        // Save file in persistent data path
        string path = Path.Combine(Application.persistentDataPath, "positions.json");

        File.WriteAllText(path, json);

        Debug.Log($"JSON file created at: {path}");
    }

    void Start()
    {
        GenerateJSON();
    }
}
