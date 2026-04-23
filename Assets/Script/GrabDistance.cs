using UnityEngine;

public class GrabLitch : MonoBehaviour
{
    Collider isTrig;
    public bool canGrab;
    private void Start()
    {
        canGrab = false;
        isTrig = GetComponent<Collider>();
    }
    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("BadNPC"))
        {
            canGrab = true;
        } else
        {
            canGrab = false;
        }
    }
}
