using TMPro;
using UnityEngine;

public class ExperimentUI : MonoBehaviour
{
    #region Variables
    private bool experimentStart;
    public TextMeshProUGUI instructions;
    public int rating;
    #endregion
    
    #region Methods
    private void Start()
    {
        if (instructions == null)
        {
            Debug.Log("Get instructions.");
            instructions = GameObject.Find("Instructions").GetComponent<TextMeshProUGUI>();
        }
    }
    
    #endregion
}
