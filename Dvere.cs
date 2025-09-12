using UnityEngine;

public class Dvere: MonoBehaviour
{
    public float height = 5;
    public float time = 2;
    public float okolko = 0;


    public bool otvoreni = false;

    private void Update()
    {
        if (otvoreni)
        {
            Otvoreni();
        }
        else
        {
            Zavreni();
        }
    }

    private void Otvoreni()
    {
        if (transform.localPosition.y > height)
        {
            return;
        }
        okolko = height / time;
        transform.position += new Vector3(0,okolko,0)*Time.deltaTime;
    }

    private void Zavreni()
    {
        if (transform.localPosition.y < 0)
        {
            return;
        }
        okolko = height / time;
        transform.position -= new Vector3(0, okolko, 0) * Time.deltaTime;
    }
}
