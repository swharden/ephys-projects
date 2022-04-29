import pathlib

template = """
<!doctype html>
<html lang="en">
  <head>
    <meta charset="utf-8">
    <meta name="viewport" content="width=device-width, initial-scale=1">
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.1.3/dist/css/bootstrap.min.css" rel="stylesheet">
    <title>SwiChR Sweeps</title>
    <style>
      a {text-decoration: none; color: black;}
      a:hover {text-decoration: underline;}
    </style>
  </head>
  <body>
    <div class="container">
      <h1 class="my-5">SwiChR Sweeps</h1>
      CONTENT
    </div>
    <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.1.3/dist/js/bootstrap.bundle.min.js" integrity="sha384-ka7Sk0Gln4gmtz2MlQnikT1wXgYsOg+OMhuP+IlRH9sENBO0LRn5q+8nbTov4+1p" crossorigin="anonymous"></script>
  </body>
</html>
"""

if __name__ == "__main__":

    outputFolder = pathlib.Path("./output")
    imageFilenames = [x.name for x in outputFolder.glob("*.png")]
    html = ""

    for filename in imageFilenames:
        anchor = filename[:-4]
        filenameAbf = filename.split("-sw")[0]+".abf"
        sweepNumber =  int(filename.split("-sw")[1][:-4]) + 1
        html += f"<h3 class='mt-5' id='{anchor}'><a href='#{anchor}'>{filenameAbf} sweep {sweepNumber}</a></h3>"
        html += f"<a href='{filename}'><img src='{filename}' /></a>"
        print(filename)

    html = template.replace("CONTENT", html)
    pathlib.Path("./output/index.html").write_text(html)
