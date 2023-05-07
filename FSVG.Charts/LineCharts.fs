﻿namespace FSVG.Charts

open System.IO
open FSVG

[<RequireQualifiedAccess>]
module LineCharts =

    open System
    open FSVG

    type SeriesCollection<'T> =
        { SplitValueHandler: float -> 'T -> 'T -> string
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
        { BottomOffset: float
          LeftOffset: float
          TopOffset: float
          RightOffset: float
          Title: string option
          XLabel: string option
          YLabel: string option
          LegendStyle: LegendStyle option
          YMajorMarks: float list
          YMinorMarks: float list }

    and LegendStyle =
        { Position: LegendPosition
          Bordered: bool }

    let private createTitle (settings: Settings) (width: float) =
        match settings.Title with
        | Some title ->
            $"""<text x="{settings.LeftOffset + (width / 2.)}" y="{5.}" style="font-size: 4px; text-anchor: middle; font-family: 'roboto'">{title}</text>"""
        | None -> String.Empty

    let private createXMarks
        (settings: Settings)
        (seriesCollection: SeriesCollection<'T>)
        (height: float)
        (barWidth: float)
        =
        let height = height + settings.TopOffset

        seriesCollection.PointNames
        |> List.mapi (fun i pn ->
            let floatI = float i

            $"""<path d="M {settings.LeftOffset + (barWidth * floatI)} {height + 1.} L {settings.LeftOffset + (barWidth * floatI)} {height}" fill="none" stroke="grey" style="stroke-width: 0.1" />
                        <text x="{settings.LeftOffset + (barWidth * floatI)}" y="{height + 3.}" style="font-size: 2px; text-anchor: middle; font-family: 'roboto'">{pn}</text>""")
        |> String.concat Environment.NewLine

    let private createXLabel (settings: Settings) (height: float) (width: float) =
        match settings.XLabel with
        | Some label ->
            let height = height + settings.TopOffset
            $"""<text x="{settings.LeftOffset + (width / 2.)}" y="{height + 6.}" style="font-size: 2px; text-anchor: middle; font-family: 'roboto'">{label}</text>"""
        | None -> String.Empty

    let private createYLabel (settings: Settings) (height: float) (width: float) =
        match settings.YLabel with
        | Some label ->
            // The 52/ 48 in the transform come from the fact that font size is 2.
            $"""<text x="{2.}" y="{settings.TopOffset + (height / 2.)}" style="font-size: 2px; text-anchor: middle; font-family: 'roboto'; transform: rotate(270deg) translateX(-52px) translateY(-48px)">{label}</text>"""

        | None -> String.Empty

    let private createYMarks
        (settings: Settings)
        (height: float)
        (width: float)
        (maxValue: 'T)
        (minValue: 'T)
        (seriesCollection: SeriesCollection<'T>)
        =
        let zero =
            let y = float (height + settings.TopOffset)
            let value = seriesCollection.SplitValueHandler 0. minValue maxValue

            $"""<path d="M {settings.LeftOffset - 1.} {y} L {settings.LeftOffset} {y}" fill="none" stroke="grey" style="stroke-width: 0.2" />
                            <text x="{8}" y="{y + 0.5}" style="font-size: 2px; text-anchor: end; font-family: 'roboto'">{value}</text>"""

        let major =
            settings.YMajorMarks
            |> List.map (fun m ->
                let y = float (height + settings.TopOffset) - ((float m / 100.) * float height) // + settings.TopOffset
                //(float normalizedValue / 100.) * float maxHeight
                let value = seriesCollection.SplitValueHandler m minValue maxValue

                $"""<path d="M {settings.LeftOffset - 1.} {y} L {width + settings.LeftOffset} {y}" fill="none" stroke="grey" style="stroke-width: 0.2" />
                            <text x="{8}" y="{y + 0.5}" style="font-size: 2px; text-anchor: end; font-family: 'roboto'">{value}</text>""")
            |> String.concat Environment.NewLine


        let minor =
            settings.YMinorMarks
            |> List.map (fun m ->
                let y = float (height + settings.TopOffset) - ((float m / 100.) * float height) // + settings.TopOffset
                //(float normalizedValue / 100.) * float maxHeight
                let value = seriesCollection.SplitValueHandler m maxValue minValue

                $"""<path d="M {settings.LeftOffset - 1.} {y} L {width + settings.LeftOffset} {y}" fill="none" stroke="grey" style="stroke-width: 0.1" />
                            <text x="{8}" y="{y + 0.5}" style="font-size: 2px; text-anchor: end; font-family: 'roboto'">{value}</text>""")
            |> String.concat Environment.NewLine

        [ zero; major; minor ] |> String.concat Environment.NewLine

    let private createYAxis (bottomOffset: int) (leftOffset: int) (height: int) =
        $"""<path d="M {leftOffset} {bottomOffset} L {leftOffset} {bottomOffset + height}" fill="none" stroke="grey" stroke-width="0.2" />"""

    let private createXAxis (height: int) (leftOffset: int) (length: int) =
        $"""<path d="M {leftOffset} {height} L {leftOffset + length} {height}" fill="none" stroke="grey" stroke-width="0.2" />"""


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

    let generate (settings: Settings) (seriesCollection: SeriesCollection<'T>) (minValue: 'T) (maxValue: 'T) =
        let (vbHeight, vbWidth) =
            match settings.LegendStyle with
            | Some legendStyle ->
                match legendStyle.Position with
                | LegendPosition.Right -> 100, 120
                | LegendPosition.Bottom -> 120, 100
            | None -> 100, 100


        // TODO validate series?
        let height = 100. - settings.TopOffset - settings.BottomOffset

        let width = 100. - settings.LeftOffset - settings.RightOffset

        let pointWidth = width / float (seriesCollection.SeriesLength() - 1)

        let chart =
            [ createTitle settings width
              createXAxis 90 10 80
              createYAxis 10 10 80
              createXMarks settings seriesCollection height pointWidth
              createXLabel settings height width
              createYMarks settings height width maxValue minValue seriesCollection
              createYLabel settings height width
              createLegend settings seriesCollection ]

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

                    let invertedY = settings.BottomOffset + (((100. - value) / 100.) * height)

                    { X = settings.LeftOffset + (float i * pointWidth)
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
                          $"""<path d="{path} L {100. - settings.RightOffset} {100. - settings.BottomOffset} L {settings.LeftOffset} {100. - settings.BottomOffset} Z" fill="{s.Color.GetValue()}" stroke="none" style="stroke-width: {series.Style.StokeWidth}" />"""
                      | None -> () ])

        chart @ renderSeries
        |> String.concat Environment.NewLine
        |> boilerPlate true vbWidth vbHeight
