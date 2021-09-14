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
import abfTools
import statsTools



def getBaselineAndMaxDrugSlope(abfFilePath, filterSize = 15, regressionSize = 15, show = True):
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
    sweepPeriod = abf.sweepLengthSec / 60.0 # minutes
    
    plt.figure(figsize=(8, 6))
    ax1 = plt.subplot(211)
    plt.title(abf.abfID)
    plt.ylabel("Mean Current (pA)")
    
    rawCurrents = abfTools.getMeanBySweep(abf, 3, 10)
    rawTimes = abf.sweepTimesMin
    plt.plot(rawTimes, rawCurrents, 'ko', alpha=.2, fillstyle='none', label="raw data")

    smoothCurrents = statsTools.smoothY(rawCurrents, filterSize)
    smoothTimes = statsTools.smoothY(rawTimes, filterSize)
    plt.plot(smoothTimes, smoothCurrents, '-', color="C1", alpha=.5, label="smoothed data")
    
    # determine drug region based on first tag time
    drugTimeStart = abfTools.getFirstTagTime(abfFilePath)
    drugSearchWidth = 5 # minutes
    drugTimeEnd = drugTimeStart + drugSearchWidth
    plt.axvspan(drugTimeStart, drugTimeEnd, color='r', alpha=.1, lw=0)
    
    # determine baseline region based on drug time
    baselineTimeStart = drugTimeStart - 4
    baselineTimeEnd = drugTimeStart
    baselineIndexStart, baselineIndexEnd = statsTools.rangeIndex(smoothTimes, baselineTimeStart, baselineTimeEnd)
    baselineCurrent = smoothCurrents[baselineIndexStart:baselineIndexEnd]
    plt.axvspan(baselineTimeStart, baselineTimeEnd, color='b', alpha=.1, lw=0)
    
    # isolate smoothed baseline currents
    baselineCurrents = smoothCurrents[baselineIndexStart:baselineIndexEnd]
    baselineTimes = smoothTimes[baselineIndexStart:baselineIndexEnd]
    baselineSlope, baselineIntercept, r, p, stdErr = scipy.stats.linregress(baselineTimes, baselineCurrents)
    
    # calculate linear regression of baseline region
    baselineRegressionXs = np.linspace(baselineTimeStart, baselineTimeEnd)
    baselineRegressionYs = baselineRegressionXs * baselineSlope + baselineIntercept
    plt.plot(baselineRegressionXs, baselineRegressionYs, color='b', ls='--', label="baseline slope")
    plt.grid(alpha=.5, ls='--')
    if show:
        print(f"Baseline slope: {baselineSlope} pA/min")
    
    # perform a moving window linear regression on the smoothed currents
    segments = statsTools.getMovingWindowSegments(smoothCurrents, regressionSize)
    segSlopes = getAllSegmentSlopes(segments, sweepPeriod)   
    segTimesOffset = (regressionSize * sweepPeriod)
    segTimes = np.arange(len(segSlopes)) * sweepPeriod + segTimesOffset    
    plt.subplot(212, sharex = ax1)
    plt.axvspan(baselineTimeStart, baselineTimeEnd, color='b', alpha=.1, lw=0)
    plt.plot(segTimes, segSlopes, 'k.-', lw=.5, ms=2, label="local slope")
    plt.grid(alpha=.5, ls='--')
    
    # search the drug range for the most negative slope
    plt.axvspan(drugTimeStart, drugTimeEnd, color='r', alpha=.1)
    drugSlopeMin = statsTools.rangeMin(segSlopes, segTimes, drugTimeStart, drugTimeEnd)
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
    

def plotExperiment(abfFilePath, drugStartTime, measurementTime, drugMeasurementDelay):
    """
    Plot holding current for a time-course drug experiment.
    drugStartTime indicates the time (in minutes) the drug was added.
    measurementTime is the size (in minutes) of the region to fit a curve to.
    """
    
    # figure out the drug end time and baseline times based on the drug start time
    drugEndTime = drugStartTime + measurementTime
    baselineEndTime = drugStartTime
    baselineStartTime = baselineEndTime - measurementTime

    # shift the drug measurement region to the right by the drug measurement delay
    drugStartTime = drugStartTime + drugMeasurementDelay
    drugEndTime = drugEndTime + drugMeasurementDelay

    segmentMean, t = abfTools.meanIhold(abfFilePath)

    slope1,intercept1 = getRegression(t, segmentMean, baselineStartTime, baselineEndTime)
    slope2,intercept2 = getRegression(t, segmentMean, drugStartTime, drugEndTime)

    plt.figure(figsize=(8, 4))
    plt.axvspan(baselineStartTime, baselineEndTime,color="blue",alpha=0.1)
    fittedXs1 = np.linspace(baselineStartTime, baselineEndTime)
    fittedYs1 = fittedXs1 * slope1 + intercept1
    plt.plot(fittedXs1, fittedYs1, '--', label=f"slope1={slope1:0.2}", color = "blue")

    plt.axvspan(drugStartTime, drugEndTime,color="red",alpha=0.1)
    fittedXs2 = np.linspace(drugStartTime, drugEndTime)
    fittedYs2 = fittedXs2 * slope2 + intercept2
    plt.plot(fittedXs2, fittedYs2, '--', label=f"slope2={slope2:0.2}", color="red")

    plt.plot(t, segmentMean, ".", color="black", alpha=0.5,markersize=8, label="data")
    abfName = os.path.basename(abfFilePath)
    plt.title(abfName, fontsize=20)
    plt.ylabel("Holding Current (pA)", fontsize=12)
    plt.xlabel("Time (minutes)", fontsize=12)
    plt.grid(alpha=.2, ls='--')
    plt.legend()

    #plt.show()
    return round(slope1,3), round(slope2,3)

def consecutiveSlopes(ys, xs):
    """
    Get slopes of consecutive data points.
    """
    slopes = []
    samplePeriod  = xs[1]-xs[0]
    for i in range(len(ys)-1):
        slope = (ys[i+1]-ys[i])/(samplePeriod)
        slopes.append(slope)
    return slopes

def identifyBySlope(abfIDs, slopesDrug, slopesBaseline, threshold):
    """
    Identify a responder by comparing the change of slopes to a given threshold.
    """
    responders=[]
    nonResponders=[]
    for i in range(len(abfIDs)):

        deltaSlope = round(slopesDrug[i]-slopesBaseline[i],3)   # pA / min
        if deltaSlope> threshold:
            nonResponders.append(abfIDs[i])

        else:
            responders.append(abfIDs[i])

    return responders, nonResponders

def identifyByCurrent(abfIDs, slopesDrug, slopesBaseline,threshold):
    """
    Identify a responder by asking whether the change of current is BIGGER than a given threshold.
    """
    responders=[]
    nonResponders=[]
    for i in range(len(abfIDs)):
        deltaCurrent = round(slopesDrug[i]-slopesBaseline[i],3)   # pA / min
        if deltaCurrent > threshold:
            nonResponders.append(abfIDs[i])
        else:
            responders.append(abfIDs[i])

    return responders, nonResponders

if __name__ == "__main__":
    raise Exception("this file must be imported, not run directly")