using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HeroStatsDisplay : MonoBehaviour
{
    [Header("Componentes de UI")]
    public TextMeshProUGUI nameText;
    public Slider hpSlider;
    public Slider energySlider;
    public TextMeshProUGUI statsText; // Para Attack, Defense, Speed

    public void MostrarHeroe(HeroStats hero)
    {
        if (hero == null) return;

        nameText.text = hero.unitName;
        
        hpSlider.maxValue = hero.maxHP;
        hpSlider.value = hero.currentHP;
        
        energySlider.maxValue = hero.maxEnergy;
        energySlider.value = hero.currentEnergy;
        statsText.text = $"Ataque: {hero.GetTotalAttack()}\n" +
                         $"Defensa: {hero.defense}\n" +
                         $"Velocidad: {hero.speed}";
    }
}
