using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WID_Info : MonoBehaviour
{
    private Transform Following { get; set; }
    [SerializeField] private Text _txtName;
    [SerializeField] private Text _txtDetails;

    public void Initialise(Transform following, string wpnName, int curAmmo, int backupAmmo, WeaponRangeType type)
    {
        Following = following;
        _txtName.text = wpnName;
        _txtDetails.text = $"AMMO: {curAmmo} / {backupAmmo}\n{type}";
    }

    void Update()
    {
        transform.position = Following.position + Vector3.up * 0.5f;
        // make the widget always face to camera
        transform.forward =Camera.main.transform.forward;
    }
}
