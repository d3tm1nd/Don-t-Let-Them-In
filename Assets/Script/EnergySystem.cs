using UnityEngine;
using UnityEngine.UI;

public class EnergySystem : MonoBehaviour
{
    public Image[] energyIcons;   // ลาก 4 ภาพ Energy มา

    void Update()
    {
        for (int i = 0; i < energyIcons.Length; i++)
            energyIcons[i].enabled = i < PhaseManager.Instance.energy;
    }

    public void OnUseEnergyButton()
    {
        PhaseManager.Instance.UseEnergy();
    }
}