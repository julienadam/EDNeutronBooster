module MainApp

open System
open System.Windows

// Create the View and bind it to the View Model
let mainWindowViewModel = Application.LoadComponent(new System.Uri("/App;component/mainwindow.xaml", UriKind.Relative)) :?> Window

// Application Entry point
[<STAThread>]
[<EntryPoint>]
let main(_) = (new Application()).Run(mainWindowViewModel)