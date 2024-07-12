using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public static CameraFollow Instance;
    private Transform target;
    // Start is called before the first frame update
    void Awake()
    {
        if(Instance == null) 
        {
            Instance = this;
        }
        else if(Instance != this)
        {
            Destroy(Instance);
        }
    }

    private void OnDestroy()
    {
        Instance = null;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (target == null) return;

        transform.SetPositionAndRotation(target.position, target.rotation);
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
}
