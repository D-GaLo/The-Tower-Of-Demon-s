using UnityEngine;
using UnityEngine.SceneManagement;

public class EasterEggsManager : MonoBehaviour {
    public static EasterEggsManager Instance;

    [Header("Omori - Something")]
    public GameObject spriteSomething;
    [Tooltip("Tiempo en segundos. 5 minutos = 300")]
    public float tiempoParaAparecer = 300f; 
    private float temporizadorIdle = 0f;
    private bool somethingActivo = false;

    private KeyCode[] konamiCode = { KeyCode.UpArrow, KeyCode.UpArrow, KeyCode.DownArrow, KeyCode.DownArrow, KeyCode.LeftArrow, KeyCode.RightArrow, KeyCode.LeftArrow, KeyCode.RightArrow, KeyCode.B, KeyCode.A };
    private int indiceKonami = 0;

    private string bufferTeclas = "";
    
    [HideInInspector] public bool isZeldaMapActive = false;

    void Awake() {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject); 
        } else {
            Destroy(gameObject);
        }
    }

    void Start() {
        if (spriteSomething != null) spriteSomething.SetActive(false);
        PlayerPrefs.SetInt("KonamiActivado", 0);
    }

    void Update() {
        DetectarKonami();
        Detectar1998();
        ProcesarIdleOmori();
    }

    void DetectarKonami() {
        if (SceneManager.GetActiveScene().name != "Menu") return; 

        if (Input.anyKeyDown) {
            if (Input.GetKeyDown(konamiCode[indiceKonami])) {
                indiceKonami++;
                if (indiceKonami == konamiCode.Length) {
                    ActivarKonami();
                    indiceKonami = 0;
                }
            } else if (!Input.GetMouseButtonDown(0) && !Input.GetMouseButtonDown(1)) {
                if (Input.GetKeyDown(konamiCode[0])) indiceKonami = 1; 
                else indiceKonami = 0;
            }
        }
    }

    void ActivarKonami() {
        Debug.Log("¡CÓDIGO KONAMI ACTIVADO!");
        PlayerPrefs.SetInt("KonamiActivado", 1); 
        PlayerPrefs.Save();
        
        if (AudioManager.Instance != null) {
            AudioManager.Instance.SendMessage("PlayKonami", SendMessageOptions.DontRequireReceiver);
        }
    }

    void Detectar1998() {
        foreach (char c in Input.inputString) {
            bufferTeclas += c;
            if (bufferTeclas.Length > 10) {
                bufferTeclas = bufferTeclas.Substring(bufferTeclas.Length - 10);
            }

            if (bufferTeclas.EndsWith("1998")) {
                ActivarZelda1998();
                bufferTeclas = ""; 
            }
        }
    }

    void ActivarZelda1998() {
        string escenaActual = SceneManager.GetActiveScene().name;
        if (escenaActual == "Menu") {
            MainMenuController menuCtrl = FindObjectOfType<MainMenuController>();
            if (menuCtrl != null) menuCtrl.DetenerMusicaMenu();

            if (AudioManager.Instance != null) AudioManager.Instance.SendMessage("PlayMusicaMenuZelda", SendMessageOptions.DontRequireReceiver);
        } else {
            isZeldaMapActive = true;
            if (AudioManager.Instance != null) AudioManager.Instance.SendMessage("PlayMusicaAmbientalZelda", SendMessageOptions.DontRequireReceiver);
        }
    }

    void ProcesarIdleOmori() {
        string escenaActual = SceneManager.GetActiveScene().name;
        if (escenaActual != "Mapa") return; 

        if (GameFlowController.Instance != null && GameFlowController.Instance.enCombate) {
            if (somethingActivo) {
                somethingActivo = false;
                if (spriteSomething != null) spriteSomething.SetActive(false);
            }
            temporizadorIdle = 0f;
            return;
        }

        if (Input.anyKey || Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0) {
            if (somethingActivo) {
                somethingActivo = false;
                if (spriteSomething != null) spriteSomething.SetActive(false);
                if (AudioManager.Instance != null) AudioManager.Instance.SendMessage("PlayOmoriVanish", SendMessageOptions.DontRequireReceiver);
            }
            temporizadorIdle = 0f;
        } else {
            temporizadorIdle += Time.deltaTime;
            if (temporizadorIdle >= tiempoParaAparecer && !somethingActivo) {
                somethingActivo = true;
                if (spriteSomething != null) {
                    Camera cam = Camera.main;
                    if (cam != null) {
                        spriteSomething.transform.position = cam.transform.position + new Vector3(0, 0, 10f); 
                    }
                    spriteSomething.SetActive(true);
                }
            }
        }
    }

    public void ResetearSecretos() {
        PlayerPrefs.SetInt("KonamiActivado", 0);
        PlayerPrefs.Save();
        
        isZeldaMapActive = false;
        indiceKonami = 0;
        bufferTeclas = "";
        
        somethingActivo = false;
        temporizadorIdle = 0f;
        if (spriteSomething != null) spriteSomething.SetActive(false);
    }
}