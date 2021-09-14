"""
Slope tools contains helper functions for measuring slow changes in holding current
using linear regressions. 
"""

import pandas as pd
import matplotlib.pyplot as plt
import scipy.stats
import numpy as np
import os
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


def smoothY(ys, windowSize):
    """
    Get smoothed ys and xs by averaging every n=windowSize sweeps/indexes.
    """
    smoothYs = []
    for i in range(len(ys)-1):
        start = i
        end = i+windowSize+1
        if end > len(ys)-1:
            break
        else:
            smoothY = np.mean(ys[start:end])
            smoothYs.append(smoothY)

    return smoothYs


def rangeIndex(xs, rangeXStart, rangeXEnd):
    """
    Output the indexes of the rangeXStart (the first point that >= rangeXStart) and the rangeXEnd (the last point that <= rangeXStart).
    """

    for i in range(len(xs)):
        if xs[i] <= rangeXStart:
            rangeStartIndex = i
        elif xs[i] > rangeXStart and xs[i] <= rangeXEnd:
            i = i+1
            rangeEndIndex = i
        else:
            break

    return rangeStartIndex, rangeEndIndex


def getMovingWindowSegments(data, windowSize):
    """
    Given a 1D list of data, slide a window along to create individual segments
    and return a list of lists (each of length windowSize)
    """
    segmentCount = len(data) - windowSize
    segments = [None] * segmentCount
    for i in range(segmentCount):
        segments[i] = data[i:i+windowSize]
    return segments


def rangeMin(ys, xs, rangeStart, rangeEnd):
    """
    Calculate the max ys between a given period.
    """

    for i in range(len(xs)):
        if xs[i] < rangeStart or xs[i] == rangeStart:
            rangeStartIndex = i
        elif xs[i] > rangeStart and (xs[i] < rangeEnd or xs[i] == rangeEnd):
            i = i+1
            rangeEndIndex = i
        else:
            break

    rangeMin = np.min(ys[rangeStartIndex:rangeEndIndex])
    return rangeMin


def getBaselineAndMaxDrugSlope(abfFilePath, filterSize=15, regressionSize=15, show=True):
    """
    This method analyzes holding current in an ABF and returns baseline slope and drug slope.

    Arguments:
        filterSize: number of points (sweeps) for the moving window average
        regressionSize: number of points (sweeps) to use to calculate regression slopes during the drug range

    Returns:
        baseline regression slope (over full range)
        peak drug regression slope (regression over defined size)
    """

    abf = pyabf.ABF(abfFilePath)
    sweepPeriod = abf.sweepLengthSec / 60.0  # minutes

    plt.figure(figsize=(8, 6))
    ax1 = plt.subplot(211)
    plt.title(abf.abfID)
    plt.ylabel("Mean Current (pA)")

    rawCurrents = getMeanBySweep(abf, 3, 10)
    rawTimes = abf.sweepTimesMin
    plt.plot(rawTimes, rawCurrents, 'ko', alpha=.2,
             fillstyle='none', label="raw data")

    smoothCurrents = smoothY(rawCurrents, filterSize)
    smoothTimes = smoothY(rawTimes, filterSize)
    plt.plot(smoothTimes, smoothCurrents, '-',
             color="C1", alpha=.5, label="smoothed data")

    # determine drug region based on first tag time
    drugTimeStart = getFirstTagTime(abfFilePath)
    drugSearchWidth = 5  # minutes
    drugTimeEnd = drugTimeStart + drugSearchWidth
    plt.axvspan(drugTimeStart, drugTimeEnd, color='r', alpha=.1, lw=0)

    # determine baseline region based on drug time
    baselineTimeStart = drugTimeStart - 4
    baselineTimeEnd = drugTimeStart
    baselineIndexStart, baselineIndexEnd = rangeIndex(
        smoothTimes, baselineTimeStart, baselineTimeEnd)
    baselineCurrent = smoothCurrents[baselineIndexStart:baselineIndexEnd]
    plt.axvspan(baselineTimeStart, baselineTimeEnd, color='b', alpha=.1, lw=0)

    # isolate smoothed baseline currents
    baselineCurrents = smoothCurrents[baselineIndexStart:baselineIndexEnd]
    baselineTimes = smoothTimes[baselineIndexStart:baselineIndexEnd]
    baselineSlope, baselineIntercept, r, p, stdErr = scipy.stats.linregress(
        baselineTimes, baselineCurrents)

    # calculate linear regression of baseline region
    baselineRegressionXs = np.linspace(baselineTimeStart, baselineTimeEnd)
    baselineRegressionYs = baselineRegressionXs * baselineSlope + baselineIntercept
    plt.plot(baselineRegressionXs, baselineRegressionYs,
             color='b', ls='--', label="baseline slope")
    plt.grid(alpha=.5, ls='--')
    if show:
        print(f"Baseline slope: {baselineSlope} pA/min")

    # perform a moving window linear regression on the smoothed currents
    segments = getMovingWindowSegments(smoothCurrents, regressionSize)
    segSlopes = getAllSegmentSlopes(segments, sweepPeriod)
    segTimesOffset = (regressionSize * sweepPeriod)
    segTimes = np.arange(len(segSlopes)) * sweepPeriod + segTimesOffset
    plt.subplot(212, sharex=ax1)
    plt.axvspan(baselineTimeStart, baselineTimeEnd, color='b', alpha=.1, lw=0)
    plt.plot(segTimes, segSlopes, 'k.-', lw=.5, ms=2, label="local slope")
    plt.grid(alpha=.5, ls='--')

    # search the drug range for the most negative slope
    plt.axvspan(drugTimeStart, drugTimeEnd, color='r', alpha=.1)
    drugSlopeMin = rangeMin(segSlopes, segTimes, drugTimeStart, drugTimeEnd)
    drugSlopeMinIndex = segSlopes.index(drugSlopeMin)
    drugSlopeMinTime = segTimes[drugSlopeMinIndex]
    if show:
        print(f"Drug slope: {drugSlopeMin} pA/min")

    plt.axvline(drugSlopeMinTime, color='r', ls='--')
    plt.axhline(drugSlopeMin, color='r', ls='--', label="peak effect slope")
    plt.axhline(baselineSlope, color='b', ls='--', label="baseline slope")

    plt.ylabel("Slope (pA/min)")
    plt.xlabel("Time (minutes)")
    plt.legend(fontsize=8)
    plt.tight_layout()

    # add some marks to the top plot
    plt.subplot(211)
    plt.axvline(drugSlopeMinTime, color='r', ls='--', label="peak effect")
    plt.legend(fontsize=8)

    if show:
        plt.show()

    return baselineSlope, drugSlopeMin


def getSingleSegmentSlope(segment, samplePeriod):
    """
    Return the slope of a linear line fitted to a single segment.
    Sample period must be in minutes, and returned slope will be pA/min.
    """
    xs = np.arange(len(segment)) * samplePeriod
    slope, intercept, r, p, stdErr = scipy.stats.linregress(xs, segment)
    return slope


def getAllSegmentSlopes(segments, samplePeriod):
    """
    Given a list of segments, return a list of slopes (one per segment).
    Sample period must be in minutes, and returned slopes will be pA/min.
    """
    slopes = []
    for segment in segments:
        slope = getSingleSegmentSlope(segment, samplePeriod)
        slopes.append(slope)
    return slopes


def getRegression(ys, samplePeriod):
    """
    Make linear regression and return the slope and intercept 
    based on the given ys and sample period.
    """

    xs = np.arange(len(ys)) * samplePeriod
    slope, intercept, r, p, stdErr = scipy.stats.linregress(xs, ys)

    return slope, intercept


def consecutiveSlopes(ys, xs):
    """
    Get slopes of consecutive data points.
    """
    slopes = []
    samplePeriod = xs[1]-xs[0]
    for i in range(len(ys)-1):
        slope = (ys[i+1]-ys[i])/(samplePeriod)
        slopes.append(slope)
    return slopes


def identifyBySlope(abfIDs, slopesDrug, slopesBaseline, threshold):
    """
    Identify a responder by comparing the change of slopes to a given threshold.
    """
    responders = []
    nonResponders = []
    for i in range(len(abfIDs)):

        deltaSlope = round(slopesDrug[i]-slopesBaseline[i], 3)   # pA / min
        if deltaSlope > threshold:
            nonResponders.append(abfIDs[i])

        else:
            responders.append(abfIDs[i])

    return responders, nonResponders


def identifyByCurrent(abfIDs, slopesDrug, slopesBaseline, threshold):
    """
    Identify a responder by asking whether the change of current is BIGGER than a given threshold.
    """
    responders = []
    nonResponders = []
    for i in range(len(abfIDs)):
        deltaCurrent = round(slopesDrug[i]-slopesBaseline[i], 3)   # pA / min
        if deltaCurrent > threshold:
            nonResponders.append(abfIDs[i])
        else:
            responders.append(abfIDs[i])

    return responders, nonResponders


if __name__ == "__main__":
    raise Exception("this file must be imported, not run directly")
