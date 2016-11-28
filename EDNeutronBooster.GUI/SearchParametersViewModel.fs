namespace ViewModels

open System.Windows
open FSharp.ViewModule

type SearchParametersViewModel() as self =
    inherit ViewModelBase()

    let startX = self.Factory.Backing(<@ self.StartX @>, 0.0m, id)
    let startY = self.Factory.Backing(<@ self.StartY @>, 0.0m, id)
    let startZ = self.Factory.Backing(<@ self.StartZ @>, 0.0m, id)
    let goalX = self.Factory.Backing(<@ self.GoalX @>, 0.0m, id)
    let goalY = self.Factory.Backing(<@ self.GoalY @>, 0.0m, id)
    let goalZ = self.Factory.Backing(<@ self.GoalZ @>, 0.0m, id)
    let jumpRange = self.Factory.Backing(<@ self.JumpRange @>, 57.0m, id)
    let maxDistFromStar = self.Factory.Backing(<@ self.MaxDistFromStar @>, 100.0m, id)

    let isCommandActive x =
        printfn "%A" x
        true

    let calculatePathCommand =
        self.Factory.CommandSyncParamChecked(
            ignore,
            isCommandActive,
            [
                <@ self.StartX @>; <@ self.StartY @>; <@ self.StartZ @>
                <@ self.GoalX @>; <@ self.GoalY @>; <@ self.GoalZ @>
                <@ self.JumpRange @>; <@ self.MaxDistFromStar @>
            ])

    //do
        // Add in property dependencies
        // self.DependencyTracker.AddPropertyDependencies(<@@ self.FullName @@>, [ <@@ self.FirstName @@> ; <@@ self.LastName @@> ])

    member x.StartX with get() = startX.Value and set value = startX.Value <- value
    member x.StartY with get() = startY.Value and set value = startY.Value <- value
    member x.StartZ with get() = startZ.Value and set value = startZ.Value <- value
    member x.GoalX with get() = goalX.Value and set value = goalX.Value <- value
    member x.GoalY with get() = goalY.Value and set value = goalY.Value <- value
    member x.GoalZ with get() = goalZ.Value and set value = goalZ.Value <- value
    member x.JumpRange with get() = jumpRange.Value and set value = jumpRange.Value <- value
    member x.MaxDistFromStar with get() = maxDistFromStar.Value and set value = maxDistFromStar.Value <- value
    member x.CalculatePathCommand = calculatePathCommand