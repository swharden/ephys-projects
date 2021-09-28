"""
This script starts with a folder of BMP files generated with ImageJ.
It calculates a mean baseline image and uses that to create dF/F images.
Images are then plotted, annotated, saved in another folder, and encoded as a video.
"""

from os import path
import pathlib
import numpy as np
import matplotlib.pyplot as plt
import cv2


def makeFigures(inputFolder: pathlib.Path, outputFolder: pathlib.Path,
                baselineFrame1: int, baselineFrame2: int, secPerFrame: float):
    print("GENERATING FIGURES")
    imagePaths = sorted(inputFolder.glob("*.bmp"))
    imageStack = np.dstack([plt.imread(x) for x in imagePaths])
    imageStackBaseline = imageStack[:, :, baselineFrame1:baselineFrame2]
    imageBaseline = np.mean(imageStackBaseline, axis=2)

    dffLimit = 100
    for i in range(len(imagePaths)):
        thisImage = imageStack[:, :, i]
        dFF = (thisImage / imageBaseline) - 1
        title = f"{imagePaths[i].name} ({secPerFrame * i / 60.0 :0.02f} min)"
        subtitle = "10 µM norepinephrine" if i > 15 else "baseline"
        plt.title(f"{title}\n{subtitle}")
        plt.imshow(dFF * 100, cmap=plt.cm.bwr, vmin=-dffLimit, vmax=dffLimit)
        plt.colorbar(label="ΔF/F (%)")
        saveFile = outputFolder.joinpath(imagePaths[i].name+".png")
        print(f"Saving: {saveFile.name}")
        plt.savefig(saveFile)
        plt.close()


def makeVideo(imageFolder: pathlib.Path, fps: float = 5):
    print("Encoding video...")
    imagePaths = [x for x in imageFolder.glob("*.png")]
    outputFile = str(imageFolder.joinpath("../video.mp4"))

    firstFrame = cv2.imread(str(imagePaths[0]))
    height, width, layers = firstFrame.shape

    video = cv2.VideoWriter(outputFile, 0, fps, (width, height))
    for imagePath in imagePaths:
        image = cv2.imread(str(imagePath))
        video.write(image)

    cv2.destroyAllWindows()
    video.release()


if __name__ == "__main__":
    inputFolder = pathlib.Path(
        R"X:\Data\C57\GRABNE\2021-09-23-ne-washon\TSeries-09232021-1216-1850-ne-washon\Analysis\01-raw-bmp")
    outputFolder = pathlib.Path(
        R"X:\Data\C57\GRABNE\2021-09-23-ne-washon\TSeries-09232021-1216-1850-ne-washon\Analysis\02-annotated")

    makeFigures(inputFolder, outputFolder, 5, 15, 22.874986)
    makeVideo(outputFolder)
