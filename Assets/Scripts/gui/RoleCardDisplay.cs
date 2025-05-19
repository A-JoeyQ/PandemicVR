using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static ENUMS;

public class RoleCardDisplay : MonoBehaviour
{

    private RoleCard roleCardData;

    public RoleCard RoleCardData
    {
        get { return roleCardData; }
        set { 
            roleCardData = value;
            UpdateRole();
        }
    }

    public Image background;

    public TextMeshProUGUI roleName;
    public TextMeshProUGUI roleText;

    // Start is called before the first frame update
    void Start()
    {

    }

    private void UpdateRole()
    {
        roleName.text = roleCardData.roleName;
        roleText.text = roleCardData.roleText;
        background.sprite = roleCardData.mainArtwork;
    }
}