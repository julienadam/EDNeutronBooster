#load @"../paket-files/include-scripts/net45/include.main.group.fsx"
#r @"bin/debug/EDNeutronBooster.dll"

open SystemInfo
open Geometry

// Change that to wherever your files are located
let bodiesFile = @"c:\users\nidhogg\Desktop\bodies.jsonl"
let systemsFile = @"c:\users\nidhogg\Desktop\systems.csv"

// Setup the pathfinding parameters
let unboostedRange = 57.0m
let maxDistanceFromMainStar = 100m
let start = { Location = { X = 0m; Y = 0m; Z = 0m }; Id = -11; Name = "Sol"; DistanceFromMainStar = 0m }
let goal = { Location = { X = -5805.78125m; Y = 129.875m; Z = -5969.875m }; Id = -22; Name = "EAFOTS CoR CG location"; DistanceFromMainStar = 0m }

// Load the neutron star data from the cache on disk or build it from the EDDB data
let sw = System.Diagnostics.Stopwatch.StartNew()
let allSystems = NeutronStarDatabase.getOrBuildNeutronStarDatabase bodiesFile systemsFile
printfn "Built or read the database in %s" (sw.Elapsed.ToString())

// Calculate the path
sw.Restart()
let (systems, pathFound) = PathFinding.filterSystemsAndCalculatePath start goal unboostedRange allSystems maxDistanceFromMainStar
printfn "Calculated path from %s to %s in %s. %i steps." (sw.Elapsed.ToString()) start.Name goal.Name pathFound.Length
sw.Stop()

// Get some data about the path vs. the ideal path
let range = unboostedRange * 4.0m
let totalDistance = pathFound |> Seq.sumBy (fun (_,d) -> d)
let distanceFromStartToGoal = dist start.Location goal.Location
let numSteps = pathFound.Length
let theoreticalShortestSteps = distanceFromStartToGoal / range
let stepsOutOfSingleJumpRange = pathFound |> List.filter (fun (_,d) -> d > range)

// Plot and show in a browser window
open Plotting
plotPath systems pathFound 1900 1000
