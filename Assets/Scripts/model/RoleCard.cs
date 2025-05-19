using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ENUMS;

[CreateAssetMenu(fileName = "New Role Card", menuName = "Cards/Role Card")]
public class RoleCard : ScriptableObject
{
    public string roleName;
    public string roleText;

    public Color roleColor;

    public Sprite mainArtwork;
}
