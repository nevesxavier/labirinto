using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controla o jogo inteiro: 3 fases, 10 itens coletáveis no total e a interface.
///
/// Não precisa ser colocado na cena: ele se cria sozinho ao apertar Play,
/// desde que exista um objeto com MovimentoPersonagem (o jogador).
///
/// Fase 1 -> o mapa pintado na cena "labirinto".
/// Fase 2 e 3 -> labirintos gerados por código (GeradorLabirinto).
/// </summary>
public class GerenciadorJogo : MonoBehaviour
{
    public static GerenciadorJogo Instancia { get; private set; }

    // ------------------------------------------------------------------
    // Configuração (altere aqui para mudar o jogo)
    // ------------------------------------------------------------------

    // Quantidade de itens em cada fase: 3 + 3 + 4 = 10
    public static readonly int[] ItensPorFase = { 3, 3, 4 };

    // Onde ficam os itens da fase 1 (pontos alcançáveis dentro do labirinto da cena)
    private static readonly Vector2[] ItensFase1 =
    {
        new Vector2(-12.5f, 62.5f),
        new Vector2(1.5f, 26.5f),
        new Vector2(6.5f, 62.5f)
    };

    // Fases geradas: tamanho do labirinto (em células) e semente do sorteio.
    // Índice 0 = fase 2, índice 1 = fase 3.
    private static readonly int[] TamanhoGerado = { 8, 12 };
    private static readonly int[] SementeGerada = { 11, 29 };

    // Cada fase gerada fica numa posição diferente do mundo, longe do mapa da fase 1
    private const float DistanciaEntreFases = 300f;

    public static int TotalFases
    {
        get { return ItensPorFase.Length; }
    }

    public static int TotalItens
    {
        get
        {
            int soma = 0;
            for (int i = 0; i < ItensPorFase.Length; i++) soma += ItensPorFase[i];
            return soma;
        }
    }

    // ------------------------------------------------------------------
    // Estado
    // ------------------------------------------------------------------
    private enum Estado { Menu, Jogando, Pausado, FaseConcluida, Vitoria }

    public int FaseAtual { get; private set; }          // 0, 1 ou 2
    public int ColetadosFase { get; private set; }
    public int ColetadosTotal { get; private set; }

    private Estado estado = Estado.Menu;

    private MovimentoPersonagem jogador;
    private Rigidbody2D corpoJogador;
    private CameraFollow seguidorCamera;
    private InterfaceUsuario interfaceUsuario;

    private Vector2 posicaoInicialFase1;
    private GameObject faseGerada;
    private readonly List<GameObject> itensAtuais = new List<GameObject>();

    // ------------------------------------------------------------------
    // Inicialização automática
    // ------------------------------------------------------------------
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void CriarAutomaticamente()
    {
        if (Instancia != null) return;
        if (FindFirstObjectByType<MovimentoPersonagem>() == null) return;

        new GameObject("GerenciadorJogo").AddComponent<GerenciadorJogo>();
    }

    private void Awake()
    {
        Instancia = this;
    }

    private void Start()
    {
        jogador = FindFirstObjectByType<MovimentoPersonagem>();
        if (jogador == null)
        {
            Debug.LogError("GerenciadorJogo: nenhum objeto com MovimentoPersonagem na cena.");
            enabled = false;
            return;
        }

        // O ItemColetavel só reconhece quem tem a tag "Player"
        jogador.gameObject.tag = "Player";

        corpoJogador = jogador.GetComponent<Rigidbody2D>();
        seguidorCamera = FindFirstObjectByType<CameraFollow>();
        posicaoInicialFase1 = jogador.transform.position;

        interfaceUsuario = gameObject.AddComponent<InterfaceUsuario>();
        interfaceUsuario.Construir();

        PrepararFase(0);
        MostrarMenuInicial();
    }

    private void OnDestroy()
    {
        Time.timeScale = 1f;
        if (Instancia == this) Instancia = null;
    }

    // ------------------------------------------------------------------
    // Entrada de teclado (atalhos dos painéis e pausa)
    // ------------------------------------------------------------------
    private void Update()
    {
        if (interfaceUsuario == null) return;

        if (estado == Estado.Jogando)
        {
            if (Input.GetKeyDown(KeyCode.Escape)) Pausar();
            return;
        }

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)
            || Input.GetKeyDown(KeyCode.Space))
        {
            interfaceUsuario.ExecutarAcaoPrincipal();
        }
        else if (Input.GetKeyDown(KeyCode.R))
        {
            interfaceUsuario.ExecutarAcaoSecundaria();
        }
        else if (estado == Estado.Pausado && Input.GetKeyDown(KeyCode.Escape))
        {
            Retomar();
        }
    }

    // ------------------------------------------------------------------
    // Fluxo do jogo
    // ------------------------------------------------------------------
    private void MostrarMenuInicial()
    {
        estado = Estado.Menu;
        Time.timeScale = 0f;

        interfaceUsuario.MostrarPainel(
            "LABIRINTO",
            "Colete os " + TotalItens + " itens espalhados pelas " + TotalFases + " fases.\n"
                + "Mova-se com W, A, S, D ou as setas.",
            "Jogar  (Enter)",
            ReiniciarJogo);
    }

    private void Pausar()
    {
        estado = Estado.Pausado;
        Time.timeScale = 0f;

        interfaceUsuario.MostrarPainel(
            "PAUSADO",
            "Itens coletados: " + ColetadosTotal + " / " + TotalItens,
            "Continuar  (Enter)",
            Retomar,
            "Reiniciar jogo  (R)",
            ReiniciarJogo);
    }

    private void Retomar()
    {
        estado = Estado.Jogando;
        Time.timeScale = 1f;
        interfaceUsuario.EsconderPainel();
    }

    private void ReiniciarJogo()
    {
        ColetadosTotal = 0;
        IniciarFase(0);
    }

    private void ProximaFase()
    {
        IniciarFase(FaseAtual + 1);
    }

    private void IniciarFase(int indice)
    {
        PrepararFase(indice);

        estado = Estado.Jogando;
        Time.timeScale = 1f;
        interfaceUsuario.EsconderPainel();
    }

    /// <summary>Chamado pelo ItemColetavel quando o jogador pega um item.</summary>
    public void ColetarItem()
    {
        if (estado != Estado.Jogando) return;

        ColetadosFase++;
        ColetadosTotal++;
        AtualizarHud();

        if (ColetadosFase >= ItensPorFase[FaseAtual])
        {
            ConcluirFase();
        }
    }

    private void ConcluirFase()
    {
        Time.timeScale = 0f;

        if (FaseAtual < TotalFases - 1)
        {
            estado = Estado.FaseConcluida;
            interfaceUsuario.MostrarPainel(
                "FASE " + (FaseAtual + 1) + " CONCLUÍDA!",
                "Itens coletados: " + ColetadosTotal + " / " + TotalItens,
                "Próxima fase  (Enter)",
                ProximaFase);
        }
        else
        {
            estado = Estado.Vitoria;
            interfaceUsuario.MostrarPainel(
                "VOCÊ VENCEU!",
                "Você coletou os " + TotalItens + " itens e terminou as "
                    + TotalFases + " fases!",
                "Jogar de novo  (Enter)",
                ReiniciarJogo);
        }
    }

    // ------------------------------------------------------------------
    // Montagem de cada fase
    // ------------------------------------------------------------------
    private void PrepararFase(int indice)
    {
        FaseAtual = indice;
        ColetadosFase = 0;

        // Limpa o que sobrou da fase anterior
        for (int i = 0; i < itensAtuais.Count; i++)
        {
            if (itensAtuais[i] != null) Destroy(itensAtuais[i]);
        }
        itensAtuais.Clear();

        if (faseGerada != null)
        {
            Destroy(faseGerada);
            faseGerada = null;
        }

        Vector2 inicio;
        List<Vector2> posicoesItens;

        if (indice == 0)
        {
            // Fase 1: usa o mapa que já está na cena
            inicio = posicaoInicialFase1;
            posicoesItens = new List<Vector2>(ItensFase1);
        }
        else
        {
            // Fases 2 e 3: labirinto gerado
            Vector3 origem = new Vector3(DistanciaEntreFases * indice, 0f, 0f);
            GeradorLabirinto.Resultado resultado = GeradorLabirinto.Gerar(
                TamanhoGerado[indice - 1],
                TamanhoGerado[indice - 1],
                SementeGerada[indice - 1],
                origem,
                ItensPorFase[indice]);

            faseGerada = resultado.raiz;
            inicio = resultado.posicaoInicial;
            posicoesItens = resultado.posicoesItens;
        }

        CriarItens(posicoesItens);
        PosicionarJogador(inicio);
        AtualizarHud();
    }

    private void CriarItens(List<Vector2> posicoes)
    {
        GameObject modelo = Resources.Load<GameObject>("ItemColetavel");
        if (modelo == null)
        {
            Debug.LogError("GerenciadorJogo: prefab 'ItemColetavel' não encontrado em Assets/Resources.");
            return;
        }

        for (int i = 0; i < posicoes.Count; i++)
        {
            Vector3 posicao = new Vector3(posicoes[i].x, posicoes[i].y, 0f);
            GameObject item = Instantiate(modelo, posicao, Quaternion.identity);
            item.name = "Item_" + (i + 1);
            itensAtuais.Add(item);
        }
    }

    private void PosicionarJogador(Vector2 posicao)
    {
        Vector3 atual = jogador.transform.position;
        jogador.transform.position = new Vector3(posicao.x, posicao.y, atual.z);

        if (corpoJogador != null)
        {
            corpoJogador.position = posicao;
            corpoJogador.linearVelocity = Vector2.zero;
        }

        // A câmera pula direto para o jogador (senão ela "voaria" até a nova fase)
        if (seguidorCamera != null)
        {
            Vector3 d = seguidorCamera.deslocamento;
            seguidorCamera.transform.position = new Vector3(posicao.x + d.x, posicao.y + d.y, d.z);
        }
    }

    private void AtualizarHud()
    {
        if (interfaceUsuario == null) return;

        interfaceUsuario.AtualizarHud(
            FaseAtual + 1, TotalFases,
            ColetadosTotal, TotalItens,
            ColetadosFase, ItensPorFase[FaseAtual]);
    }
}
