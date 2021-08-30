"""
This module contains methods for working with ABF files
"""

import numpy as np
import pyabf


def getFirstTagTime(abfFilePath):
    """
    Return the time (in minutes) of the first tag in an ABF file
    """
    abf = pyabf.ABF(abfFilePath, False)
    if (len(abf.tagTimesMin) > 0):
        return abf.tagTimesMin[0]
    else:
        raise Exception(
            "cannot get the first tag time because this ABF does not have any tags")

def getMeanBySweep(abf, markerTime1, markerTime2):
    """
    Return the mean value between the markers for every sweep.
    """
    assert isinstance(abf, pyabf.ABF)
    
    pointsPerSecond = abf.dataRate
    sweepIndex1 = pointsPerSecond * markerTime1
    sweepIndex2 = pointsPerSecond * markerTime2

    means = []
    for i in range(abf.sweepCount):
        abf.setSweep(i)
        segment = abf.sweepY[sweepIndex1:sweepIndex2]
        segmentMean = np.mean(segment)
        means.append(segmentMean)

    return means


if __name__ == "__main__":
    raise Exception("this file must be imported, not run directly")