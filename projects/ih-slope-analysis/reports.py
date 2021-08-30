import matplotlib.pyplot as plt
import matplotlib.figure
import pathlib


class ReportPage:
    """
    This class builds a report that can be saved as a HTML file.
    """

    def __init__(self, outFolder: pathlib.Path, title: str):
        self.title = title
        self.titleSafe = self._safeName(title)
        self.imageCount = 0
        self.body = []

        self.outFolder = outFolder
        self.imageFolder = self.outFolder.joinpath("images")
        self._makeFolders([self.outFolder, self.imageFolder])

    def addHtml(self, html: str):
        self.body.append(html)

    def addHr(self):
        self.body.append("<hr>")

    def _makeFolders(self, folders: list[pathlib.Path]):
        for folder in folders:
            if not folder.exists():
                folder.mkdir()

    def addCode(self, code: str):
        self.body.append(f"<div><code>{code}</code></div>")

    def addHeading(self, text: str):
        self.body.append(f"<h1>{text}</h1>")

    def addTitle(self, text: str):
        self.body.append(
            f"<h1 style='text-align: center; font-size: 300%;'>{text}</h1><hr>")

    def addFigure(self, fig: matplotlib.figure.Figure):
        self.imageCount += 1
        saveFileName = f"{self.titleSafe}_{self.imageCount}.png"
        saveFilePath = self.imageFolder.joinpath(saveFileName)
        fig.savefig(saveFilePath)
        self.body.append(
            f"<div><img src='{self.imageFolder.name}/{saveFileName}'></div>")

    def _safeName(self, name: str) -> str:
        """convert a string into a filename-safe string"""
        chars = list(name)
        for i, c in enumerate(chars):
            if (c.isalpha()):
                chars[i] = c.lower()
            elif (c.isnumeric()):
                chars[i] = c
            else:
                chars[i] = "_"
        name = "".join(chars)
        while "__" in name:
            name = name.replace("__", "_")
        name = name.strip("_")
        return name

    def save(self):
        filePath = self.outFolder.joinpath(self.titleSafe+".html")
        with open(filePath, 'w') as f:
            f.write("<html>")
            f.write("<head>")
            f.write("<link rel='stylesheet' href='../style.css'>")
            f.write("</head>")
            f.write("\n".join(self.body))
            f.write("</html>")
        print(f"wrote: {filePath}")
