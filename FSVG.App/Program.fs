open System.Drawing
open System.IO
open FSVG
open FSVG.Helpers
open FSVG.Dsl

module Test1 =
    
    let run _ =

        let style =
            { Style.Default() with
                Stroke = Some "black"
                StrokeWidth = Some 1 }

        let fillGreen =
            ({ Style.Default() with Fill = Some "green" })

        let sky =
            ({ Style.Default() with Fill = Some "skyblue" })

        let reflection =
            ({ Style.Default() with
                Fill = Some "green"
                Opacity = Some 0.2 })

        let water =
            ({ Style.Default() with
                Fill = Some "blue"
                Opacity = Some 0.5 })

        let sun =
            ({ Style.Default() with Fill = Some "yellow" })

        let wave =
            ({ Style.Default() with
                Stroke = Some "blue"
                StrokeWidth = Some 1. })

        svg [ rect sky 0 0 50 100 0 0

              circle sun 20 20 10

              path
                  fillGreen
                  ([ createBezierCommands
                     <| SvgPoints.Create(
                         [ SvgPoint.Create(100., 20.)
                           SvgPoint.Create(50., 50.)
                           SvgPoint.Create(0., 20.) ]
                     )
                     [ v 50; h 100; v 0; z ] ]
                   |> List.concat)

              path
                  reflection
                  ([ createBezierCommands
                     <| SvgPoints.Create(
                         [ SvgPoint.Create(100., 80.)
                           SvgPoint.Create(50., 50.)
                           SvgPoint.Create(0., 80.) ]
                     )
                     [ v 50; h 100; v 0; z ] ]
                   |> List.concat)

              rect water 0 0 50 100 0 50 ]
        |> saveToFile "C:\\ProjectData\\TestSvgs\\FSVG_test.svg" 100 100

module LineChartTest =
    
    open FSVG.Charts
    
    let run _ =
        
        let settings =
            ({ LeftOffset = 10
               BottomOffset = 10
               TopOffset = 10
               RightOffset = 10
               Title = None
               XLabel = None
               YMajorMarks = [ 50; 100 ]
               YMinorMarks = [ 25; 75 ] }: LineCharts.Settings)
            
        let series =
            ({ Normalizer = fun p -> (float p.Value / float p.MaxValue) * 100.
               SplitValueHandler =
                   fun percent maxValue ->
                       (float maxValue / float 100) * float percent
                       |> int
                       |> fun r -> r.ToString()
               Points =
                   [ { Name = "Item 1"
                       Value = 20 }
                     { Name = "Item 2"
                       Value = 40 }
                     { Name = "Item 3"
                       Value = 30 }
                     { Name = "Item 4"
                       Value = 70 }
                     { Name = "Item 5"
                       Value = 80 } ] }: LineCharts.Series<int>)
            
        LineCharts.generate settings series 100
        |> fun r -> File.WriteAllText("C:\\ProjectData\\TestSvgs\\FSVG-test_line_chart.svg", r)
        
        
LineChartTest.run ()

// For more information see https://aka.ms/fsharp-console-apps
printfn "Hello from F#"
