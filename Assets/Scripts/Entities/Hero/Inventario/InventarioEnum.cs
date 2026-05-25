using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventarioEnum : MonoBehaviour
{

    public static InventarioEnum Instance { get; private set; }

    private int[] cantidades;

    [Header("Sprites de Items")]
    public Sprite spriteEspada;
    public Sprite spriteBaculo;
    public Sprite spriteEscudo;
    public Sprite spriteLlave;
    public Sprite spriteTijeras;

    private void Awake(){
        if (Instance == null) { 
            Instance = this; 
            DontDestroyOnLoad(gameObject); 
        } else { 
            Destroy(gameObject); 
            return;
        }

        int cuenta = System.Enum.GetValues(typeof(Item)).Length;
        cantidades = new int[cuenta];
    }

    public void AddItem(Item item, int cantidad){
        if(item == Item.None) return;

        cantidades[(int) item] += cantidad;

        InventarioSlotUI[] todosLosSlots = FindObjectsOfType<InventarioSlotUI>();
        foreach (InventarioSlotUI slot in todosLosSlots)
        {
            if (slot.item == item)
            {
                slot.Refrescar();
            }
        }
    }
    
    public void RemoveItem(Item item, int cantidad){
        if(item == Item.None) return;

        cantidades[(int)item] = Mathf.Max(0, cantidades[(int)item] - cantidad);
    }

    public int GetCantidad(Item item){
        return cantidades[(int)item];
    }

    public Sprite GetSpriteDeItem(Item item)
    {
        switch (item)
        {
            case Item.Espada:
                return spriteEspada;
            case Item.Baculo:
                return spriteBaculo;
            case Item.Escudo:
                return spriteEscudo;
            case Item.Llave:
                return spriteLlave;
            case Item.Tijeras:
                return spriteTijeras;
            default:
                return null; // Si no encuentra nada o es Item.None
        }
    }
}