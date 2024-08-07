﻿namespace FSVG.Charts

open FSVG
open FSVG.Charts.Axes

[<RequireQualifiedAccess>]
module BarCharts =

    open System
    open FSVG

    type SeriesCollection<'T> =
        { SplitValueHandler: ValueSplitter<'T>
          Normalizer: ValueNormalizer<'T>
          SectionNames: string list
          Series: Series<'T> list }

        member sc.SeriesLength() = sc.SectionNames.Length

        member sc.Validate() =

            // Check all series are the same length and have the same "values"

            ()

    and Series<'T> =
        { Name: string
          Style: SeriesStyle
          Values: 'T list }

        member s.GetValue(index: int, defaultValue: 'T) =
            s.Values |> List.tryItem index |> Option.defaultValue defaultValue

    and SeriesStyle = { Color: SvgColor; StokeWidth: float }

    type Settings =
        { ChartDimensions: ChartDimensions
          Title: string option
          XLabel: string option
          YLabel: string option
          LegendStyle: LegendStyle option
          ChartDirection: ChartDirection
          SectionPadding: PaddingType
          MajorMarks: float list
          MinorMarks: float list }

    and [<RequireQualifiedAccess>] ChartDirection =
        | Horizontal
        | Vertical

    let private createTitle (settings: Settings) (width: float) =
        match settings.Title with
        | Some title ->
            $"""<text x="{width / 2.}" y="{5.}" style="font-size: 4px; text-anchor: middle; font-family: 'roboto'">{title}</text>"""
        | None -> String.Empty

    let private createLegend (settings: Settings) (seriesCollection: SeriesCollection<'T>) =
        match settings.LegendStyle with
        | None -> []
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
                             StrokeLineCap = None
                             StrokeDashArray = None
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
                             StrokeLineCap = None
                             StrokeDashArray = None
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
            //|> String.concat Environment.NewLine
            | LegendPosition.Bottom -> []

    let generate<'T> (settings: Settings) (seriesCollection: SeriesCollection<'T>) (minValue: 'T) (maxValue: 'T) =
        let (vbHeight, vbWidth) =
            match settings.LegendStyle with
            | Some legendStyle ->
                match legendStyle.Position with
                | LegendPosition.Right -> 100, 120
                | LegendPosition.Bottom -> 120, 100
            | None -> 100, 100

        let chart =
            match settings.ChartDirection with
            | ChartDirection.Horizontal -> failwith "TODO"
            | ChartDirection.Vertical ->
                let xAxis =
                    ({ Markers = seriesCollection.SectionNames
                       Label = settings.XLabel
                       DisplayType = AxisDisplayType.Section 
                       ChartDimensions = settings.ChartDimensions }
                    : Axes.StaticAxisSettings)
                    |> Axes.AxisType.Static
                    |> Axes.createXAxis<'T>

                let yAxis =
                    ({ MajorMarkers = settings.MajorMarks
                       MinorMarkers = settings.MinorMarks
                       ValueSplitter = seriesCollection.SplitValueHandler
                       MaxValue = maxValue
                       MinValue = minValue
                       Label = settings.YLabel
                       ChartDimensions = settings.ChartDimensions }
                    : Axes.DynamicAxisSettings<'T>)
                    |> Axes.AxisType.Dynamic
                    |> Axes.createYAxis<'T>

                let sectionWidth =
                    settings.ChartDimensions.ActualWidth
                    / float (seriesCollection.SeriesLength())

                let sectionPadding =
                    match settings.SectionPadding with
                    | PaddingType.Specific v -> v
                    | PaddingType.Percent v -> (sectionWidth / 100.) * v

                let barWidth =
                    (sectionWidth - (sectionPadding * 2.))
                    / (float <| seriesCollection.Series.Length)

                let bars =
                    seriesCollection.Series
                    |> List.mapi (fun sI s ->

                        s.Values
                        |> List.mapi (fun vI v ->

                            let value =
                                seriesCollection.Normalizer
                                    { MaxValue = maxValue
                                      MinValue = minValue
                                      Value = v }


                            let height = (value / 100.) * settings.ChartDimensions.ActualHeight

                            (*
                            let s1 =
                                $"""<rect width="{barWidth}" height="{height}" x="{startPoint.X}" y="{float startPoint.Y - height}" fill="{color}" />
                                    <text x="{float startPoint.X + (float width / 2.)}" y="{float startPoint.Y - height - 2.}" style="font-size: 2px; text-anchor: middle; font-family: 'roboto'">{valueLabel}</text>"""
                            *)

                            ({ Height = height
                               Width = barWidth
                               X =
                                 settings.ChartDimensions.LeftOffset
                                 + (float vI * sectionWidth)
                                 + sectionPadding
                                 + (float sI * barWidth)
                               Y =
                                 settings.ChartDimensions.BottomOffset + settings.ChartDimensions.ActualHeight
                                 - height
                               RX = 0.
                               RY = 0.
                               Style =
                                 { Fill = s.Style.Color.GetValue() |> Some
                                   Stroke = None
                                   StrokeWidth = None
                                   Opacity = Some 1.
                                   StrokeLineCap = None
                                   StrokeDashArray = None
                                   GenericValues = Map.empty } }
                            : RectElement)
                            |> Element.Rect
                            |> Element.GetString))
                    |> List.concat

                [ createTitle settings vbWidth
                  yield! xAxis
                  yield! yAxis
                  yield! createLegend settings seriesCollection
                  yield! bars ]

        chart |> String.concat Environment.NewLine |> boilerPlate true vbWidth vbHeight
