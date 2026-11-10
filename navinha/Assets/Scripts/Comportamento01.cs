using UnityEngine;

// Estado 1: comportamento independente do Animator.
public class Comportamento01 : StateMachineBehaviour
{
    private ChefaoCombate combate;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo info, int layerIndex)
    {
        combate = animator.GetComponent<ChefaoCombate>();
        if (combate == null) combate = animator.GetComponentInParent<ChefaoCombate>();
        if (combate != null) combate.IniciarEstado(0);
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo info, int layerIndex)
    {
        if (combate == null || !combate.PodeAgir) return;
        float t = Time.time * 1f;
        float a = combate.Amplitude;
        Vector3 origem = combate.Origem;
        float x = origem.x + Mathf.Sin(t) * a * .45f;
        float y = origem.y;
        combate.Mover(new Vector3(x, y, origem.z));
        combate.Atacar(1.5f, 1, 0f, 4f, Color.yellow);
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo info, int layerIndex)
    {
        if (combate != null) combate.EncerrarEstado();
    }
}
