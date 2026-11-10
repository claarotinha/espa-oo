using UnityEngine;

// Estado 6: comportamento independente do Animator.
public class Comportamento06 : StateMachineBehaviour
{
    private ChefaoCombate combate;
    private float tempo;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo info, int layerIndex)
    {
        combate = animator.GetComponent<ChefaoCombate>();
        if (combate == null) combate = animator.GetComponentInParent<ChefaoCombate>();
        if (combate != null) combate.IniciarEstado(5);
        tempo = 0f;
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo info, int layerIndex)
    {
        if (combate == null || !combate.PodeAgir) return;
        tempo += Time.deltaTime;
        float t = Time.time * 1.6f;
        float a = combate.Amplitude;
        Vector3 origem = combate.Origem;
        float x = origem.x + Mathf.PingPong(t * 2.4f, a * 2f) - a;
        float y = origem.y;
        combate.Mover(new Vector3(x, y, origem.z));
        combate.Atacar(0.95f, 3, 20f, 5.5f, new Color(1f, .5f, 0f));
        if (tempo >= combate.DuracaoEstado) combate.ProximoEstado(5);
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo info, int layerIndex)
    {
        if (combate != null) combate.EncerrarEstado();
    }
}
