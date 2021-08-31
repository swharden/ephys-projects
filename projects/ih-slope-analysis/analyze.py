import pathlib
import groups
import slopeTools
import reports
import matplotlib.pyplot as plt

# initial filtering to reduce sweep-to-sweep current variation
filterSize = 10

# number of points to include in each slope calculation (linear regression)
regressionSize = 17


def makeReport(outFolder: pathlib.Path, abfPaths: list[pathlib.Path], title: str):
    report = reports.ReportPage(outFolder, title)
    report.addTitle(title)

    abfIDs = []
    baselineSlopes = []
    drugSlopes = []

    for abfPath in abfPaths:
        print(abfPath)
        baselineSlope, drugSlope = slopeTools.getBaselineAndMaxDrugSlope(
            abfPath, filterSize, regressionSize, show=False)

        baselineSlopes.append(baselineSlope)
        drugSlopes.append(drugSlope)
        abfIDs.append(pathlib.Path(abfPath).name)

        report.addHeading(pathlib.Path(abfPath).name)
        report.addCode(f"path: {abfPath}")
        report.addCode(f"baseline slope: {baselineSlope}")
        report.addCode(f"drug slope: {drugSlope}")
        report.addFigure(plt.gcf())
        plt.close()
        report.addHr()

    addTable(report, abfIDs, baselineSlopes, drugSlopes)

    report.save()


def addTable(report: reports.ReportPage, abfIDs, baselineSlopes, drugSlopes):

    report.addHeading("Table")
    report.addHtml("<table>")
    report.addHtml("<tr>")
    report.addHtml("<th>ABF</th>")
    report.addHtml("<th>baseline slope</th>")
    report.addHtml("<th>drug slope</th>")
    report.addHtml("</tr>")

    for i in range(len(baselineSlopes)):
        report.addHtml("<tr>")
        report.addHtml(f"<td>{abfIDs[i]}</td>")
        report.addHtml(f"<td>{baselineSlopes[i]}</td>")
        report.addHtml(f"<td>{drugSlopes[i]}</td>")
        report.addHtml("</tr>")

    report.addHtml("</table>")


if __name__ == "__main__":
    outFolder = pathlib.Path(__file__).parent.joinpath("output")

    makeReport(outFolder, groups.tgot10nm, "TGOT (10 nM)")
    makeReport(outFolder, groups.tgot10nm_L368, "TGOT (10 nM) +L368")

    makeReport(outFolder, groups.tgot50nm, "TGOT (50 nM) all")
    makeReport(outFolder, groups.tgot50nm_NAc, "TGOT (50 nM) to NAc")
    makeReport(outFolder, groups.tgot50nm_L368, "TGOT (50 nM) +L368")

    makeReport(outFolder, groups.opto, "Opto")
    makeReport(outFolder, groups.opto_L368, "Opto +L368")
