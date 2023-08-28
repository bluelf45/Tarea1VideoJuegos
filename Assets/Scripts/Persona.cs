using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EstadoPersona
{
    Viva,
    Muerta
}

public class Persona : MonoBehaviour
{
    public float vida = 100f;
    public EstadoPersona estado = EstadoPersona.Viva;

    private float perdidaVidaVel = 2f;
    private float curarVel = 4f;

    private GameObject textoVida;

    private GameObject doctorCurando;

    void ActualizarTexto()
    {
        if (estado == EstadoPersona.Muerta)
        {
            textoVida.GetComponent<TextMesh>().text = "Muerta";
        }
        else if (textoVida) {
            textoVida.GetComponent<TextMesh>().text = "Vida: " + vida.ToString("0");
        }
    }

    void QuitarVida()
    {
        if (vida > 0) {
            vida -= Time.deltaTime * perdidaVidaVel;
        } else {
            estado = EstadoPersona.Muerta;
        }
    }

    void Curar()
    {
        vida += Time.deltaTime * curarVel;
        vida = (vida > 100f) ? 100f : vida;
    }

    void StunEnte()
    {
        GameObject[] entes = GameObject.FindGameObjectsWithTag("Ente");
        GameObject enteSeleccionado = null;

        float minDistancia = float.PositiveInfinity;

        foreach (var obj in entes)
        {
            if (obj.GetComponent<Ente>().estado != EstadoEnte.Comeback)
            {
                float distancia = (transform.position - obj.transform.position).sqrMagnitude;

                if (distancia < minDistancia)
                {
                    enteSeleccionado = obj.gameObject;
                    minDistancia = distancia;
                }
            }
        }

        if (enteSeleccionado != null)
        {
            enteSeleccionado.GetComponent<Ente>().SetToComeback();
        }
    }

    void Start()
    {
        textoVida = this.gameObject.transform.Find("FloatingText").gameObject;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Doctor")
        {
            doctorCurando = other.gameObject;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Doctor")
        {
            doctorCurando = null;
        }
    }

    void Update()
    {
        if (vida <= 20)
        {
            StunEnte();
        }

        if (doctorCurando != null)
        {
            Curar();
        }
        else
        {
            QuitarVida();
        }
        
        ActualizarTexto();
    }
}
