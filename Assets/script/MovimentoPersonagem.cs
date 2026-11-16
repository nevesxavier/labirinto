
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class MovimentoPersonagem : MonoBehaviour
{
    [Header("Movimentação")]
    public float velocidade = 7f;

    private Rigidbody2D rb;
    private Vector2 movimento;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        movimento.x = 0;
        movimento.y = 0;

        if (Input.GetKey(KeyCode.W))
            movimento.y = 1;

        if (Input.GetKey(KeyCode.S))
            movimento.y = -1;

        if (Input.GetKey(KeyCode.A))
            movimento.x = -1;

        if (Input.GetKey(KeyCode.D))
            movimento.x = 1;

        movimento = movimento.normalized;
    }

    void FixedUpdate()
    {
        rb.linearVelocity = movimento * velocidade;
    }
}
