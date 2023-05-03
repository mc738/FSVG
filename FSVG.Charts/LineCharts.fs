namespace FSVG.Charts

open System.IO

[<RequireQualifiedAccess>]
module LineCharts =

    open System
    open FSVG

    type Series<'T> =
        { Normalizer: ValueNormalizer<'T>
          //ToString: 'T -> string
          SplitValueHandler: float -> 'T -> string
          Points: LineChartPoint<'T> list }

    and LineChartPoint<'T> = { Name: string; Value: 'T }

    type Settings =
        { BottomOffset: float
          LeftOffset: float
          TopOffset: float
          RightOffset: float
          Title: string option
          XLabel: string option
          YMajorMarks: float list
          YMinorMarks: float list }

    let private createTitle (settings: Settings) (width: float) =
        match settings.Title with
        | Some title ->
            $"""<text x="{settings.LeftOffset + (width / 2.)}" y="{5.}" style="font-size: 4px; text-anchor: middle; font-family: 'roboto'">{title}</text>"""
        | None -> String.Empty

    let private createXMarks (settings: Settings) (series: Series<'T>) (height: float) (barWidth: float) =
        let height = height + settings.TopOffset

        series.Points
        |> List.mapi (fun i p ->
            let floatI = float i

            $"""<path d="M {settings.LeftOffset + (barWidth * floatI)} {height + 1.} L {settings.LeftOffset + (barWidth * floatI)} {height}" fill="none" stroke="grey" style="stroke-width: 0.1" />
                        <text x="{settings.LeftOffset + (barWidth * floatI)}" y="{height + 3.}" style="font-size: 2px; text-anchor: middle; font-family: 'roboto'">{p.Name}</text>""")
        |> String.concat Environment.NewLine

    let private createXLabel (settings: Settings) (height: float) (width: float) =
        match settings.XLabel with
        | Some label ->
            let height = height + settings.TopOffset
            $"""<text x="{settings.LeftOffset + (width / 2.)}" y="{height + 6.}" style="font-size: 2px; text-anchor: middle; font-family: 'roboto'">{label}</text>"""
        | None -> String.Empty

    let private createYMarks (settings: Settings) (height: float) (width: float) (maxValue: 'T) (series: Series<'T>) =
        let major =
            settings.YMajorMarks
            |> List.map (fun m ->
                let y = float (height + settings.TopOffset) - ((float m / 100.) * float height) // + settings.TopOffset
                //(float normalizedValue / 100.) * float maxHeight
                let value = series.SplitValueHandler m maxValue

                $"""<path d="M {settings.LeftOffset - 1.} {y} L {width + settings.LeftOffset} {y}" fill="none" stroke="grey" style="stroke-width: 0.2" />
                            <text x="{8}" y="{y + 0.5}" style="font-size: 2px; text-anchor: end; font-family: 'roboto'">{value}</text>""")
            |> String.concat Environment.NewLine


        let minor =
            settings.YMinorMarks
            |> List.map (fun m ->
                let y = float (height + settings.TopOffset) - ((float m / 100.) * float height) // + settings.TopOffset
                //(float normalizedValue / 100.) * float maxHeight
                let value = series.SplitValueHandler m maxValue

                $"""<path d="M {settings.LeftOffset - 1.} {y} L {width + settings.LeftOffset} {y}" fill="none" stroke="grey" style="stroke-width: 0.1" />
                            <text x="{8}" y="{y + 0.5}" style="font-size: 2px; text-anchor: end; font-family: 'roboto'">{value}</text>""")
            |> String.concat Environment.NewLine

        [ major; minor ] |> String.concat Environment.NewLine

    let private createYAxis (bottomOffset: int) (leftOffset: int) (height: int) =
        $"""<path d="M {leftOffset} {bottomOffset} L {leftOffset} {bottomOffset + height}" fill="none" stroke="grey" stroke-width="0.2" />"""

    let private createXAxis (height: int) (leftOffset: int) (length: int) =
        $"""<path d="M {leftOffset} {height} L {leftOffset + length} {height}" fill="none" stroke="grey" stroke-width="0.2" />"""

    let private generatePoint
        (startPoint: SvgPoint)
        (width: int)
        (maxHeight: int)
        (normalizedValue: int)
        (color: string)
        (valueLabel: string)
        =
        let height = (float normalizedValue / 100.) * float maxHeight

        $"""<rect width="{width}" height="{height}" x="{startPoint.X}" y="{float startPoint.Y - height}" fill="{color}" />
                <text x="{float startPoint.X + (float width / 2.)}" y="{float startPoint.Y - height - 2.}" style="font-size: 2px; text-anchor: middle; font-family: 'roboto'">{valueLabel}</text>"""

    let generate (settings: Settings) (series: Series<'T>) (maxValue: 'T) =
        let height = 100. - settings.TopOffset - settings.BottomOffset

        let width = 100. - settings.LeftOffset - settings.RightOffset

        let pointWidth = width / float (series.Points.Length - 1)

        let chart =
            [ createTitle settings width
              createXAxis 90 10 80
              createYAxis 10 10 80
              createXMarks settings series height pointWidth
              createXLabel settings height width
              createYMarks settings height width maxValue series ]


        series.Points
        |> List.mapi (fun i p ->
            let value = series.Normalizer { MaxValue = maxValue; Value = p.Value }

            let invertedY = settings.BottomOffset + (((100. - value) / 100.) * height)

            { X = settings.LeftOffset + (float i * pointWidth)
              Y = invertedY })
        |> fun p ->
            { Values = p |> Array.ofList
              CurrentIndex = 0 }
        |> Helpers.createBezierCommands
        |> fun r ->
            let path = r |> List.map (fun c -> c.Render()) |> String.concat " "
            [ $"""<path d="{path}" fill="none" stroke="grey" style="stroke-width: 0.3" />"""
              $"""<path d="{path} L {100. - settings.RightOffset} {100. - settings.BottomOffset} L {settings.LeftOffset} {100. - settings.BottomOffset} Z" fill="rgba(255,0,0,0.1)" stroke="none" style="stroke-width: 0.3" />""" ]
        |> fun r -> chart @ r |> String.concat Environment.NewLine |> boilerPlate true
