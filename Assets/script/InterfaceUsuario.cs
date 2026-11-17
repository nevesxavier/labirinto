using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Interface do jogo, criada inteiramente por código (não precisa montar nada
/// no editor): HUD com fase e itens + painel de menu / pausa / fase concluída / vitória.
/// </summary>
public class InterfaceUsuario : MonoBehaviour
{
    private static readonly Color CorPlaca = new Color(0.05f, 0.07f, 0.12f, 0.70f);
    private static readonly Color CorDestaque = new Color(1f, 0.82f, 0.30f, 1f);

    private TextMeshProUGUI textoFase;
    private TextMeshProUGUI textoItens;
    private TextMeshProUGUI textoItensFase;

    private GameObject painel;
    private TextMeshProUGUI painelTitulo;
    private TextMeshProUGUI painelSubtitulo;
    private TextMeshProUGUI rotuloBotao1;
    private TextMeshProUGUI rotuloBotao2;
    private Button botao1;
    private Button botao2;
    private Action acao1;
    private Action acao2;

    public bool PainelVisivel
    {
        get { return painel != null && painel.activeSelf; }
    }

    public bool TemBotaoSecundario
    {
        get { return botao2 != null && botao2.gameObject.activeSelf; }
    }

    // ------------------------------------------------------------------
    // Construção
    // ------------------------------------------------------------------
    public void Construir()
    {
        GarantirEventSystem();

        GameObject canvasGO = new GameObject("InterfaceCanvas", typeof(RectTransform));
        canvasGO.transform.SetParent(transform, false);

        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        CanvasScaler escala = canvasGO.AddComponent<CanvasScaler>();
        escala.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        escala.referenceResolution = new Vector2(1920f, 1080f);
        escala.matchWidthOrHeight = 0.5f;

        canvasGO.AddComponent<GraphicRaycaster>();

        ConstruirHud(canvasGO.transform);
        ConstruirPainel(canvasGO.transform);

        painel.SetActive(false);
    }

    private void GarantirEventSystem()
    {
        if (UnityEngine.Object.FindFirstObjectByType<EventSystem>() != null)
            return;

        GameObject go = new GameObject("EventSystem");
        go.transform.SetParent(transform, false);
        go.AddComponent<EventSystem>();
        go.AddComponent<StandaloneInputModule>();
    }

    private void ConstruirHud(Transform pai)
    {
        // Placa da esquerda: fase atual
        RectTransform placaFase = CriarRect("PlacaFase", pai,
            new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(30f, -30f), new Vector2(380f, 80f));
        placaFase.gameObject.AddComponent<Image>().color = CorPlaca;

        textoFase = CriarTexto(placaFase, "TextoFase", "FASE 1 / 3", 46f,
            TextAlignmentOptions.Center, Color.white);
        textoFase.fontStyle = FontStyles.Bold;
        Esticar(textoFase.rectTransform, 10f);

        // Placa da direita: itens
        RectTransform placaItens = CriarRect("PlacaItens", pai,
            new Vector2(1f, 1f), new Vector2(1f, 1f),
            new Vector2(-30f, -30f), new Vector2(430f, 130f));
        placaItens.gameObject.AddComponent<Image>().color = CorPlaca;

        textoItens = CriarTexto(placaItens, "TextoItens", "ITENS 0 / 10", 46f,
            TextAlignmentOptions.Center, CorDestaque);
        textoItens.fontStyle = FontStyles.Bold;
        textoItens.rectTransform.anchorMin = new Vector2(0f, 0.45f);
        textoItens.rectTransform.anchorMax = new Vector2(1f, 1f);
        textoItens.rectTransform.offsetMin = new Vector2(10f, 0f);
        textoItens.rectTransform.offsetMax = new Vector2(-10f, -8f);

        textoItensFase = CriarTexto(placaItens, "TextoItensFase", "Nesta fase: 0 / 3", 30f,
            TextAlignmentOptions.Center, Color.white);
        textoItensFase.rectTransform.anchorMin = new Vector2(0f, 0f);
        textoItensFase.rectTransform.anchorMax = new Vector2(1f, 0.45f);
        textoItensFase.rectTransform.offsetMin = new Vector2(10f, 8f);
        textoItensFase.rectTransform.offsetMax = new Vector2(-10f, 0f);

        // Dica de controles, embaixo à esquerda
        RectTransform dica = CriarRect("Dica", pai,
            new Vector2(0f, 0f), new Vector2(0f, 0f),
            new Vector2(30f, 24f), new Vector2(620f, 50f));
        TextMeshProUGUI textoDica = CriarTexto(dica, "TextoDica",
            "WASD / setas: mover   |   Esc: pausar", 26f,
            TextAlignmentOptions.Left, new Color(1f, 1f, 1f, 0.75f));
        Esticar(textoDica.rectTransform, 0f);
    }

    private void ConstruirPainel(Transform pai)
    {
        // Fundo escurecido que cobre a tela toda
        GameObject fundo = new GameObject("Painel", typeof(RectTransform));
        fundo.transform.SetParent(pai, false);
        Esticar((RectTransform)fundo.transform, 0f);
        Image imgFundo = fundo.AddComponent<Image>();
        imgFundo.color = new Color(0f, 0f, 0f, 0.75f);
        painel = fundo;

        // Cartão central
        RectTransform cartao = CriarRect("Cartao", fundo.transform,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            Vector2.zero, new Vector2(1000f, 620f));
        cartao.gameObject.AddComponent<Image>().color = new Color(0.10f, 0.13f, 0.20f, 0.97f);

        painelTitulo = CriarTexto(cartao, "Titulo", "TITULO", 84f,
            TextAlignmentOptions.Center, CorDestaque);
        painelTitulo.fontStyle = FontStyles.Bold;
        painelTitulo.rectTransform.anchorMin = new Vector2(0f, 1f);
        painelTitulo.rectTransform.anchorMax = new Vector2(1f, 1f);
        painelTitulo.rectTransform.pivot = new Vector2(0.5f, 1f);
        painelTitulo.rectTransform.anchoredPosition = new Vector2(0f, -40f);
        painelTitulo.rectTransform.sizeDelta = new Vector2(-40f, 120f);

        painelSubtitulo = CriarTexto(cartao, "Subtitulo", "subtitulo", 38f,
            TextAlignmentOptions.Center, Color.white);
        painelSubtitulo.rectTransform.anchorMin = new Vector2(0f, 1f);
        painelSubtitulo.rectTransform.anchorMax = new Vector2(1f, 1f);
        painelSubtitulo.rectTransform.pivot = new Vector2(0.5f, 1f);
        painelSubtitulo.rectTransform.anchoredPosition = new Vector2(0f, -165f);
        painelSubtitulo.rectTransform.sizeDelta = new Vector2(-80f, 170f);

        botao1 = CriarBotao(cartao, "Botao1", new Vector2(0f, -350f),
            new Vector2(560f, 100f), new Color(0.20f, 0.55f, 0.35f, 1f), out rotuloBotao1);
        botao1.onClick.AddListener(() => Disparar(1));

        botao2 = CriarBotao(cartao, "Botao2", new Vector2(0f, -470f),
            new Vector2(560f, 80f), new Color(0.35f, 0.38f, 0.48f, 1f), out rotuloBotao2);
        botao2.onClick.AddListener(() => Disparar(2));
    }

    // ------------------------------------------------------------------
    // API pública
    // ------------------------------------------------------------------
    public void AtualizarHud(int fase, int totalFases, int coletadosTotal, int totalItens,
                             int coletadosFase, int itensDaFase)
    {
        if (textoFase == null) return;

        textoFase.text = "FASE " + fase + " / " + totalFases;
        textoItens.text = "ITENS " + coletadosTotal + " / " + totalItens;
        textoItensFase.text = "Nesta fase: " + coletadosFase + " / " + itensDaFase;
    }

    public void MostrarPainel(string titulo, string subtitulo,
                              string rotulo1, Action aoClicar1,
                              string rotulo2 = null, Action aoClicar2 = null)
    {
        painelTitulo.text = titulo;
        painelSubtitulo.text = subtitulo;

        rotuloBotao1.text = rotulo1;
        acao1 = aoClicar1;

        bool temSegundo = !string.IsNullOrEmpty(rotulo2);
        botao2.gameObject.SetActive(temSegundo);
        rotuloBotao2.text = temSegundo ? rotulo2 : "";
        acao2 = aoClicar2;

        painel.SetActive(true);
    }

    public void EsconderPainel()
    {
        if (painel != null) painel.SetActive(false);
    }

    public void ExecutarAcaoPrincipal()
    {
        Disparar(1);
    }

    public void ExecutarAcaoSecundaria()
    {
        if (TemBotaoSecundario) Disparar(2);
    }

    private void Disparar(int numero)
    {
        Action acao = numero == 1 ? acao1 : acao2;

        // Evita que Enter/Espaço "cliquem" de novo no botão que ficou selecionado
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);

        if (acao != null) acao();
    }

    // ------------------------------------------------------------------
    // Utilitários de UI
    // ------------------------------------------------------------------
    private static RectTransform CriarRect(string nome, Transform pai, Vector2 ancora,
                                           Vector2 pivo, Vector2 posicao, Vector2 tamanho)
    {
        GameObject go = new GameObject(nome, typeof(RectTransform));
        RectTransform rect = (RectTransform)go.transform;
        rect.SetParent(pai, false);
        rect.anchorMin = ancora;
        rect.anchorMax = ancora;
        rect.pivot = pivo;
        rect.anchoredPosition = posicao;
        rect.sizeDelta = tamanho;
        return rect;
    }

    private static void Esticar(RectTransform rect, float margem)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.offsetMin = new Vector2(margem, margem);
        rect.offsetMax = new Vector2(-margem, -margem);
    }

    private static TextMeshProUGUI CriarTexto(Transform pai, string nome, string conteudo,
                                              float tamanho, TextAlignmentOptions alinhamento,
                                              Color cor)
    {
        GameObject go = new GameObject(nome, typeof(RectTransform));
        go.transform.SetParent(pai, false);

        TextMeshProUGUI texto = go.AddComponent<TextMeshProUGUI>();
        texto.text = conteudo;
        texto.fontSize = tamanho;
        texto.alignment = alinhamento;
        texto.color = cor;
        texto.raycastTarget = false;
        return texto;
    }

    private static Button CriarBotao(Transform pai, string nome, Vector2 posicaoDoTopo,
                                     Vector2 tamanho, Color cor, out TextMeshProUGUI rotulo)
    {
        RectTransform rect = CriarRect(nome, pai,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), posicaoDoTopo, tamanho);

        Image imagem = rect.gameObject.AddComponent<Image>();
        imagem.color = cor;

        Button botao = rect.gameObject.AddComponent<Button>();
        botao.targetGraphic = imagem;

        ColorBlock cores = botao.colors;
        cores.normalColor = Color.white;
        cores.highlightedColor = new Color(0.85f, 0.85f, 0.85f, 1f);
        cores.pressedColor = new Color(0.65f, 0.65f, 0.65f, 1f);
        botao.colors = cores;

        rotulo = CriarTexto(rect, "Rotulo", "", 40f, TextAlignmentOptions.Center, Color.white);
        rotulo.fontStyle = FontStyles.Bold;
        Esticar(rotulo.rectTransform, 8f);

        return botao;
    }
}
