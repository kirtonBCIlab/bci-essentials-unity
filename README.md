# [BCI Essentials Unity](https://docs.bci.games/bessy-unity)
A Unity package for development of BCI applications. This environment needs a BCI Essentials back end *([BCI-essentials-Python](https://github.com/kirtonBCIlab/bci-essentials-python))* to work properly.

## Getting Started
**More information in [the documentation](https://docs.bci.games/bessy-unity/getting-started)**.

### Install into Unity Project
Follow these instructions to install BCI Essentials Unity as a package to an existing project.  For instructions on how to add packages hosted on Github using Unity's Package Manager [click here](https://docs.unity3d.com/Manual/upm-ui-giturl.html)

1. Install [LSL4Unity Package](https://github.com/labstreaminglayer/LSL4Unity) using git URL: `https://github.com/labstreaminglayer/LSL4Unity.git`
2. Install [BCI Essentials package](https://github.com/kirtonBCIlab/bci-essentials-unity) using git URL: `https://github.com/kirtonBCIlab/bci-essentials-unity.git`

*Note - tested with Unity version 6000.3.9f1 and 2021.3.15.f1, some editor versions might not work.*

## Serial Port Trigger Output

BCI Essentials supports sending single-byte trigger codes over a serial port to a hardware trigger box alongside LSL markers. This is useful for synchronizing BCI events with EEG amplifiers that accept triggers via a serial/parallel stim channel.

### Setup

Add a **`SerialMarkerWriter`** component to the **BCIController** GameObject (the same object that has the `BCIController` script). This is a drop-in replacement for `MarkerWriter` that handles both LSL output and serial triggers in a single component. Because it extends `MarkerWriter`, the existing `CommunicationComponentProvider` wiring discovers it automatically. LSL output is completely unchanged; serial triggers fire alongside it.

Configure the serial port in the Inspector:

| Field | Description | Default |
|---|---|---|
| **Port Name** | Serial port, e.g. `COM3` (Windows) or `/dev/ttyUSB0` (Linux/macOS). Use the **Scan Ports** button to list available ports with device names. | `COM3` |

Advanced port settings (Baud Rate, Parity, Data Bits, Stop Bits, Write Timeout, Connect On Awake) are available in a collapsed foldout. Most trigger boxes work with the defaults (9600 baud, 8N1).

Trigger byte settings (status bytes, pulse width, stimulus encoding) are in the **Trigger Settings** foldout. Status bytes default to 240-245 and must match Python's `DEFAULT_TRIGGER_MAP`.

**Verbose Log** logs every marker push (both LSL and serial) to the Console. **Fake Mode** simulates serial output without hardware.

### How It Works

- When a BCI marker is pushed (e.g. Trial Started, P300 flash), the trigger byte is sent over the serial port, held for **Pulse Width Ms** (default 10 ms), then followed by a `0` byte to reset the trigger line. This produces a pulse of known minimum width that the EEG amplifier's stim channel can reliably detect.
- On application exit or when the serial port is disconnected, a `0` byte is sent to ensure the trigger line is reset to baseline.
- LSL marker output is always unaffected.

### Default Byte Mapping

Status markers use the high byte range (240-245) by default to stay well clear of stimulus values:

| Event | Default Byte |
|---|---|
| Trial Started | 240 |
| Trial Ends | 241 |
| Training Complete | 242 |
| Train Classifier | 243 |
| Update Classifier | 244 |
| Done with RS Collection | 245 |
| Stimulus flash | `stimulus_index + 1` (1-indexed, e.g. stimulus 0 -> byte 1) |

All status bytes are configurable on the `SerialMarkerWriter` component but should stay above any stimulus byte values to avoid collisions.

### Per-Stimulus Byte Overrides

Individual stimulus objects can override the default byte for their stimulus index. Add a **`SerialTriggerOverride`** component to any stimulus GameObject and set:

- **Stimulus Index** - the 0-based index this stimulus corresponds to in the marker.
- **Trigger Byte** - the byte to send instead of the default `stimulus_index + 1`.

The override auto-registers with the parent `SerialMarkerWriter` on `Start` and unregisters on `OnDestroy`.

You can also register overrides from code:

```csharp
var writer = GetComponentInParent<SerialMarkerWriter>();
writer.RegisterStimulusOverride(stimulusIndex: 0, triggerByte: 50);
writer.RegisterStimulusOverride(stimulusIndex: 1, triggerByte: 51);
```

To remove a single override: `writer.UnregisterStimulusOverride(stimulusIndex)`.
To clear all overrides: `writer.ClearStimulusOverrides()`.
To replace the entire trigger map: `writer.SetCustomTriggerMap(dictionary)`.

### Python Side

On the Python side, use `LslStimMarkerSource` to read trigger bytes from the EEG amplifier's stim channel and convert them back to marker strings. See the [bci-essentials-python](https://github.com/kirtonBCIlab/bci-essentials-python) documentation for details.

[Contributing](CONTRIBUTING.md)
