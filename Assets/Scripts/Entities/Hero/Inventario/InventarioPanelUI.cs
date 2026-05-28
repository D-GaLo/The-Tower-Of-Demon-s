using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class InventarioPanelUI : MonoBehaviour {
    public static InventarioPanelUI Instance { get; private set; }
    
    [Tooltip("El objeto principal Panel_Inventario")]
    public GameObject panelVisual; 

    [Header("Referencias del Grid")]
    public Transform contenedorGrid;
    public GameObject slotPrefab;

    [Header("Referencias del Panel de Info")]
    public GameObject panelInfo;
    public Image iconoInfo;
    public TextMeshProUGUI tituloInfo;
    public TextMeshProUGUI descInfo;
    public TextMeshProUGUI statsInfo;
    public GameObject btnEquipar; 
    public TextMeshProUGUI textoTituloPestana; 
    public TextMeshProUGUI textoCantidadInventario; 
    public TextMeshProUGUI textoMensajeError;
    public GameObject btnDesequipar; 

    private ItemCategory pestanaActual = ItemCategory.Objeto;
    
    private EquipSlotUI slotActivoParaEquipar;
    private ItemData itemSeleccionado;
    
    private float timeScalePrevio;

    void Awake() {
        if (Instance == null) Instance = this;
    }

    void Start() {
        if (panelInfo != null) panelInfo.SetActive(false);
        if (btnDesequipar != null) btnDesequipar.SetActive(false);
        if (textoMensajeError != null) textoMensajeError.gameObject.SetActive(false);
    }

    void Update() {
        if (panelVisual != null && panelVisual.activeSelf && Input.GetKeyDown(KeyCode.Escape)) {
            CerrarInventario();
        }
    }

    public void CerrarInventario() {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayClic();
        if (panelVisual != null) panelVisual.SetActive(false);
        slotActivoParaEquipar = null; 
        if (btnDesequipar != null) btnDesequipar.SetActive(false);
        
        Time.timeScale = timeScalePrevio; 
    }

    public void AbrirInventarioGeneral() {
        slotActivoParaEquipar = null; 
        if (panelVisual != null) panelVisual.SetActive(true);
        if (btnEquipar != null) btnEquipar.SetActive(false); 
        if (btnDesequipar != null) btnDesequipar.SetActive(false); 
        
        timeScalePrevio = Time.timeScale; 
        Time.timeScale = 0f; 

        BotonPestanaObjetos(); 
    }

    public void AbrirParaSlot(EquipSlotUI slot) {
        slotActivoParaEquipar = slot;
        if (panelVisual != null) panelVisual.SetActive(true);

        if (btnDesequipar != null) {
            btnDesequipar.SetActive(slot.equipo.slots[slot.slotIndex] != null);
        }

        timeScalePrevio = Time.timeScale; 
        Time.timeScale = 0f; 

        if (slot.slotIndex == 0) BotonPestanaArmas();
        else BotonPestanaObjetos();
    }

    public void BotonPestanaArmas() => AbrirPestana(ItemCategory.Arma);
    public void BotonPestanaObjetos() => AbrirPestana(ItemCategory.Objeto);
    public void BotonPestanaClaves() => AbrirPestana(ItemCategory.ObjetoClave);

    public void AbrirPestana(ItemCategory categoria) {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayClic();
        pestanaActual = categoria;

        if (textoTituloPestana != null) {
            if (categoria == ItemCategory.Arma) textoTituloPestana.text = "ARMAS";
            else if (categoria == ItemCategory.Objeto) textoTituloPestana.text = "OBJETOS";
            else if (categoria == ItemCategory.ObjetoClave) textoTituloPestana.text = "OBJETOS CLAVE";
        }

        if (panelInfo != null) panelInfo.SetActive(false); 
        ActualizarGrid();
    }

    public void ActualizarGrid() {
        foreach (Transform child in contenedorGrid) Destroy(child.gameObject);
        if (InventarioEnum.Instance == null) return;
        
        List<ItemData> objetosAMostrar = InventarioEnum.Instance.ObtenerObjetosPorCategoria(pestanaActual);

        foreach (ItemData item in objetosAMostrar) {
            GameObject nuevoSlot = Instantiate(slotPrefab, contenedorGrid);
            InventarioSlotUI slotScript = nuevoSlot.GetComponent<InventarioSlotUI>();
            
            int cantidad = InventarioEnum.Instance.GetCantidad(item.itemID);
            slotScript.Configurar(item, cantidad, this);
            
            Button btn = nuevoSlot.GetComponent<Button>();
            if (btn != null) btn.onClick.AddListener(slotScript.AlHacerClic);
        }
    }

    public void MostrarDetalles(ItemData data, int cantidad) {
        itemSeleccionado = data;
        if (panelInfo != null) panelInfo.SetActive(true);
        iconoInfo.sprite = data.itemIcon;
        tituloInfo.text = data.itemName;
        
        if (textoCantidadInventario != null) {
            if (data.maxStack > 1) {
                textoCantidadInventario.text = $"En inventario: {cantidad}";
                textoCantidadInventario.gameObject.SetActive(true);
            } else {
                textoCantidadInventario.gameObject.SetActive(false);
            }
        }

        bool puedeEquiparEnSlot = false;
        bool esElObjetoEquipado = false; 
        string mensajeError = "";

        if (slotActivoParaEquipar != null) {
            HeroStats heroeActual = slotActivoParaEquipar.equipo.GetComponent<HeroStats>();
            puedeEquiparEnSlot = slotActivoParaEquipar.equipo.PuedeEquipar(slotActivoParaEquipar.slotIndex, data);
            
            esElObjetoEquipado = (slotActivoParaEquipar.equipo.slots[slotActivoParaEquipar.slotIndex] == data);

            if (data.category == ItemCategory.Arma && data.requiredClass != heroeActual.unitClass) {
                mensajeError = $"¡Esta arma no es para la clase {heroeActual.unitClass}!";
            } 
            else if (!esElObjetoEquipado && cantidad <= 0) {
                mensajeError = $"No tienes más unidades disponibles para equipar.";
            }
        }

        descInfo.text = data.itemDescription;

        if (textoMensajeError != null) {
            textoMensajeError.text = mensajeError;
            if (string.IsNullOrEmpty(mensajeError)) {
                textoMensajeError.gameObject.SetActive(false);
            } else {
                textoMensajeError.color = new Color(1f, 0.33f, 0.33f); 
                textoMensajeError.gameObject.SetActive(true);
            }
        }

        string stats = "";
        if (data.bonusATK > 0) stats += $"ATK +{data.bonusATK}   ";
        if (data.bonusDEF > 0) stats += $"DEF +{data.bonusDEF}   ";
        if (data.bonusSPD > 0) stats += $"SPD +{data.bonusSPD}   ";
        if (data.bonusHP > 0) stats += $"HP +{data.bonusHP}   ";
        if (data.bonusEnergy > 0) stats += $"ENG +{data.bonusEnergy}   ";
        if (data.bonusMastery > 0) stats += $"MST +{data.bonusMastery}   ";

        if (data.requiredClass != UnitClass.None && data.category == ItemCategory.Arma) {
            stats += $"\n<color=#FFD700>Clase requerida: {data.requiredClass}</color>";
        }
        statsInfo.text = stats;

        if (slotActivoParaEquipar != null) {
            if (btnEquipar != null) btnEquipar.SetActive(puedeEquiparEnSlot && cantidad > 0 && !esElObjetoEquipado);
            if (btnDesequipar != null) btnDesequipar.SetActive(esElObjetoEquipado);
        } else {
            if (btnEquipar != null) btnEquipar.SetActive(false);
            if (btnDesequipar != null) btnDesequipar.SetActive(false);
        }
    }

    public void AccionBotonEquipar() {
        if (slotActivoParaEquipar != null && itemSeleccionado != null) {
            if (slotActivoParaEquipar.equipo.Equipar(slotActivoParaEquipar.slotIndex, itemSeleccionado)) {
                if (AudioManager.Instance != null) AudioManager.Instance.PlayEspada(); 
                
                slotActivoParaEquipar.Refrescar();
                CerrarInventario(); 
                
                if (HeroStatsPanelUI.Instance != null) HeroStatsPanelUI.Instance.ActualizarVistaActiva();
            }
        }
    }

    public void AccionBotonDesequipar() {
        if (slotActivoParaEquipar != null && slotActivoParaEquipar.equipo.slots[slotActivoParaEquipar.slotIndex] != null) {
            slotActivoParaEquipar.equipo.Desequipar(slotActivoParaEquipar.slotIndex);
            
            if (AudioManager.Instance != null) AudioManager.Instance.PlayClic();
            
            slotActivoParaEquipar.Refrescar();
            CerrarInventario(); 
            
            if (HeroStatsPanelUI.Instance != null) HeroStatsPanelUI.Instance.ActualizarVistaActiva();
        }
    }
}