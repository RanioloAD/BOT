using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobotUpgradesAndVisual : MonoBehaviour
{
    public KeyCode actionKey = KeyCode.F;
    public GameObject Luz;
    bool OnOff;


    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(actionKey) && GameManager.Instance.flashlight)
        {
            OnOff = !OnOff;
            Luz.SetActive(OnOff);
        }

        if (OnOff)
        {
            GameManager.Instance.ConsumirBateria(1);
        }
    }
}
