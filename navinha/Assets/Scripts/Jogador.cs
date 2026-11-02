using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class Jogador : MonoBehaviour
{
    [SerializeField] private float velocidade = 6f;
    [SerializeField] private int danosSuportados = 3;
    private int danosRecebidos;
    private Camera cameraJogo;
    private SpriteRenderer visual;
    private bool derrotado;
    private Color corOriginal;
    private Coroutine efeitoDano;
    public event Action<Jogador> DanoRecebido;
    public int DanosRecebidos => danosRecebidos;
    public int DanosRestantes => Mathf.Max(0, danosSuportados - danosRecebidos);
    public bool Derrotado => derrotado;
    public event Action<Jogador> DerrotaOcorrida;

    private void Awake()
    {
        cameraJogo = Camera.main;
        visual = GetComponent<SpriteRenderer>();
        if (visual != null) corOriginal = visual.color;
    }

    private void Update()
    {
        if (derrotado || Keyboard.current == null || cameraJogo == null) return;
        Vector2 direcao = Vector2.zero;
        var teclado = Keyboard.current;
        if (teclado.aKey.isPressed || teclado.leftArrowKey.isPressed) direcao.x--;
        if (teclado.dKey.isPressed || teclado.rightArrowKey.isPressed) direcao.x++;
        if (teclado.wKey.isPressed || teclado.upArrowKey.isPressed) direcao.y++;
        if (teclado.sKey.isPressed || teclado.downArrowKey.isPressed) direcao.y--;
        transform.position += (Vector3)(direcao.normalized * velocidade * Time.deltaTime);
        float meioY = cameraJogo.orthographicSize;
        float meioX = meioY * cameraJogo.aspect;
        float margemX = visual != null ? visual.bounds.extents.x : 0.4f;
        float margemY = visual != null ? visual.bounds.extents.y : 0.4f;
        Vector3 centro = cameraJogo.transform.position;
        Vector3 posicao = transform.position;
        posicao.x = Mathf.Clamp(posicao.x, centro.x - meioX + margemX, centro.x + meioX - margemX);
        posicao.y = Mathf.Clamp(posicao.y, centro.y - meioY + margemY, centro.y + meioY - margemY);
        transform.position = posicao;
    }

    public void ReceberDano()
    {
        if (derrotado) return;
        danosRecebidos++;
        DanoRecebido?.Invoke(this);
        if (efeitoDano != null) StopCoroutine(efeitoDano);
        efeitoDano = StartCoroutine(PiscarDano());
        // Tres acertos sao suportados; a derrota acontece no quarto.
        if (danosRecebidos > danosSuportados)
        {
            derrotado = true;
            if (visual != null) visual.enabled = false;
            Debug.Log("Navinha derrotada.");
            DerrotaOcorrida?.Invoke(this);
        }
    }

    private IEnumerator PiscarDano()
    {
        if (visual == null) yield break;
        for (int i = 0; i < 4; i++)
        {
            visual.color = new Color(1f, .2f, .2f, 1f);
            yield return new WaitForSeconds(.1f);
            visual.color = corOriginal;
            yield return new WaitForSeconds(.1f);
        }
        efeitoDano = null;
    }

    private void OnDisable()
    {
        if (visual != null) visual.color = corOriginal;
    }
}
