#load @"../paket-files/include-scripts/net45/include.main.group.fsx"

open FSharp.Data
open FSharp.Data.JsonExtensions
open FSharp.Data.Runtime.BaseTypes
open FSharp.Data.CsvExtensions
open System.Collections.Generic

type BodyDescription = JsonProvider< @"{""id"":1497,""created_at"":1466612947,""updated_at"":1477503588,""name"":""42 AQUILAE A"",""system_id"":205,""group_id"":2,""group_name"":""Star"",""type_id"":null,""type_name"":null,""distance_to_arrival"":null,""full_spectral_class"":null,""spectral_class"":""F"",""spectral_sub_class"":null,""luminosity_class"":null,""luminosity_sub_class"":null,""surface_temperature"":7350,""is_main_star"":null,""age"":1720,""solar_masses"":1.2578,""solar_radius"":1.2509,""catalogue_gliese_id"":null,""catalogue_hipp_id"":""95556"",""catalogue_hd_id"":""185124"",""volcanism_type_id"":null,""volcanism_type_name"":null,""atmosphere_type_id"":null,""atmosphere_type_name"":null,""terraforming_state_id"":null,""terraforming_state_name"":null,""earth_masses"":null,""radius"":null,""gravity"":null,""surface_pressure"":null,""orbital_period"":296444.7,""semi_major_axis"":43.13,""orbital_eccentricity"":0.3455,""orbital_inclination"":39.4,""arg_of_periapsis"":104.98,""rotational_period"":0,""is_rotational_period_tidally_locked"":false,""axis_tilt"":0,""eg_id"":1808,""belt_moon_masses"":null,""rings"":[],""atmosphere_composition"":[],""solid_composition"":[],""materials"":[],""is_landable"":0}">

let parsedSystems = 
    System.IO.File.ReadLines(@"c:\users\nidhogg\desktop\bodies.jsonl") 
    |> Seq.map BodyDescription.Parse
    |> List.ofSeq
    
let neutronSystems =
    parsedSystems
    |> Seq.filter (fun x -> x.TypeName.JsonValue = JsonValue.String("Neutron star"))
    |> Seq.map (fun x -> x.Id)
    |> Set.ofSeq


// id,edsm_id,name,x,y,z,population,is_populated,government_id,government,allegiance_id,allegiance,state_id,
// state,security_id,security,primary_economy_id,primary_economy,power,power_state,power_state_id,needs_permit,
// updated_at,simbad_ref,controlling_minor_faction_id,controlling_minor_faction,reserve_type_id,reserve_type

// type SystemDescription = CsvProvider<"sample_systems.csv">
//let systems = SystemDescription.Load().Cache()


// for line in System.IO.File.ReadLines(@"c:\users\nidhogg\desktop\systems.csv") |> Seq.skip 1 do
//     let split = line.Split(',')
//     try
//         let x = (System.Int32.Parse(split.[0]), split.[2], System.Decimal.Parse(split.[3]), System.Decimal.Parse(split.[4]), System.Decimal.Parse(split.[5]))
//         x |> ignore
//     with
//     | _ -> printfn "%A" line
//     ()

let neutronSystemDetails =
    System.IO.File.ReadLines(@"c:\users\nidhogg\desktop\systems.csv")
    |> Seq.map (fun line ->
        try
            let split = line.Split(',')
            (System.Int32.Parse(split.[0]), split.[2], System.Decimal.Parse(split.[3]), System.Decimal.Parse(split.[4]), System.Decimal.Parse(split.[5]))
        with
            | _ -> (-1, "", 0m, 0m, 0m)
        )  
    |> Seq.filter (fun (id, name, x, y, z) -> neutronSystems |> Set.contains id)
    |> List.ofSeq 


let sol = (0m,0m,0m)
let eafots = (-5805.78125m, 129.875m, -5969.875m)

let mid (x1:decimal,y1:decimal,z1:decimal) (x2:decimal,y2:decimal,z2:decimal) = (x2 - x1) / 2m, (y2 - y1) / 2m , (z2 - z1) / 2m 

let sqrt (x:decimal) = (decimal) (System.Math.Sqrt((float) x))
let squared (x:decimal) =(decimal) (System.Math.Pow((float) x, 2.))
let dist (x1:decimal,y1:decimal,z1:decimal) (x2:decimal,y2:decimal,z2:decimal) = sqrt (squared (x2 - x1) + squared(y2 - y1) + squared (z2 - z1))
let midpoint = mid sol eafots 
let radius = (dist sol eafots) / 2m

// Find all points inside a circle centered between the two points (might want to add a few % points here to handle edge cases)
let systemsInSphere = 
    neutronSystemDetails 
    |> Seq.filter (fun (id, name, x, y, z) -> dist midpoint (x,y,z) < radius)
    |> List.ofSeq

// Add the star and end points
let startAndEnd = [(-2, "start", 0m,0m,0m); (-3, "end", -5805.78125m, 129.875m, -5969.875m)]
let allPointsForProblem = Seq.concat [|systemsInSphere;startAndEnd|]


// Build a graph of each star to its x closest stars
let maxNeighbours = 5
allPointsForProblem
    |> Seq.map (fun (id, name, x, y, z) -> 
        let found = new System.Collections.Generic.SortedDictionary<decimal, int>()
        for (oId, oName, oX, oY, oZ) in systemsInSphere do
            if oId <> id then
                found.Add(dist (x,y,z) (oX,oY,oZ), oId)
                if found.Count > maxNeighbours then
                    found.Remove(System.Linq.Enumerable.Last(found.Keys)) |> ignore
        (id, name, x, y, z, found) 
        )
    |> List.ofSeq


// Find a path




// Plot everything in a nice 3d graph

open XPlot.Plotly

let x1 = allPointsForProblem |> Seq.map (fun (id, name, x, y, z) -> (double) x) |> Seq.toArray
let y1 = allPointsForProblem |> Seq.map (fun (id, name, x, y, z) -> (double) y) |> Seq.toArray
let z1 = allPointsForProblem |> Seq.map (fun (id, name, x, y, z) -> (double) z) |> Seq.toArray
let text = allPointsForProblem |> Seq.map (fun (id, name, x, y, z) -> name) |> Seq.toArray

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
