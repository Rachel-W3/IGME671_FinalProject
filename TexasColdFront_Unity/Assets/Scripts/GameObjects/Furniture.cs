using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace tcf.obj 
{
public class Furniture : MonoBehaviour
{
    [SerializeField] float createableFuel;

    bool interacting = false;

    Rigidbody2D rb;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        //Null check
        if(rb != null)
        {
            rb.freezeRotation = true;
            rb.isKinematic = false;
        }
    }

    public void BeginInteraction(GameObject interactingObject)
    {
        interacting = true;
        rb.isKinematic = true;

        //Set parent so it moves with the player
        //    NOTE: May need to be changed as player controller is developed
        transform.parent = interactingObject.transform;
    }

    public void EndInteraction()
    {
        interacting = false;
        rb.isKinematic = false;
        transform.parent = null;
    }

    /// <summary>
    /// Break down the furniture to create the fuel
    /// </summary>
    public void BreakDownFurniture()
    {
        //Add fuel to overall amount
        Resources.Instance.AddFuel(createableFuel);
        //Destroy this object
        Destroy(this.gameObject);
    }
}
}