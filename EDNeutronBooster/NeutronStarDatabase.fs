module NeutronStarDatabase

open FSharp.Data
open FSharp.Data.JsonExtensions
open FSharp.Data.Runtime.BaseTypes
open FSharp.Data.CsvExtensions
open System.IO
open Newtonsoft.Json
open Geometry
open SystemInfo

type BodyDescription = JsonProvider< @"{""id"":1497,""created_at"":1466612947,""updated_at"":1477503588,""name"":""42 AQUILAE A"",""system_id"":205,""group_id"":2,""group_name"":""Star"",""type_id"":null,""type_name"":null,""distance_to_arrival"":null,""full_spectral_class"":null,""spectral_class"":""F"",""spectral_sub_class"":null,""luminosity_class"":null,""luminosity_sub_class"":null,""surface_temperature"":7350,""is_main_star"":null,""age"":1720,""solar_masses"":1.2578,""solar_radius"":1.2509,""catalogue_gliese_id"":null,""catalogue_hipp_id"":""95556"",""catalogue_hd_id"":""185124"",""volcanism_type_id"":null,""volcanism_type_name"":null,""atmosphere_type_id"":null,""atmosphere_type_name"":null,""terraforming_state_id"":null,""terraforming_state_name"":null,""earth_masses"":null,""radius"":null,""gravity"":null,""surface_pressure"":null,""orbital_period"":296444.7,""semi_major_axis"":43.13,""orbital_eccentricity"":0.3455,""orbital_inclination"":39.4,""arg_of_periapsis"":104.98,""rotational_period"":0,""is_rotational_period_tidally_locked"":false,""axis_tilt"":0,""eg_id"":1808,""belt_moon_masses"":null,""rings"":[],""atmosphere_composition"":[],""solid_composition"":[],""materials"":[],""is_landable"":0}">
let inline (+/) path1 path2 = Path.Combine(path1, path2)
let localAppData = System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData) +/ "EDNeutronBooster"
let neutronStarDatabasePath = localAppData +/ "neutron_star_systems.jsonl"

let getSystemsWithNeutronStarAndDistanceFromMainStar eddbBodiesFile = 
    System.IO.File.ReadLines(eddbBodiesFile) 
    |> Seq.choose (fun l -> 
        let parsed = BodyDescription.Parse l
        match parsed.TypeName.JsonValue with
        | JsonValue.String("Neutron star") -> 
            if parsed.IsMainStar.JsonValue = JsonValue.Boolean(true) then
                Some(parsed.Id, 0m)
            else
                match parsed.DistanceToArrival.JsonValue with
                | JsonValue.Number d -> Some(parsed.Id, d)
                | _ -> None
        | _ -> None)
    |> Map.ofSeq

let getRelevantSystemDetails neutronSystems eddbSystemsFile =
    System.IO.File.ReadLines(eddbSystemsFile)
    |> Seq.choose (fun line ->
        try
            // old school CSV "parsing" yay ! (could have used something better but hey it works ... (most of the time :)
            let split = line.Split(',')
            let id = System.Int32.Parse(split.[0])
            match neutronSystems |> Map.tryFind id with
            | Some(s) ->
                Some(
                    {
                        Id = id
                        Name = split.[2].Replace("\"", "")
                        DistanceFromMainStar = neutronSystems.[id]
                        Location =
                        {
                            X = System.Decimal.Parse(split.[3])
                            Y = System.Decimal.Parse(split.[4])
                            Z = System.Decimal.Parse(split.[5])
                        }
                    }
                    )
            | _ -> None
        with
        | _ -> None
        )

let readDatabase () =
    System.IO.File.ReadLines neutronStarDatabasePath
    |> Seq.map (fun l -> Newtonsoft.Json.JsonConvert.DeserializeObject<SystemInfo>(l))
    |> List.ofSeq

let getOrBuildNeutronStarDatabase eddbBodiesFile eddbSystemsFile =
    if not (File.Exists neutronStarDatabasePath) then
        let neutronSystems = getSystemsWithNeutronStarAndDistanceFromMainStar eddbBodiesFile

        Directory.CreateDirectory(localAppData) |> ignore
        use sw = File.CreateText neutronStarDatabasePath

        let serializer = JsonSerializer.Create()
        getRelevantSystemDetails neutronSystems eddbSystemsFile
        |> Seq.iter (fun system ->
            serializer.Serialize(sw, system)
            sw.WriteLine())

    readDatabase ()