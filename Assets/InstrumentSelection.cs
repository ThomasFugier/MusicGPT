using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class InstrumentSelection : MonoBehaviour
{

    public InstrumentToggle[] toggles;
    public List<InstrumentToggle> selectedInstruments = new List<InstrumentToggle>();
    public MusicPlayer musicPlayer;

    void Start()
    {
        toggles[0].toggle.isOn = !toggles[0].toggle.isOn;
        toggles[0].toggle.isOn = true;
    }

    
    
    void Update()
    {
        
    }

    public void InstrumentSelected(int i)
    {
        if (toggles[i].toggle.isOn)
        {
            if (selectedInstruments.Contains(toggles[i]) == false)
            {
                selectedInstruments.Add(toggles[i]);
                toggles[i].EnableSelection(selectedInstruments.Count);
            }
        }

        else
        {
            if (selectedInstruments.Contains(toggles[i]) == true)
            {
                selectedInstruments.Remove(toggles[i]);
                toggles[i].DisableSelection();
            }
        }

        for(int j = 0; j < selectedInstruments.Count; j++)
        {
            if(j == 0 && selectedInstruments.Count == 1)
            {
                selectedInstruments[j].toggle.interactable = false;
            }

            else
            {
                selectedInstruments[j].toggle.interactable = true;
            }
                
            selectedInstruments[j].selectionIndexText.text = (j + 1).ToString();
        }

        List<Instrument> selected = new List<Instrument>();

        for(int a = 0; a < selectedInstruments.Count; a++)
        {
            selected.Add(selectedInstruments[a].instrument);
        }

        musicPlayer.instrumentsForTracks = selected;

        CheckThereIsAtLeastOneSelected();
    }

    public void CheckThereIsAtLeastOneSelected()
    {
        if(selectedInstruments.Count == 0)
        {
            toggles[0].toggle.isOn = !toggles[0].toggle.isOn;
            toggles[0].toggle.isOn = true;
        }
    }
}
