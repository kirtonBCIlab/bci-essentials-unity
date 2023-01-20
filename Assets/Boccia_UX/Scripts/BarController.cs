using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarController : MonoBehaviour
{
    Animator barAnim;

    public void DropButtonPressed() {
        barAnim.SetBool("isOpening", true);
        Invoke("Close", 3);
    }
    void Close() {
        barAnim.SetBool("isOpening", false);
    }
    // Start is called before the first frame update
    void Start()
    {
        barAnim = this.transform.parent.GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
