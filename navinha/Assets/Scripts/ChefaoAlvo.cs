using UnityEngine;

// Alvo temporario para testar a colisao antes de implementar os estados do chefao.
public class ChefaoAlvo : MonoBehaviour
{
    [SerializeField] private int vida = 60;
    public int Vida => vida;
    public void ReceberDano(int dano)
    {
        vida = Mathf.Max(0, vida - Mathf.Max(0, dano));
        if (vida == 0) Debug.Log("Alvo atingido ate zerar a vida.");
    }
}
