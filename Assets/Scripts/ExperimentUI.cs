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
            instructions = GameObject.Find("Instructions").GetComponent<TextMeshProUGUI>();
        }
    }

    public void ratingOne()
    {
        rating = 1;
    }
    public void ratingTwo()
    {
        rating = 2;
    }
    public void ratingThree()
    {
        rating = 3;
    }
    public void ratingFour()
    {
        rating = 4;
    }
    public void ratingFive()
    {
        rating = 5;
    }
    
    #endregion
}
