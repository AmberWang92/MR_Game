using System.Collections;
using UnityEngine;

public class BossController : MonoBehaviour
{
    public ParticleSystem inkEffect;
    public float inkDuration = 3f;
    public float attackInterval = 8f;

    private bool isSpraying = false;
    private float timer = 0f;

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= attackInterval)
        {
            StartCoroutine(SprayInk());
            timer = 0f;
        }
    }

    IEnumerator SprayInk()
    {
        Debug.Log("Ink Attack Incoming!");
        isSpraying = true;

        inkEffect.Play();
        yield return new WaitForSeconds(inkDuration);
        inkEffect.Stop();

        isSpraying = false;
    }

    public bool IsSpraying()
    {
        return isSpraying;
    }
}
