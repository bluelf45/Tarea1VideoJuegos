using System.Collections;
using System.Collections.Generic;
using UnityEngine;

enum EstadoDoctor
{
    Idle,
    Huir,
    Curar
}

public class Doctor : MonoBehaviour
{
    private float vel = 4f;
    [SerializeField] private EstadoDoctor estado = EstadoDoctor.Idle;

    public GameObject personaSeleccionada;
    private GameObject[] entesPersiguiendo = new GameObject[0];
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
            bool siendoCurado = false;

            foreach (var doctor in doctores)
            {
                if (doctor != this.gameObject && doctor.GetComponent<Doctor>().personaSeleccionada == obj)
                {
                    siendoCurado = true;
                }
            }

            if (!siendoCurado)
            {
                int vida = (int)Mathf.Round(obj.GetComponent<Persona>().vida);
                float vidaDist = Vector3.Distance(obj.transform.position, transform.position);

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

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Ente")
        {
            Destroy(this.gameObject);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
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
            if (personaSeleccionada.GetComponent<Persona>().estado == EstadoPersona.Muerta || personaSeleccionada.GetComponent<Persona>().vida >= 100f)
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
