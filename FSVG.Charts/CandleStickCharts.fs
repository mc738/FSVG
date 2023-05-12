namespace FSVG.Charts

[<RequireQualifiedAccess>]
module CandleStickCharts =

    open System
    open FSVG
    open FSVG.Charts.Axes

    type SeriesCollection<'T> =
        { SplitValueHandler: ValueSplitter<'T>
          Normalizer: ValueNormalizer<'T>
          ValueComparer: ValueComparer<'T>
          SectionNames: string list
          Series: Series<'T> list }

        member sc.SeriesLength() = sc.SectionNames.Length

        member sc.Validate() =

            // Check all series are the same length and have the same "values"

            ()

    and Series<'T> =
        { Name: string
          Style: SeriesStyle
          OpenValue: 'T
          CloseValue: 'T
          HighValue: 'T
          LowValue: 'T }

    and SeriesStyle = { Color: SvgColor; StokeWidth: float }

    type Settings =
        { ChartDimensions: ChartDimensions
          Title: string option
          XLabel: string option
          YLabel: string option
          LegendStyle: LegendStyle option
          SectionPadding: PaddingType
          MajorMarks: float list
          MinorMarks: float list }

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
            settings.ChartDimensions.ActualWidth / float (seriesCollection.SeriesLength())

        let sectionPadding =
            match settings.SectionPadding with
            | PaddingType.Specific v -> v
            | PaddingType.Percent v -> (sectionWidth / 100.) * v

        let barWidth =
            (sectionWidth - (sectionPadding * 2.))
            

        let bars =
            seriesCollection.Series
            |> List.mapi (fun i s ->
                let normalizeValue (value: 'T) =
                    ({ MaxValue = maxValue
                       MinValue = minValue
                       Value = value })
                    |> seriesCollection.Normalizer
                    
                let top, bottom, color =
                    // ValueA is CloseValue because this is comparing if the value went up or down over the period.
                    match
                        { ValueA = s.CloseValue
                          ValueB = s.OpenValue }
                        |> seriesCollection.ValueComparer
                    with
                    | ValueComparisonResult.GreaterThan ->
                        normalizeValue s.CloseValue, normalizeValue  s.OpenValue, SvgColor.Named "green"
                    | ValueComparisonResult.LessThan ->
                        normalizeValue s.OpenValue, normalizeValue s.CloseValue, SvgColor.Named "red"
                    | ValueComparisonResult.Equal ->
                        normalizeValue s.CloseValue, normalizeValue  s.OpenValue, SvgColor.Named "green"

                let height = (settings.ChartDimensions.ActualHeight / 100. ) * (top - bottom)
                    
                [ ({ Height = height
                     Width = barWidth
                     X =
                       settings.ChartDimensions.LeftOffset
                       + sectionPadding
                       + (float i * sectionWidth)
                     Y =
                       settings.ChartDimensions.BottomOffset +
                       (((100. - bottom - (top - bottom)) / 100.) * settings.ChartDimensions.ActualHeight)
                     RX = 0.
                     RY = 0.
                     Style =
                       { Fill = color.GetValue() |> Some
                         Stroke = None
                         StrokeWidth = None
                         Opacity = Some 1.
                         GenericValues = Map.empty } }
                  : RectElement)
                  |> Element.Rect
                  |> Element.GetString

                  ({ X1 =
                       settings.ChartDimensions.LeftOffset
                       + (float i * sectionWidth)
                       + (sectionWidth / 2.)
                     X2 =
                       settings.ChartDimensions.LeftOffset
                       + (float i * sectionWidth)
                       + (sectionWidth / 2.)
                     Y1 =
                         settings.ChartDimensions.BottomOffset
                         + ((100. - normalizeValue s.HighValue) / 100.) * settings.ChartDimensions.ActualHeight
                     Y2 =
                         settings.ChartDimensions.BottomOffset
                         + ((100. - normalizeValue s.LowValue) / 100.) * settings.ChartDimensions.ActualHeight
                     Style =
                       { Fill = None
                         Stroke = color.GetValue() |> Some
                         StrokeWidth = Some 1.
                         Opacity = Some 1.
                         GenericValues = Map.empty } }
                  : LineElement)
                  |> Element.Line
                  |> Element.GetString ])
            |> List.concat

        [ createTitle settings vbWidth
          yield! xAxis
          yield! yAxis
          yield! createLegend settings seriesCollection
          yield! bars ]
        |> String.concat Environment.NewLine
        |> boilerPlate true vbWidth vbHeight
