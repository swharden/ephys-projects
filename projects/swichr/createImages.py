import pyabf
import matplotlib
import matplotlib.pyplot as plt
from AbfDev import AbfDev, Sweep, Epoch
import pathlib


def saveSwichrSweepImages(abfPath: pathlib.Path):
    abf = AbfDev(abfPath)
    outputFolder = pathlib.Path("./output")
    if not outputFolder.exists():
        outputFolder.mkdir()
    for sweepIndex in range(abf.sweepCount):
        sw = abf.getSweep(sweepIndex)
        saveAs = pathlib.Path(f"./output/{abf.abfID}-sw{sweepIndex}.png")
        saveSwichrSweepImage(sw, saveAs)


def saveSwichrSweepImage(sweep: Sweep, saveAs: pathlib.Path):

    fig = plt.figure(figsize=(8, 6))
    gs = fig.add_gridspec(2, hspace=0)
    axs = gs.subplots(sharex=True)

    axs[0].plot(sweep.x, sweep.y, color='b')
    axs[1].plot(sweep.x, sweep.c, color='r')
    axs[0].margins(0, .1)

    axs[0].set_ylabel(f"Recording ({sweep.yUnits})")
    axs[1].set_ylabel(f"Command ({sweep.cUnits})")

    colorIndex = 0
    blinkColors = ["blue", "red"]
    for i, epoch in enumerate(sweep.epochs):
        if (sum(epoch.digitalStates) == 0):
            continue
        color = blinkColors[colorIndex]
        colorIndex += 1
        for ax in axs:
            ax.axvspan(epoch.startTime, epoch.endTime,
                       alpha=.2, color=color, lw=0)

    for ax in axs:
        ax.grid(alpha=.5, ls='--')
        ax.label_outer()

    plt.suptitle(f"{sweep.filename} sweep {sweep.sweepNumber}")
    plt.tight_layout()
    plt.savefig(saveAs)
    print(f"Saved: {saveAs}")
    # plt.show()
    plt.close()


def getAbfsWithStimuli(abfFolderPath: str) -> list[pathlib.Path]:
    abfPaths = []
    abfFolderPath = pathlib.Path(abfFolderPath)
    for abfFilePath in abfFolderPath.glob("*.abf"):
        abf = pyabf.ABF(abfFilePath)
        if abf.sweepCount > 5:
            continue  # ignore drug application and sIPSC experiments
        digSumByEpoch = [sum(x) for x in abf.sweepEpochs.digitalStates]
        digitalOutputsAreToggled = len(set(digSumByEpoch)) > 1
        print(f"digital outputs in {abf.abfID}: {digitalOutputsAreToggled}")
        if (digitalOutputsAreToggled):
            abfPaths.append(abfFilePath)
    return abfPaths


if __name__ == "__main__":
    abfFolder = "X:/Data/AT2-Cre/ACC-ChR2-or-CwiChRca/experiments/demonstrate-SwiChRca"
    abfPaths = getAbfsWithStimuli(abfFolder)
    for abfPath in abfPaths:
        saveSwichrSweepImages(abfPath)

    # TODO: ignore these
    # 2022_04_19_0023
    # 2022_04_19_0028