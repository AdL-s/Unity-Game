using UnityEngine;
using UnityEngine.UI;

public class DoorTrigger : MonoBehaviour
{
    [SerializeField]
    private Dvere dvere;
    
    private void OnTriggerExit(Collider other)
    {
       if (other.gameObject.layer == 11)
        {
            dvere.otvoreni = false;
        }
        
    }
    private void OnTriggerEnter(Collider other)
    {
       if (other.gameObject.layer == 11)
        {
            dvere.otvoreni = true;
        }
    }



}
