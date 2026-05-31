using UnityEngine;

[System.Serializable]
public class SonidoConfig {
    public AudioClip clip;
    [Range(0f, 1f)] public float volumen = 1f;
}

public class AudioManager : MonoBehaviour {
    public static AudioManager Instance;

    [Header("AudioSources")]
    [Tooltip("Arrastrar un AudioSource para la música.")]
    public AudioSource musicaSource;
    [Tooltip("Arrastrar un AudioSource para los efectos.")]
    public AudioSource sfxSource;

    [Header("Música")]
    public SonidoConfig musicaAmbiental;
    public SonidoConfig musicaCombateNormal;
    public SonidoConfig musicaCombateJefe;
    public SonidoConfig musicaCombateFinal; 

    [Header("SFX - Combate")]
    public SonidoConfig sfxVictoria;
    public SonidoConfig sfxDerrota;
    public SonidoConfig sfxQTEPerfect;
    public SonidoConfig sfxQTEGreat;
    public SonidoConfig sfxQTEFailure;
    public SonidoConfig sfxGolpe;
    public SonidoConfig sfxCargaEnergia;
    public SonidoConfig sfxRugido;

    [Header("SFX - Exploración")]
    public SonidoConfig sfxEspada;
    public SonidoConfig sfxTijeras;
    public SonidoConfig sfxLlave;
    public SonidoConfig sfxPuerta;
    public SonidoConfig sfxFuente;
    public SonidoConfig sfxSonidoClic;
    public SonidoConfig sfxSonidoInteraccion;
    
    void Awake() {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start() {
        ReproducirMusicaAmbiental();
    }

    public void ReproducirMusicaAmbiental() {
        ReproducirMusica(musicaAmbiental);
    }

    public void ReproducirMusicaCombate(bool esJefe) {
        ReproducirMusica(esJefe ? musicaCombateJefe : musicaCombateNormal);
    }

    private void ReproducirMusica(SonidoConfig config) {
        if (musicaSource == null || config == null || config.clip == null) return;
        
        if (musicaSource.clip == config.clip) return;
        
        musicaSource.clip = config.clip;
        musicaSource.volume = config.volumen;
        musicaSource.Play();
    }

    private void ReproducirSFX(SonidoConfig config) {
        if (sfxSource == null || config == null || config.clip == null) return;
        sfxSource.PlayOneShot(config.clip, config.volumen);
    }

    public void PlayVictoria() => ReproducirSFX(sfxVictoria);
    public void PlayDerrota() => ReproducirSFX(sfxDerrota);
    public void PlayQTEPerfect() => ReproducirSFX(sfxQTEPerfect);
    public void PlayQTEGreat() => ReproducirSFX(sfxQTEGreat);
    public void PlayQTEFailure() => ReproducirSFX(sfxQTEFailure);
    public void PlayEspada() => ReproducirSFX(sfxEspada);
    public void PlayTijeras() => ReproducirSFX(sfxTijeras);
    public void PlayLlave() => ReproducirSFX(sfxLlave);
    public void PlayPuerta() => ReproducirSFX(sfxPuerta);
    public void PlayFuente() => ReproducirSFX(sfxFuente);
    public void PlayClic() => ReproducirSFX(sfxSonidoClic);
    public void PlayGolpe() => ReproducirSFX(sfxGolpe);
    public void PlayInteraccion() => ReproducirSFX(sfxSonidoInteraccion);
    public void PlayCargaEnergia() => ReproducirSFX(sfxCargaEnergia);
    public void PlayRugido() => ReproducirSFX(sfxRugido);
    
    public void PlayMusicaCombateFinal() => ReproducirMusica(musicaCombateFinal);

}