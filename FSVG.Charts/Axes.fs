namespace FSVG.Charts

open System

module Axes =

    [<RequireQualifiedAccess>]
    type AxisType<'T> =
        | Static of StaticAxisSettings
        | Dynamic of DynamicAxisSettings<'T>

    and StaticAxisSettings =
        { Markers: string list
          Label: string option
          ChartDimensions: ChartDimensions }

    and DynamicAxisSettings<'T> =
        { MajorMarkers: float list
          MinorMarkers: float list
          ValueSplitter: ValueSplitter<'T>
          MaxValue: 'T
          MinValue: 'T
          Label: string option
          ChartDimensions: ChartDimensions }

        member das.SplitValue(percent: float) =
            { MinValue = das.MinValue
              MaxValue = das.MaxValue
              Percentage = percent }
            |> das.ValueSplitter

    let private xAxis (dimensions: ChartDimensions) =
        $"""<path d="M {dimensions.LeftOffset} {dimensions.Bottom} L {dimensions.LeftOffset + dimensions.ActualWidth} {dimensions.Bottom}" fill="none" stroke="grey" stroke-width="0.2" />"""

    let private yAxis (dimensions: ChartDimensions) =
        $"""<path d="M {dimensions.LeftOffset} {dimensions.BottomOffset} L {dimensions.LeftOffset} {dimensions.Bottom}" fill="none" stroke="grey" stroke-width="0.2" />"""

    let private createStaticXMarks (settings: StaticAxisSettings) =
        let height =
            settings.ChartDimensions.ActualHeight + settings.ChartDimensions.TopOffset

        let markWidth =
            settings.ChartDimensions.ActualWidth / float (settings.Markers.Length - 1)

        settings.Markers
        |> List.mapi (fun i pn ->
            let floatI = float i

            $"""<path d="M {settings.ChartDimensions.LeftOffset + (markWidth * floatI)} {height + 1.} L {settings.ChartDimensions.LeftOffset + (markWidth * floatI)} {height}" fill="none" stroke="grey" style="stroke-width: 0.1" />
                        <text x="{settings.ChartDimensions.LeftOffset + (markWidth * floatI)}" y="{height + 3.}" style="font-size: 2px; text-anchor: middle; font-family: 'roboto'">{pn}</text>""")

    let private createDynamicYMarks<'T> (settings: DynamicAxisSettings<'T>) =

        let zero =
            let y = settings.ChartDimensions.ActualHeight + settings.ChartDimensions.TopOffset
            let value = settings.SplitValue 0.

            $"""<path d="M {settings.ChartDimensions.LeftOffset - 1.} {y} L {settings.ChartDimensions.LeftOffset} {y}" fill="none" stroke="grey" style="stroke-width: 0.2" />
                            <text x="{8}" y="{y + 0.5}" style="font-size: 2px; text-anchor: end; font-family: 'roboto'">{value}</text>"""

        let major =
            settings.MajorMarkers
            |> List.map (fun m ->
                let y =
                    (settings.ChartDimensions.ActualHeight + settings.ChartDimensions.TopOffset)
                    - ((m / 100.) * settings.ChartDimensions.ActualHeight) // + settings.TopOffset
                //(float normalizedValue / 100.) * float maxHeight
                let value = settings.SplitValue m

                $"""<path d="M {settings.ChartDimensions.LeftOffset - 1.} {y} L {settings.ChartDimensions.ActualWidth + settings.ChartDimensions.LeftOffset} {y}" fill="none" stroke="grey" style="stroke-width: 0.2" />
                            <text x="{8}" y="{y + 0.5}" style="font-size: 2px; text-anchor: end; font-family: 'roboto'">{value}</text>""")

        let minor =
            settings.MinorMarkers
            |> List.map (fun m ->
                let y =
                    (settings.ChartDimensions.ActualHeight + settings.ChartDimensions.TopOffset)
                    - ((m / 100.) * settings.ChartDimensions.ActualHeight) // + settings.TopOffset
                //(float normalizedValue / 100.) * float maxHeight
                let value = settings.SplitValue m

                $"""<path d="M {settings.ChartDimensions.LeftOffset - 1.} {y} L {settings.ChartDimensions.ActualHeight + settings.ChartDimensions.LeftOffset} {y}" fill="none" stroke="grey" style="stroke-width: 0.1" />
                            <text x="{8}" y="{y + 0.5}" style="font-size: 2px; text-anchor: end; font-family: 'roboto'">{value}</text>""")

        [ zero; yield! major; yield! minor ]

    let private createXLabel (dimensions: ChartDimensions) (label: string) =
        $"""<text x="{dimensions.XMiddle}" y="{dimensions.Bottom + 6.}" style="font-size: 2px; text-anchor: middle; font-family: 'roboto'">{label}</text>"""

    let private createYLabel (dimensions: ChartDimensions) (label: string) =
        $"""<text x="{2.}" y="{dimensions.YMiddle}" style="font-size: 2px; text-anchor: middle; font-family: 'roboto'; transform: rotate(270deg) translateX(-52px) translateY(-48px)">{label}</text>"""

    let createXAxis<'T> (axisType: AxisType<'T>) =
        match axisType with
        | AxisType.Static settings ->
            [ xAxis settings.ChartDimensions
              yield! createStaticXMarks settings
              match settings.Label with
              | Some l -> createXLabel settings.ChartDimensions l
              | None -> () ]
        | AxisType.Dynamic settings -> failwith "TODO"

    let createYAxis<'T> (axisType: AxisType<'T>) =
        match axisType with
        | AxisType.Static settings -> failwith "TODO"
        | AxisType.Dynamic settings ->
            [ yAxis settings.ChartDimensions
              yield! createDynamicYMarks settings
              match settings.Label with
              | Some l -> createYLabel settings.ChartDimensions l
              | None -> () ]
