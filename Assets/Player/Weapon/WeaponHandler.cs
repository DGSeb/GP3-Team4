using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponHandler : MonoBehaviour
{
    [Header("Keybinds")]
    public KeyCode shootKey = KeyCode.Mouse0;

    [Header("Objects")]
    public GameObject Effect;
    public Transform Emitter;

    private void Update()
    {
        if (Input.GetKeyDown(shootKey))
        {
            print("Pew!");
            Instantiate(Effect, Emitter);
        }
    }
}
