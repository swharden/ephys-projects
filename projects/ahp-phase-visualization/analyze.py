from datetime import time
import pyabf
import pyabf.tools.ap
import numpy as np
import matplotlib.pyplot as plt
import pathlib

PATH_HERE = pathlib.Path(__file__).parent

ABF_PATHS = PATH_HERE.joinpath("abfs.txt").read_text().split("\n")
ABF_PATHS = [x for x in ABF_PATHS if ".abf" in x]

PATH_OUT = PATH_HERE.joinpath("output")
if not PATH_OUT.exists():
    PATH_OUT.mkdir()
for oldFile in PATH_OUT.glob("*.*"):
    oldFile.unlink()


def getRampFirstAP(abf: pyabf.ABF,
                   sweepIndex: int,
                   time1: float = 6,
                   time2: float = 11,
                   padMillisec: float = 250) -> np.ndarray:

    abf.setSweep(sweepIndex)
    indexes = pyabf.tools.ap.ap_points_currentSweep(abf, dVthresholdPos=10)

    # isolate indexes in ramp
    rampIndexes = [x for x in indexes
                   if x > time1 * abf.dataRate
                   and time2 * abf.dataRate]
    firstRampApIndex = rampIndexes[0]
    padPoints = int(padMillisec * abf.dataRate / 1000)
    trace = abf.sweepY[firstRampApIndex-padPoints:firstRampApIndex+padPoints]
    return trace


def showTrace(trace: np.ndarray, sampleRate: float, axs, lineWidth: float, lineColor: str, label: str):

    # generate Xs centered at the AP
    xs = np.arange(len(trace)) / sampleRate * 1000
    xs -= xs[-1]/2

    # determine amount of padding for zoomed-in view
    zoomInPad = xs[-1]/20
    zoomInPadPoints = int(len(trace)/20)
    centerI = int(len(trace)/2)
    zoomInI1 = centerI - zoomInPadPoints
    zoomInI2 = centerI + zoomInPadPoints

    # calculate first derivative
    dY = np.diff(trace)
    dY = np.append(dY, dY[-1])

    # show the full length of the trace
    ax0: plt.Axes = axs[0]
    ax0.plot(xs, trace, lw=lineWidth, color=lineColor, label=label)
    ax0.set_title("First AP in Ramp")
    ax0.grid(alpha=.5, ls='--')
    ax0.margins(x=0)
    ax0.legend(loc='upper left')
    ax0.set_ylabel("Voltage (mV)")
    ax0.set_xlabel("Time (ms)")

    # zoom in to the center 10%
    ax1: plt.Axes = axs[1]
    ax1.plot(xs, trace, lw=lineWidth, color=lineColor)
    ax1.set_title("Fast Component")
    ax1.axis([-zoomInPad, +zoomInPad, None, None])
    ax1.grid(alpha=.5, ls='--')
    ax1.margins(x=0)
    ax1.set_ylabel("Voltage (mV)")
    ax1.set_xlabel("Time (ms)")

    # first derivative
    ax2: plt.Axes = axs[2]
    ax2.set_title("Derivative")
    ax2.plot(xs, dY, lw=lineWidth, color=lineColor)
    ax2.axis([-zoomInPad, +zoomInPad, None, None])
    ax2.grid(alpha=.5, ls='--')
    ax2.margins(x=0)
    ax2.set_ylabel("ΔV (mV/ms)")
    ax2.set_xlabel("Time (ms)")

    # phase plot
    ax3: plt.Axes = axs[3]
    ax3.set_title("Phase Plot")
    ax3.plot(trace[zoomInI1:zoomInI2], dY[zoomInI1:zoomInI2],
             lw=lineWidth, color=lineColor)
    ax3.grid(alpha=.5, ls='--')
    ax3.set_ylabel("ΔV (mV/ms)")
    ax3.set_xlabel("Voltage (mV)")


def makeIndex(folderPath: pathlib.Path):
    with open(PATH_OUT.joinpath("index.html"), 'w') as f:
        f.write("<html><body>")
        for image in folderPath.glob("*.png"):
            print(image)
            f.write(
                f"<div style='margin-top: 5em;'><a href='{image.name}'><img src='{image.name}'></a></div>")
        f.write("</body></html>")

if __name__ == "__main__":

    for abfPath in ABF_PATHS:
        abf = pyabf.ABF(abfPath)
        tagSweep = int(abf.tagSweeps[0])
        print(f"\n{abf.abfID}")
        print(f"{abf.tagComments[0]} @ sweep {tagSweep}")
        baselineSweep = tagSweep - 5
        baselineTimeMin = baselineSweep * abf.sweepLengthSec / 60
        drugSweep = tagSweep + 30
        drugTimeMin = drugSweep * abf.sweepLengthSec / 60
        print(f"baseline @ {baselineTimeMin} min, drug @ {drugTimeMin} min")

        fig, axs = plt.subplots(nrows=1, ncols=4, figsize=(15, 4))
        fig.suptitle(abf.abfID)

        trace = getRampFirstAP(abf, baselineSweep)
        showTrace(trace, abf.dataRate, axs, 3, '#8cb9e6', "control")

        trace = getRampFirstAP(abf, drugSweep)
        showTrace(trace, abf.dataRate, axs, 1, 'k', "TGOT")

        plt.tight_layout()

        saveFilePath = PATH_OUT.joinpath(abf.abfID + ".png")
        plt.savefig(saveFilePath)
        plt.close()
        print(saveFilePath)

    makeIndex(PATH_OUT)
