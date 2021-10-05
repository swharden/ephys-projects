```
dotnet run
```

```
ffmpeg.exe -framerate 5 -y -i "./small-annotated/small%%04d.png" -c:v libx264 -pix_fmt yuv420p "video.mp4"
```