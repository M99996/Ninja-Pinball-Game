using System.Collections;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    private static CameraShake instance;
    public static CameraShake Instance
    {
        get
        {
            if (instance == null)
            {
                instance = Camera.main?.GetComponent<CameraShake>();
                if (instance == null && Camera.main != null)
                {
                    GameObject cameraObj = Camera.main.gameObject;
                    instance = cameraObj.AddComponent<CameraShake>();
                }
            }
            return instance;
        }
    }

    private Vector3 originalPosition;
    private bool isShaking = false;

    void Awake()
    {
        if (Camera.main != null && Camera.main.gameObject == gameObject)
        {
            originalPosition = transform.localPosition;
        }
    }

    public void Shake(float duration, float magnitude)
    {
        if (!isShaking)
        {
            StartCoroutine(ShakeCoroutine(duration, magnitude));
        }
    }

    private IEnumerator ShakeCoroutine(float duration, float magnitude)
    {
        isShaking = true;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            transform.localPosition = originalPosition + new Vector3(x, y, 0f);

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = originalPosition;
        isShaking = false;
    }
}

