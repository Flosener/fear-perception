using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using Valve.VR;

public class ExperimentManager : MonoBehaviour
{
    #region Variables
    
    // Flags
    private bool _beginExperiment;
    private bool _experimentDone;
    private bool _participantResponse;
    private bool _enableResponse;
    private bool _enableRating;
    [SerializeField] private SteamVR_Action_Boolean _response;
    [SerializeField] private SteamVR_Action_Boolean _up;
    [SerializeField] private SteamVR_Action_Boolean _down;
    // Scripts
    private ExperimentUI _uiManager;
    
    // GOs
    private Transform _participant;
    private Transform _leftHand;
    private Transform _rightHand;
    private Vector3 _couchPos;
    private Vector3 _stimulusPos;
    [SerializeField] private List<GameObject> _stimuli;
    private GameObject _imageStand;
    private GameObject _focusPoint;
    private Transform _stimuliRoot;
    private GameObject _stimulus;

    // Helpers
    private float _startTime;
    private float _elapsedTime;
    private float _distanceHead;
    private float _distanceHandLeft;
    private float _distanceHandRight;
    
    // Data variables
    [SerializeField] private int _participantID;
    private float _RT;
    private string _stimulusName;
    private string _condition;
    private string _mode;
    private float _engagement;
    private int _rating;
    private float _size;

    #endregion

    private IEnumerator Start()
    {
        // To-do: Create data directory if non-existent
        // Directory.CreateDirectory("Data");

        // Find necessary GO's
        _uiManager = GameObject.Find("InstructionsCanvas").GetComponent<ExperimentUI>();
        _participant = GameObject.Find("VRCamera").transform;
        _leftHand = GameObject.Find("LeftHand").transform;
        _rightHand = GameObject.Find("RightHand").transform;
        _couchPos = GameObject.Find("sofa").transform.position;
        _focusPoint = GameObject.Find("FocusPoint");
        _imageStand = GameObject.Find("ImageStand");
        _stimuliRoot = GameObject.Find("Stimuli").transform;
        
        _down.AddOnStateDownListener(LeftPressed, SteamVR_Input_Sources.LeftHand);
        _up.AddOnStateDownListener(RightPressed, SteamVR_Input_Sources.RightHand);
        _response.AddOnStateDownListener(GetResponse, SteamVR_Input_Sources.Any);

        _imageStand.SetActive(false);
        _focusPoint.SetActive(false);

        // Show instructions to the participants, wait for them to begin the experiment and disable instructions.
        _beginExperiment = false;
        StartCoroutine(HandleInstructions());
        yield return new WaitUntil(() => _beginExperiment);
        _beginExperiment = false;

        // Start experiment.
        StartCoroutine(Experiment(1f, 2*3, 10)); // condition x mode = 6 blocks á 10 trials
        yield return new WaitUntil(() => _experimentDone);
        
        // Experiment ended
        _uiManager.instructions.text = "Thank you for your participation!";
        yield return new WaitForSeconds(3f);
        _experimentDone = false;
        // Application.Quit();
    }
    
    // Check for user input each frame.
    private void Update()
    {
        CalculateEngagement();
        _elapsedTime = Time.time - _startTime;
    }

    private IEnumerator Experiment(float seconds, int blocks, int trials)
    {
        for (var i = 0; i < blocks; i++)
        {
            for (var j = 0; j < trials; j++)
            {
                _engagement = 1000f;
                
                // Show focus point
                _focusPoint.SetActive(true);
                yield return new WaitForSeconds(seconds);
                _focusPoint.SetActive(false);
                
                // Show random stimulus and measure trial time
                _stimulus = GetStimulus();
                _elapsedTime = 0f;
                _startTime = Time.time;
                _enableResponse = true;
                yield return new WaitUntil(() => _participantResponse || _elapsedTime >= 30f);
                _participantResponse = false;

                // Remove stimulus after response
                _imageStand.SetActive(false);
                Destroy(_stimulus);

                // After participant response, show rating UI
                _enableRating = true;
                _rating = 3;
                _uiManager.instructions.text = $"Rating of uneasiness: {_rating}";
                yield return new WaitForSeconds(0.5f);
                yield return new WaitUntil(() => _participantResponse);
                AddRecord(_participantID, i+1, j+1, _RT, _condition, _mode, _stimulusName, _size, _engagement, _rating, "/fear-perception/Data/results.txt");
                _uiManager.instructions.text = "";
                _participantResponse = false;
                _enableRating = false;
                _enableResponse = false;
            }

            // Only show "block ended" instructions if this was not the last block
            if (i + 1 != blocks)
            {
                _uiManager.instructions.text = "Block ended. Press 'SPACE' to start the next block.";
                _enableResponse = true;
                yield return new WaitForSeconds(0.5f);
                yield return new WaitUntil(() => _participantResponse);
                _uiManager.instructions.text = "";
                _participantResponse = false;
                _enableResponse = false;
            }
        }
        _experimentDone = true;
    }

    private GameObject GetStimulus()
    {
        // Randomly get condition, mode and stimulus number
        _condition = Random.Range(0,2) == 0 ? "spider" : "rabbit";
        _mode = Random.Range(0,3) switch
        {
            0 => "image",
            1 => "static",
            _ => "dynamic"
        };
        _stimulusName = _condition + Random.Range(1,6).ToString();
        
        // Instantiate stimulus
        if (_mode == "image")
        {
            _imageStand.SetActive(true);
            var tex = Resources.Load<Texture2D>(_stimulusName);
            _imageStand.GetComponentInChildren<RawImage>().texture = tex;
            _size = -1f;
        }
        else
        {
            foreach (var s in _stimuli.Where(s => s.name == _stimulusName))
            {
                // Spawn stimulus prefab
                var stimulus = Instantiate(s, _stimuliRoot);
                
                // Critical condition (spider)
                if (_condition == "spider")
                {
                    // Get spider size
                    stimulus.transform.localScale = Random.Range(0, 3) switch
                    {
                        0 => new Vector3(0.1f, 0.1f, 0.1f),
                        1 => new Vector3(0.2f, 0.2f, 0.2f),
                        _ => new Vector3(0.3f, 0.3f, 0.3f)
                    };
                    _size = stimulus.transform.localScale.x;

                    // Play animation in dynamic mode
                    if (_mode == "dynamic")
                    {
                        // To-do: increase animation speed
                        var anim = stimulus.GetComponent<Animation>();
                        anim["Spider_Idle"].speed = 2.0f;
                        anim["Spider_Move"].speed = 2.0f;
                        anim["Spider_Attack"].speed = 2.0f;
                        anim.PlayQueued("Spider_Idle", QueueMode.CompleteOthers);
                        anim.PlayQueued("Spider_Move", QueueMode.CompleteOthers);
                        anim.PlayQueued("Spider_Attack", QueueMode.CompleteOthers);
                        anim["Spider_Move"].wrapMode = WrapMode.Loop;
                        anim.PlayQueued("Spider_Move", QueueMode.CompleteOthers);
                    }
                }
                // Control condition (rabbit)
                else
                {
                    _size = stimulus.transform.localScale.x;
                    
                    if (_mode == "dynamic")
                    {
                        var animator = stimulus.GetComponent<Animator>();
                        animator.SetTrigger("runAnim");
                    }
                }
                    
                // Stop looping through stimuli list
                return stimulus;
            }
        }

        return null;
    }
    
    private IEnumerator HandleInstructions()
    {
        _enableResponse = true;

        _uiManager.instructions.text = "Welcome to your therapy session. Sit down on the couch and press 'SPACE' to proceed.";
        yield return new WaitForSeconds(0.5f);
        yield return new WaitUntil(() => _participantResponse);
        
        _participantResponse = false;

        _uiManager.instructions.text = "In the following trials you will encounter two types of animals which you have to " +
                                       "engage with as much as possible. When you think it is enough, just " +
                                       "press 'SPACE' to end the trial. Press 'SPACE' to proceed.";
        yield return new WaitForSeconds(0.5f);
        yield return new WaitUntil(() => _participantResponse);
        _participantResponse = false;

        _uiManager.instructions.text = "After each trial please rate your uneasiness during the trial. The rating ranges " +
                                       "from 1 (I felt great) over 3 (neutral) to 5 (I felt very uneasy). " +
                                       "Press 'SPACE' to begin the experiment.";
        yield return new WaitForSeconds(0.5f);
        yield return new WaitUntil(() => _participantResponse);
        _participantResponse = false;

        _uiManager.instructions.text = "";
        _beginExperiment = true;
        _enableResponse = false;
    }

    private void CalculateEngagement()
    {
        if (!_imageStand.activeSelf && _stimulus == null) return;
        
        // Engagement score is the euclidean distance of the participant to the stimulus in 3D space
        _stimulusPos = _mode == "image" ? _imageStand.transform.position : _stimulus.transform.position;
        _distanceHead = (float) Math.Sqrt(Math.Pow(_participant.position.x - _stimulusPos.x, 2) +
                                      Math.Pow(_participant.position.y - _stimulusPos.y, 2) +
                                      Math.Pow(_participant.position.z - _stimulusPos.z, 2));
        _distanceHandLeft = (float) Math.Sqrt(Math.Pow(_leftHand.position.x - _stimulusPos.x, 2) +
                                          Math.Pow(_leftHand.position.y - _stimulusPos.y, 2) +
                                          Math.Pow(_leftHand.position.z - _stimulusPos.z, 2));
        _distanceHandRight = (float) Math.Sqrt(Math.Pow(_rightHand.position.x - _stimulusPos.x, 2) +
                                          Math.Pow(_rightHand.position.y - _stimulusPos.y, 2) +
                                          Math.Pow(_rightHand.position.z - _stimulusPos.z, 2));
        
        // Update engagement score
        if (_distanceHead < _engagement && _distanceHead != 0f)
        {
            _engagement = 1/_distanceHead;
        }
        
        if (_distanceHandLeft < _engagement && _distanceHandLeft != 0f)
        {
            _engagement = 1/_distanceHandLeft;
        }
        
        if (_distanceHandRight < _engagement && _distanceHandRight != 0f)
        {
            _engagement = 1/_distanceHandRight;
        }
    }
    
    // Method for binding and writing data to .csv file.
    // Origin: Max O'Didily, https://www.youtube.com/watch?v=vDpww7HsdnM&ab_channel=MaxO%27Didily.
    private void AddRecord(int participantID, int blockCount, int trialCount, float RT, string condition, string mode, string stimulusName, float size, float engagement, int rating, string filepath)
    {
        var cond = condition == "spider" ? "critical" : "control";
        
        try
        {
            // Instantiate StreamWriter object and write line to file including all recorded variables.
            using (StreamWriter file = new StreamWriter(@filepath, true))
            {
                file.WriteLine(participantID + "," + blockCount + "," + trialCount + "," + RT + "," + cond + "," + mode + "," + stimulusName + "," + size + "," + engagement + "," + rating);
            }
        }
        catch (Exception ex)
        {
            throw new ApplicationException("An error occured: " + ex);
        }
    }
    
    private void GetResponse(SteamVR_Action_Boolean fromActionBoolean, SteamVR_Input_Sources fromInputSources)
    {
        // If P sits on couch and gives input, proceed with experiment
        if (_enableResponse
            && Math.Abs(_participant.position.x - _couchPos.x) <= 0.85f 
            && Math.Abs(_participant.position.z - _couchPos.z) <= 0.45f)
        {
            _participantResponse = true;
        }
    }

    private void LeftPressed(SteamVR_Action_Boolean fromActionBoolean, SteamVR_Input_Sources fromInputSources)
    {
        if (_enableRating && _rating > 1)
        {
            _rating--;
            _uiManager.instructions.text = $"Rating of uneasiness: {_rating}";
        }
    }
    
    private void RightPressed(SteamVR_Action_Boolean fromActionBoolean, SteamVR_Input_Sources fromInputSources)
    {
        if (_enableRating && _rating < 5)
        {
            _rating++;
            _uiManager.instructions.text = $"Rating of uneasiness: {_rating}";
        }
    }
}
