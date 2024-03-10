using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponHandler : MonoBehaviour
{
    [Header("Keybinds")]
    public KeyCode shootKey = KeyCode.Mouse0;

    [Header("References")]
    public GameObject Effect;
    public GameObject hitEffect;
    public GameObject trail;
    public Transform Emitter;
    public Camera playerCam;
    public PlayerCam recoilScript;

    [Header("Options")]
    [SerializeField] float cooldown;
    [SerializeField] float travelSpeedFactor;

    [Header("Recoil Vectors")]
    [SerializeField] private float recoilX;
    [SerializeField] private float recoilY;
    [SerializeField] private float recoilZ;

    private bool canFire = true;

    private void Update()
    {
        if (Input.GetKey(shootKey) && canFire && GameManager.isPlayerActive)
        {
            canFire = false;
            StartCoroutine(Shoot(cooldown));
        }
    }

    private IEnumerator Shoot(float waitTime)
    {
        print("Pew!");
        recoilScript.RecoilFire(recoilX, recoilY, recoilZ);

        GameObject effectInstance = Instantiate(Effect, Emitter);
        Destroy(effectInstance, 5f);

        Vector3 aimPosition = playerCam.transform.position + playerCam.transform.forward * 100;

        RaycastHit hit;
        if (Physics.Raycast(playerCam.transform.position, playerCam.transform.forward, out hit, 9999f))
        {
            Debug.Log(hit.transform.name);

            aimPosition = hit.point;

            GameObject trailInstance = Instantiate(trail, Emitter);
            Destroy(trailInstance, 5f);

            StartCoroutine(LerpPosition(aimPosition, Vector3.Distance(Emitter.position, aimPosition) * travelSpeedFactor, trailInstance));

            //Vector3.Distance(Emitter.position, aimPosition) * travelSpeedFactor
        }
        else
        {
            GameObject trailInstance = Instantiate(trail, Emitter);
            Destroy(trailInstance, 5f);
            StartCoroutine(LerpPosition(aimPosition, Vector3.Distance(Emitter.position, aimPosition) * travelSpeedFactor, trailInstance));
        }

        yield return new WaitForSeconds(waitTime);

        canFire = true;
    }

    IEnumerator LerpPosition(Vector3 targetPosition, float duration, GameObject thingToLerp)
    {
        if (thingToLerp != null)
        {
            float time = 0;
            Vector3 startPosition = Emitter.transform.position;

            while (time < duration && thingToLerp != null)
            {
                thingToLerp.transform.position = Vector3.Lerp(startPosition, targetPosition, time / duration);
                time += Time.deltaTime;
                yield return null;
            }
            if (thingToLerp != null)
            {
                thingToLerp.transform.position = targetPosition;
                thingToLerp.GetComponent<TrailRenderer>().enabled = false;

                GameObject hitEffectInstance = Instantiate(hitEffect, thingToLerp.transform.position, thingToLerp.transform.rotation);
                Destroy(hitEffectInstance, 5f);
            }
        }
        else
        {
            yield return null;
        }
    }
}
