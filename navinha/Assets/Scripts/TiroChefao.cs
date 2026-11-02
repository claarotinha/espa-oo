using UnityEngine;

public class TiroChefao : MonoBehaviour
{
    private Vector2 direcao;
    private float velocidade;
    private float duracao = 7f;
    public void Iniciar(Vector2 dir, float vel) { direcao = dir; velocidade = vel; }
    private void Update()
    {
        transform.position += (Vector3)(direcao * velocidade * Time.deltaTime);
        duracao -= Time.deltaTime;
        if (duracao <= 0) Destroy(gameObject);
    }
    private void OnTriggerEnter2D(Collider2D outro)
    {
        if (outro.TryGetComponent<Jogador>(out var jogador))
        {
            jogador.ReceberDano();
            Destroy(gameObject);
        }
    }
}
