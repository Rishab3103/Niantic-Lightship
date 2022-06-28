using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Niantic.ARDK.Utilities.Input.Legacy;

public class ARProjectileManager : MonoBehaviour
{
    [SerializeField]
    private Camera ARCamera;

    [SerializeField]
    private GameObject[] projectilesPrefab;

    [SerializeField]
    private float force = 200.0f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (PlatformAgnosticInput.touchCount <= 0) return;

        var touch = PlatformAgnosticInput.GetTouch(0);

        if(touch.phase==TouchPhase.Began)
        {
            LaunchRandomProjectile();
        }
        
    }

    void LaunchRandomProjectile()
    {
        var prefab = projectilesPrefab[Random.Range(0, projectilesPrefab.Length)];
        var projectile = Instantiate(prefab, ARCamera.transform.position, Quaternion.identity);

        var projectileRigidBpdy = projectile.GetComponent<Rigidbody>();
        projectileRigidBpdy.AddForce(ARCamera.transform.forward * force);
    }
}
