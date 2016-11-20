module Geometry

type Point3D = { X:decimal; Y:decimal; Z:decimal }

let mid loc1 loc2 = { X = (loc2.X - loc1.X) / 2m; Y = (loc2.Y - loc1.Y) / 2m; Z = (loc2.Z - loc1.Z) / 2m }
let sqrt (x:decimal) = (decimal) (System.Math.Sqrt((float) x))
let squared (x:decimal) = (decimal) (System.Math.Pow((float) x, 2.))
let dist loc1 loc2 = sqrt (squared (loc2.X - loc1.X) + squared(loc2.Y - loc1.Y) + squared (loc2.Z - loc1.Z))
