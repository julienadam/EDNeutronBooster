module PathFinding

open SystemInfo
open Geometry

let getNeutronStarsInSphereAroundMidpointMatchingMaxDistanceFromStar allNeutronStars start goal maxDistanceFromMainStar = 
    let midpoint = mid start.Location goal.Location
    let radius = (dist start.Location goal.Location) / 2m

    // Find all neutron stars inside a sphere centered between the start and goal points
    let systemsInSphere = 
        allNeutronStars 
        |> Seq.filter (fun sys -> sys.DistanceFromMainStar <= maxDistanceFromMainStar && dist midpoint sys.Location <= radius)
        |> List.ofSeq

    // Add the start and goal points
    [start; goal] @ systemsInSphere

let calculatePath start goal jumpRange systems =
    let rec findPath currentLocation goal steps initialJumpRange currentRange=
        let distanceToGoal = dist goal.Location currentLocation.Location
        
        if distanceToGoal < currentRange then
            // We are in range of the goal, return the steps, including the goal as the final point of the path
            steps @ [(goal, distanceToGoal)]
        else
            // Find all other systems that are in our current range, and are closer to the goal than the current location
            // and order them by their distance to the goal
            let systemsOnPathAndInRange =
                systems
                |> Seq.choose (fun consideredSystem ->
                    if consideredSystem = currentLocation then None // Skip current location
                    else
                        let distanceToConsideredSystem = dist currentLocation.Location consideredSystem.Location
                        let distanceFromConsideredToGoal = dist goal.Location consideredSystem.Location

                        if distanceFromConsideredToGoal > distanceToGoal then
                            None
                        else if distanceToConsideredSystem > currentRange then
                            None
                        else
                            Some(consideredSystem, distanceFromConsideredToGoal, distanceToConsideredSystem)
                )
                |> Seq.sortBy (fun (_, distanceFromConsideredToGoal, _) -> distanceFromConsideredToGoal)
                |> List.ofSeq

            match systemsOnPathAndInRange.Length with
            | 0 -> 
                // nothing found, try again with a bigger range
                findPath currentLocation goal steps initialJumpRange (currentRange * 1.1m)
            | _ -> 
                // Take the system closest to the goal, add it to the steps and recurse using that system as the starting point
                // also resetting the jump range to its default
                let (best, _, distanceToConsideredSystem) = systemsOnPathAndInRange.Head
                findPath best goal (steps @ [(best, distanceToConsideredSystem)]) initialJumpRange initialJumpRange

    let boostedRange = jumpRange * 4.0m
    findPath (start) (goal) [(start, 0m)] boostedRange boostedRange

let filterSystemsAndCalculatePath start goal jumpRange allSystems maxDistanceFromMainStar =
    let filteredSystems = getNeutronStarsInSphereAroundMidpointMatchingMaxDistanceFromStar allSystems start goal maxDistanceFromMainStar
    let path = calculatePath start goal jumpRange filteredSystems
    (filteredSystems, path)