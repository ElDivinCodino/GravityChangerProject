using UnityEngine;

public class WeaponSwitching : MonoBehaviour
{
    public int selectedWeapon = 0;
    public float secondsToSwitch = 0.4f;

    Animator anim;
    float nextTimeToSwitch;

    void Start()
    {
        SelectWeapon();
        anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (nextTimeToSwitch >= Time.time)
            return;

        int previousSelectedWeapon = selectedWeapon;

        if (Input.GetAxis("Mouse ScrollWheel") > 0f)
        {
            if (selectedWeapon < transform.childCount - 1)
                selectedWeapon++;
            else
                selectedWeapon = 0;
        }

        if (Input.GetAxis("Mouse ScrollWheel") < 0f)
        {
            if (selectedWeapon > 0)
                selectedWeapon--;
            else
                selectedWeapon = transform.childCount - 1;
        }

        if (Input.GetKeyDown (KeyCode.Alpha1))
            selectedWeapon = 0;

        if (Input.GetKeyDown (KeyCode.Alpha2) && transform.childCount >= 2)
            selectedWeapon = 1;

        if (Input.GetKeyDown (KeyCode.Alpha3) && transform.childCount >= 3)
            selectedWeapon = 2;

        if (Input.GetKeyDown (KeyCode.Alpha4) && transform.childCount >= 4)
            selectedWeapon = 3;

        if (previousSelectedWeapon != selectedWeapon)
        {
            anim.SetTrigger("ChangeWeapon");
            nextTimeToSwitch = Time.time + secondsToSwitch;
        }
    }

    void SelectWeapon()
    {
        int i = 0;

        foreach(Transform weapon in transform)
        {
            if (i == selectedWeapon)
                weapon.gameObject.SetActive(true);
            else
                weapon.gameObject.SetActive(false);

            i++;
        }
    }
}
