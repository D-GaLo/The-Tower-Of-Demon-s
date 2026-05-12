using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HeroUI : MonoBehaviour {
    [Header("Textos")]
    public TextMeshProUGUI nombreText;
    public TextMeshProUGUI hpText;
    public TextMeshProUGUI energiaText; 

    [Header("Barras (Imágenes en modo Filled)")]
    public Image barraHP;
    public Image barraEnergia; 

    [Header("Círculos de Información")]
    public Image iconoClase;
    public Image iconoPosicion;

    private HeroStats miHeroe;

    public void ConfigurarUI(HeroStats heroe) {
        miHeroe = heroe;
        if (nombreText != null) nombreText.text = miHeroe.unitName;
        
        // --- COLOREAR CLASE Y POSICIÓN EN LA UI ---
        if (iconoClase != null) {
            if (heroe.unitClass == UnitClass.Melee) iconoClase.color = Color.red;
            else if (heroe.unitClass == UnitClass.Rango) iconoClase.color = Color.green;
            else if (heroe.unitClass == UnitClass.Tanque) iconoClase.color = Color.blue;
        }

        if (iconoPosicion != null) {
            if (heroe.unitPosition == UnitPosition.Tierra) iconoPosicion.color = new Color(0.6f, 0.3f, 0f); // Café
            else if (heroe.unitPosition == UnitPosition.Volando) iconoPosicion.color = Color.cyan;
            else if (heroe.unitPosition == UnitPosition.BajoTierra) iconoPosicion.color = Color.black;
        }

        ActualizarVida();
        ActualizarEnergia(); 
    }

    public void ActualizarVida() {
        if (miHeroe != null) {
            if (hpText != null) hpText.text = miHeroe.currentHP.ToString();
            if (barraHP != null && miHeroe.maxHP > 0) {
                float porcentajeHP = (float)miHeroe.currentHP / (float)miHeroe.maxHP;
                barraHP.fillAmount = porcentajeHP;
            }
        }
    }

    public void ActualizarEnergia() {
        if (miHeroe != null) {
            if (energiaText != null) energiaText.text = miHeroe.currentEnergy.ToString();
            if (barraEnergia != null && miHeroe.maxEnergy > 0) {
                float porcentajeEnergia = (float)miHeroe.currentEnergy / (float)miHeroe.maxEnergy;
                barraEnergia.fillAmount = porcentajeEnergia;
            }
        }
    }
}