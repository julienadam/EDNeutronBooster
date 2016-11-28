namespace ViewModels

open System.Windows
open FSharp.ViewModule

type MainViewModel() as self =
    inherit ViewModelBase()

    member self.CalculatePath() = System.Windows.MessageBox.Show("Foo")