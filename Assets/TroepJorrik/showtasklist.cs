using UnityEngine;

public class showtasklist : MonoBehaviour
{
    // Optional: assign a GameObject in the Inspector that this script will show
    public GameObject targetObject;

    // Show the Inspector-assigned object (set it active)
    public void ShowAssigned()
    {
        if (targetObject != null)
            targetObject.SetActive(!targetObject.activeSelf);
    }

    // Show any provided GameObject (set it active)
    public void ShowObject(GameObject obj)
    {
        if (obj != null)
            obj.SetActive(!obj.activeSelf);
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
