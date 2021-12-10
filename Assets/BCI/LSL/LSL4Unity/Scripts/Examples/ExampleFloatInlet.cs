using UnityEngine;
using System;
using System.Linq;
using Assets.LSL4Unity.Scripts.AbstractInlets;

namespace Assets.LSL4Unity.Scripts.Examples
{
    /// <summary>
    /// Just an example implementation for a Inlet recieving float values
    /// </summary>
    public class ExampleFloatInlet : AStringInlet
    {
        private string input;

        protected override void Process(string[] newSample, double timeStamp)
        {
            input = newSample[0];
            print("Received " + input);

            //Call CoRoutine to do further processing

                
        }
    }
}