import pathlib
import pyabf
import matplotlib.pyplot as plt
import numpy as np
from dataclasses import dataclass


@dataclass
class DsiTriplet:
    """
    Holds paths for 3 ABFs composing a DSI set:
        path1 - baseline
        path2 - depolarization
        path3 - recovery
    """
    parent: str
    path1: pathlib.Path
    path2: pathlib.Path
    path3: pathlib.Path


def getDsiTriplets(folder: pathlib.Path) -> dict[str, list[DsiTriplet]]:
    """
    Return a list of DSI triplets for each parent.
    """
    abfPaths = list(folder.glob("*.abf"))
    print(f"reading headers of {len(abfPaths)} ABFs...")

    tripletsByParent = {}

    parent = "orphan"
    for abfPath in abfPaths:
        tifPath = pathlib.Path(str(abfPath)[:-4]+".tif")
        if (tifPath.exists()):
            parent = abfPath.name[:-4]
        abf = pyabf.ABF(abfPath, loadData=False)
        if (abf.protocol.startswith("0612")):

            abfPathPrevious = abfPaths[abfPaths.index(abfPath)-1]
            abf1 = pyabf.ABF(abfPathPrevious, False)
            expectedProtocolPrevious = abf1.protocol.startswith("0611")

            abfPathNext = abfPaths[abfPaths.index(abfPath)+1]
            abf3 = pyabf.ABF(abfPathNext, False)
            expectedProtocolNext = abf3.protocol.startswith("0613")

            if expectedProtocolPrevious and expectedProtocolNext:
                triplet = DsiTriplet(parent,
                                     abfPathPrevious, abfPath, abfPathNext)
                tripletsByParent.setdefault(parent, []).append(triplet)

            if not expectedProtocolPrevious or not expectedProtocolNext:
                print(
                    f"WARNING - DSI ABF with unexpected adjacent ABF: {abfPath}")

    tripletCount = sum([len(v) for (k, v) in tripletsByParent.items()])
    parentCount = len(tripletsByParent.keys())
    print(f"Found {tripletCount} DSI triplets from {parentCount} parents.")

    return tripletsByParent


def plotEvokedTrace(segment: np.ndarray, segmentXs: np.ndarray, xOffset: float, color: str):
    xs = segmentXs + xOffset
    minI = np.nanargmin(segment)
    minY = segment[minI]
    minX = xs[minI]
    plt.plot(xs, segment, '-', color=color)
    plt.plot(minX, minY, 'r.', ms=15, alpha=.5)


def PlotTriplet(triplet: DsiTriplet, xOffset: float):
    """
    Given an ABF triplet, plot the mean of the first ABF
    and the first sweep of the last ABF on top of it.
    Traces will be plotted onto an existing future.
    Apply a horizontal offset (in seconds).
    """

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


def createRepeatedTripletFigure(tripletList: list[DsiTriplet], saveAs: str):
    """
    Generate figures a figure showing each triplet in the given list.
    This figure represents all DSI runs for a single cell.
    """

    plt.figure(figsize=(10, 6))

    for i, triplet in enumerate(tripletList):
        PlotTriplet(triplet, xOffset=.2 * i)

    plt.grid(alpha=.5, ls='--')
    plt.title(f"Parent: {tripletList[0].parent}")
    plt.ylabel("Δ Current (pA)")
    plt.gca().get_xaxis().set_visible(False)
    plt.legend(loc="lower right")
    plt.tight_layout()

    if (saveAs is None):
        plt.show()
        return

    print(saveAs)
    plt.savefig(saveAs)
    plt.close()


def analyzeFolder(abfFolder: pathlib.Path):
    """
    Generate auto-analysis figures for every cell in a folder.
    Each cell has many DSI sets, and a single figure will be made
    for each cell (showing all DSI sets for that cell).
    """
    outputFolder = abfFolder.joinpath("_autoanalysis")

    for parent, tripletList in getDsiTriplets(abfFolder).items():

        print()
        print(parent)
        if (len(tripletList) < 5):
            print(f"Skipping because only {len(tripletList)} triplets")
            continue

        saveAs = outputFolder.joinpath(parent+"_dsi.png")
        createRepeatedTripletFigure(tripletList, saveAs)


if __name__ == "__main__":
    analyzeFolder(pathlib.Path(R"X:/Data/SD/DSI/CA1/DIC-1"))
    analyzeFolder(pathlib.Path(R"X:/Data/SD/DSI/CA1/ABFs"))
    print("DONE")
