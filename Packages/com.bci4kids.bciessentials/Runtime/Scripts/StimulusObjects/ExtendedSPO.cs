using UnityEngine;

namespace BCIEssentials.StimulusObjects
{
    public class ExtendedSPO : SPO
    {
        // Start is called before the first frame update
        public override float StartStimulus()
        {
            // Make Big
            float scaleValue = 1.4f;
            Vector3 objectScale = transform.localScale;
            transform.localScale = new Vector3(objectScale.x * scaleValue, objectScale.y * scaleValue,
                objectScale.z * scaleValue);

            // Don't touch this
            // Return time since stim
            return Time.time;

        }

        public override void StopStimulus()
        {
            // Make Small
            float scaleValue = 1.4f;
            Vector3 objectScale = transform.localScale;
            transform.localScale = new Vector3(objectScale.x / scaleValue, objectScale.y / scaleValue,
                objectScale.z / scaleValue);

        }

        public override void Select()
        {
            // Blow Up
            Destroy(transform.gameObject);
        }

    }
}
