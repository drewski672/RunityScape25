using UnityEngine;

public class TreeNode : MonoBehaviour
{
    public int hitsRemaining = 5;
    public int xpPerLog = 25;

    public bool IsDepleted { get; private set; }

    public void Deplete()
    {
        IsDepleted = true;
        Debug.Log("Tree depleted.");
        gameObject.SetActive(false);
    }
}
