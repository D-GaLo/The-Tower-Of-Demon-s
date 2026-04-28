using UnityEngine;
using System.Collections.Generic;

public class PartyFollower : MonoBehaviour {
    public Transform targetToFollow; 
    public float distanciaSeguridad = 1f; 
    public int framesDeRetraso = 10; 

    private SpriteRenderer spriteHeroe;
    private List<Vector3> historialPosiciones = new List<Vector3>();
    private float escalaFija = 0.7f;

    void Start() {
        spriteHeroe = GetComponent<SpriteRenderer>();
        transform.localScale = new Vector3(escalaFija, escalaFija, 1f);
    }

    void FixedUpdate() {
        if (targetToFollow == null) return;

        historialPosiciones.Add(targetToFollow.position);

        if (historialPosiciones.Count > framesDeRetraso) {
            Vector3 puntoDestino = historialPosiciones[0];
            float distanciaAlLider = Vector3.Distance(transform.position, targetToFollow.position);

            // Solo nos movemos si el líder se aleja para mantener la formación de serpiente
            if (distanciaAlLider > distanciaSeguridad) {
                // MoveTowards es más suave y evita que los personajes "tiemblen"
                transform.position = Vector3.MoveTowards(transform.position, puntoDestino, 5f * Time.fixedDeltaTime);
                
                // Flip visual según la dirección del movimiento
                float difX = puntoDestino.x - transform.position.x;
                if (difX > 0.01f) spriteHeroe.flipX = false;
                else if (difX < -0.01f) spriteHeroe.flipX = true;
            }

            historialPosiciones.RemoveAt(0);
        }
    }
}