using UnityEngine;

public class WaveTrigger : MonoBehaviour
{
    public bool waveStart = false;

    [SerializeField]
    public WaveManager waveManager;
    [SerializeField]
    public GameObject waveMan;
    
 
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == 11)
        {   
            if (waveManager.waveEnded == false)
            {
                waveStart = false;
                waveMan.SetActive(false);

            }


        }

    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 11)
        {
            if (waveManager.waveEnded == true )
            {
                waveStart = true;
                waveMan.SetActive(true);
            }
        }
    }
}
