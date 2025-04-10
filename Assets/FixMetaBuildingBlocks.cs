using UnityEditor;
using UnityEngine;

public class FixMetaBuildingBlocks : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        EditorPrefs.DeleteKey(null);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
