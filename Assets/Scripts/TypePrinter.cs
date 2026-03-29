using UnityEngine;
using UnityEngine.UI;

public class TypePrinter : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Debug.Log("TypePrinter: " + gameObject.name + " type is: " + this.gameObject.GetType());
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
