using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HeroUI : MonoBehaviour {
    [Header("Textos y Barras")]
    public TextMeshProUGUI nombreText;
    public TextMeshProUGUI hpText;
    public TextMeshProUGUI energiaText; 
    public Image barraHP;
    public Image barraEnergia; 

    [Header("Progreso y Nivel")]
    public TextMeshProUGUI nivelText;
    public TextMeshProUGUI xpText;
    public Image barraXP;

    [Header("Imágenes en la UI (Los círculos)")]
    public Image iconoClase;
    public Image iconoPosicion;

    [Header("Sprites de Clase")]
    public Sprite spriteMelee;
    public Sprite spriteRango;
    public Sprite spriteTanque;

    [Header("Sprites de Posición")]
    public Sprite spriteTierra;
    public Sprite spriteBajoTierra;
    public Sprite spriteVolando;

    private HeroStats miHeroe;

    public void ConfigurarUI(HeroStats heroe) {
        miHeroe = heroe;
        if (nombreText != null) nombreText.text = miHeroe.unitName;
        
        ActualizarVida();
        ActualizarEnergia(); 
        ActualizarNivelYXP();
        
        AsignarIconos();
    }

    public void ActualizarNivelYXP() {
        if (miHeroe != null) {
            if (nivelText != null) nivelText.text = "Lv. " + miHeroe.level;

            if (miHeroe.level >= miHeroe.maxLevel) {
                if (barraXP != null) barraXP.fillAmount = 1f;
                if (xpText != null) xpText.text = "MAX";
            } 
            else {
                int xpBaseDeEsteNivel = miHeroe.xpParaNivel[miHeroe.level - 1]; 
                int xpParaSiguienteNivel = miHeroe.xpParaNivel[miHeroe.level];
                
                int xpConseguidaEnEsteNivel = miHeroe.currentXP - xpBaseDeEsteNivel;
                int xpRequeridaEnEsteNivel = xpParaSiguienteNivel - xpBaseDeEsteNivel;

                if (barraXP != null) {
                    barraXP.fillAmount = (float)xpConseguidaEnEsteNivel / (float)xpRequeridaEnEsteNivel;
                }

                if (xpText != null) {
                    xpText.text = $"{xpConseguidaEnEsteNivel} / {xpRequeridaEnEsteNivel}";
                }
            }
        }
    }

    void AsignarIconos() {
        if (iconoClase != null) {
            switch (miHeroe.unitClass) {
                case UnitClass.Melee: iconoClase.sprite = spriteMelee; break;
                case UnitClass.Rango: iconoClase.sprite = spriteRango; break;
                case UnitClass.Tanque: iconoClase.sprite = spriteTanque; break;
            }
            iconoClase.color = Color.white; 
        }

        if (iconoPosicion != null) {
            switch (miHeroe.unitPosition) {
                case UnitPosition.Tierra: iconoPosicion.sprite = spriteTierra; break;
                case UnitPosition.BajoTierra: iconoPosicion.sprite = spriteBajoTierra; break;
                case UnitPosition.Volando: iconoPosicion.sprite = spriteVolando; break;
            }
            iconoPosicion.color = Color.white; 
        }
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