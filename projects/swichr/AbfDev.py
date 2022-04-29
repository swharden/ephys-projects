import pyabf
import pathlib
import numpy as np
from dataclasses import dataclass


@dataclass
class Epoch:
    """Values of a single epoch column from an episodic waveform table"""
    epochIndex: int
    firstIndex: int
    lastIndex: int
    startTime: float
    endTime: float
    duration: float
    level: float
    epochType: int
    pulseWidth: float
    pulsePeriod: float
    digitalStates: list[bool]


def createEpochList(abf: pyabf.ABF) -> list[Epoch]:
    """Create an Epoch from the currently-loaded sweep"""

    epochs = []
    epochCount = len(abf.sweepEpochs.p1s)
    for i in range(epochCount):
        p1 = abf.sweepEpochs.p1s[i]
        p2 = abf.sweepEpochs.p2s[i]
        t1 = p1 / abf.sampleRate
        t2 = p2 / abf.sampleRate

        epoch = Epoch(
            epochIndex=i,
            firstIndex=p1,
            lastIndex=p2,
            startTime=t1,
            endTime=t2,
            duration=t2-t1,
            level=abf.sweepEpochs.levels[i],
            epochType=abf.sweepEpochs.types[i],
            pulseWidth=abf.sweepEpochs.pulseWidths[i],
            pulsePeriod=abf.sweepEpochs.pulsePeriods[i],
            digitalStates=abf.sweepEpochs.digitalStates[i])

        epochs.append(epoch)
    return epochs


class Sweep:
    """ADC and DAC data for a single ABF sweep"""

    def __init__(self, abf: pyabf.ABF, sweep: int, channel: int):

        abf.setSweep(sweep, channel)

        self.sweep = sweep
        self.sweepIndex = sweep
        self.sweepNumber = sweep + 1
        self.channel = channel

        self.x = abf.sweepX
        self.x: np.ndarray
        self.xUnits = str(abf.sweepUnitsX)

        self.y = abf.sweepY
        self.y: np.ndarray
        self.yUnits = str(abf.sweepUnitsY)

        self.c = abf.sweepC
        self.c: np.ndarray
        self.cUnits = str(abf.sweepUnitsC)

        self.path = pathlib.Path(abf.abfFilePath)
        self.filename = self.path.name
        self.sampleRate = int(abf.sampleRate)
        self.startTime = float(abf.sweepIntervalSec * sweep)
        self.epochs = createEpochList(abf)

    def __repr__(self):
        return f"{self.path.name} Sweep {self.sweep} (Ch{self.channel})"


class AbfDev(pyabf.ABF):
    """Experimental version of the ABF class used to propose new functionality"""

    def __init__(self, abfFilePath):
        super().__init__(abfFilePath)

    def getSweep(self, sweepIndex: int, channelIndex: int = 0) -> Sweep:
        return Sweep(self, sweepIndex, channelIndex)

    def getSweeps(self, channelIndex: int = 0) -> list[Sweep]:
        return [self.getSweep(sweepIndex, channelIndex) for sweepIndex in range(abf.sweepCount)]
