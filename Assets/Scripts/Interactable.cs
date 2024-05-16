using UnityEngine;

public class Interactable : MonoBehaviour
{

    public virtual void MouseOver()
    {
        // Runs when the mouse Hovers Over this
    }

    public virtual void MouseExit()
    {
        // Runs when the mouse Exits this
    }

    public virtual void InteractStart()
    {
        // Runs when E is Pressed on the Object
    }

    public virtual void InteractEnd()
    {
        // Runs when E is Released on the Object
    }
}
