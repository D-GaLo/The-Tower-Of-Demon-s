using UnityEngine;
using System.Collections;

public enum UnitClass { None, Melee, Rango, Tanque }
public enum UnitPosition { Tierra, BajoTierra, Volando }

public class UnitStats : MonoBehaviour {
    [Header("Identidad y Tipos")]
    public string unitName;
    public UnitClass unitClass;
    public UnitPosition unitPosition;

    [Header("Nivel")]
    public int level = 1;

    [Header("Estadísticas Base")]
    public int maxHP = 100;
    public int currentHP;
    public int attack = 10;
    public int defense = 5;
    public int speed = 10; 
    public int mastery = 0; 

    public virtual void TakeDamage(int damage) {
        currentHP -= damage;
        if (currentHP < 0) currentHP = 0;
        
        Debug.Log($"{unitName} recibió {damage} de daño. HP restante: {currentHP}");

        if (AudioManager.Instance != null) AudioManager.Instance.PlayGolpe();
        StartCoroutine(ParpadearDano());
    }

    IEnumerator ParpadearDano() {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null) {
            sr.color = Color.red;
            yield return new WaitForSeconds(0.15f);
            sr.color = Color.white;
        } else {
            UnityEngine.UI.Image img = GetComponent<UnityEngine.UI.Image>();
            if (img != null) {
                img.color = Color.red;
                yield return new WaitForSeconds(0.15f);
                img.color = Color.white;
            }
        }
    }
}