
using UnityEngine;

public class ItemColetavel : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D outro)
    {
        if (outro.CompareTag("Player"))
        {
            if (GerenciadorColetaveis.Instancia != null)
            {
                GerenciadorColetaveis.Instancia.ColetarItem();
            }

            Destroy(gameObject);
        }
    }
}
