module Octree

open Geometry

type BoundingBox = BoundingBox of Point3D * Point3D

let inline isPointInsideBoundingBox (BoundingBox(minLocation, maxLocation)) location =
    (location.X > minLocation.X) && (location.X <= maxLocation.X) && (location.Y > minLocation.Y) && (location.Y <= maxLocation.Y) && (location.Z > minLocation.Z) && (location.Z <= maxLocation.Z)

// split points according to their locations in the box
let inline splitPoints (BoundingBox (min, max) as bb) bs (fLoc:('a -> Point3D)) =
    match bs with
    | [] -> [(bb,[])]
    | b1::[] -> [(bb,[b1])]
    | _ ->
           let mid = Geometry.mid min max
           let box1 = BoundingBox (min, mid)
           let box2 = BoundingBox ({ X = min.X; Y= mid.Y; Z = min.Z}, { X = mid.X; Y = max.Y; Z = mid.Z})
           let box3 = BoundingBox ({ X = mid.X; Y= min.Y; Z = min.Z}, { X = max.X; Y = mid.Y; Z = mid.Z})
           let box4 = BoundingBox ({ X = mid.X; Y= mid.Y; Z = min.Z}, { X = max.X; Y = max.Y; Z = mid.Z})
           let box5 = BoundingBox ({ X = min.X; Y= min.Y; Z = mid.Z}, { X = mid.X; Y = mid.Y; Z = max.Z})
           let box6 = BoundingBox ({ X = min.X; Y= mid.Y; Z = mid.Z}, { X = mid.X; Y = max.Y; Z = max.Z})
           let box7 = BoundingBox ({ X = mid.X; Y= min.Y; Z = mid.Z}, { X = max.X; Y = mid.Y; Z = max.Z})
           let box8 = BoundingBox (mid, max)

           let boxes = [box1;box2;box3;box4;box5;box6;box7;box8]
           let splitPts = boxes |> List.map (fun (box:BoundingBox) -> bs |> List.filter (fun value -> isPointInsideBoundingBox box (fLoc (value))))

           let boxesAndPts = List.filter (fun (_,pts) -> not (List.isEmpty pts)) (List.zip boxes splitPts)
           boxesAndPts

type Octree = Octree of decimal * Octree list

// build the tree
let rec buildTree (BoundingBox (min, max) as bb) bs (fLoc:('a -> Point3D)) =
    let size = List.min [abs (max.X - min.X); abs (max.Y - min.Y); abs (max.Z - min.Z)]

    match bs with
    | [] -> Octree (size, [])
    | b1::[] -> Octree (size, [])
    | _ ->
        let boxesAndPts = splitPoints bb bs fLoc
        let subTrees = List.map (fun (bb',bs') -> buildTree bb' bs' fLoc) boxesAndPts
        Octree (size, subTrees)