namespace FSVG.Charts

open System

/// <summary>
/// This module is designed for internal use
/// </summary>
module Axes =

    [<RequireQualifiedAccess>]
    type AxisType<'T> =
        | Static of StaticAxisSettings
        | Dynamic of DynamicAxisSettings<'T>

    and StaticAxisSettings =
        { Markers: string list
          Label: string option
          DisplayType: AxisDisplayType
          ChartDimensions: ChartDimensions }
        
    and [<RequireQualifiedAccess>] AxisDisplayType =
        /// A section displays the title in the center.
        | Section
        // A marker displays the title on the left starting point.
        | Marker

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

    let private createStaticXMarkers (settings: StaticAxisSettings) =
        let height =
            settings.ChartDimensions.ActualHeight + settings.ChartDimensions.TopOffset

        let markWidth =
            match settings.DisplayType with
            | AxisDisplayType.Section -> settings.ChartDimensions.ActualWidth / float (settings.Markers.Length)
            | AxisDisplayType.Marker -> settings.ChartDimensions.ActualWidth / float (settings.Markers.Length - 1)

        settings.Markers
        |> List.mapi (fun i pn ->
            let floatI = float i
            
            let textX =
                match settings.DisplayType with
                | AxisDisplayType.Section -> settings.ChartDimensions.LeftOffset + (markWidth * floatI) + (markWidth / 2.)
                | AxisDisplayType.Marker -> settings.ChartDimensions.LeftOffset + (markWidth * floatI)
            
            $"""<path d="M {settings.ChartDimensions.LeftOffset + (markWidth * floatI)} {height + 1.} L {settings.ChartDimensions.LeftOffset + (markWidth * floatI)} {height}" fill="none" stroke="grey" style="stroke-width: 0.1" />
                        <text x="{textX}" y="{height + 3.}" style="font-size: 2px; text-anchor: middle; font-family: 'roboto'">{pn}</text>""")

    let private createDynamicXMarkers<'T> (settings: DynamicAxisSettings<'T>) =

        let zero =
            let value = settings.SplitValue 0.

            $"""<path d="M {settings.ChartDimensions.LeftOffset} {settings.ChartDimensions.Bottom + 1.} L {settings.ChartDimensions.LeftOffset} {settings.ChartDimensions.LeftOffset}" fill="none" stroke="grey" style="stroke-width: 0.2" />
                            <text x="{settings.ChartDimensions.LeftOffset}" y="{settings.ChartDimensions.Bottom + 3.}" style="font-size: 2px; text-anchor: middle; font-family: 'roboto'">{value}</text>"""

        let major =
            settings.MajorMarkers
            |> List.map (fun m ->
                let x =
                    (settings.ChartDimensions.LeftOffset)
                    + ((m / 100.) * settings.ChartDimensions.ActualWidth) // + settings.TopOffset
                //(float normalizedValue / 100.) * float maxHeight
                let value = settings.SplitValue m

                $"""<path d="M {x} {settings.ChartDimensions.Bottom + 1.} L {x} {settings.ChartDimensions.Bottom}" fill="none" stroke="grey" style="stroke-width: 0.2" />
                            <text x="{x}" y="{settings.ChartDimensions.Bottom + 3. + 0.5}" style="font-size: 2px; text-anchor: middle; font-family: 'roboto'">{value}</text>""")

        let minor =
            settings.MinorMarkers
            |> List.map (fun m ->
                let x =
                    (settings.ChartDimensions.LeftOffset)
                    + ((m / 100.) * settings.ChartDimensions.ActualWidth) // + settings.TopOffset
                //(float normalizedValue / 100.) * float maxHeight
                let value = settings.SplitValue m

                $"""<path d="M {x} {settings.ChartDimensions.Bottom + 1.} L {x} {settings.ChartDimensions.Bottom}" fill="none" stroke="grey" style="stroke-width: 0.2" />
                            <text x="{x}" y="{settings.ChartDimensions.Bottom + 3. + 0.5}" style="font-size: 2px; text-anchor: middle; font-family: 'roboto'">{value}</text>""")

        [ zero; yield! major; yield! minor ]

    let private createDynamicYMarkers<'T> (settings: DynamicAxisSettings<'T>) =

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
              yield! createStaticXMarkers settings
              match settings.Label with
              | Some l -> createXLabel settings.ChartDimensions l
              | None -> () ]
        | AxisType.Dynamic settings ->
            [ xAxis settings.ChartDimensions
              yield! createDynamicXMarkers settings
              match settings.Label with
              | Some l -> createXLabel settings.ChartDimensions l
              | None -> () ]

    let createYAxis<'T> (axisType: AxisType<'T>) =
        match axisType with
        | AxisType.Static settings -> failwith "TODO"
        | AxisType.Dynamic settings ->
            [ yAxis settings.ChartDimensions
              yield! createDynamicYMarkers settings
              match settings.Label with
              | Some l -> createYLabel settings.ChartDimensions l
              | None -> () ]
