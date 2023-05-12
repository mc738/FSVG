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
                 : LegendStyle)
                 |> Some
               YMajorMarkers = [ 50; 100 ]
               YMinorMarkers = [ 25; 75 ] }
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
               YMajorMarkers = [ 50; 100 ]
               YMinorMarkers = [ 25; 75 ] }
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

module ScatterChartTest =

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
               XMajorMarkers = [ 50; 100 ]
               XMinorMarkers = [ 25; 75 ]
               YMajorMarkers = [ 50; 100 ]
               YMinorMarkers = [ 25; 75 ] }
            : ScatterCharts.Settings)

        let seriesCollection =
            ({ XSplitValueHandler = floatValueSplitter
               YSplitValueHandler = floatValueSplitter
               XNormalizer = floatRangeNormalizer
               YNormalizer = floatRangeNormalizer
               Series =
                 [ ({ Name = "Series 1"
                      Style = { Color = SvgColor.Grey }
                      Values =
                        [ { X = 0.; Y = 100.; R = None }
                          { X = 10.; Y = 110.; R = None }
                          { X = 20.; Y = 130.; R = Some 2. }
                          { X = 50.; Y = 150.; R = Some 5. } ] }
                   : ScatterCharts.Series<float, float>) ] }
            : ScatterCharts.SeriesCollection<float, float>)

        ScatterCharts.generate settings seriesCollection 0 100 100 200
        |> fun r -> File.WriteAllText("C:\\ProjectData\\TestSvgs\\FSVG-test_scatter_chart.svg", r)

        ()

module BarChartsTest =

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
               ChartDirection = BarCharts.ChartDirection.Vertical
               SectionPadding = PaddingType.Specific 1.
               LegendStyle = None
               MajorMarks = [ 50; 100 ]
               MinorMarks = [ 25; 75 ] }
            : BarCharts.Settings)

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
               SectionNames = [ "Item 1"; "Item 2"; "Item 3"; "Item 4"; "Item 5" ]
               Series =
                 [ ({ Name = "Series 1"
                      Style =
                        { Color = SvgColor.Grey
                          StokeWidth = 0.3 }
                      Values = [ -50; 40; -20; 40; -30 ] }
                   : BarCharts.Series<int>)
                   ({ Name = "Series 1"
                      Style =
                        { Color = SvgColor.Black
                          StokeWidth = 0.3 }
                      Values = [ 20; -40; 30; -70; 80 ] }
                   : BarCharts.Series<int>) ] }
            : BarCharts.SeriesCollection<int>)

        BarCharts.generate settings seriesCollection -100 100
        |> fun r -> File.WriteAllText("C:\\ProjectData\\TestSvgs\\FSVG-test_bar_chart.svg", r)

module PieChartsTest =

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
               IsDonut = false }
            : PieCharts.Settings)

        let seriesCollection =
            ({ Normalizer = rangeNormalizer<int> float
               Series =
                 [ { Name = "Test 1"
                     Style = { Color = SvgColor.Named "green" }
                     Value = 12 }
                   { Name = "Item 2"
                     Style = { Color = SvgColor.Named "orange" }
                     Value = 13 }
                   { Name = "Item 3"
                     Style = { Color = SvgColor.Named "blue" }
                     Value = 25 }
                   { Name = "Item 4"
                     Value = 30
                     Style = { Color = SvgColor.Named "pink" } }
                   { Name = "Item 5"
                     Style = { Color = SvgColor.Named "yellow" }
                     Value = 20 } ] }
            : PieCharts.SeriesCollection<int>)
            
        PieCharts.generate settings seriesCollection 0 100
        |> fun r -> File.WriteAllText("C:\\ProjectData\\TestSvgs\\FSVG-test_pie_chart.svg", r)

module CandleStickChartsTest =
    
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
               SectionPadding = PaddingType.Specific 1.
               LegendStyle = None
               MajorMarkers = [ 50; 100 ]
               MinorMarkers = [ 25; 75 ] }
            : CandleStickCharts.Settings)

        let seriesCollection =
            ({ SplitValueHandler = valueSplitter float
               Normalizer = rangeNormalizer<int> float
               ValueComparer = fun (p: ValueComparisonParameters<int>) ->
                   match p.ValueA > p.ValueB with
                   | true -> ValueComparisonResult.GreaterThan
                   | false -> ValueComparisonResult.LessThan
               SectionNames = [ "Item 1"; "Item 2"; "Item 3"; "Item 4"; "Item 5" ]
               Series =
                 [ ({ Name = "Series 1"
                      Style =
                        { Color = SvgColor.Grey
                          StokeWidth = 0.3 }
                      HighValue = 60
                      LowValue = 30
                      OpenValue = 35
                      CloseValue = 50 }
                   : CandleStickCharts.Series<int>)
                   ({ Name = "Series 1"
                      Style =
                        { Color = SvgColor.Grey
                          StokeWidth = 0.3 }
                      HighValue = 70
                      LowValue = 40
                      OpenValue = 50
                      CloseValue = 60 }
                   : CandleStickCharts.Series<int>)
                   ({ Name = "Series 1"
                      Style =
                        { Color = SvgColor.Grey
                          StokeWidth = 0.3 }
                      HighValue = 70
                      LowValue = 30
                      OpenValue = 60
                      CloseValue = 50 }
                   : CandleStickCharts.Series<int>)
                   ({ Name = "Series 1"
                      Style =
                        { Color = SvgColor.Grey
                          StokeWidth = 0.3 }
                      HighValue = 80
                      LowValue = 30
                      OpenValue = 50
                      CloseValue = 75 }
                   : CandleStickCharts.Series<int>)
                   ({ Name = "Series 1"
                      Style =
                        { Color = SvgColor.Grey
                          StokeWidth = 0.3 }
                      HighValue = 50
                      LowValue = 25
                      OpenValue = 30
                      CloseValue = 40 }
                   : CandleStickCharts.Series<int>) ] }
            : CandleStickCharts.SeriesCollection<int>)

        CandleStickCharts.generate settings seriesCollection 0 100
        |> fun r -> File.WriteAllText("C:\\ProjectData\\TestSvgs\\FSVG-candle_stick_chart.svg", r)
        
        
        ()

LineChartTest.run ()
LineChartTest2.run ()
ScatterChartTest.run ()
BarChartsTest.run ()
PieChartsTest.run ()
CandleStickChartsTest.run ()
// For more information see https://aka.ms/fsharp-console-apps
printfn "Hello from F#"
