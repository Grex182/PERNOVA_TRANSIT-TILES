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
        if (GameManager.instance.StationManager.isTrainMoving)
        {
            animator.SetTrigger("Close");
            animator.ResetTrigger("Open");
            //animator.SetTrigger("Moving");
        }
        else if (!GameManager.instance.StationManager.isTrainMoving && GameManager.instance.StationManager.hasPassengersSpawned)
        {
            animator.SetTrigger("Open");
            animator.ResetTrigger("Close");
        }
    }
}
