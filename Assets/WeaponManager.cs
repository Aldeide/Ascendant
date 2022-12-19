using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponManager : MonoBehaviour
{
    public Weapon currentWeapon;
    public float isFiring;

    private GameObject weaponModel;
    public float lastFired = 0;
    public float fireDelay = 100;
    public Transform muzzleTransform;

    void Start()
    {
        if (currentWeapon == null)
        {
            Debug.Log("No Weapon equipped");
            return;
        }
        // A weapon is equipped. Instantiating the weapon and computing some values.
        weaponModel = Instantiate(currentWeapon.weaponModel, transform.Find("First-Person").transform.Find("Head Location").transform);
        lastFired = 0;
        fireDelay = currentWeapon.fireRate / 60.0f;
        muzzleTransform = weaponModel.transform.Find("Muzzle").transform;
    }

    // Update is called once per frame
    void Update()
    {
        if (lastFired > 0)
        {
            lastFired -= Time.deltaTime;
        }
        if (currentWeapon == null)
        {
            Debug.Log("No Weapon equipped");
            return;
        }
        if (isFiring > 0 && lastFired <= 0)
        {
            var projectile = Instantiate(currentWeapon.projectile, muzzleTransform.position, muzzleTransform.rotation);
            lastFired = fireDelay;
        }
    }

    public void OnFire(InputAction.CallbackContext context)
    {
        isFiring = context.ReadValue<float>();
        Debug.Log(isFiring);
    }
}
