
using UnityEngine;
using TMPro;

public class GerenciadorColetaveis : MonoBehaviour
{
    public static GerenciadorColetaveis Instancia;

    [Header("Configurações")]
    public TMP_Text textoColetaveis;

    public int quantidadeColetada = 0;
    public int quantidadeTotal = 10;

    private void Awake()
    {
        Instancia = this;
        AtualizarTexto();
    }

    public void ColetarItem()
    {
        quantidadeColetada++;

        AtualizarTexto();

        Debug.Log("Itens coletados: " +
            quantidadeColetada + "/" + quantidadeTotal);
    }

    private void AtualizarTexto()
    {
        if (textoColetaveis != null)
        {
            textoColetaveis.text =
                "Itens: " + quantidadeColetada +
                " / " + quantidadeTotal;
        }
    }
}
