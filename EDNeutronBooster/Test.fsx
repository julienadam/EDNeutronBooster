#load @"../paket-files/include-scripts/net45/include.main.group.fsx"
#r @"bin/debug/EDNeutronBooster.exe"

open System.Collections.Generic
open System.IO
open SystemInfo
open Geometry

let sw = System.Diagnostics.Stopwatch.StartNew()
let allSystems = NeutronStarDatabase.getOrBuildNeutronStarDatabase @"c:\users\nidhogg\Desktop\bodies.jsonl" @"c:\users\nidhogg\Desktop\systems.csv"
printfn "Built or read the database in %s" (sw.Elapsed.ToString())

let unboostedRange = 57.0m
let maxDistanceFromMainStar = 100m
let sol = { Location = { X = 0m; Y = 0m; Z = 0m }; Id = -11; Name = "Sol"; DistanceFromMainStar = 0m }
let CG = { Location = { X = -5805.78125m; Y = 129.875m; Z = -5969.875m }; Id = -22; Name = "CG location"; DistanceFromMainStar = 0m }

// Calculate the path
sw.Restart()
let (systems, pathFound) = PathFinding.filterSystemsAndCalculatePath sol CG unboostedRange allSystems maxDistanceFromMainStar
printfn "Calculated path in %s" (sw.Elapsed.ToString())
sw.Stop()

// Get some data about the path vs. the ideal path
let range = unboostedRange * 4.0m
let totalDistance = pathFound |> Seq.sumBy (fun (_,d) -> d)
let distanceFromStartToGoal = dist sol.Location CG.Location
let numSteps = pathFound |> List.length
let theoreticalShortestSteps = distanceFromStartToGoal / range
let stepsOutOfSingleJumpRange = pathFound |> List.filter (fun (_,d) -> d > range)

// Plot all of that in a nice 3D plot
open Plotting
plotPath systems pathFound 1900 1000
