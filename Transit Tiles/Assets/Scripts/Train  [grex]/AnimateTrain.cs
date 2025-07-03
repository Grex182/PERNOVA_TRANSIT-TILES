using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
            animator.SetTrigger("Open");
            animator.ResetTrigger("Close");
        }
    }
}
