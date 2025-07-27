using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class AnimateTrain : MonoBehaviour
{
    [SerializeField] private Animator animator;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (TutorialManager.Instance != null)
        {
            if (TutorialManager.Instance.canAnimateTrain)
            {
                animator.SetTrigger("Close");
                animator.ResetTrigger("Open");
            }
            else if (TutorialManager.Instance.canAnimateTrain)
            {
                animator.SetBool("isMoving", false);
                animator.SetTrigger("Open");
                animator.ResetTrigger("Close");
            }
        }

        if (LevelManager.Instance == null) { return; }

        if (LevelManager.Instance.currState == MovementState.Card)
        {
            animator.SetTrigger("Close");
            animator.ResetTrigger("Open");
        }
        else if (LevelManager.Instance.currState == MovementState.Station)
        {
            animator.SetBool("isMoving", false);
            animator.SetTrigger("Open");
            animator.ResetTrigger("Close");
        }
    }

    public void SetMovingAnimSpeed(float speedMult)
    {
        MovementState state = LevelManager.Instance != null ?
            LevelManager.Instance.currState :
            TutorialManager.Instance.currState;

        if (state == MovementState.Travel)
        {
            animator.SetBool("isMoving", true);

            if (speedMult > 0f)
            {
                animator.speed = speedMult;
            }
        }
        else
        {
            animator.SetBool("isMoving", false);
            animator.speed = 1f;
        }
    }
}
