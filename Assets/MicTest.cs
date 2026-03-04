using UnityEngine;

public class MicTest : MonoBehaviour
{
    private AudioClip clip;
    private string micName;

    void Start()
    {
        micName = Microphone.devices[0];
        Debug.Log("Usando mic: " + micName);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            clip = Microphone.Start(micName, false, 5, 44100);
            Debug.Log("Grabando...");
        }

        if (Input.GetKeyUp(KeyCode.R))
        {
            Microphone.End(micName);
            Debug.Log("Grabación detenida");
        }
    }
}