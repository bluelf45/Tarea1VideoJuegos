using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EstadoDoctor
{
    Idle,
    Muerto,
    Huir,
    Curar
}

public class Doctor : MonoBehaviour
{
    public float vel = 4f;
    public EstadoDoctor estado = EstadoDoctor.Idle;

    public GameObject personaSeleccionada;
    private GameObject[] entesPersiguiendo = new GameObject[0];
    // Se utiliza el ente mas cercano para huir exclusivamente de el en caso de que hayan mas de 1 ente persiguiendo
    private GameObject enteMasCercano;

    void EncontrarPersonaPocaVida()
    {
        GameObject[] personas = GameObject.FindGameObjectsWithTag("Persona");
        GameObject[] doctores = GameObject.FindGameObjectsWithTag("Doctor");

        int minVida = int.MaxValue;
        float minVidaDist = float.PositiveInfinity;

        foreach (var obj in personas)
        {
            if (obj.GetComponent<Persona>().estado == EstadoPersona.Muerta) 
            {
                continue;
            }
            bool siendoCurada = false;

            // Revisa si es que la persona a curar ya esta siendo curada, de ser así buscar otra
            foreach (var doctor in doctores)
            {
                var varsDoctor = doctor.GetComponent<Doctor>();
                if (doctor != this.gameObject && varsDoctor.estado != EstadoDoctor.Muerto && varsDoctor.personaSeleccionada == obj)
                {
                    siendoCurada = true;
                }
            }

            if (!siendoCurada)
            {
                // Se transforma a int para luego comparar solo los enteros. Si se comparan decimales, el caso de iguales ocurrirá muy poco
                int vida = (int)Mathf.Round(obj.GetComponent<Persona>().vida);
                float vidaDist = Vector3.Distance(obj.transform.position, transform.position);

                // Se va a la persona con menor vida, en caso de ser iguales se va a la mas cercana
                if (vida < minVida || (vida == minVida && vidaDist < minVidaDist))
                {
                    estado = EstadoDoctor.Curar;

                    personaSeleccionada = obj;
                    minVida = vida;
                    minVidaDist = vidaDist;
                }
            }
        }
    }

    void DetectarEntes()
    {
        GameObject[] entes = GameObject.FindGameObjectsWithTag("Ente");

        foreach (var obj in entes)
        {
            var varsEnte = obj.GetComponent<Ente>();

            if (varsEnte.estado == EstadoEnte.Hunt && varsEnte.doctorSeleccionado == this.gameObject)
            {
                System.Array.Resize(ref entesPersiguiendo, entesPersiguiendo.Length + 1);
                entesPersiguiendo[entesPersiguiendo.Length - 1] = obj;
            }
        }

        if (entesPersiguiendo.Length > 0)
        {
            estado = EstadoDoctor.Huir;
        }
    }

    void EnteMasCercano()
    {
        float minDist = float.PositiveInfinity;

        foreach (var obj in entesPersiguiendo)
        {
            float dist = Vector3.Distance(obj.transform.position, transform.position);

            if (dist < minDist)
            {
                enteMasCercano = obj;
                minDist = dist;
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.tag == "Ente" && other.gameObject.GetComponent<Ente>().estado == EstadoEnte.Hunt)
        {
            estado = EstadoDoctor.Muerto;
        }
    }

    void Update()
    {
        if (estado == EstadoDoctor.Muerto)
        {
            transform.rotation = Quaternion.Euler(new Vector3(0, 0, 90));
            transform.position = new Vector3(transform.position.x, 0.5f, transform.position.z);
        }
        else
        {
            DetectarEntes();

            if (estado == EstadoDoctor.Idle)
            {
                EncontrarPersonaPocaVida();
            }
            // Revisa cuando Ente para de perseguir
            else if (entesPersiguiendo.Length > 0)
            {
                bool noPersiguiendo = false;

                foreach (var obj in entesPersiguiendo)
                {
                    var varsEnte = obj.GetComponent<Ente>();
                    if (varsEnte.estado != EstadoEnte.Hunt || varsEnte.doctorSeleccionado != this.gameObject)
                    {
                        noPersiguiendo = true;
                        break;
                    }
                }

                if (noPersiguiendo)
                {
                    estado = EstadoDoctor.Idle;
                    entesPersiguiendo = new GameObject[0];
                }
            }

            if (estado == EstadoDoctor.Curar)
            {
                if (personaSeleccionada.GetComponent<Persona>().vida >= 100f || personaSeleccionada.GetComponent<Persona>().estado == EstadoPersona.Muerta)
                {
                    estado = EstadoDoctor.Idle;
                }
                Vector3 nuevaPos = personaSeleccionada.transform.position;
                transform.position = Vector3.MoveTowards(transform.position, new Vector3(nuevaPos.x, transform.position.y, nuevaPos.z), vel * Time.deltaTime);
            }
            else if (estado == EstadoDoctor.Huir)
            {
                EnteMasCercano();

                Vector3 nuevaPos = enteMasCercano.transform.position;
                transform.position = Vector3.MoveTowards(transform.position, new Vector3(nuevaPos.x, transform.position.y, nuevaPos.z), -1 * vel * Time.deltaTime);
            }
        }
    }
}
