using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class TrashData : MonoBehaviour
{
    [SerializeField] GameObject _trashParent;
    [SerializeField] GameObject _puddleParent;

    [SerializeField] float _speed = 1f;
    [SerializeField] float _scaleSpeed = 0.9f;
    [SerializeField] float _scaleAccel = 1f;
    private bool isBeingRemoved = false;

    // Start is called before the first frame update
    void Start()
    {
        //Random Trash objects
        foreach (Transform child in _trashParent.transform)
        {
            child.gameObject.SetActive(Random.Range(0, 2) == 0);

            int rotate = Random.Range(0, 5);

            child.transform.Rotate(0, rotate * 90, 0);
        }

        int puddleInt = Random.Range(0, _puddleParent.transform.childCount);
        
        Transform puddle = _puddleParent.transform.GetChild(puddleInt);

        int rotatePuddle = Random.Range(0, 5);
        puddle.gameObject.SetActive(true);
        puddle.Rotate(0, rotatePuddle * 90, 0);


    }

    // Update is called once per frame
    void Update()
    {
        if (isBeingRemoved)
        {
            this.transform.localPosition += Vector3.up * _speed * Time.deltaTime;

            _scaleSpeed -= _scaleAccel * Time.deltaTime;
            this.transform.localScale += Vector3.one * _scaleSpeed * Time.deltaTime;


            if (this.transform.localScale.z < 0.1f)
            {
                Destroy(gameObject);
            }
        }
    }

    public void TrashRemove()
    {
        isBeingRemoved = true;
    }
}
