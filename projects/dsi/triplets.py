
import pathlib
import pyabf
from dataclasses import dataclass


@dataclass
class DsiTriplet:
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
