using System.Collections;
using UnityEngine;
using System;
using BCIEssentials.Controllers;
using BCIEssentials.StimulusEffects;
using UnityEngine.UI;
using System.Collections.Generic;
using BCIEssentials.LSLFramework;

namespace BCIEssentials.ControllerBehaviors
{
    public class StimulusPresentationControllerBehavior : ContinualStimulusControllerBehavior
    {
        public override BCIBehaviorType BehaviorType => BCIBehaviorType.TVEP;

        [SerializeField] private float setFreqFlash;
        [SerializeField] private float realFreqFlash;

        private int[] frames_on = new int[99];
        private int[] frame_count = new int[99];
        private float period;
        private int[] frame_off_count = new int[99];
        private int[] frame_on_count = new int[99];

        //start of emily stuff
        public Camera mainCam;
        public Text _displayText;
        private bool _offMessages;
        private bool _restingState;
        private bool _open;
        private bool _closed;
        private string stimulusString = "";
        private Dictionary<int, string> orderDict = new Dictionary<int, string>();


        protected override void Start()
        {
            base.Start();
            
            mainCam = Camera.main;
            mainCam.enabled = true;
        
            _displayText = GameObject.Find("TextToDisplay").GetComponent<Text>();

            //randomize order of stimulus presentation 
            Randomize();

            //set first frequency
            setFreqFlash = 9.6f;
            CleanUpAfterStimulusRun();
            PopulateObjectList();
        }

        protected override void UpdateObjectListConfiguration()
        {
            for (int i = 0; i < _selectableSPOs.Count; i++)
            {
                frames_on[i] = 0;
                frame_count[i] = 0;
                period = targetFrameRate / setFreqFlash;
                frame_off_count[i] = (int)Math.Ceiling(period / 2);
                frame_on_count[i] = (int)Math.Floor(period / 2);
                realFreqFlash = targetFrameRate / (float)(frame_off_count[i] + frame_on_count[i]);
            }
        }

        protected override void SendWindowMarker(int trainingIndex = -1)
        {
            string markerString;
                
            if(_offMessages)
            {
                markerString = "Stimulus Off";
            }
            else
            {
                markerString = new TVEPEventMarker
                (
                    SPOCount, windowLength,
                    new[] {realFreqFlash}, trainingIndex
                ).MarkerString;
            }

            if(_restingState && _open)
            {
                markerString = "Resting state, eyes open";
            }
            if(_restingState && _closed)
            {
                markerString = "Resting state, eyes closed";
            }

            MarkerWriter.PushString(markerString);
        }
        

        private IEnumerator RunFlashingRoutine()
        {
            for (int i = 0; i < _selectableSPOs.Count; i++)
            {
                frame_count[i]++;
                if (frames_on[i] == 1)
                {
                    if (frame_count[i] >= frame_on_count[i])
                    {
                        _selectableSPOs[i].StopStimulus();
                        frames_on[i] = 0;
                        frame_count[i] = 0;
                    }
                }
                else
                {
                    if (frame_count[i] >= frame_off_count[i])
                    {
                        _selectableSPOs[i].StartStimulus();
                        frames_on[i] = 1;
                        frame_count[i] = 0;
                    }
                }
            }
            yield return null;
        }

        protected override void CleanUpAfterStimulusRun()
        {
            foreach (var spo in _selectableSPOs)
            {
                if (spo != null)
                {
                    spo.StopStimulus();
                }
            }
        }

        public void StopStimulusRun(int j, int l)
        {
            if (j == 2)
            {
                setFreqFlash = 16f;
                //need to call these methods so all of the appropriate flashing variables are updated 
                PopulateObjectList();
            }
            else if (j == 3)
            {
                setFreqFlash = 36f;
                PopulateObjectList();
            }
            else if (j == 6)
            {
                setFreqFlash = 9.6f;
                ColorFlashEffect2 spoEffect = _selectableSPOs[0].GetComponent<ColorFlashEffect2>();

                if (spoEffect != null)
                {
                    if (l == 0)
                       SetMaterial(1);
                    else if (l == 1)
                        SetMaterial(2);
                    else if (l == 2)
                        SetMaterial(3);
                    else if (l == 3)
                        SetMaterial(4);
                    else if (l == 4)
                        SetMaterial(5);
                    else if (l == 5)
                        SetMaterial(6);

                    PopulateObjectList();
                }
            }
        }
            
        protected override IEnumerator RunStimulusRoutine()
        {
            //setup variables for camera rotation 
            var _rotateAway = Vector3.zero;
            _rotateAway.y = 90f;

            var _rotateBack = Vector3.zero;
            _rotateBack.y = -90f;
            
            mainCam.transform.Rotate(_rotateAway);
             StartCoroutine(DisplayTextOnScreen("+"));
            _restingState = true;
            _open = true;
            //1 minute eyes open Resting State 
            yield return new WaitForSecondsRealtime(60f); //60
            _open = false;
            _closed = true;
        
            StartCoroutine(DisplayTextOnScreen("Close"));
            //1 minute eyes closed Resting State 
            yield return new WaitForSecondsRealtime(60f); //60
            _restingState = false;
            _closed = false;
            mainCam.transform.Rotate(_rotateBack);

            //set initial color and contrast
            ColorFlashEffect2 spoEffect = _selectableSPOs[0].GetComponent<ColorFlashEffect2>();
            SetMaterial(0);
            stimulusString = ", "  + orderDict[0];

            //5 seconds count down before starting
            _offMessages = true;                    
            mainCam.transform.Rotate(_rotateBack);
            StartCoroutine(DisplayTextOnScreen("5"));
            yield return new WaitForSecondsRealtime(5f);
            mainCam.transform.Rotate(_rotateAway);
            _offMessages = false;

            for(var l = 0 ; l < 7; l++)
            //this loops through all 7 stimuli  
            {
                for(var k = 0; k < 3; k++)
                {
                //do this 3 times so each stimulus is played at all 3 frequencies 
                    for(var j = 0; j < 3; j++)
                    //do this for loop 3 times (12 seconds on 8 seconds off * 3)
                    {
                        for(var i = 0; i <144*12; i++) //(StimulusRunning)
                        //the number that i is less than is the amount of seconds to flash for 
                        //144 = 1 second (frame rate is 144 Hz) so 12 seconds = i < 144*12
                        {
                            yield return RunFlashingRoutine();
                        }

                        //rotate the camera away from the stimuli objects when they are off
                        mainCam.transform.Rotate(_rotateAway);
                        _offMessages = true;

                        //control the 3 second countdown during every 8 seconds off
                        if (k <= 2)
                        {
                            if(k < 2 || (k ==2 && j < 2))
                            {   
                                yield return new WaitForSecondsRealtime(5f); //5
                                StartCoroutine(DisplayTextOnScreen("3"));
                                yield return new WaitForSecondsRealtime(3f); 
                            }
                        }

                        //rotate the camera back to facing the stimulus objects 
                        mainCam.transform.Rotate(_rotateBack);
                        _offMessages = false;

                        //change the frequency of the stimulus object
                        if (j == 2)
                        {
                            if (k == 0)
                            {
                                StopStimulusRun(j, 0);
                            }
                            if (k == 1)
                            {
                                StopStimulusRun(j+1, 0);
                            }
                        }

                    }
                }

                mainCam.transform.Rotate(_rotateAway);

                //wait 20 seconds between sets and display the countdown 
                //the first call to StartCountDown displays a message to respond to the survey
                //(immediately after the flashing stops). And then wait 15 seconds before starting the 5 second countdown. 
                _offMessages = true;
                StartCoroutine(DisplayTextOnScreen("Survey"));
                yield return new WaitForSecondsRealtime(15f);  //15

                if(l != 6)
                {
                    StartCoroutine(DisplayTextOnScreen("5"));
                    yield return new WaitForSecondsRealtime(4f);
                }

                //when StopStimulusRun is called with 6, the frequency is set to 9.6 and the stimulus contrast/texture is changed
                StopStimulusRun(6, l); 
                yield return new WaitForSecondsRealtime(1f);
                mainCam.transform.Rotate(_rotateBack); 
                _offMessages = false;
                        
                if(l == 6)
                {
                    mainCam.transform.Rotate(_rotateAway);
                    _offMessages = true;
                    yield return new WaitForSecondsRealtime(8f);
                    StartCoroutine(DisplayTextOnScreen("End"));
                    yield return new WaitForSecondsRealtime(2f);
                    _offMessages = false;

                    _restingState = true;
                    _open = true;
                    StartCoroutine(DisplayTextOnScreen("+"));

                    //1 minute eyes open Resting State 
                    yield return new WaitForSecondsRealtime(60f); //60
                    _open = false;
                    _closed = true;

                    StartCoroutine(DisplayTextOnScreen("Close"));

                    //1 minutes eye closed Resting State 
                    yield return new WaitForSecondsRealtime(60f); //60
                    _restingState = false;
                    _closed = false;

                    StartCoroutine(DisplayTextOnScreen("EndOfSession"));
                    break;
                }
            }
        }



//////Helper Methods
        public IEnumerator DisplayTextOnScreen(string textOption)
        {
            if(textOption == "3")
            {
                _displayText.text = "3";
                yield return new WaitForSecondsRealtime(1.0f);
                _displayText.text = "2";
                yield return new WaitForSecondsRealtime(1.0f);
                _displayText.text = "1";
                yield return new WaitForSecondsRealtime(1.0f);
                _displayText.text = "";
            }
            else if(textOption == "5")
            {
                _displayText.text = "Starting in...";
                yield return new WaitForSecondsRealtime(2.0f);
                _displayText.text = "3 seconds";
                yield return new WaitForSecondsRealtime(1.0f);
                _displayText.text = "2 seconds";
                yield return new WaitForSecondsRealtime(1.0f);
                _displayText.text = "1 second";
                yield return new WaitForSecondsRealtime(1.0f);
               _displayText.text = "";
            }
            else if(textOption == "End")
            {
                _displayText. text = "Look at the plus sign";
                yield return new WaitForSecondsRealtime(2.0f);
                _displayText.text = "";
            }
            else if(textOption == "EndOfSession")
            {
                _displayText. text = "End";
                yield return new WaitForSecondsRealtime(2.0f);
            }
            else if(textOption == "Survey")
            {
                _displayText.text = "Survey";
                yield return new WaitForSecondsRealtime(5.0f);
                _displayText.text = "";
            }
              else if(textOption == "+")
            {
                _displayText.text = "+";
                yield return new WaitForSecondsRealtime(60.0f);
                _displayText.text = "";
            }
            else if(textOption == "Close")
            {
                _displayText.text = "Close your eyes";
                yield return new WaitForSecondsRealtime(60.0f);
                _displayText.text = "";
            }
        } 
        private void SetMaterial(int key)
        {
            ColorFlashEffect2 spoEffect = _selectableSPOs[0].GetComponent<ColorFlashEffect2>();
            if (orderDict.TryGetValue(key, out string material))
            {       
                if (material == "MinContrast")
                {
                    spoEffect.SetContrast(ColorFlashEffect2.ContrastLevel.Min);
                    stimulusString = ", MinContrast";
                }
                else if (material == "MaxContrast")
                {
                    spoEffect.SetContrast(ColorFlashEffect2.ContrastLevel.Max);
                    stimulusString = ", MaxContrast";
                }
                else if (material == "Worms")
                {
                    spoEffect.SetTextureExternal(ColorFlashEffect2.TextureSelection.Worms);
                    stimulusString = ", Worms";
                }
                else if (material == "Static")
                {
                    spoEffect.SetTextureExternal(ColorFlashEffect2.TextureSelection.Static);
                    stimulusString = ", Static";
                }
                else if (material == "Checkerboard")
                {
                    spoEffect.SetTextureExternal(ColorFlashEffect2.TextureSelection.Checkerboard);
                    stimulusString = ", Checkerboard";
                }
                else if (material == "Voronoi")
                {
                    spoEffect.SetTextureExternal(ColorFlashEffect2.TextureSelection.Voronoi);
                    stimulusString = ", Voronoi";
                }
                else if (material == "WoodGrain")
                {
                    spoEffect.SetTextureExternal(ColorFlashEffect2.TextureSelection.Wood);
                    stimulusString = ", WoodGrain";
                }
            }
        }

        private void Randomize()
        {
                orderDict.Add(0, "MaxContrast");
                orderDict.Add(1, "MinContrast");
                orderDict.Add(2, "Worms");
                orderDict.Add(3, "Voronoi"); 
                orderDict.Add(4, "WoodGrain");
                orderDict.Add(5, "Checkerboard");
                orderDict.Add(6, "Static");

                System.Random random = new System.Random();
                List<int> keys = new List<int>(orderDict.Keys);
                int num = keys.Count;

                while (num > 1)
                {
                    num--;
                    int k = random.Next(num + 1);
                    int temp = keys[k];
                    keys[k] = keys[num];
                    keys[num] = temp;
                }

                List<string> values = new List<string>(orderDict.Values);
                    
                int n = values.Count;
                while (n > 1)
                {
                    n--;
                    int k = random.Next(n + 1);
                    string temp = values[k];
                    values[k] = values[n];
                    values[n] = temp;
                }

                Dictionary<int, string> intDict = new Dictionary<int, string>();
                    
                for (int i = 0; i < keys.Count; i++)
                {
                    intDict.Add(keys[i], values[i]);
                }

                List<KeyValuePair<int, string>> keyValuePairs = new List<KeyValuePair<int, string>>(intDict);
                    
                int c = keyValuePairs.Count;
                while (c > 1)
                {
                    c--;
                    int k = random.Next(c + 1);
                    KeyValuePair<int, string> temp = keyValuePairs[k];
                    keyValuePairs[k] = keyValuePairs[c];
                    keyValuePairs[c] = temp;
                }

                Dictionary<int, string> randomDict = new Dictionary<int, string>();

                foreach (var k in keyValuePairs)
                    randomDict.Add(k.Key, k.Value);

                orderDict = new Dictionary<int, string>(randomDict);       
            }
            
    }
}

    

