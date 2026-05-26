using System.Collections.Generic;
using UnityEngine;

public class InventarioEnum : MonoBehaviour
{
    public static InventarioEnum Instance { get; private set; }

    [Header("Base de Datos Maestra")]
    [Tooltip("Arrastra aquí TODOS los archivos de objetos que crees en Unity")]
    public List<ItemData> baseDeDatos = new List<ItemData>();

    private int[] cantidades;
    private bool[] descubiertos;

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
        descubiertos = new bool[cuenta];
    }

    public void AddItem(Item item, int cantidad){
        if(item == Item.None) return;

        ItemData data = GetItemData(item);
        int max = data != null ? data.maxStack : 99;

        cantidades[(int)item] = Mathf.Min(max, cantidades[(int)item] + cantidad);
        descubiertos[(int)item] = true;
    }
    
    public void RemoveItem(Item item, int cantidad){
        if(item == Item.None) return;
        cantidades[(int)item] = Mathf.Max(0, cantidades[(int)item] - cantidad);
    }

    public int GetCantidad(Item item){
        return cantidades[(int)item];
    }

    public ItemData GetItemData(Item item) {
        foreach(var data in baseDeDatos) {
            if (data.itemID == item) return data;
        }
        return null;
    }

    public List<ItemData> ObtenerObjetosPorCategoria(ItemCategory categoria) {
        List<ItemData> lista = new List<ItemData>();
        foreach(var data in baseDeDatos) {
            if (data.category == categoria && descubiertos[(int)data.itemID]) {
                lista.Add(data);
            }
        }
        return lista;
    }
}