using UnityEngine;
using UnityEngine.UI;

public class TOOGLE : MonoBehaviour
{
    private Toggle toggle;
    void Start()
    {
        toggle = GetComponent<Toggle>();
        if (toggle == null) return;
        toggle.isOn = PlayerPrefs.GetString("degree") == toggle.name;
        toggle.onValueChanged.AddListener(ChangeDegree);
    }

    public void ChangeDegree(bool value)
    {
        if (toggle == null) return;
        if (value)
        {
            PlayerPrefs.SetString("degree", toggle.name);
            PlayerPrefs.Save();
        }
        var mgr = FindObjectOfType<SceneAndUserMgt>();
        if (mgr != null) mgr.DegreeSet();
    }
}
