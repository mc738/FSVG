namespace FSVG.Charts

open System
open FSVG

[<RequireQualifiedAccess>]
module ScatterCharts =

    type SeriesCollection<'T, 'U> =
        { XSplitValueHandler: ValueSplitter<'T>
          YSplitValueHandler: ValueSplitter<'U>
          XNormalizer: ValueNormalizer<'T>
          YNormalizer: ValueNormalizer<'U>
          Series: Series<'T, 'U> list }

    and Series<'T, 'U> =
        { Name: string
          Style: SeriesStyle
          Values: SeriesValue<'T, 'U> list }

    and SeriesValue<'T, 'U> = { X: 'T; Y: 'U; R: float option }

    and SeriesStyle = { Color: SvgColor }

    type Settings =
        { ChartDimensions: ChartDimensions
          Title: string option
          XLabel: string option
          YLabel: string option
          LegendStyle: LegendStyle option
          XMajorMarkers: float list
          XMinorMarkers: float list
          YMajorMarkers: float list
          YMinorMarkers: float list }

    let generate<'T, 'U>
        (settings: Settings)
        (seriesCollection: SeriesCollection<'T, 'U>)
        (minXValue: 'T)
        (maxXValue: 'T)
        (minYValue: 'U)
        (maxYValue: 'U)
        =

        let (vbHeight, vbWidth) =
            match settings.LegendStyle with
            | Some legendStyle ->
                match legendStyle.Position with
                | LegendPosition.Right -> 100, 120
                | LegendPosition.Bottom -> 120, 100
            | None -> 100, 100

        let xAxis =
            ({ MajorMarkers = settings.YMajorMarkers
               MinorMarkers = settings.YMinorMarkers
               ValueSplitter = seriesCollection.XSplitValueHandler
               MaxValue = maxXValue
               MinValue = minXValue
               Label = settings.YLabel
               ChartDimensions = settings.ChartDimensions }
            : Axes.DynamicAxisSettings<'T>)
            |> Axes.AxisType.Dynamic
            |> Axes.createXAxis<'T>

        let yAxis =
            ({ MajorMarkers = settings.YMajorMarkers
               MinorMarkers = settings.YMinorMarkers
               ValueSplitter = seriesCollection.YSplitValueHandler
               MaxValue = maxYValue
               MinValue = minYValue
               Label = settings.YLabel
               ChartDimensions = settings.ChartDimensions }
            : Axes.DynamicAxisSettings<'U>)
            |> Axes.AxisType.Dynamic
            |> Axes.createYAxis<'U>

        let chart = [ yield! xAxis; yield! yAxis ]

        let renderPoints =
            seriesCollection.Series
            |> List.collect (fun series ->
                series.Values
                |> List.map (fun v ->
                    let y =
                        seriesCollection.YNormalizer
                            { MaxValue = maxYValue
                              MinValue = minYValue
                              Value = v.Y }

                    let normalizedX =
                        seriesCollection.XNormalizer
                            { MaxValue = maxXValue
                              MinValue = minXValue
                              Value = v.X }
                    
                    let invertedY =
                        settings.ChartDimensions.BottomOffset
                        + (((100. - y) / 100.) * settings.ChartDimensions.ActualHeight)

                    let x =
                        settings.ChartDimensions.LeftOffset
                        + ((normalizedX / 100.) * settings.ChartDimensions.ActualWidth)
                        
                    { CX = x
                      CY = invertedY
                      R = v.R |> Option.defaultValue 1.
                      Style =
                        { Fill = series.Style.Color.GetValue() |> Some
                          Stroke = None
                          StrokeWidth = None
                          Opacity = Some 1.
                          GenericValues = Map.empty } }
                    |> Circle
                    |> Element.GetString))

        chart @ renderPoints
        |> String.concat Environment.NewLine
        |> boilerPlate true vbWidth vbHeight
