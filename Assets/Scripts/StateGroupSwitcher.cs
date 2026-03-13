using UnityEngine;

public class StateGroupSwitcher : MonoBehaviour
{
    [Header("State Key")]
    public string stateKey = "Fountain_Offering_Done";

    [Header("Groups")]
    public GameObject beforeStateGroup;
    public GameObject afterStateGroup;

    private void Start()
    {
        Refresh();
    }

    public void Refresh()
    {
        bool state = false;

        if (PuzzleStateManager.Instance != null)
        {
            state = PuzzleStateManager.Instance.GetState(stateKey);
        }

        if (beforeStateGroup != null)
            beforeStateGroup.SetActive(!state);

        if (afterStateGroup != null)
            afterStateGroup.SetActive(state);
    }
}