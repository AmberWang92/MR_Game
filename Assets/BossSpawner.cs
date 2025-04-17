using UnityEngine;
using Meta.XR.MRUtilityKit;

public class BossSpawner : MonoBehaviour
{
    public GameObject bossPrefab;

    void Start()
    {
        // 确保 MRUK 初始化完毕
        MRUK.Instance.RegisterSceneLoadedCallback(OnSceneLoaded);
    }

    void OnSceneLoaded()
    {
        Debug.Log("MRUK Initialized");

        // 获取当前房间的所有地面类型 Anchor
        var currentRoom = MRUK.Instance.GetCurrentRoom();
        if (currentRoom == null)
        {
            Debug.LogWarning("No current room found!");
            return;
        }

        var floorAnchor = currentRoom.FloorAnchor;

        if (floorAnchor == null)
        {
            Debug.LogWarning("No floor anchors found!");
            return;
        }

        // 获取玩家位置和朝向
        var camera = Camera.main.transform;
        Vector3 forward = camera.forward;
        forward.y = 0;
        forward.Normalize();

        Vector3 desiredPos = camera.position + forward * 1.5f;

        if (floorAnchor == null)
        {
            Debug.LogWarning("No floor anchors found!");
            return;
        }
        else
        {
            Vector3 spawnPos = floorAnchor.transform.position;
            GameObject boss = Instantiate(bossPrefab, spawnPos, Quaternion.identity);

            Vector3 lookAt = new Vector3(camera.position.x, spawnPos.y, camera.position.z);
            boss.transform.LookAt(lookAt);

            Debug.Log("Boss spawned on floor at " + spawnPos);
        }
    }
}
