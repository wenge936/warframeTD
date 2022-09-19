using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Turret : MonoBehaviour
{
    private Transform target;
    private Enemy targetEnemy;

    [Header("General")]
    public float range = 15f;
    public Transform firePoint;
    public Transform partToRotate;
    public float turnSpeed = 10f;

    [Header("Use Bullets/Missiles (Default)")]
    public GameObject bulletPrefab;
    public float fireRate = 1f;
    public int shots = 1;
    public float burstRate = 0.2f;
    private float fireCountdown = 0f;

    [Header("Use Laser")]
    public float slowAmount = 0.5f;
    public int damageOverTime = 30;
    public bool useLaser = false;
    public Light impactLight;
    public ParticleSystem impactEffect;
    public LineRenderer lineRenderer;

    [Header("Unity Setup Fields")]
    public string enemyTag = "Enemy";



    // Start is called before the first frame update
    void Start()
    {
        InvokeRepeating("UpdateTarget", 0f, 0.5f);
    }

    void UpdateTarget() {

        GameObject[] enemies = GameObject.FindGameObjectsWithTag(enemyTag);
        float shortestDistance = Mathf.Infinity;
        GameObject nearestEnemy = null;
        foreach (GameObject enemy in enemies) {
            float distanceToEnemy = Vector3.Distance(transform.position, enemy.transform.position);
            if (distanceToEnemy < shortestDistance) {
                shortestDistance = distanceToEnemy;
                nearestEnemy = enemy;
            }
            
            if (nearestEnemy != null && shortestDistance <= range) {
                target = nearestEnemy.transform;
                targetEnemy = nearestEnemy.GetComponent<Enemy>();
            }
            else {
                target = null;
            }
        }

    }
    // Update is called once per frame
    void Update() {
        if (target == null) 
        {
            if (useLaser)
            {
                lineRenderer.enabled = false;
                impactEffect.Stop();
                impactLight.enabled = false;
            }
            return;
        }

        LockOnTarget();

        if (useLaser)
        {
            Laser();
        } 
        else
        {
            if (fireCountdown <= 0)
            {
                StartCoroutine(Shoot());
                fireCountdown = 1f / fireRate;
            }

            fireCountdown -= Time.deltaTime;
        }
    }

    void Laser()
    {

        targetEnemy.TakeDamage(damageOverTime * Time.deltaTime);
        targetEnemy.Slow(slowAmount);
        if (!lineRenderer.enabled)
        {
            lineRenderer.enabled = true;
            impactEffect.Play();
            impactLight.enabled = true;
        }

        lineRenderer.SetPosition(0, firePoint.position);
        lineRenderer.SetPosition(1, target.position);

        Vector3 dir = firePoint.position - target.position;
        impactEffect.transform.position = target.position + dir.normalized;
        impactEffect.transform.rotation = Quaternion.LookRotation(dir);

        
    }

    void LockOnTarget()
    {
        Vector3 dir = target.position - transform.position;
        Quaternion lookRotation = Quaternion.LookRotation(dir);
        Vector3 rotation = Quaternion.Lerp(partToRotate.rotation, lookRotation, Time.deltaTime * turnSpeed).eulerAngles;
        partToRotate.rotation = Quaternion.Euler(0f, rotation.y, 0f);
    }

    IEnumerator Shoot()
    {
        for (int i = 0; i < shots; i++)
        {
            GameObject currentBullet = (GameObject) Instantiate (bulletPrefab, firePoint.position, firePoint.rotation);
            Bullet bullet = currentBullet.GetComponent<Bullet>();
            if (bullet != null)
            {
                bullet.Seek(target);
            }
            yield return new WaitForSeconds(burstRate);
        }
    }

    void OnDrawGizmosSelected() {

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}
