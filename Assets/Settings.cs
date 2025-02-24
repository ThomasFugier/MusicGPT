using UnityEngine;
using UnityEngine.UI;
using UISwitcher;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class Settings : MonoBehaviour
{
    public UINullableToggle highlightScales;
    public UINullableToggle lockKeys;

    public List<Toggle> modesToggles = new List<Toggle>();
    public List<Toggle> tonaliteToggles = new List<Toggle>();

    public Button playCompositionButton;
}
