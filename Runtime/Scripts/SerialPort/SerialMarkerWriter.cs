using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

using SysSerialPort = System.IO.Ports.SerialPort;
using Parity = System.IO.Ports.Parity;
using StopBits = System.IO.Ports.StopBits;

namespace BCIEssentials.SerialPort
{
    using LSLFramework;

    /// <summary>
    /// Drop-in replacement for <see cref="MarkerWriter"/> that additionally sends
    /// trigger bytes over a serial port for every marker pushed.
    /// Add this single component to the BCIController GameObject to get both
    /// LSL markers and serial triggers.
    /// </summary>
    public class SerialMarkerWriter : MarkerWriter
    {
        [Tooltip("Serial port name (e.g. COM3 on Windows, /dev/ttyUSB0 on Linux)")]
        public string PortName = "COM3";

        [Tooltip("Communication speed in bits per second. Must match your trigger box")]
        public int BaudRate = 9600;

        [Tooltip("Parity bit setting. Most trigger boxes use None")]
        public Parity Parity = Parity.None;

        [Tooltip("Number of data bits per byte. Standard is 8")]
        public int DataBits = 8;

        [Tooltip("Number of stop bits. Standard is One")]
        public StopBits StopBits = StopBits.One;

        [Tooltip("Automatically open the serial port on Awake")]
        public bool ConnectOnAwake = true;

        [Tooltip("Write timeout in milliseconds. 0 for no timeout")]
        public int WriteTimeoutMs = 500;

        [Tooltip("Milliseconds to hold the trigger value before resetting to 0")]
        public int PulseWidthMs = 10;

        [Tooltip("Byte sent when a trial starts. Must match Python DEFAULT_TRIGGER_MAP")]
        public byte TrialStartedByte = 240;

        [Tooltip("Byte sent when a trial ends")]
        public byte TrialEndsByte = 241;

        [Tooltip("Byte sent when training is complete")]
        public byte TrainingCompleteByte = 242;

        [Tooltip("Byte sent to request classifier training")]
        public byte TrainClassifierByte = 243;

        [Tooltip("Byte sent to request classifier update")]
        public byte UpdateClassifierByte = 244;

        [Tooltip("Byte sent when resting state collection is done")]
        public byte DoneWithRSCollectionByte = 245;

        [Tooltip("Encode stimuli as TargetByte/NonTargetByte instead of per-stimulus index values")]
        public bool UseSimpleTargetEncoding = false;

        [Tooltip("Byte value for target stimuli when Simple Target Encoding is on")]
        public byte TargetByte = 1;

        [Tooltip("Byte value for non-target stimuli when Simple Target Encoding is on")]
        public byte NonTargetByte = 2;

        [Tooltip("Consecutive write failures before auto-disconnect. 0 to disable")]
        public int MaxConsecutiveWriteErrors = 5;

        [Tooltip("Log every marker push to the Console, including LSL and serial trigger bytes")]
        public bool VerboseLog = false;

        [Tooltip("Simulate serial output without hardware. Bytes are recorded for testing")]
        public bool FakeMode = false;


        public bool IsConnected =>
            _fakeConnected || (_port != null && _port.IsOpen);

        public int ConsecutiveWriteErrors { get; private set; }
        public int LastFakeByteSent { get; private set; } = -1;
        public int FakeBytesWritten { get; private set; }


        private SysSerialPort _port;
        private readonly byte[] _writeBuffer = new byte[1];
        private bool _fakeConnected;
        private BlockingCollection<byte> _pulseQueue;
        private Thread _writerThread;
        private Dictionary<string, byte> _customTriggerMap;
        private Dictionary<int, byte> _stimulusOverrides;
        private volatile bool _isEnabled = true;


        void Awake()
        {
            _isEnabled = enabled;
            if (ConnectOnAwake)
                Connect();
        }

        void OnEnable() => _isEnabled = true;
        void OnDisable() => _isEnabled = false;

        void OnDestroy() => Disconnect();


        public override void PushMarker(IMarker marker)
        {
            PrintLogs = VerboseLog;
            base.PushMarker(marker);
            SendMarkerTrigger(marker);
        }


        public void Connect()
        {
            if (IsConnected)
            {
                Debug.LogWarning($"SerialMarkerWriter: already connected to {(FakeMode ? "FAKE" : PortName)}");
                return;
            }

            if (FakeMode)
            {
                _fakeConnected = true;
                ConsecutiveWriteErrors = 0;
                FakeBytesWritten = 0;
                LastFakeByteSent = -1;
                Debug.Log("SerialMarkerWriter: connected in FAKE mode.");
                return;
            }

            if (string.IsNullOrWhiteSpace(PortName))
            {
                Debug.LogError("SerialMarkerWriter: PortName is empty, cannot connect.");
                return;
            }

            StartWriterThread();

            try
            {
                _port = new SysSerialPort(PortName, BaudRate, Parity, DataBits, StopBits)
                {
                    DtrEnable = true,
                    RtsEnable = false,
                    WriteTimeout = WriteTimeoutMs > 0 ? WriteTimeoutMs : -1,
                };
                _port.Open();
                ConsecutiveWriteErrors = 0;
                Debug.Log($"SerialMarkerWriter: connected to {PortName} at {BaudRate} baud");
            }
            catch (UnauthorizedAccessException)
            {
                Debug.LogError(
                    $"SerialMarkerWriter: access denied for {PortName}. "
                    + "The port may be in use by another application."
                );
                DisposePort();
            }
            catch (System.IO.IOException ioEx)
            {
                Debug.LogError(
                    $"SerialMarkerWriter: I/O error opening {PortName}. {ioEx.Message}"
                );
                DisposePort();
            }
            catch (ArgumentException argEx)
            {
                Debug.LogError(
                    $"SerialMarkerWriter: invalid port configuration. {argEx.Message}"
                );
                DisposePort();
            }
            catch (Exception ex)
            {
                Debug.LogError(
                    $"SerialMarkerWriter: unexpected error opening {PortName}. "
                    + $"{ex.GetType().Name}: {ex.Message}"
                );
                DisposePort();
            }
        }

        public void Disconnect()
        {
            StopWriterThread();

            if (IsConnected)
                SendByte(0);

            _fakeConnected = false;

            if (_port == null) return;
            try
            {
                if (_port.IsOpen)
                    _port.Close();
            }
            catch (Exception ex)
            {
                Debug.LogWarning(
                    $"SerialMarkerWriter: error while closing {PortName}. {ex.Message}"
                );
            }
            finally
            {
                DisposePort();
            }
        }

        public void Reconnect()
        {
            Disconnect();
            Connect();
        }

        public static string[] GetAvailablePorts()
        {
            try
            {
                return SysSerialPort.GetPortNames();
            }
            catch (Exception ex)
            {
                Debug.LogWarning(
                    $"SerialMarkerWriter: could not enumerate serial ports. {ex.Message}"
                );
                return Array.Empty<string>();
            }
        }

        /// <summary>
        /// Returns available serial ports with device descriptions.
        /// On Windows, queries the system for friendly device names.
        /// On other platforms, falls back to port names only.
        /// </summary>
        public static string[] GetAvailablePortsWithDescriptions()
        {
            string[] ports = GetAvailablePorts();
            if (ports.Length == 0)
                return ports;

            var descriptions = GetPortDeviceNames();
            var result = new string[ports.Length];
            for (int i = 0; i < ports.Length; i++)
            {
                if (descriptions.TryGetValue(ports[i], out string deviceName))
                    result[i] = $"{ports[i]} - {deviceName}";
                else
                    result[i] = ports[i];
            }
            return result;
        }


        /// <summary>
        /// Resolve the marker to a trigger byte and send it as a pulse.
        /// Does nothing when the component is disabled.
        /// </summary>
        /// <param name="marker">
        /// Marker to resolve and send
        /// </param>
        public void SendMarkerTrigger(IMarker marker)
        {
            if (!_isEnabled) return;
            if (marker == null)
            {
                Debug.LogWarning("SerialMarkerWriter: received null marker, ignoring.");
                return;
            }
            byte triggerByte = ResolveTriggerByte(marker);
            if (triggerByte != 0)
            {
                if (VerboseLog)
                    Debug.Log($"SerialMarkerWriter: {marker.GetType().Name} -> byte {triggerByte} (0x{triggerByte:X2})");
                SendPulse(triggerByte);
            }
        }

        /// <summary>
        /// Write a trigger byte, hold for <see cref="PulseWidthMs"/>, then reset to 0.
        /// Enqueued to a background thread in real mode; synchronous in fake mode.
        /// </summary>
        /// <param name="value">Trigger byte to pulse</param>
        public void SendPulse(byte value)
        {
            if (FakeMode)
            {
                SendByte(value);
                SendByte(0);
                return;
            }

            if (_pulseQueue != null && !_pulseQueue.IsAddingCompleted)
            {
                _pulseQueue.Add(value);
            }
            else
            {
                Debug.LogWarning(
                    "SerialMarkerWriter: pulse queue not available, "
                    + "sending synchronously on the calling thread."
                );
                SendByte(value);
                if (PulseWidthMs > 0)
                    Thread.Sleep(PulseWidthMs);
                SendByte(0);
            }
        }

        /// <summary>
        /// Write a single raw byte to the serial port, or record it in fake mode.
        /// Auto-disconnects after <see cref="MaxConsecutiveWriteErrors"/> consecutive failures.
        /// </summary>
        /// <param name="value">Byte to write</param>
        public void SendByte(byte value)
        {
            if (!_isEnabled) return;
            if (!IsConnected)
            {
                Debug.LogWarning("SerialMarkerWriter: cannot send, port not open.");
                return;
            }

            if (FakeMode)
            {
                LastFakeByteSent = value;
                FakeBytesWritten++;
                if (VerboseLog)
                    Debug.Log($"SerialMarkerWriter [FAKE]: wrote byte {value} (0x{value:X2})");
                return;
            }

            try
            {
                _writeBuffer[0] = value;
                _port.Write(_writeBuffer, 0, 1);
                ConsecutiveWriteErrors = 0;
                if (VerboseLog)
                    Debug.Log($"SerialMarkerWriter: wrote byte {value} (0x{value:X2}) to {PortName}");
            }
            catch (TimeoutException)
            {
                HandleWriteError($"write timeout (>{WriteTimeoutMs}ms) sending byte {value}");
            }
            catch (InvalidOperationException)
            {
                HandleWriteError($"port {PortName} was closed unexpectedly");
            }
            catch (Exception ex)
            {
                HandleWriteError($"write error ({ex.GetType().Name}): {ex.Message}");
            }
        }


        /// <summary>
        /// Returns the current trigger mapping as a dictionary.
        /// Uses custom map if set, otherwise builds from Inspector fields.
        /// </summary>
        public Dictionary<string, byte> GetTriggerMap()
        {
            if (_customTriggerMap != null)
                return new Dictionary<string, byte>(_customTriggerMap);

            var map = new Dictionary<string, byte>
            {
                { nameof(TrialStartedMarker),                    TrialStartedByte },
                { nameof(TrialEndsMarker),                       TrialEndsByte },
                { nameof(TrainingCompleteMarker),                 TrainingCompleteByte },
                { nameof(TrainClassifierMarker),                  TrainClassifierByte },
                { nameof(UpdateClassifierMarker),                 UpdateClassifierByte },
                { nameof(DoneWithRestingStateCollectionMarker),   DoneWithRSCollectionByte },
            };

            if (UseSimpleTargetEncoding)
            {
                map["target"] = TargetByte;
                map["non_target"] = NonTargetByte;
            }

            return map;
        }

        /// <summary>
        /// Override the default trigger byte for a specific stimulus index
        /// </summary>
        /// <param name="stimulusIndex">
        /// Index of the stimulus to override <i>(0-indexed)</i>
        /// </param>
        /// <param name="triggerByte">
        /// Byte value to send instead of the default
        /// </param>
        public void RegisterStimulusOverride(int stimulusIndex, byte triggerByte)
        {
            _stimulusOverrides ??= new Dictionary<int, byte>();
            _stimulusOverrides[stimulusIndex] = triggerByte;
        }

        /// <param name="stimulusIndex">
        /// Index of the stimulus override to remove <i>(0-indexed)</i>
        /// </param>
        public void UnregisterStimulusOverride(int stimulusIndex)
        {
            _stimulusOverrides?.Remove(stimulusIndex);
        }

        public void ClearStimulusOverrides()
        {
            _stimulusOverrides?.Clear();
        }

        /// <summary>
        /// Replace the trigger map at runtime.
        /// Pass null to revert to field-based defaults.
        /// </summary>
        /// <param name="map">
        /// Custom mapping of marker type names to trigger bytes
        /// </param>
        public void SetCustomTriggerMap(Dictionary<string, byte> map)
        {
            _customTriggerMap = map != null
                ? new Dictionary<string, byte>(map)
                : null;
            Debug.Log(
                map != null
                    ? $"SerialMarkerWriter: custom trigger map set ({map.Count} entries)"
                    : "SerialMarkerWriter: reverted to field-based trigger map"
            );
        }

        /// <summary>
        /// Map a marker to its trigger byte.
        /// Returns 0 when no trigger should be sent.
        /// </summary>
        /// <param name="marker">
        /// Marker to resolve
        /// </param>
        public byte ResolveTriggerByte(IMarker marker)
        {
            if (_customTriggerMap != null)
            {
                string typeName = marker.GetType().Name;
                if (_customTriggerMap.TryGetValue(typeName, out byte customByte))
                    return customByte;
            }

            switch (marker)
            {
                case TrialStartedMarker _:                  return TrialStartedByte;
                case TrialEndsMarker _:                     return TrialEndsByte;
                case TrainingCompleteMarker _:               return TrainingCompleteByte;
                case TrainClassifierMarker _:                return TrainClassifierByte;
                case UpdateClassifierMarker _:               return UpdateClassifierByte;
                case DoneWithRestingStateCollectionMarker _: return DoneWithRSCollectionByte;
            }

            if (UseSimpleTargetEncoding)
                return ResolveSimpleTargetByte(marker);

            switch (marker)
            {
                case SingleFlashP300EventMarker p300s:
                    return ResolveStimulusByte(p300s.StimulusIndex);
                case MultiFlashP300EventMarker p300m when p300m.StimulusIndices.Length > 0:
                    return ResolveStimulusByte(p300m.StimulusIndices[0]);
                case MIEventMarker mi when mi.TrainingTargetIndex >= 0:
                    return ResolveStimulusByte(mi.TrainingTargetIndex);
                case SSVEPEventMarker ssvep when ssvep.TrainingTargetIndex >= 0:
                    return ResolveStimulusByte(ssvep.TrainingTargetIndex);
                default:
                    if (marker is not EpochEventMarker)
                        Debug.LogWarning(
                            $"SerialMarkerWriter: no byte mapping for {marker.GetType().Name}"
                        );
                    return 0;
            }
        }


        private byte ResolveStimulusByte(int stimulusIndex)
        {
            if (_stimulusOverrides != null &&
                _stimulusOverrides.TryGetValue(stimulusIndex, out byte overrideByte))
                return overrideByte;
            return (byte)(stimulusIndex + 1);
        }

        private byte ResolveSimpleTargetByte(IMarker marker)
        {
            switch (marker)
            {
                case SingleFlashP300EventMarker p300s:
                    return p300s.TrainingTargetIndex >= 0 &&
                           p300s.StimulusIndex == p300s.TrainingTargetIndex
                        ? TargetByte : NonTargetByte;
                case MultiFlashP300EventMarker p300m:
                    return p300m.TrainingTargetIndex >= 0 &&
                           p300m.StimulusIndices.Contains(p300m.TrainingTargetIndex)
                        ? TargetByte : NonTargetByte;
                case MIEventMarker mi:
                    return mi.TrainingTargetIndex >= 0 ? TargetByte : NonTargetByte;
                case SSVEPEventMarker ssvep:
                    return ssvep.TrainingTargetIndex >= 0 ? TargetByte : NonTargetByte;
                default:
                    if (marker is not EpochEventMarker)
                        Debug.LogWarning(
                            $"SerialMarkerWriter: no simple-target mapping for {marker.GetType().Name}"
                        );
                    return 0;
            }
        }

        private void StartWriterThread()
        {
            if (_writerThread != null && _writerThread.IsAlive)
                return;

            _pulseQueue = new BlockingCollection<byte>();
            _writerThread = new Thread(WriterThreadLoop)
            {
                Name = "SerialMarkerWriter",
                IsBackground = true,
            };
            _writerThread.Start();
        }

        private void StopWriterThread()
        {
            _pulseQueue?.CompleteAdding();

            if (_writerThread != null && _writerThread.IsAlive)
            {
                if (!_writerThread.Join(TimeSpan.FromSeconds(2)))
                {
                    Debug.LogWarning(
                        "SerialMarkerWriter: writer thread did not stop in time."
                    );
                }
            }

            _pulseQueue?.Dispose();
            _pulseQueue = null;
            _writerThread = null;
        }

        private void WriterThreadLoop()
        {
            try
            {
                foreach (var value in _pulseQueue.GetConsumingEnumerable())
                {
                    SendByte(value);
                    if (PulseWidthMs > 0)
                        Thread.Sleep(PulseWidthMs);
                    SendByte(0);
                }
            }
            catch (OperationCanceledException) { }
            catch (ObjectDisposedException) { }
        }

        private void HandleWriteError(string message)
        {
            ConsecutiveWriteErrors++;
            if (MaxConsecutiveWriteErrors > 0 && ConsecutiveWriteErrors >= MaxConsecutiveWriteErrors)
            {
                Debug.LogError(
                    $"SerialMarkerWriter: {message}. "
                    + $"{ConsecutiveWriteErrors} consecutive errors, auto-disconnecting."
                );
                Disconnect();
            }
            else
            {
                Debug.LogError($"SerialMarkerWriter: {message} (error {ConsecutiveWriteErrors})");
            }
        }

        private void DisposePort()
        {
            try { _port?.Dispose(); }
            catch { }
            _port = null;
        }

        private static Dictionary<string, string> GetPortDeviceNames()
        {
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            try
            {
                var psi = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "powershell",
                    Arguments = "-NoProfile -Command \"Get-CimInstance Win32_PnPEntity "
                        + "| Where-Object { $_.Name -match 'COM\\d+' } "
                        + "| ForEach-Object { $_.Name }\"",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                };
                using (var proc = System.Diagnostics.Process.Start(psi))
                {
                    string output = proc.StandardOutput.ReadToEnd();
                    proc.WaitForExit(5000);
                    foreach (var line in output.Split('\n'))
                    {
                        var trimmed = line.Trim();
                        if (string.IsNullOrEmpty(trimmed)) continue;
                        var match = System.Text.RegularExpressions.Regex.Match(
                            trimmed, @"\((COM\d+)\)");
                        if (match.Success)
                        {
                            string port = match.Groups[1].Value;
                            string description = trimmed
                                .Replace(match.Value, "")
                                .Trim();
                            result[port] = description;
                        }
                    }
                }
            }
            catch { }
#endif
            return result;
        }
    }
}
