using System.Collections;
using System.Collections.Generic;
using DataBinding.BindingExtensions;
using UnityEngine;

public class BindableCollectionsExample : MonoBehaviour
{
    private BindableDictionary<string, string> _bindableDictionary;
    void Start()
    {
        _bindableDictionary = new BindableDictionary<string, string>();
        
        _bindableDictionary.Add("001","this is 001");

        _bindableDictionary["002"] = "this is 002";

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
