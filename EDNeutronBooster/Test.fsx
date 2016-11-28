#load @"../paket-files/include-scripts/net45/include.main.group.fsx"
#r @"bin/debug/EDNeutronBooster.dll"

open SystemInfo
open Geometry

// Load the neutron star data from the cache on disk or build it from the EDDB data
let sw = System.Diagnostics.Stopwatch.StartNew()
let allSystems = NeutronStarDatabase.getOrBuildNeutronStarDatabase () |> Seq.toList
printfn "Built or read the database in %s" (sw.Elapsed.ToString())

// Setup the pathfinding parameters
let unboostedRange = 57.0m
let maxDistanceFromMainStar = 100m
let start = { Location = { X = -114.78125m; Y = 80.71875m; Z = -4.875m }; Id = -11; Name = "HR 6421 "; DistanceFromMainStar = 0m }
let goal = { Location = { X = -5784.84375m; Y = 216.375m; Z = -6100.0625m }; Id = -22; Name = "Formidine Rift CoR CG location"; DistanceFromMainStar = 0m }
// let goal = { Location = { X = -3187.9375m; Y = 63m; Z = 8592.15625m }; Id = -22; Name = "Conflux CoR CG location"; DistanceFromMainStar = 0m }
// let goal = { Location = { X = 7890.46875m; Y = 137.3125m; Z = 7508.03125m }; Id = -22; Name = "Hawkin's Gap CoR CG location"; DistanceFromMainStar = 0m }

// Calculate the path
sw.Restart()
let (systems, pathFound) = PathFinding.filterSystemsAndCalculatePath start goal unboostedRange allSystems maxDistanceFromMainStar
printfn "Calculated path from %s to %s in %s. %i steps." (sw.Elapsed.ToString()) start.Name goal.Name pathFound.Length
sw.Stop()

let printPath path =
    path |> Seq.iter (fun (sys, d) -> System.Console.WriteLine("{0}\t{1:f2}", sys.Name, d))

printPath pathFound

// Get some data about the path vs. the ideal path
let range = unboostedRange * 4.0m
let totalDistance = pathFound |> Seq.sumBy (fun (_,d) -> d)
let distanceFromStartToGoal = dist start.Location goal.Location
let numSteps = pathFound.Length - 1
let theoreticalShortestSteps = distanceFromStartToGoal / range
let stepsOutOfSingleJumpRange = pathFound |> List.filter (fun (_,d) -> d > range)

// Plot and show in a browser window
open Plotting
plotPath systems pathFound 1900 1000
