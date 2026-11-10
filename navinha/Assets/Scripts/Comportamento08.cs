using UnityEngine;

// Estado 8: comportamento independente do Animator.
public class Comportamento08 : StateMachineBehaviour
{
    private ChefaoCombate combate;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo info, int layerIndex)
    {
        combate = animator.GetComponent<ChefaoCombate>();
        if (combate == null) combate = animator.GetComponentInParent<ChefaoCombate>();
        if (combate != null) combate.IniciarEstado(7);
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo info, int layerIndex)
    {
        if (combate == null || !combate.PodeAgir) return;
        float t = Time.time * 2.4f;
        float a = combate.Amplitude;
        Vector3 origem = combate.Origem;
        float x = origem.x + Mathf.Sin(t * 3.3f) * a * .85f;
        float y = origem.y + Mathf.Sin(t * 2f) * .35f;
        combate.Mover(new Vector3(x, y, origem.z));
        combate.Atacar(0.55f, 5, 35f, 7f, Color.red);
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo info, int layerIndex)
    {
        if (combate != null) combate.EncerrarEstado();
    }
}
