namespace FSVG.Charts

open System.IO
open FSVG
open FSVG.Charts.Axes

[<RequireQualifiedAccess>]
module LineCharts =

    open System
    open FSVG

    type SeriesCollection<'T> =
        { SplitValueHandler: ValueSplitter<'T>
          Normalizer: ValueNormalizer<'T>
          PointNames: string list
          Series: Series<'T> list }

        member sc.SeriesLength() = sc.PointNames.Length

        member sc.Validate() =

            // Check all series are the same length and have the same "values"

            ()

    and Series<'T> =
        { Name: string
          Style: SeriesStyle
          Values: 'T list }

        member s.GetValue(index: int, defaultValue: 'T) =
            s.Values |> List.tryItem index |> Option.defaultValue defaultValue

    and SeriesStyle =
        { Color: SvgColor
          StokeWidth: float
          LineType: LineType
          Shading: ShadingOptions option }

    and ShadingOptions = { Color: SvgColor }

    and [<RequireQualifiedAccess>] LineType =
        | Bezier
        | Straight

    type Settings =
        { ChartDimensions: ChartDimensions
          Title: string option
          XLabel: string option
          YLabel: string option
          LegendStyle: LegendStyle option
          YMajorMarkers: float list
          YMinorMarkers: float list }

    let private createTitle (settings: Settings) (width: float) =
        match settings.Title with
        | Some title ->
            $"""<text x="{width / 2.}" y="{5.}" style="font-size: 4px; text-anchor: middle; font-family: 'roboto'">{title}</text>"""
        | None -> String.Empty

    let private createLegend (settings: Settings) (seriesCollection: SeriesCollection<'T>) =
        match settings.LegendStyle with
        | None -> String.Empty
        | Some value ->
            match value.Position with
            | LegendPosition.Right ->
                let legendHeight =
                    seriesCollection.Series.Length * 2 + ((seriesCollection.Series.Length - 1) * 2)
                    |> float

                let start = 50. - (legendHeight / 2.)

                seriesCollection.Series
                |> List.mapi (fun i s ->
                    let y = ((float i * 2.) + (float i * 2.)) + start

                    [ Dsl.rect
                          ({ Fill = s.Style.Color.GetValue() |> Some
                             Opacity = Some 1.
                             Stroke = None
                             StrokeWidth = None
                             GenericValues = Map.empty }
                          : FSVG.Common.Style)
                          0.
                          0.
                          2.
                          2.
                          100.
                          y
                      |> Element.GetString
                      Dsl.text
                          ({ Fill = SvgColor.Black.GetValue() |> Some
                             Stroke = None
                             StrokeWidth = None
                             Opacity = Some 1.
                             GenericValues =
                               [ "font-family", "roboto"
                                 "font-size", "2px"
                                 // This is from testing. It could be tweaked.
                                 "transform", "translateY(1.75%)" ]
                               |> Map.ofList }
                          : FSVG.Common.Style)
                          104.
                          y
                          [ TextType.Literal s.Name ]
                      |> Element.GetString ])
                |> List.concat
                |> String.concat Environment.NewLine
            | LegendPosition.Bottom -> String.Empty

    let generate<'T> (settings: Settings) (seriesCollection: SeriesCollection<'T>) (minValue: 'T) (maxValue: 'T) =
        let (vbHeight, vbWidth) =
            match settings.LegendStyle with
            | Some legendStyle ->
                match legendStyle.Position with
                | LegendPosition.Right -> 100, 120
                | LegendPosition.Bottom -> 120, 100
            | None -> 100, 100


        // TODO validate series?
        //let height = 100. - settings.TopOffset - settings.BottomOffset

        //let width = 100. - settings.LeftOffset - settings.RightOffset

        //let pointWidth = width / float (seriesCollection.SeriesLength() - 1)

        let xAxis =
            ({ Markers = seriesCollection.PointNames
               Label = settings.XLabel
               DisplayType = AxisDisplayType.Marker
               ChartDimensions = settings.ChartDimensions }
            : Axes.StaticAxisSettings)
            |> Axes.AxisType.Static
            |> Axes.createXAxis<'T>

        let yAxis =
            ({ MajorMarkers = settings.YMajorMarkers
               MinorMarkers = settings.YMinorMarkers
               ValueSplitter = seriesCollection.SplitValueHandler
               MaxValue = maxValue
               MinValue = minValue
               Label = settings.YLabel
               ChartDimensions = settings.ChartDimensions }
            : Axes.DynamicAxisSettings<'T>)
            |> Axes.AxisType.Dynamic
            |> Axes.createYAxis<'T>

        let chart =
            [ createTitle settings vbWidth
              yield! xAxis
              yield! yAxis
              createLegend settings seriesCollection ]

        let pointWidth =
            settings.ChartDimensions.ActualWidth
            / float (seriesCollection.SeriesLength() - 1)

        let renderSeries =
            seriesCollection.Series
            |> List.collect (fun series ->
                series.Values
                |> List.mapi (fun i v ->
                    let value =
                        seriesCollection.Normalizer
                            { MaxValue = maxValue
                              MinValue = minValue
                              Value = v }

                    let invertedY =
                        settings.ChartDimensions.BottomOffset
                        + (((100. - value) / 100.) * settings.ChartDimensions.ActualHeight)

                    { X = settings.ChartDimensions.LeftOffset + (float i * pointWidth)
                      Y = invertedY })
                |> fun p ->
                    { Values = p |> Array.ofList
                      CurrentIndex = 0 }
                |> fun ps ->
                    match series.Style.LineType with
                    | LineType.Bezier -> Helpers.createBezierCommands ps
                    | LineType.Straight -> Helpers.createStraightCommands ps
                |> fun r ->
                    let path = r |> List.map (fun c -> c.Render()) |> String.concat " "

                    [ $"""<path d="{path}" fill="none" stroke="{series.Style.Color.GetValue()}" style="stroke-width: {series.Style.StokeWidth}" />"""
                      match series.Style.Shading with
                      | Some s ->
                          $"""<path d="{path} L {settings.ChartDimensions.Right} {settings.ChartDimensions.Bottom} L {settings.ChartDimensions.LeftOffset} {settings.ChartDimensions.Bottom} Z" fill="{s.Color.GetValue()}" stroke="none" style="stroke-width: {series.Style.StokeWidth}" />"""
                      | None -> () ])

        chart @ renderSeries
        |> String.concat Environment.NewLine
        |> boilerPlate true vbWidth vbHeight
