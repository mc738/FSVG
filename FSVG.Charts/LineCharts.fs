namespace FSVG.Charts

open System.IO

[<RequireQualifiedAccess>]
module LineCharts =

    open System
    open FSVG
    
    type SeriesCollection<'T> =
        { SplitValueHandler: float -> 'T -> 'T -> string
          Normalizer: ValueNormalizer<'T>
          PointNames: string list
          Series: Series<'T> list }

        member sc.SeriesLength() =
            sc.PointNames.Length

        member sc.Validate() =

            // Check all series are the same length and have the same "values"

            ()
    
    and Series<'T> =
        { Style: SeriesStyle
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
          YMajorMarks: float list
          YMinorMarks: float list }

    let private createTitle (settings: Settings) (width: float) =
        match settings.Title with
        | Some title ->
            $"""<text x="{settings.LeftOffset + (width / 2.)}" y="{5.}" style="font-size: 4px; text-anchor: middle; font-family: 'roboto'">{title}</text>"""
        | None -> String.Empty

    let private createXMarks (settings: Settings) (seriesCollection: SeriesCollection<'T>) (height: float) (barWidth: float) =
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

    let private createYMarks (settings: Settings) (height: float) (width: float) (maxValue: 'T) (minValue: 'T) (seriesCollection: SeriesCollection<'T>) =
        let zero =
            let y = float (height + settings.TopOffset)
            let value = seriesCollection.SplitValueHandler 0. maxValue minValue
            $"""<path d="M {settings.LeftOffset - 1.} {y} L {settings.LeftOffset} {y}" fill="none" stroke="grey" style="stroke-width: 0.2" />
                            <text x="{8}" y="{y + 0.5}" style="font-size: 2px; text-anchor: end; font-family: 'roboto'">{value}</text>"""
        
        let major =
            settings.YMajorMarks
            |> List.map (fun m ->
                let y = float (height + settings.TopOffset) - ((float m / 100.) * float height) // + settings.TopOffset
                //(float normalizedValue / 100.) * float maxHeight
                let value = seriesCollection.SplitValueHandler m maxValue minValue

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

    let generate (settings: Settings) (seriesCollection: SeriesCollection<'T>) (maxValue: 'T) (minValue: 'T) =
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
              createYMarks settings height width maxValue minValue seriesCollection ]

        let renderSeries =
            seriesCollection.Series
            |> List.collect (fun series ->
                series.Values
                |> List.mapi (fun i v ->
                    let value = seriesCollection.Normalizer { MaxValue = maxValue; MinValue = minValue; Value = v }
                
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
            
        chart @ renderSeries |> String.concat Environment.NewLine |> boilerPlate true
