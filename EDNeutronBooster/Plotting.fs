module Plotting

open XPlot.Plotly
open SystemInfo
open Geometry

let plotPath systems pathFound width height = 

    let getSeries systems f = systems |> Seq.map f |> Seq.toArray
    let getX systems = getSeries systems (fun s -> (double) s.Location.X)
    let getY systems = getSeries systems (fun s -> (double) s.Location.Y)
    let getZ systems = getSeries systems (fun s -> (double) s.Location.Z)
    let getText systems = getSeries systems (fun s -> s.Name)

    let traceAllNeutronStars =
        Scatter3d(
            x = getX systems,
            y = getY systems,
            z = getZ systems,
            text = getText systems,
            mode = "markers",
            marker =
                Marker(
                    size = 2.,
                    symbol = "circle",
                    line =
                        Line(
                            color = "rgba(217, 217, 217, 0.14)",
                            width = 0.5
                        ),
                    opacity = 0.8
                )
       )

    let rawPath = pathFound |> Seq.map (fun (a,_) -> a)

    let tracePath =
        Scatter3d(
            x = getX rawPath,
            y = getY rawPath,
            z = getZ rawPath,
            text = getText rawPath,
            mode = "lines",
            line =
                Line(
                    color = "#FF0000",
                    width = 2.
                )
        )

    [traceAllNeutronStars; tracePath ]
        |> Chart.Plot
        |> Chart.WithWidth width
        |> Chart.WithHeight height
        |> Chart.WithLegend(true)
        |> Chart.WithLabels(["Neutron stars"; "Path"])
        |> Chart.Show
