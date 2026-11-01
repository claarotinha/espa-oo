using UnityEngine;
using UnityEngine.UI;

public class VidasUI : MonoBehaviour
{
    [SerializeField] private Jogador jogador;
    [SerializeField] private Image[] icones;
    private void Update()
    {
        if (jogador == null) return;
        for (int i = 0; i < icones.Length; i++)
            if (icones[i] != null) icones[i].enabled = i < jogador.DanosRestantes;
    }
}
