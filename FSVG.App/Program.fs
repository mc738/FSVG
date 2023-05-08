open System.Drawing
open System.IO
open FSVG
open FSVG.Charts
open FSVG.Helpers
open FSVG.Dsl

module Test1 =

    let run _ =

        let style =
            { Style.Default() with
                Stroke = Some "black"
                StrokeWidth = Some 1 }

        let fillGreen =
            ({ Style.Default() with
                Fill = Some "green" })

        let sky =
            ({ Style.Default() with
                Fill = Some "skyblue" })

        let reflection =
            ({ Style.Default() with
                Fill = Some "green"
                Opacity = Some 0.2 })

        let water =
            ({ Style.Default() with
                Fill = Some "blue"
                Opacity = Some 0.5 })

        let sun =
            ({ Style.Default() with
                Fill = Some "yellow" })

        let wave =
            ({ Style.Default() with
                Stroke = Some "blue"
                StrokeWidth = Some 1. })

        svg
            [ rect sky 0 0 50 100 0 0

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
            ({ ChartDimensions =
                { Height = 100.
                  Width = 100.
                  LeftOffset = 10
                  BottomOffset = 10
                  TopOffset = 10
                  RightOffset = 10 }
               Title = Some "Test Chart 1"
               XLabel = Some "Test 1"
               YLabel = Some "Test 2"
               LegendStyle =
                 ({ Bordered = false
                    Position = LegendPosition.Right }
                 : LineCharts.LegendStyle)
                 |> Some
               YMajorMarks = [ 50; 100 ]
               YMinorMarks = [ 25; 75 ] }
            : LineCharts.Settings)

        (*
        "Item 1"
        "Item 2"
        "Item 3"
        "Item 4"
        "Item 5"
        *)

        let seriesCollection =
            ({ SplitValueHandler = valueSplitter float
               Normalizer = rangeNormalizer<int> float
               PointNames = [ "Item 1"; "Item 2"; "Item 3"; "Item 4"; "Item 5" ]
               Series =
                 [ ({ Name = "Series 1"
                      Style =
                        { Color = SvgColor.Grey
                          StokeWidth = 0.3
                          LineType = LineCharts.LineType.Straight
                          Shading =
                            ({ Color = SvgColor.Rgba(255uy, 0uy, 0uy, 0.3) }: LineCharts.ShadingOptions)
                            |> Some }
                      Values = [ 50; 40; 20; 40; 30 ] }
                   : LineCharts.Series<int>)
                   ({ Name = "Series 2"
                      Style =
                        { Color = SvgColor.Black
                          StokeWidth = 0.3
                          LineType = LineCharts.LineType.Bezier
                          Shading = None }
                      Values = [ 20; 40; 30; 70; 80 ] }
                   : LineCharts.Series<int>) ] }
            : LineCharts.SeriesCollection<int>)

        LineCharts.generate settings seriesCollection 0 100
        |> fun r -> File.WriteAllText("C:\\ProjectData\\TestSvgs\\FSVG-test_line_chart.svg", r)

module LineChartTest2 =

    open FSVG.Charts

    let run _ =

        let settings =
            ({ ChartDimensions =
                { Height = 100.
                  Width = 100.
                  LeftOffset = 10
                  BottomOffset = 10
                  TopOffset = 10
                  RightOffset = 10 }
               Title = None
               XLabel = Some "Test 1"
               YLabel = Some "Test 2"
               LegendStyle = None
               YMajorMarks = [ 50; 100 ]
               YMinorMarks = [ 25; 75 ] }
            : LineCharts.Settings)

        (*
        "Item 1"
        "Item 2"
        "Item 3"
        "Item 4"
        "Item 5"
        *)

        let seriesCollection =
            ({ SplitValueHandler = valueSplitter float
               Normalizer = rangeNormalizer<int> float
               PointNames = [ "Item 1"; "Item 2"; "Item 3"; "Item 4"; "Item 5" ]
               Series =
                 [ ({ Name = "Series 1"
                      Style =
                        { Color = SvgColor.Grey
                          StokeWidth = 0.3
                          LineType = LineCharts.LineType.Straight
                          Shading =
                            ({ Color = SvgColor.Rgba(255uy, 0uy, 0uy, 0.3) }: LineCharts.ShadingOptions)
                            |> Some }
                      Values = [ -50; 40; -20; 40; -30 ] }
                   : LineCharts.Series<int>)
                   ({ Name = "Series 1"
                      Style =
                        { Color = SvgColor.Black
                          StokeWidth = 0.3
                          LineType = LineCharts.LineType.Bezier
                          Shading = None }
                      Values = [ 20; -40; 30; -70; 80 ] }
                   : LineCharts.Series<int>) ] }
            : LineCharts.SeriesCollection<int>)

        LineCharts.generate settings seriesCollection -100 100
        |> fun r -> File.WriteAllText("C:\\ProjectData\\TestSvgs\\FSVG-test_line_chart-2.svg", r)


LineChartTest.run ()
LineChartTest2.run ()

// For more information see https://aka.ms/fsharp-console-apps
printfn "Hello from F#"
