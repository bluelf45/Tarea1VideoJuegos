using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EstadoEnte
{
    Idle,
    Hunt,
    Comeback
}

public class Ente : MonoBehaviour
{
    public EstadoEnte estado = EstadoEnte.Idle;
    public GameObject doctorSeleccionado;

    private float vel = 2f;
    private float rangeDetect = 5f;
    private float rangeChase = 7f;
    private float[] rangeMov = { 12, 9 };
    private float randomX;
    private float randomZ;

    void EncontrarDoctorCercano()
    {
        float range = (estado == EstadoEnte.Hunt) ? rangeChase : rangeDetect;

        Collider[] doctores = Physics.OverlapSphere(transform.position, range);
        
        float minDistancia = float.PositiveInfinity;
        int cantidadDoc = 0;

        if (doctores.Length > 0)
        {
            foreach (var obj in doctores)
            {
                if (obj.tag == "Doctor" && obj.gameObject.GetComponent<Doctor>().estado != EstadoDoctor.Muerto)
                {
                    cantidadDoc++;
                    estado = EstadoEnte.Hunt;
                    float distancia = (transform.position - obj.transform.position).sqrMagnitude;

                    if (distancia < minDistancia)
                    {
                        doctorSeleccionado = obj.gameObject;
                        minDistancia = distancia;
                    }
                }
            }
        }
        
        if (cantidadDoc == 0)
        {
            estado = EstadoEnte.Idle;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Doctor" && estado == EstadoEnte.Hunt)
        {
            estado = EstadoEnte.Idle;
        }
    }

    void Update()
    {
        // Cuando el ente se salga del rango especificado
        if (Mathf.Abs(transform.position.x) >= rangeMov[0] || Mathf.Abs(transform.position.z) >= rangeMov[1])
        {
            estado = EstadoEnte.Comeback;
            randomX = Random.Range(-6f, 6f);
            randomZ = Random.Range(-4.5f, 4.5f);
        }

        if (estado == EstadoEnte.Idle)
        {
            EncontrarDoctorCercano(); 
        }
        else if (estado == EstadoEnte.Hunt)
        {
            EncontrarDoctorCercano();

            Vector3 nuevaPos = doctorSeleccionado.transform.position;
            transform.position = Vector3.MoveTowards(transform.position, new Vector3(nuevaPos.x, transform.position.y, nuevaPos.z), vel * Time.deltaTime);
        }
        else if (estado == EstadoEnte.Comeback)
        {
            if (transform.position.x == randomX && transform.position.z == randomZ)
            {
                estado = EstadoEnte.Idle;
            }
            else
            {
                transform.position = Vector3.MoveTowards(transform.position, new Vector3(randomX, transform.position.y, randomZ), vel * Time.deltaTime);
            }
        }
    }
}
