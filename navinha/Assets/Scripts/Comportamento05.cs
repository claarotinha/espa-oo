using UnityEngine;

// Estado 5: comportamento independente do Animator.
public class Comportamento05 : StateMachineBehaviour
{
    private ChefaoCombate combate;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo info, int layerIndex)
    {
        combate = animator.GetComponent<ChefaoCombate>();
        if (combate == null) combate = animator.GetComponentInParent<ChefaoCombate>();
        if (combate != null) combate.IniciarEstado(4);
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo info, int layerIndex)
    {
        if (combate == null || !combate.PodeAgir) return;
        float t = Time.time * 1.6f;
        float a = combate.Amplitude;
        Vector3 origem = combate.Origem;
        float x = origem.x + Mathf.Sin(t) * Mathf.Cos(t * 2f) * a;
        float y = origem.y + Mathf.Sin(t * 2f) * .35f;
        combate.Mover(new Vector3(x, y, origem.z));
        combate.Atacar(0.95f, 3, 20f, 5.5f, new Color(1f, .5f, 0f));
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo info, int layerIndex)
    {
        if (combate != null) combate.EncerrarEstado();
    }
}
