
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Personagem")]
    public Transform personagem;

    [Header("Suavidade da Câmera")]
    public float suavidade = 3f;

    [Header("Deslocamento da Câmera")]
    public Vector3 deslocamento = new Vector3(0f, 0.5f, -10f);

    void LateUpdate()
    {
        if (personagem == null)
            return;

        Vector3 destino = personagem.position + deslocamento;

        transform.position = Vector3.Lerp(
            transform.position,
            destino,
            suavidade * Time.deltaTime
        );
    }
}
