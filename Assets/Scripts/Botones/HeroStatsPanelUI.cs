using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HeroStatsPanelUI : MonoBehaviour
{
    [Header("Referencias del Panel")]
    public GameObject statsPanel;
    public Button openButton;

    [Header("Héroes (asigna los 3 GameObjects con HeroStats)")]
    public HeroStats[] heroes = new HeroStats[3];

    [Header("Botones de Pestañas")]
    public Button[] tabButtons = new Button[3];
    public Color activeTabColor = Color.white;
    public Color inactiveTabColor = new Color(0.7f, 0.7f, 0.7f);

    [Header("Paneles de Contenido (uno por héroe)")]
    public GameObject[] contentPanels = new GameObject[3];

    [Header("Campos de Texto — Héroe 1")]
    public TextMeshProUGUI hero1_Name;
    public TextMeshProUGUI hero1_Class;
    public TextMeshProUGUI hero1_HP;
    public TextMeshProUGUI hero1_Energy;
    public TextMeshProUGUI hero1_Attack;
    public TextMeshProUGUI hero1_Defense;
    public TextMeshProUGUI hero1_Speed;
    public TextMeshProUGUI hero1_Mastery;
    public TextMeshProUGUI hero1_Weapon;

    [Header("Campos de Texto — Héroe 2")]
    public TextMeshProUGUI hero2_Name;
    public TextMeshProUGUI hero2_Class;
    public TextMeshProUGUI hero2_HP;
    public TextMeshProUGUI hero2_Energy;
    public TextMeshProUGUI hero2_Attack;
    public TextMeshProUGUI hero2_Defense;
    public TextMeshProUGUI hero2_Speed;
    public TextMeshProUGUI hero2_Mastery;
    public TextMeshProUGUI hero2_Weapon;

    [Header("Campos de Texto — Héroe 3")]
    public TextMeshProUGUI hero3_Name;
    public TextMeshProUGUI hero3_Class;
    public TextMeshProUGUI hero3_HP;
    public TextMeshProUGUI hero3_Energy;
    public TextMeshProUGUI hero3_Attack;
    public TextMeshProUGUI hero3_Defense;
    public TextMeshProUGUI hero3_Speed;
    public TextMeshProUGUI hero3_Mastery;
    public TextMeshProUGUI hero3_Weapon;

    private int currentTab = 0;

    void Awake()
    {
        statsPanel.SetActive(false);

        //openButton.onClick.AddListener(TogglePanel);

        for (int i = 0; i < tabButtons.Length; i++)
        {
            int index = i; 
            tabButtons[i].onClick.AddListener(() => SwitchTab(index));
        }

        PopulateAllHeroes();
        SwitchTab(0);
    }

    public void TogglePanel()
    {
        statsPanel.SetActive(!statsPanel.activeSelf);
    }


    public void SwitchTab(int tabIndex)
    {
        currentTab = tabIndex;

        for (int i = 0; i < contentPanels.Length; i++)
        {
            contentPanels[i].SetActive(i == tabIndex);
            var colors = tabButtons[i].colors;
            colors.normalColor = (i == tabIndex) ? activeTabColor : inactiveTabColor;
            tabButtons[i].colors = colors;
        }
    }

    void PopulateAllHeroes()
    {
        if (heroes.Length < 3) return;

        PopulateHero(heroes[0],
            hero1_Name, hero1_Class, hero1_HP, hero1_Energy,
            hero1_Attack, hero1_Defense, hero1_Speed,
            hero1_Mastery, hero1_Weapon);

        PopulateHero(heroes[1],
            hero2_Name, hero2_Class, hero2_HP, hero2_Energy,
            hero2_Attack, hero2_Defense, hero2_Speed,
            hero2_Mastery, hero2_Weapon);

        PopulateHero(heroes[2],
            hero3_Name, hero3_Class, hero3_HP, hero3_Energy,
            hero3_Attack, hero3_Defense, hero3_Speed,
            hero3_Mastery, hero3_Weapon);
    }

    void PopulateHero(
        HeroStats h,
        TextMeshProUGUI nameT, TextMeshProUGUI classT,
        TextMeshProUGUI hpT, TextMeshProUGUI energyT,
        TextMeshProUGUI atkT, TextMeshProUGUI defT, TextMeshProUGUI spdT,
        TextMeshProUGUI mastT, TextMeshProUGUI weapT)
    {
        if (h == null) return;

        nameT.text    = h.unitName;
        classT.text   = $"{h.unitClass} · {h.unitPosition}";
        hpT.text      = $"{h.currentHP} / {h.maxHP}";
        energyT.text  = $"{h.currentEnergy} / {h.maxEnergy}";
        atkT.text     = h.attack.ToString();
        defT.text     = h.defense.ToString();
        spdT.text     = h.speed.ToString();
        mastT.text    = $"Nivel {h.mastery}";
        weapT.text    = h.equippedWeapon != null ? h.equippedWeapon.weaponName : "Sin arma";
    }
}