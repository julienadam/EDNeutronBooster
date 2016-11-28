namespace Views

open FsXaml
open System.Windows
open System.Windows.Input

type SearchParametersViewBase = XAML<"SearchParametersView.xaml">

type SearchParametersView() as self =
    inherit SearchParametersViewBase()

    static member CalculatePathCommandProperty =
        DependencyProperty.Register(
            "CalculatePathCommand",
            typeof<ICommand>,
            typeof<SearchParametersView>,
            new UIPropertyMetadata(null));

    member self.CalculatePathCommand
        with get() = base.GetValue(SearchParametersView.CalculatePathCommandProperty) :?> ICommand
        and set(value:ICommand) = base.SetValue(SearchParametersView.CalculatePathCommandProperty, value)
