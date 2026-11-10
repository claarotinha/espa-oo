using UnityEngine;

// Estado 7: comportamento independente do Animator.
public class Comportamento07 : StateMachineBehaviour
{
    private ChefaoCombate combate;
    private float tempo;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo info, int layerIndex)
    {
        combate = animator.GetComponent<ChefaoCombate>();
        if (combate == null) combate = animator.GetComponentInParent<ChefaoCombate>();
        if (combate != null) combate.IniciarEstado(6);
        tempo = 0f;
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo info, int layerIndex)
    {
        if (combate == null || !combate.PodeAgir) return;
        tempo += Time.deltaTime;
        float t = Time.time * 2.4f;
        float a = combate.Amplitude;
        Vector3 origem = combate.Origem;
        float x = origem.x + Mathf.Sin(t * 2.6f) * a;
        float y = origem.y;
        combate.Mover(new Vector3(x, y, origem.z));
        combate.Atacar(0.55f, 5, 35f, 7f, Color.red);
        if (tempo >= combate.DuracaoEstado) combate.ProximoEstado(6);
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo info, int layerIndex)
    {
        if (combate != null) combate.EncerrarEstado();
    }
}
