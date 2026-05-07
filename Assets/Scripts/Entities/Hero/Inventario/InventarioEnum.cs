using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventarioEnum : MonoBehaviour
{
    //public static InventarioEnum Instance;


    public static InventarioEnum Instance
    {
        get { return Instance; }
        set { Instance = value; }
    }

    private int[] cantidades;

    private void Awake(){
        if (Instance == null) { 
            Instance = this; 
            DontDestroyOnLoad(gameObject); 
        }else { 
            Destroy(gameObject); return;
        }

        int cuenta = System.Enum.GetValues(typeof(Item)).Length;
        cantidades=new int[cuenta];
    }

    public  void AddItem(Item item, int cantidad){
        if(item == Item.None) return;

        cantidades[(int) item] +=cantidad;
    }
    

    public  void RemoveItem(Item item, int cantidad){
        if(item==Item.None) return;

        cantidades[(int)item]=Mathf.Max(0,cantidades[(int)item]- cantidad);
    }

    public int GetCantidad(Item item){
        return cantidades[(int)item];
    }
}
