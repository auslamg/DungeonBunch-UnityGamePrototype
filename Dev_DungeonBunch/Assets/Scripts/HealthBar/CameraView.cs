using UnityEngine;
/// <summary>
/// El objeto apunta al vector.forward de la camara del jugador
/// </summary>
public class CameraView : MonoBehaviour
{
    [Header("Camara hacia la que va a apuntar")]
    public Transform camPlayer;

    // Update is called once per frame
    void LateUpdate()
    {
        transform.LookAt(transform.position + camPlayer.forward);
    }
}
