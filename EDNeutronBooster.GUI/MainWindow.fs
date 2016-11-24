namespace Views

open FsXaml

type MainWindowBase = XAML<"MainWindow.xaml">

type MainWindow() =
    inherit MainWindowBase()
