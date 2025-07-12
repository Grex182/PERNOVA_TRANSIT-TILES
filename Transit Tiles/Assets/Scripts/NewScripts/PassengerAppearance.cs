using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PassengerAppearance : MonoBehaviour
{

    [SerializeField] private PassengerData _data;

    [SerializeField] private GameObject _genderObj;
    private  GameObject _genderChild;
    [SerializeField] private GameObject _genderFemale;

    [SerializeField] private GameObject _lowerFemaleObj;
    [SerializeField] private GameObject _lowerMaleObj;
    private  GameObject _lowerChild;

    [SerializeField] private GameObject _hairFemaleObj;
    [SerializeField] private GameObject _hairMaleObj;
    private  GameObject _hairChild;

    [SerializeField] private GameObject _topFemaleObj;
    [SerializeField] private GameObject _topMaleObj;
    private  GameObject topChild;


    [SerializeField]
    private Color[] _skinColors;

    [SerializeField]
    private Color[] _bottomsColors;

    [SerializeField]
    private Color[] _hairColors;

    [SerializeField]
    private Color[] _shoeColors;



    // Start is called before the first frame update
    void Start()
    {
        Initialize();
    }

    // Update is called once per frame
    public void Initialize()
    {
        //Select Gender
        SelectChild(_genderObj, ref _genderChild, _skinColors);
        //Check if Female
        if (_genderChild == _genderFemale) //Female Clothing Selection
        {
            //Lower Clothing
            SelectChild(_lowerFemaleObj, ref _lowerChild, _bottomsColors);
            //Hair
            SelectChild(_hairFemaleObj, ref _hairChild, _hairColors);
            //Top Clothing
            SelectChild(_topFemaleObj, ref topChild, _hairColors);

            _lowerMaleObj.SetActive(false);
            _hairMaleObj.SetActive(false);
            _topMaleObj.SetActive(false);
        }
        else //Male Clothing Selection
        {
            //Lower Clothing
            SelectChild(_lowerMaleObj, ref _lowerChild, _bottomsColors);
            //Hair
            SelectChild(_hairMaleObj, ref _hairChild, _hairColors);
            //Top Clothing
            SelectChild(_topMaleObj, ref topChild, _hairColors);


            _lowerFemaleObj.SetActive(false);
            _hairFemaleObj.SetActive(false);
            _topFemaleObj.SetActive(false);
        }

        topChild.GetComponent<SkinnedMeshRenderer>().material.color = LevelManager.Instance.GetColorFromEnum(_data.targetStation);
    }

    private void SelectChild(GameObject _parent,ref GameObject _chosenChild, Color[] _colorArray)
    {
        List<GameObject> _childrenArray = new List<GameObject>();


        // Loop through all children
        for (int i = 0; i < _parent.transform.childCount; i++)
        {
            Transform child = _parent.transform.GetChild(i);
            _childrenArray.Add(child.gameObject);
        }
        //Deactivate all children
        foreach (GameObject child in _childrenArray)
        {
            child.SetActive(false);
        }
        //Pick a Chosen Child
        int randNumber = Random.Range(0, _childrenArray.Count);


        _chosenChild = _childrenArray[randNumber];
        _chosenChild.SetActive(true);

        ColorChild(_chosenChild, _colorArray);

    }

    private void ColorChild(GameObject _chosenChild, Color[] _colorArray)
    {
        _chosenChild.GetComponent<SkinnedMeshRenderer>().material.color = _colorArray[Random.Range(0, _colorArray.Length)];

        //Accent and Shoe Color
        if (_chosenChild.GetComponent<SkinnedMeshRenderer>().materials.Length > 1 ) 
        {
            _chosenChild.GetComponent<SkinnedMeshRenderer>().materials[1].color = _shoeColors[Random.Range(0, _shoeColors.Length)];
        }
    }

    


}

