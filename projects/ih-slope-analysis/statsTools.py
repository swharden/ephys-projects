import numpy as np
import matplotlib.pyplot as plt
import scipy.stats
import pandas as pd

def pairedTTest(sample1, sample2):
    """
    Run the paired t-test and return the p value
    """
    tStatistic, pValue = scipy.stats.ttest_rel(sample1, sample2)
    # NOTE: for unpaired, two-sample t-test use ttest_ind()
    return pValue


def descriptiveStats(sample):
    """
    Report the mean, standard deviation, standard error
    """
    mean = np.mean(sample)
    stDev = np.std(sample)
    stdErr = stDev/np.sqrt(len(sample))
    return mean, stDev, stdErr


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


def rangeMean(ys, xs, rangeStart, rangeEnd):
    """
    Calculate the mean ys between a given period.
    """

    for i in range(len(xs)):
        if xs[i] < rangeStart or xs[i] == rangeStart:
            rangeStartIndex = i
        elif xs[i] > rangeStart and (xs[i] < rangeEnd or xs[i] == rangeEnd):
            i = i+1
            rangeEndIndex = i
        else:
            break

    rangeMean = np.mean(ys[rangeStartIndex:rangeEndIndex])
    return rangeMean

def rangeMax(ys, xs, rangeStart, rangeEnd):
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

    rangeMax = np.max(ys[rangeStartIndex:rangeEndIndex])
    return rangeMax

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




def rangeStats(data, start, end, outputType, interDataInterval, tagTime):
    """
    Calculate the mean, max, or min during a selected time persegSlopesiod (min).
    """
    raise Exception("This function is no longer used.")

    indexStart = int((start+(tagTime-5))/interDataInterval)
    indexEnd = int((end+(tagTime-5))/interDataInterval)
    data = data[indexStart:indexEnd]
    if outputType == "mean":
        output = sum(data)/len(data)
    elif outputType == "max":
        output = max(data)
    elif outputType == "min":
        output = min(data)

    else:
        print("select outputType from mean, max, min")

    return output


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

def responderLessThanThreshold(cellNames, drugEffects, threshold):
    """
    Identify responders when the value of drug effects are less than the value of the given threshold. 
    This function will report the abfIDs of responders and non-responders, and calculate the response rate.
    """
    response=[]
    
    for i in range(len(cellNames)):
        responderCriteria = drugEffects[i] <= threshold
        if responderCriteria:
            response.append("responder")
        else:
            response.append("non-responder")
    df = pd.DataFrame(columns = ["ABF#", "Drug Effect", "Response"])
    df["ABF#"] = cellNames
    df["Drug Effect"] = drugEffects
    df["Response"] = response
    
    responseRate = response.count("responder")*100/len(response)
    responderNumber = response.count("responder")
    nonResponderNumber = response.count("non-responder")
    responseDf = pd.DataFrame([responderNumber, nonResponderNumber], index = ["Responder n", "Non-responder n"])
    display(responseDf)
    display(df)

def responderByDelta(cellNames, drugEffects, threshold):
    """
    Identify responders by evaluating the absolute value of difference between drug and baseline surpasses the given threshold in absolute value. 
    This function will report the abfIDs of responders and non-responders, and calculate the response rate.
    """
    responders=[]
    nonResponders=[]
    for i in range(len(cellNames)):
        responderCriteria = drugEffects[i] >= threshold
        if responderCriteria:
            responders.append(cellNames[i])
        else:
            nonResponders.append(cellNames[i])

    responseRate = round(len(responders)/len(cellNames)*100, 3)
    print(f"Responders = {len(responders)}")
    print(f"Non-Responders = {len(nonResponders)}")
    print(f"Response rate = {responseRate}%")
    print("Responders: " + ", ".join(responders))
    print("Non-Responders: " + ", ".join(nonResponders))
    return responseRate


if __name__ == "__main__":
    raise Exception("this file must be imported, not run directly")