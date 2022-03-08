import pathlib
import pyabf
from triplets import getDsiTriplets
from triplets import DsiTriplet
import matplotlib.pyplot as plt
import numpy as np


def plotEvokedTrace(segment: np.ndarray, segmentXs: np.ndarray, xOffset: float, color: str):
    xs = segmentXs + xOffset
    minI = np.nanargmin(segment)
    minY = segment[minI]
    minX = xs[minI]
    plt.plot(xs, segment, '-', color=color)
    plt.plot(minX, minY, 'r.', ms=15, alpha=.5)


def PlotTriplet(triplet: DsiTriplet, xOffset: float):

    artifactPoint1 = 28936
    artifactPoint2 = 28970

    segmentTime1 = 1.420
    segmentTime2 = 1.550

    abf1 = pyabf.ABF(triplet.path1)
    segmentPoint1 = int(abf1.sampleRate * segmentTime1)
    segmentPoint2 = int(abf1.sampleRate * segmentTime2)
    segmentPointCount = segmentPoint2 - segmentPoint1
    segmentXs = np.arange(segmentPointCount) / abf1.sampleRate + xOffset

    baselineSegments = np.zeros((abf1.sweepCount, segmentPointCount))
    for sweepIndex in range(abf1.sweepCount):
        abf1.setSweep(sweepIndex, baseline=[1.3, 1.4])
        abf1.sweepY[artifactPoint1:artifactPoint2] = np.nan
        baselineSegments[sweepIndex] = abf1.sweepY[segmentPoint1:segmentPoint2]

    baselineMean = np.mean(baselineSegments, axis=0)
    minI = np.nanargmin(baselineMean)
    label = "mean sweep before DSI" if xOffset == 0 else None
    plt.plot(segmentXs, baselineMean, '-',
             color='k', label=label, alpha=.5, lw=2)
    plt.plot(segmentXs[minI], baselineMean[minI],
             '.', ms=20, mfc='none', color='k', mew=2)

    abf3 = pyabf.ABF(triplet.path3)
    abf3.setSweep(0, baseline=[1.3, 1.4])
    abf3.sweepY[artifactPoint1:artifactPoint2] = np.nan
    segment = abf3.sweepY[segmentPoint1:segmentPoint2]
    minI = np.nanargmin(baselineMean)
    label = "first sweep after DSI" if xOffset == 0 else None
    plt.plot(segmentXs, segment, '-', color='k', label=label, alpha=1, lw=1)
    plt.plot(segmentXs[minI], segment[minI], '.', ms=20, color='k')


def analyzeFolder(abfFolder: pathlib.Path):
    """
    Generate auto-analysis figures for every DSI set in a folder.
    """
    outputFolder = abfFolder.joinpath("_autoanalysis")
    for parent, tripletList in getDsiTriplets(abfFolder).items():

        print()
        print(parent)
        if (len(tripletList) < 5):
            print(f"Skipping because only {len(tripletList)} triplets")
            continue

        plt.figure(figsize=(10, 6))

        for i, triplet in enumerate(tripletList):
            PlotTriplet(triplet, xOffset=.2 * i)

        plt.grid(alpha=.5, ls='--')
        plt.title(f"Parent: {parent}")
        plt.ylabel("Δ Current (pA)")
        plt.gca().get_xaxis().set_visible(False)
        plt.legend(loc="lower right")
        plt.tight_layout()

        saveFig = True
        if (saveFig):
            outputPath = outputFolder.joinpath(parent+"_dsi2.png")
            print(outputPath)
            plt.savefig(outputPath)
            plt.close()
        else:
            plt.show()


if __name__ == "__main__":
    analyzeFolder(pathlib.Path(R"X:/Data/SD/DSI/CA1/DIC-1"))
    analyzeFolder(pathlib.Path(R"X:\Data\SD\DSI\CA1\ABFs"))
    print("DONE")
