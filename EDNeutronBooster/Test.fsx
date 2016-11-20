#load @"../paket-files/include-scripts/net45/include.main.group.fsx"

open FSharp.Data
open FSharp.Data.JsonExtensions
open FSharp.Data.Runtime.BaseTypes
open FSharp.Data.CsvExtensions
open System.Collections.Generic

type Point3D = { X:decimal; Y:decimal; Z:decimal}
type SystemInfo = { Location : Point3D; Id: int; Name: string }

let mid sys1 sys2 = { X = (sys2.X - sys1.X) / 2m; Y = (sys2.Y - sys1.Y) / 2m; Z = (sys2.Z - sys1.Z) / 2m } 
let sqrt (x:decimal) = (decimal) (System.Math.Sqrt((float) x))
let squared (x:decimal) =(decimal) (System.Math.Pow((float) x, 2.))
let dist sys1 sys2 = sqrt (squared (sys2.X - sys1.X) + squared(sys2.Y - sys1.Y) + squared (sys2.Z - sys1.Z))

type BodyDescription = JsonProvider< @"{""id"":1497,""created_at"":1466612947,""updated_at"":1477503588,""name"":""42 AQUILAE A"",""system_id"":205,""group_id"":2,""group_name"":""Star"",""type_id"":null,""type_name"":null,""distance_to_arrival"":null,""full_spectral_class"":null,""spectral_class"":""F"",""spectral_sub_class"":null,""luminosity_class"":null,""luminosity_sub_class"":null,""surface_temperature"":7350,""is_main_star"":null,""age"":1720,""solar_masses"":1.2578,""solar_radius"":1.2509,""catalogue_gliese_id"":null,""catalogue_hipp_id"":""95556"",""catalogue_hd_id"":""185124"",""volcanism_type_id"":null,""volcanism_type_name"":null,""atmosphere_type_id"":null,""atmosphere_type_name"":null,""terraforming_state_id"":null,""terraforming_state_name"":null,""earth_masses"":null,""radius"":null,""gravity"":null,""surface_pressure"":null,""orbital_period"":296444.7,""semi_major_axis"":43.13,""orbital_eccentricity"":0.3455,""orbital_inclination"":39.4,""arg_of_periapsis"":104.98,""rotational_period"":0,""is_rotational_period_tidally_locked"":false,""axis_tilt"":0,""eg_id"":1808,""belt_moon_masses"":null,""rings"":[],""atmosphere_composition"":[],""solid_composition"":[],""materials"":[],""is_landable"":0}">

let neutronSystems = 
    System.IO.File.ReadLines(@"c:\users\nidhogg\desktop\bodies.jsonl") 
    |> Seq.choose (fun l -> 
        let parsed = BodyDescription.Parse l
        match parsed.TypeName.JsonValue with
        | JsonValue.String("Neutron star") -> Some(parsed.Id)
        | _ -> None
        )
    |> Set.ofSeq


let neutronSystemDetails =
    System.IO.File.ReadLines(@"c:\users\nidhogg\desktop\systems.csv")
    |> Seq.choose (fun line ->
        try
            let split = line.Split(',')
            Some(
                { 
                    Id = System.Int32.Parse(split.[0]) 
                    Name = split.[2]
                    Location = 
                    { 
                        X = System.Decimal.Parse(split.[3])
                        Y = System.Decimal.Parse(split.[4]) 
                        Z = System.Decimal.Parse(split.[5]) 
                    }
                }
                )
        with
            | _ -> None
        )  
    |> Seq.filter (fun sys -> neutronSystems |> Set.contains sys.Id)
    |> List.ofSeq 
let sol = { Location = { X = 0m; Y = 0m; Z = 0m }; Id = -11; Name = "Sol" }
let CG = { Location = { X = -5805.78125m; Y = 129.875m; Z = -5969.875m }; Id = -22; Name = "CG location" }

let midpoint = mid sol.Location CG.Location 
let radius = (dist sol.Location CG.Location) / 2m

// Find all points inside a circle centered between the two points (might want to add a few % points here to handle edge cases)
let systemsInSphere = 
    neutronSystemDetails 
    |> Seq.filter (fun sys -> dist midpoint sys.Location < radius)
    |> List.ofSeq

// Add the star and end points
let allSystemsConsidered = [sol; CG] @ systemsInSphere

let range = 56m * 4.0m

//
//let allSystemsConsidered = 
//    [
//        { Id = 1; Location = {X = 0m; Y = 0m; Z = 0m}; Name = "Start"}
//        { Id = 2; Location = {X = 0m; Y = 0m; Z = 200m}; Name = "Step1"}
//        { Id = 3; Location = {X = 0m; Y = 0m; Z = 500m}; Name = "Step2"}
//        { Id = 4; Location = {X = 0m; Y = 0m; Z = 1000m}; Name = "End"}
//    ]

let rec findPath currentLocation goal steps jumpRange currentRange=
    let distanceToGoal = dist goal.Location currentLocation.Location
    if distanceToGoal < currentRange then
        steps @ [(goal, distanceToGoal)]
    else
        let systemsOnPathAndInRange =
            allSystemsConsidered
            |> Seq.choose (fun p ->
                if p = currentLocation then 
                    None
                else
                    let distanceToConsideredSystem = dist currentLocation.Location p.Location
                    let distanceFromConsideredToGoal = dist goal.Location p.Location
                    if distanceFromConsideredToGoal > distanceToGoal then
                        None
                    else if distanceToConsideredSystem > currentRange then
                        None
                    else
                        Some(p, distanceFromConsideredToGoal, distanceToConsideredSystem)
            )
            |> Seq.sortBy (fun (_, distanceFromConsideredToGoal, _) -> distanceFromConsideredToGoal)
            |> List.ofSeq

        match systemsOnPathAndInRange.Length with
        | 0 -> 
            // Try again with a slightly bigger range
            findPath currentLocation goal steps jumpRange (currentRange * 1.1m)
        | _ -> 
            // Take the system closest to the goal and calculate path from that system to the goal
            // Resetting the jump range to the default
            let (best, _, distanceToConsideredSystem) = systemsOnPathAndInRange.Head
            findPath best goal (steps @ [(best, distanceToConsideredSystem)]) jumpRange jumpRange

findPath (sol) (CG) [(sol, 0m)] range range
// findPath (allSystemsConsidered.[0]) (allSystemsConsidered.[3]) [(allSystemsConsidered.[0], 0m)] range range



let allPointsById = allSystemsConsidered |> List.map (fun sys -> (sys.Id, sys)) |> Map.ofList 
// Build a graph of each star to the x other stars that are 1. Closest to the target 2. Minimum 3 times the jump aways
let neighboursById = 
    allSystemsConsidered
    |> Seq.map (fun sys -> 
        let found = new Dictionary<int, decimal>()
        for otherSys in systemsInSphere do
            let distThisOther = dist otherSys.Location sys.Location
            if otherSys.Id <> sys.Id && distThisOther > 100m && distThisOther < 250m then
                found.Add(otherSys.Id, dist sys.Location otherSys.Location)
        (sys.Id, found) 
        )
    |> Map.ofSeq

type PathingNode = { System:SystemInfo; g:float } //g = cost of path so far
let findNeighbours node =
    match neighboursById.TryFind node.System.Id with
    | Some x -> x |> Seq.map (fun kvp -> { System = allPointsById.[kvp.Key]; g = node.g + (float) kvp.Value }) |> List.ofSeq
    | None -> List.empty

// Find a path
let start = { System = sol; g = 0. }

findNeighbours start

#load "astar.fsx"
let pathFound = AStar.aStar (fun n -> n.System.Id) (fun n -> n.g) (fun n -> (float) (dist CG.Location n.System.Location)) findNeighbours CG.Id start [{System = sol; g = (float) (dist sol.Location CG.Location) }] List.empty



// Plot everything in a nice 3d graph

open XPlot.Plotly

let x1 = allSystemsConsidered |> Seq.map (fun sys -> (double) sys.Location.X) |> Seq.toArray
let y1 = allSystemsConsidered |> Seq.map (fun sys -> (double) sys.Location.Y) |> Seq.toArray
let z1 = allSystemsConsidered |> Seq.map (fun sys -> (double) sys.Location.Z) |> Seq.toArray
let text = allSystemsConsidered |> Seq.map (fun sys -> sys.Name) |> Seq.toArray
let trace1 =
    Scatter3d(
        x = x1,
        y = y1,
        z = z1,
        text = text,
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


// let sol = (0m,0m,0m)
// let eafots = (-5805.78125m, 129.875m, -5969.875m)

let trace2 =
    Scatter3d(
        x = [|0., -5805.78125|],
        y = [|0., 129.875|],
        z = [|0., -5969.875|],
        text = [|"sol", "eafots"|],
        mode = "markers",
        marker =
            Marker(
                size = 20,
                symbol = "circle",
                line =
                    Line(
                        color = "rgba(217, 0, 0, 0.14)",
                        width = 0.5
                    ),
                opacity = 0.8
            )
    )

[trace2; trace1]
    |> Chart.Plot
    |> Chart.WithWidth 1800
    |> Chart.WithHeight 1000
    |> Chart.Show
// Use A* to plot a course
