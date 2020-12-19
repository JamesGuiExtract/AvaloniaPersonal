namespace Extract.FileActionManager.Utilities.FAMServiceManager

// Allow recursively defined objects
#nowarn "40"

open System.Windows
open System.Windows.Controls

// Static class in F# = abstract, sealed, private ctor
[<AbstractClass; Sealed>]
type PasswordHelper private () =
  static let isUpdatingProperty = DependencyProperty.RegisterAttached("IsUpdating", typeof<bool>, typeof<PasswordHelper>)

  static let setIsUpdating value (dp: DependencyObject) =
    dp.SetValue(isUpdatingProperty, value)

  static let rec passwordProperty =
    DependencyProperty.RegisterAttached("Password", typeof<string>, typeof<PasswordHelper>, FrameworkPropertyMetadata(box "", PropertyChangedCallback(onPasswordPropertyChanged)))

  and attachProperty =
    DependencyProperty.RegisterAttached("Attach", typeof<bool>, typeof<PasswordHelper>, new PropertyMetadata(false, PropertyChangedCallback(attach)))

  and attach (sender: DependencyObject) (e: DependencyPropertyChangedEventArgs) =
    let passwordBox = sender :?> PasswordBox
    if passwordBox |> isNull then ()
    else
      if (e.OldValue :?> bool) then
        passwordBox.PasswordChanged.RemoveHandler (RoutedEventHandler passwordChanged)
      if (e.NewValue :?> bool) then
        passwordBox.PasswordChanged.AddHandler (RoutedEventHandler passwordChanged)

  and getIsUpdating (dp: DependencyObject) =
    dp.GetValue isUpdatingProperty :?> bool

  and onPasswordPropertyChanged (sender: DependencyObject) (e: DependencyPropertyChangedEventArgs) =
    let passwordBox = sender :?> PasswordBox
    if not (passwordBox |> getIsUpdating) then
      passwordBox.PasswordChanged.RemoveHandler (RoutedEventHandler passwordChanged)
      try
        passwordBox.Password <- string e.NewValue;
      finally
        passwordBox.PasswordChanged.AddHandler (RoutedEventHandler passwordChanged)

  and passwordChanged (sender: obj) (_: RoutedEventArgs) =
    let passwordBox = sender :?> PasswordBox
    passwordBox |> setIsUpdating true
    try
      passwordBox |> setPassword passwordBox.Password
    finally
      passwordBox |> setIsUpdating false

  and setPassword (value: string) (dp: DependencyObject) =
    dp.SetValue(passwordProperty, value)
 
  static member SetAttach (dp: DependencyObject, value: bool) = dp.SetValue(attachProperty, value)
  static member GetAttach (dp: DependencyObject) = dp.GetValue attachProperty :?> bool
  static member GetPassword (dp: DependencyObject) = dp.GetValue passwordProperty :?> string
  static member SetPassword (dp: DependencyObject, value: string) = dp |> setPassword value
  static member PasswordProperty = passwordProperty
  static member AttachProperty = attachProperty