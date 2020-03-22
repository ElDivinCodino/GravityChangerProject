using UnityEngine;
using UnityEngine.VFX;

public class Weapon : MonoBehaviour
{
    public float damage, range, impactForce, fireRate;
    public Camera shootingCam;
    public VisualEffect muzzleFlash;

    private float nextTimeToFire;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButton("Fire1") && nextTimeToFire < Time.time)
        {
            Shoot();
            nextTimeToFire = Time.time + 1f / fireRate;
        }
    }

    private void Shoot()
    {
        muzzleFlash.SendEvent("Shoot");

        RaycastHit hit;
        if (Physics.Raycast(shootingCam.transform.position, shootingCam.transform.forward, out hit, range))
        {
            Debug.Log(hit.transform.name);

            Target t = hit.transform.GetComponent<Target>();

            if (t != null)
            {
                t.TakeDamage(damage);
            }

            Rigidbody rb = hit.transform.GetComponent<Rigidbody>();

            if (rb != null)
            {
                rb.AddForce(-hit.normal * impactForce);
            }
        }
    }
}
