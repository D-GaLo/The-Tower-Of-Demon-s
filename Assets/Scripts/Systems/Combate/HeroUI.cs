using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HeroUI : MonoBehaviour {
    [Header("Textos")]
    public TextMeshProUGUI nombreText;
    public TextMeshProUGUI hpText;
    public TextMeshProUGUI energiaText; // <-- NUEVO

    [Header("Barras (Imágenes en modo Filled)")]
    public Image barraHP;
    public Image barraEnergia; // <-- NUEVO

    private HeroStats miHeroe;

    public void ConfigurarUI(HeroStats heroe) {
        miHeroe = heroe;
        if (nombreText != null) nombreText.text = miHeroe.unitName;
        ActualizarVida();
        ActualizarEnergia(); // <-- Llamamos a la energía inicial
    }

    public void ActualizarVida() {
        if (miHeroe != null) {
            if (hpText != null) hpText.text = miHeroe.currentHP.ToString();
            if (barraHP != null) {
                float porcentajeHP = (float)miHeroe.currentHP / miHeroe.maxHP;
                barraHP.fillAmount = porcentajeHP;
            }
        }
    }

    // --- NUEVO: Función para la barra amarilla ---
    public void ActualizarEnergia() {
        if (miHeroe != null) {
            if (energiaText != null) energiaText.text = miHeroe.currentEnergy.ToString();
            if (barraEnergia != null) {
                float porcentajeEnergia = (float)miHeroe.currentEnergy / miHeroe.maxEnergy;
                barraEnergia.fillAmount = porcentajeEnergia;
            }
        }
    }
}