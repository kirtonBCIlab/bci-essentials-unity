using System.Collections.Generic;
using System.Linq;

namespace BCIEssentials.LSLFramework
{
    public interface ILSLMarker
    {
        public string MarkerString {get;}
    }


    public interface ICommandMarker: ILSLMarker {}

    public struct TrialStartedMarker: ICommandMarker
    {
        public string MarkerString => "Trial Started";
    }
    public struct TrialEndsMarker: ICommandMarker 
    {
        public string MarkerString => "Trial Ends";
    }
    public struct TrainingCompleteMarker: ICommandMarker
    {
        public string MarkerString => "Training Complete";
    }
    public struct UpdateClassifierMarker: ICommandMarker
    {
        public string MarkerString => "Update Classifier";
    }


    public abstract class EventMarker: ILSLMarker
    {
        /// <summary>
        /// Number of objects or classes in the trial
        /// </summary>
        public int ObjectCount;
        /// <summary>
        /// Index of object, frequency, or class targetted for selection <i>(0-indexed)</i>
        /// </summary>
        public int TrainingTarget;

        public virtual string MarkerString
        => $"{ObjectCount},{(TrainingTarget < 0? -1: TrainingTarget + 1)}";

        public EventMarker
        (
            int objectCount, int trainingTarget
        )
        {
            ObjectCount = objectCount;
            TrainingTarget = trainingTarget;
            if (TrainingTarget > objectCount || trainingTarget < 0)
                TrainingTarget = -1;
        }
    }


    public abstract class WindowedEventMarker: EventMarker
    {
        /// <summary>
        /// Length of the processing Epoch <br/>
        /// <b>Must remain constant between trials</b>
        /// </summary>
        public float WindowLength;

        public override string MarkerString
        => $"{base.MarkerString},{WindowLength.ToString("f2")}";

        public WindowedEventMarker
        (
            int objectCount, float windowLength,
            int trainingTarget
        ): base(objectCount, trainingTarget)
        {
            WindowLength = windowLength;
        }
    }

    /// <summary>
    /// Motor Imagery event marker in the format:
    /// <br/><br/>
    /// "mi,{object count},{train target (-1 if n/a)},{window length}"
    /// </summary>
    public class MIEventMarker: WindowedEventMarker
    {
        public override string MarkerString
        => $"mi,{base.MarkerString}";

        /// <param name="objectCount">
        /// Number of objects (classes) in the trial
        /// </param>
        /// <param name="windowLength">
        /// Length of the processing Epoch <br/>
        /// <b>Must remain constant between trials</b>
        /// </param>
        /// <param name="trainingTarget">
        /// Index of class targetted for training <i>(0-indexed)</i>
        /// </param>
        public MIEventMarker
        (
            int objectCount, float windowLength,
            int trainingTarget = -1
        )
        : base(objectCount, windowLength, trainingTarget)
        {}
    }

    /// <summary>
    /// Switch event marker in the format:
    /// <br/><br/>
    /// "switch,{object count},{train target (-1 if n/a)},{window length}"
    /// </summary>
    public class SwitchEventMarker: WindowedEventMarker
    {
        public override string MarkerString
        => $"switch,{base.MarkerString}";

        /// <param name="objectCount">
        /// Number of objects (classes) in the trial
        /// </param>
        /// <param name="windowLength">
        /// Length of the processing Epoch <br/>
        /// <b>Must remain constant between trials</b>
        /// </param>
        /// <param name="trainingTarget">
        /// Index of class targetted for training <i>(0-indexed)</i>
        /// </param>
        public SwitchEventMarker
        (
            int objectCount, float windowLength,
            int trainingTarget = -1
        )
        : base(objectCount, windowLength, trainingTarget)
        {}
    }


    public abstract class VisualEvokedPotentialEventMarker: WindowedEventMarker
    {
        /// <summary>
        /// Flashing frequencies used by stimulus objects
        /// </summary>
        public float[] Frequencies;

        public override string MarkerString
        => $"{base.MarkerString}{FrequenciesString}";

        protected string FrequenciesString
        => Frequencies switch {
            null or {Length: 0} => "",
            _ => $",{string.Join(",", Frequencies)}"
        };

        public VisualEvokedPotentialEventMarker
        (
            int objectCount, float windowLength,
            float[] frequencies,
            int trainingTarget = -1
        ): base(objectCount, windowLength, trainingTarget)
        {
            Frequencies = frequencies;
        }
    }

    /// <summary>
    /// SSVEP event marker in the format:
    /// <br/><br/>
    /// "ssvep,{object count},{train target (-1 if n/a)},{window length},{...frequencies}"
    /// </summary>
    public class SSVEPEventMarker: VisualEvokedPotentialEventMarker
    {
        public override string MarkerString
        => $"ssvep,{base.MarkerString}";

        /// <param name="objectCount">
        /// Number of objects (frequencies) in the trial
        /// </param>
        /// <param name="windowLength">
        /// Length of the processing Epoch <br/>
        /// <b>Must remain constant between trials</b>
        /// </param>
        /// <param name="frequencies">
        /// Collection of flashing frequencies used by stimulus objects
        /// </param>
        /// <param name="trainingTarget">
        /// Index of object (frequency) targetted for training <i>(0-indexed)</i>
        /// </param>
        public SSVEPEventMarker
        (
            int objectCount, float windowLength,
            IEnumerable<float> frequencies,
            int trainingTarget = -1
        )
        : base
        (
            objectCount, windowLength,
            frequencies.ToArray(), trainingTarget
        ) {}
    }

    /// <summary>
    /// TVEP event marker in the format:
    /// <br/><br/>
    /// "ssvep,{object count},{train target (-1 if n/a)},{window length},{...frequencies}"
    /// </summary>
    public class TVEPEventMarker: VisualEvokedPotentialEventMarker
    {
        public override string MarkerString
        => $"tvep,{base.MarkerString}";

        /// <param name="objectCount">
        /// Number of objects (frequencies) in the trial
        /// </param>
        /// <param name="windowLength">
        /// Length of the processing Epoch <br/>
        /// <b>Must remain constant between trials</b>
        /// </param>
        /// <param name="frequencies">
        /// Collection of flashing frequencies used by stimulus objects
        /// </param>
        /// <param name="trainingTarget">
        /// Index of object (frequency) targetted for training <i>(0-indexed)</i>
        /// </param>
        public TVEPEventMarker
        (
            int objectCount, float windowLength,
            IEnumerable<float> frequencies,
            int trainingTarget = -1
        )
        : base
        (
            objectCount, windowLength,
            frequencies.ToArray(), trainingTarget
        ) {}
    }


    /// <summary>
    /// P300 event marker in the format:
    /// <br/><br/>
    /// "p300,[sm],{object count},{train target (-1 if n/a)}..."
    /// <br/>
    /// <i>where 's' or 'm' indicates single or multi flash</i>
    /// </summary>
    public abstract class P300EventMarker: EventMarker
    {
        public P300EventMarker
        (
            int objectCount, int trainingTarget
        ): base(objectCount, trainingTarget)
        {}
    }

    /// <summary>
    /// P300 event marker in the format:
    /// <br/><br/>
    /// "p300,s,{object count},{train target (-1 if n/a)},{active object}"
    /// </summary>
    public class SingleFlashP300EventMarker: P300EventMarker
    {
        public int ActiveObject;

        public override string MarkerString
        => $"p300,s,{base.MarkerString},{ActiveObject + 1}";

        /// <param name="objectCount">Number of objects in the trial</param>
        /// <param name="activeObject">
        /// Index of object being flashed <i>(0-indexed)</i>
        /// </param>
        /// <param name="trainingTarget">
        /// Index of object being targetted for training <i>(0-indexed)</i>
        /// </param>
        public SingleFlashP300EventMarker
        (
            int objectCount, int activeObject,
            int trainingTarget = -1
        ): base(objectCount, trainingTarget)
        {
            ActiveObject = activeObject;
        }
    }

    /// <summary>
    /// P300 event marker in the format:
    /// <br/><br/>
    /// "p300,m,{object count},{train target (-1 if n/a)},{...active objects}"
    /// </summary>
    public class MultiFlashP300EventMarker: P300EventMarker
    {
        public int[] ActiveObjects;

        public override string MarkerString
        => $"p300,m,{base.MarkerString}{ActiveObjectString}";

        protected string ActiveObjectString
        => ActiveObjects switch {
            null or {Length: 0} => "",
            _ => $",{string.Join(',', ActiveObjects.Select(x => ++x))}"
        };

        /// <param name="objectCount">Number of objects in the trial</param>
        /// <param name="activeObjects">
        /// Collection of object indices being flashed together <i>(0-indexed)</i>
        /// </param>
        /// <param name="trainingTarget">
        /// Index of object targetted for training <i>(0-indexed)</i>
        /// </param>
        public MultiFlashP300EventMarker
        (
            int objectCount, IEnumerable<int> activeObjects,
            int trainingTarget = -1
        )
        : base(objectCount, trainingTarget)
        {
            ActiveObjects = activeObjects.ToArray();
        }
    }
}