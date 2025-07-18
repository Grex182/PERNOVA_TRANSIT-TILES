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
        if (LevelManager.Instance.currState == MovementState.Travel)
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
