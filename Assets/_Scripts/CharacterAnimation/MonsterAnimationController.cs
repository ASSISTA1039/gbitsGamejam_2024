using System.Collections;
using UnityEngine;

public class MonsterAnimationController : MonoBehaviour
{
    private Animator animator;

    private void Start()
    {
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogError("Î´ÕÒµ½ Animator ×é¼þ£¡");
            return;
        }
        StartCoroutine(PlayAnimationSequence());
    }

    private IEnumerator PlayAnimationSequence()
    {
        string[] triggers = { "TriggerDentalCard", "TriggerJokerCard", "TriggerDogCard", "TriggerChristmasCard", "TriggerGhostCard" };

        foreach (string trigger in triggers)
        {
            animator.SetTrigger(trigger);
            yield return new WaitUntil(() => IsInState(trigger.Replace("Trigger", "")));
        }

        animator.SetTrigger("TriggerIdle");
    }

    private bool IsInState(string stateName)
    {
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        return stateInfo.IsName(stateName) && stateInfo.normalizedTime >= 1f;
    }
}
