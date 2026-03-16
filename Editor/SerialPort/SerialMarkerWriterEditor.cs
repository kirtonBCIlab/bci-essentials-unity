using UnityEditor;
using UnityEngine;

namespace BCIEssentials.SerialPort.Editor
{
    [CustomEditor(typeof(SerialMarkerWriter))]
    public class SerialMarkerWriterEditor : UnityEditor.Editor
    {
        private SerializedProperty _portName;
        private SerializedProperty _baudRate;
        private SerializedProperty _parity;
        private SerializedProperty _dataBits;
        private SerializedProperty _stopBits;
        private SerializedProperty _connectOnAwake;
        private SerializedProperty _writeTimeoutMs;

        private SerializedProperty _pulseWidthMs;
        private SerializedProperty _trialStartedByte;
        private SerializedProperty _trialEndsByte;
        private SerializedProperty _trainingCompleteByte;
        private SerializedProperty _trainClassifierByte;
        private SerializedProperty _updateClassifierByte;
        private SerializedProperty _doneWithRSCollectionByte;
        private SerializedProperty _useSimpleTargetEncoding;
        private SerializedProperty _targetByte;
        private SerializedProperty _nonTargetByte;
        private SerializedProperty _maxConsecutiveWriteErrors;

        private SerializedProperty _verboseLog;
        private SerializedProperty _fakeMode;

        private SerializedProperty _streamName;
        private SerializedProperty _streamType;
        private SerializedProperty _enableStream;

        private bool _advancedPortFoldout;
        private bool _triggersFoldout;
        private bool _statusBytesFoldout;
        private bool _stimulusEncodingFoldout;
        private bool _debugFoldout;
        private bool _lslFoldout;

        private string[] _cachedPortDescriptions;


        private void OnEnable()
        {
            _portName = serializedObject.FindProperty("PortName");
            _baudRate = serializedObject.FindProperty("BaudRate");
            _parity = serializedObject.FindProperty("Parity");
            _dataBits = serializedObject.FindProperty("DataBits");
            _stopBits = serializedObject.FindProperty("StopBits");
            _connectOnAwake = serializedObject.FindProperty("ConnectOnAwake");
            _writeTimeoutMs = serializedObject.FindProperty("WriteTimeoutMs");

            _pulseWidthMs = serializedObject.FindProperty("PulseWidthMs");
            _trialStartedByte = serializedObject.FindProperty("TrialStartedByte");
            _trialEndsByte = serializedObject.FindProperty("TrialEndsByte");
            _trainingCompleteByte = serializedObject.FindProperty("TrainingCompleteByte");
            _trainClassifierByte = serializedObject.FindProperty("TrainClassifierByte");
            _updateClassifierByte = serializedObject.FindProperty("UpdateClassifierByte");
            _doneWithRSCollectionByte = serializedObject.FindProperty("DoneWithRSCollectionByte");
            _useSimpleTargetEncoding = serializedObject.FindProperty("UseSimpleTargetEncoding");
            _targetByte = serializedObject.FindProperty("TargetByte");
            _nonTargetByte = serializedObject.FindProperty("NonTargetByte");
            _maxConsecutiveWriteErrors = serializedObject.FindProperty("MaxConsecutiveWriteErrors");

            _verboseLog = serializedObject.FindProperty("VerboseLog");
            _fakeMode = serializedObject.FindProperty("FakeMode");

            _enableStream = serializedObject.FindProperty("EnableStream");
            _streamName = serializedObject.FindProperty("StreamName");
            _streamType = serializedObject.FindProperty("StreamType");

            string prefsKey = $"{target.name}/SerialMarkerWriter";
            _advancedPortFoldout = EditorPrefs.GetBool($"{prefsKey}/AdvancedPort", false);
            _triggersFoldout = EditorPrefs.GetBool($"{prefsKey}/Triggers", false);
            _statusBytesFoldout = EditorPrefs.GetBool($"{prefsKey}/StatusBytes", false);
            _stimulusEncodingFoldout = EditorPrefs.GetBool($"{prefsKey}/StimulusEncoding", false);
            _debugFoldout = EditorPrefs.GetBool($"{prefsKey}/Debug", false);
            _lslFoldout = EditorPrefs.GetBool($"{prefsKey}/LSL", false);
        }


        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawSerialPortSection();
            DrawTriggersSection();
            DrawDebugSection();
            DrawLslSection();

            serializedObject.ApplyModifiedProperties();
        }


        private void DrawSerialPortSection()
        {
            DrawHeader("Serial Port");
            DrawNotice(
                "Configure the serial connection to your trigger box. "
                + "Only Port Name is required."
            );

            EditorGUILayout.PropertyField(_portName);

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Scan Ports", GUILayout.Width(90)))
            {
                _cachedPortDescriptions = SerialMarkerWriter.GetAvailablePortsWithDescriptions();
                if (_cachedPortDescriptions.Length == 0)
                    Debug.Log("SerialMarkerWriter: no serial ports found.");
                else
                    Debug.Log(
                        "SerialMarkerWriter: available ports\n  "
                        + string.Join("\n  ", _cachedPortDescriptions)
                    );
            }
            EditorGUILayout.EndHorizontal();

            if (_cachedPortDescriptions != null && _cachedPortDescriptions.Length > 0)
            {
                EditorGUILayout.HelpBox(
                    "Available ports:\n" + string.Join("\n", _cachedPortDescriptions),
                    MessageType.Info
                );
            }

            _advancedPortFoldout = DrawFoldout(
                _advancedPortFoldout, "Advanced Port Settings",
                $"{target.name}/SerialMarkerWriter/AdvancedPort"
            );
            if (_advancedPortFoldout)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(_baudRate);
                EditorGUILayout.PropertyField(_parity);
                EditorGUILayout.PropertyField(_dataBits);
                EditorGUILayout.PropertyField(_stopBits);
                EditorGUILayout.PropertyField(_writeTimeoutMs);
                EditorGUILayout.PropertyField(_connectOnAwake);
                EditorGUI.indentLevel--;
            }

            DrawSpace(4);
        }

        private void DrawTriggersSection()
        {
            _triggersFoldout = DrawFoldoutHeader(
                _triggersFoldout, "Trigger Settings",
                $"{target.name}/SerialMarkerWriter/Triggers"
            );
            if (!_triggersFoldout) return;

            EditorGUI.indentLevel++;
            DrawNotice(
                "Each BCI event is encoded as a single byte sent to the trigger box. "
                + "These values must match the trigger map on the Python side (bci_essentials.triggers)."
            );
            EditorGUILayout.PropertyField(_pulseWidthMs);

            _statusBytesFoldout = DrawFoldout(
                _statusBytesFoldout, "Status Bytes",
                $"{target.name}/SerialMarkerWriter/StatusBytes"
            );
            if (_statusBytesFoldout)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.HelpBox(
                    "Status bytes correspond to Python's DEFAULT_TRIGGER_MAP in bci_essentials/triggers.py. "
                    + "The defaults (240-245) are in the high byte range to avoid collision with stimulus bytes (1-N).",
                    MessageType.None
                );
                EditorGUILayout.PropertyField(_trialStartedByte);
                EditorGUILayout.PropertyField(_trialEndsByte);
                EditorGUILayout.PropertyField(_trainingCompleteByte);
                EditorGUILayout.PropertyField(_trainClassifierByte);
                EditorGUILayout.PropertyField(_updateClassifierByte);
                EditorGUILayout.PropertyField(_doneWithRSCollectionByte);
                EditorGUI.indentLevel--;
            }

            _stimulusEncodingFoldout = DrawFoldout(
                _stimulusEncodingFoldout, "Stimulus Encoding",
                $"{target.name}/SerialMarkerWriter/StimulusEncoding"
            );
            if (_stimulusEncodingFoldout)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.HelpBox(
                    "Default encoding sends stimulus index + 1 as the trigger byte. "
                    + "Python maps these back via make_p300_trigger_map() or make_mi_trigger_map().\n\n"
                    + "Simple Target Encoding sends only two byte values (target/non-target). "
                    + "Use this when Python only needs to distinguish target vs non-target.",
                    MessageType.None
                );
                EditorGUILayout.PropertyField(_useSimpleTargetEncoding);
                if (_useSimpleTargetEncoding.boolValue)
                {
                    EditorGUILayout.PropertyField(_targetByte);
                    EditorGUILayout.PropertyField(_nonTargetByte);
                }
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.PropertyField(_maxConsecutiveWriteErrors);
            EditorGUI.indentLevel--;

            DrawSpace(4);
        }

        private void DrawDebugSection()
        {
            _debugFoldout = DrawFoldoutHeader(
                _debugFoldout, "Debug",
                $"{target.name}/SerialMarkerWriter/Debug"
            );
            if (!_debugFoldout) return;

            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(_verboseLog);
            EditorGUILayout.PropertyField(_fakeMode);

            if (Application.isPlaying && _fakeMode.boolValue)
            {
                var writer = (SerialMarkerWriter)target;
                DrawSpace(4);
                EditorGUILayout.LabelField("Fake Mode Runtime State", EditorStyles.boldLabel);
                EditorGUI.indentLevel++;
                EditorGUILayout.LabelField("Connected", writer.IsConnected.ToString());
                EditorGUILayout.LabelField("Last Fake Byte Sent", writer.LastFakeByteSent.ToString());
                EditorGUILayout.LabelField("Fake Bytes Written", writer.FakeBytesWritten.ToString());
                EditorGUI.indentLevel--;
            }
            EditorGUI.indentLevel--;

            DrawSpace(4);
        }

        private void DrawLslSection()
        {
            _lslFoldout = DrawFoldoutHeader(
                _lslFoldout, "LSL Output Settings",
                $"{target.name}/SerialMarkerWriter/LSL"
            );
            if (!_lslFoldout) return;

            EditorGUI.indentLevel++;
            DrawNotice(
                "LSL marker output. When enabled, marker strings are pushed "
                + "to an LSL outlet alongside serial triggers."
            );
            EditorGUILayout.PropertyField(_enableStream);
            if (_enableStream.boolValue)
            {
                EditorGUILayout.PropertyField(_streamName);
                EditorGUILayout.PropertyField(_streamType);
            }
            EditorGUI.indentLevel--;

            DrawSpace(4);
        }


        private void DrawHeader
        (
            string headerText,
            int fontSize = 13,
            int topMargin = 6
        )
        {
            GUIStyle headerStyle = new(EditorStyles.boldLabel)
            {
                fontSize = fontSize,
                margin = new(0, 0, topMargin, 0)
            };
            GUILayout.Label(headerText, headerStyle);
        }

        private void DrawNotice(string label)
        {
            GUIStyle noticeStyle = new(EditorStyles.wordWrappedMiniLabel)
            {
                padding = new(4, 4, 0, 4)
            };
            EditorGUILayout.LabelField(label, noticeStyle);
        }

        private void DrawSpace(float pixels)
        {
            if (pixels > 0) GUILayout.Space(pixels);
        }

        private bool DrawFoldoutHeader(bool state, string label, string prefsKey)
        {
            DrawSpace(8);
            GUIStyle style = new(EditorStyles.foldoutHeader) { fontSize = 13 };
            bool newState = EditorGUILayout.BeginFoldoutHeaderGroup(state, label, style);
            EditorGUILayout.EndFoldoutHeaderGroup();
            if (newState != state)
                EditorPrefs.SetBool(prefsKey, newState);
            return newState;
        }

        private bool DrawFoldout(bool state, string label, string prefsKey)
        {
            bool newState = EditorGUILayout.Foldout(state, label, true);
            if (newState != state)
                EditorPrefs.SetBool(prefsKey, newState);
            return newState;
        }
    }
}
