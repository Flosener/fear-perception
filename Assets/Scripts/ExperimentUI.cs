using TMPro;
using UnityEngine;

public class ExperimentUI : MonoBehaviour
{
    #region Variables
    private bool experimentStart;
    public TextMeshProUGUI instructions;
    #endregion
    
    #region Methods
    private void Start()
    {
        instructions = gameObject.GetComponent<TextMeshProUGUI>();
        instructions.text = "Welcome to your therapy session.";
    }
    
    #endregion
}
