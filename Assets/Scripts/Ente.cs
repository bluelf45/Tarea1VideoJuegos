using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EstadoEnte
{
    Wander,
    Hunt,
    Comeback
}

public class Ente : MonoBehaviour
{
    public EstadoEnte estado = EstadoEnte.Wander;
    public GameObject doctorSeleccionado;

    private float velWander = 3f;
    private float velHunt = 4.75f;
    private float rangeDetect = 5f;
    private float rangeChase = 7f;
    private float[] rangeMov = { 12, 9 };
    private float randomX;
    private float randomZ;
    private float[] radiusRangeComeback = { 4, 3 };
    private float[] radiusRangeWander = { 9f, 6f };

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
            estado = EstadoEnte.Wander;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Doctor" && estado == EstadoEnte.Hunt)
        {
            estado = EstadoEnte.Wander;
        }
    }

    public void SetToComeback()
    {
        estado = EstadoEnte.Comeback;
        randomX = Random.Range(-radiusRangeComeback[0], radiusRangeComeback[0]);
        randomZ = Random.Range(-radiusRangeComeback[1], radiusRangeComeback[1]);
    }

    void Start()
    {
        randomX = Random.Range(-radiusRangeWander[0], radiusRangeWander[0]);
        randomZ = Random.Range(-radiusRangeWander[1], radiusRangeWander[1]);
    }

    void Update()
    {
        // Cuando el ente se salga del rango especificado
        if (Mathf.Abs(transform.position.x) >= rangeMov[0] || Mathf.Abs(transform.position.z) >= rangeMov[1])
        {
            SetToComeback();
        }

        if (estado == EstadoEnte.Wander)
        {
            if (transform.position.x == randomX && transform.position.z == randomZ)
            {
                randomX = Random.Range(-radiusRangeWander[0], radiusRangeWander[0]);
                randomZ = Random.Range(-radiusRangeWander[1], radiusRangeWander[1]);
            }

            EncontrarDoctorCercano();

            transform.position = Vector3.MoveTowards(transform.position, new Vector3(randomX, transform.position.y, randomZ), velWander * Time.deltaTime);
        }
        else if (estado == EstadoEnte.Hunt)
        {
            EncontrarDoctorCercano();

            Vector3 nuevaPos = doctorSeleccionado.transform.position;
            transform.position = Vector3.MoveTowards(transform.position, new Vector3(nuevaPos.x, transform.position.y, nuevaPos.z), velHunt * Time.deltaTime);
        }
        else if (estado == EstadoEnte.Comeback)
        {
            if (transform.position.x == randomX && transform.position.z == randomZ)
            {
                estado = EstadoEnte.Wander;
            }
            else
            {
                transform.position = Vector3.MoveTowards(transform.position, new Vector3(randomX, transform.position.y, randomZ), velWander * Time.deltaTime);
            }
        }
    }
}
