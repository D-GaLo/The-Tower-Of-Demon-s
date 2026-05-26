using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HeroStatsPanelUI : MonoBehaviour
{
    public static HeroStatsPanelUI Instance { get; private set; }

    [Header("Referencias del Panel")]
    public GameObject statsPanel;

    [Header("Héroes")]
    public HeroStats[] heroes = new HeroStats[3];

    [Header("Botones de Pestañas")]
    public Button[] tabButtons = new Button[3];
    public Color activeTabColor = Color.white;
    public Color inactiveTabColor = new Color(0.7f, 0.7f, 0.7f);

    [Header("Paneles de Contenido")]
    public GameObject[] contentPanels = new GameObject[3];

    [Header("3 slots para cada personaje")]
    public EquipSlotUI[] equipSlots = new EquipSlotUI[9];

    [Header("Sprites de Base de Datos UI")]
    public Sprite[] spritesPortratosHeroes = new Sprite[3]; 
    public Sprite spriteMelee;
    public Sprite spriteRango;
    public Sprite spriteTanque;
    public Sprite spriteTierra;
    public Sprite spriteBajoTierra;
    public Sprite spriteVolando;

    [Header("Campos Visuales — Héroe 1 (Sieg)")]
    public TextMeshProUGUI hero1_Name;
    public TextMeshProUGUI hero1_Class; 
    public TextMeshProUGUI hero1_Position; 
    public TextMeshProUGUI hero1_HP;
    public TextMeshProUGUI hero1_Energy;
    public TextMeshProUGUI hero1_Attack;
    public TextMeshProUGUI hero1_Defense;
    public TextMeshProUGUI hero1_Speed;
    public TextMeshProUGUI hero1_Mastery;
    public TextMeshProUGUI hero1_Nivel; 
    public Image hero1_Avatar;          
    public Image hero1_IconoClase;      
    public Image hero1_IconoPosicion;   

    [Header("Campos Visuales — Héroe 2 (Merlin)")]
    public TextMeshProUGUI hero2_Name;
    public TextMeshProUGUI hero2_Class;
    public TextMeshProUGUI hero2_Position; 
    public TextMeshProUGUI hero2_HP;
    public TextMeshProUGUI hero2_Energy;
    public TextMeshProUGUI hero2_Attack;
    public TextMeshProUGUI hero2_Defense;
    public TextMeshProUGUI hero2_Speed;
    public TextMeshProUGUI hero2_Mastery;
    public TextMeshProUGUI hero2_Nivel; 
    public Image hero2_Avatar;          
    public Image hero2_IconoClase;      
    public Image hero2_IconoPosicion;   

    [Header("Campos Visuales — Héroe 3 (Heracles)")]
    public TextMeshProUGUI hero3_Name;
    public TextMeshProUGUI hero3_Class;
    public TextMeshProUGUI hero3_Position; 
    public TextMeshProUGUI hero3_HP;
    public TextMeshProUGUI hero3_Energy;
    public TextMeshProUGUI hero3_Attack;
    public TextMeshProUGUI hero3_Defense;
    public TextMeshProUGUI hero3_Speed;
    public TextMeshProUGUI hero3_Mastery;
    public TextMeshProUGUI hero3_Nivel; 
    public Image hero3_Avatar;          
    public Image hero3_IconoClase;      
    public Image hero3_IconoPosicion;   

    private int currentTab = 0;

    void Awake()
    {
        if (Instance == null) Instance = this;

        statsPanel.SetActive(false);

        for (int i = 0; i < tabButtons.Length; i++)
        {
            int index = i; 
            tabButtons[i].onClick.AddListener(() => SwitchTab(index));
        }
        SwitchTab(0);
    }

    void Update() {
        if (statsPanel.activeSelf && Input.GetKeyDown(KeyCode.Escape)) {
            if (InventarioPanelUI.Instance != null && InventarioPanelUI.Instance.panelVisual.activeSelf) {
                return; 
            }
            CerrarPanelStats();
        }
    }

    public void AbrirPanelStats()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayClic();
        statsPanel.SetActive(true);
        Time.timeScale = 0f; 
        SwitchTab(currentTab);
    }

    public void CerrarPanelStats() {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayClic();
        statsPanel.SetActive(false);
        Time.timeScale = 1f; 
    }

    public void SwitchTab(int tabIndex)
    {   
        if (InventarioPanelUI.Instance != null && InventarioPanelUI.Instance.panelVisual != null){
            InventarioPanelUI.Instance.panelVisual.SetActive(false);
        }
        
        currentTab = tabIndex;

        for (int i = 0; i < contentPanels.Length; i++)
        {
            contentPanels[i].SetActive(i == tabIndex);
            var colors = tabButtons[i].colors;
            colors.normalColor = (i == tabIndex) ? activeTabColor : inactiveTabColor;
            tabButtons[i].colors = colors;
        }

        ActualizarVistaActiva();
    }

    public void ActualizarVistaActiva()
    {
        switch (currentTab)
        {
            case 0:
                PopulateHero(heroes[0], 0, hero1_Name, hero1_Class, hero1_Position, hero1_HP, hero1_Energy,
                    hero1_Attack, hero1_Defense, hero1_Speed, hero1_Mastery, hero1_Nivel,
                    hero1_Avatar, hero1_IconoClase, hero1_IconoPosicion,
                    equipSlots[0], equipSlots[1], equipSlots[2]);
                break;
            case 1:
                PopulateHero(heroes[1], 1, hero2_Name, hero2_Class, hero2_Position, hero2_HP, hero2_Energy,
                    hero2_Attack, hero2_Defense, hero2_Speed, hero2_Mastery, hero2_Nivel,
                    hero2_Avatar, hero2_IconoClase, hero2_IconoPosicion,
                    equipSlots[3], equipSlots[4], equipSlots[5]);
                break;
            case 2:
                PopulateHero(heroes[2], 2, hero3_Name, hero3_Class, hero3_Position, hero3_HP, hero3_Energy,
                    hero3_Attack, hero3_Defense, hero3_Speed, hero3_Mastery, hero3_Nivel,
                    hero3_Avatar, hero3_IconoClase, hero3_IconoPosicion,
                    equipSlots[6], equipSlots[7], equipSlots[8]);
                break;
        }
    }

    void PopulateHero(
        HeroStats h, int heroIndex,
        TextMeshProUGUI nameT, TextMeshProUGUI classT, TextMeshProUGUI posT,
        TextMeshProUGUI hpT, TextMeshProUGUI energyT,
        TextMeshProUGUI atkT, TextMeshProUGUI defT, TextMeshProUGUI spdT,
        TextMeshProUGUI mastT, TextMeshProUGUI nivelT,
        Image avatarImg, Image classImg, Image posImg,
        EquipSlotUI slot0, EquipSlotUI slot1, EquipSlotUI slot2)
    {
        if (h == null) return;

        if (nameT != null) nameT.text = h.unitName;
        if (classT != null) classT.text = $"Clase: {h.unitClass}";
        if (posT != null) posT.text = $"Posición: {h.unitPosition}";
        
        if (hpT != null) hpT.text = $"{h.currentHP} / {h.maxHP}";
        if (energyT != null) energyT.text = $"{h.currentEnergy} / {h.maxEnergy}";
        if (atkT != null) atkT.text = h.attack.ToString();
        if (defT != null) defT.text = h.defense.ToString();
        if (spdT != null) spdT.text = h.speed.ToString();
        if (mastT != null) mastT.text = h.mastery.ToString();
        if (nivelT != null) nivelT.text = h.level.ToString();
        
        if (avatarImg != null && heroIndex < spritesPortratosHeroes.Length) {
            avatarImg.sprite = spritesPortratosHeroes[heroIndex];
        }

        if (classImg != null) {
            if (h.unitClass == UnitClass.Melee) classImg.sprite = spriteMelee;
            else if (h.unitClass == UnitClass.Rango) classImg.sprite = spriteRango;
            else if (h.unitClass == UnitClass.Tanque) classImg.sprite = spriteTanque;
        }

        if (posImg != null) {
            if (h.unitPosition == UnitPosition.Tierra) posImg.sprite = spriteTierra;
            else if (h.unitPosition == UnitPosition.BajoTierra) posImg.sprite = spriteBajoTierra;
            else if (h.unitPosition == UnitPosition.Volando) posImg.sprite = spriteVolando;
        }

        HeroEquipment equipo = h.GetComponent<HeroEquipment>();
        if (slot0 != null) { slot0.equipo = equipo; slot0.Refrescar(); }
        if (slot1 != null) { slot1.equipo = equipo; slot1.Refrescar(); }
        if (slot2 != null) { slot2.equipo = equipo; slot2.Refrescar(); }
    }
}