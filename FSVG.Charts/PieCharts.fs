namespace FSVG.Charts

open FSVG

[<RequireQualifiedAccess>]
module PieCharts =

    open System

    type Settings =
        { ChartDimensions: ChartDimensions
          Title: string option
          IsDonut: bool }

    type SeriesCollection<'T> =
        { //SplitValueHandler: ValueSplitter<'T>
          Normalizer: ValueNormalizer<'T>
          Series: Series<'T> list }

        member sc.SeriesLength() = sc.Series.Length

        member sc.Validate() =

            // Check all series are the same length and have the same "values"

            ()

    and Series<'T> =
        { Name: string
          Style: SeriesStyle
          Value: 'T }

    and SeriesStyle = { Color: SvgColor }

    //type PTC = { X: int; Y: float  }

    let private ptc (center: SvgPoint) (radius: float) (angleInDegrees: float) =
        let angleInRadians = (angleInDegrees - 90.) * Math.PI / 180.

        let x = center.X + (radius * Math.Cos(angleInRadians))

        let y = center.Y + (radius * Math.Sin(angleInRadians))

        ({ X = x; Y = y }: SvgPoint)

    let private createSlice (center: SvgPoint) (radius: float) (startAngle: float) (endAngle: float) (isDonut: bool) =
        let start = ptc center radius endAngle
        let endP = ptc center radius startAngle

        let largeArchFlag = if endAngle - startAngle <= 180 then 0 else 1

        //match isDonut with
        //| true ->
        //    let innerStart = ptc center (radius / 2.) endAngle
        //    let innerEnd = ptc center (radius / 2.) startAngle
        //    $"M {start.X} {start.Y} A {radius} {radius} 0 {largeArchFlag} {0} {endP.X} {endP.Y} M {start.X} {start.Y} L {innerStart.X} {innerStart.Y} A {radius / 2.} {radius / 2.} 0 {largeArchFlag} {0} {innerEnd.X} {innerEnd.Y} L {endP.X} {endP.Y} z"
        //| false ->
        //    $"M {start.X} {start.Y} A {radius} {radius} 0 {largeArchFlag} {0} {endP.X} {endP.Y} L {center.X} {center.Y} z"
            
        $"M {start.X} {start.Y} A {radius} {radius} 0 {largeArchFlag} {0} {endP.X} {endP.Y} L {center.X} {center.Y} z"


    let private createPath center radius startAngle endAngle color isDonut =
        let cmd = createSlice center radius startAngle endAngle isDonut

        $"<path d=\"{cmd}\" fill=\"{color}\" stroke=\"{color}\" stroke-width=\"0\"></path>"

    let generate<'T> (settings: Settings) (seriesCollection: SeriesCollection<'T>) (minValue: 'T) (maxValue: 'T) =
        // Normalize value (to % basically)
        // multiply by 3.6 to get degrees.

        // Fold over series slices, use last one and current one as points
        let center = settings.ChartDimensions.CenterPoint

        let radius = settings.ChartDimensions.ActualWidth / 2.
        //let maxValue = handlers.MaxValue series

        let toDeg (value: float) = value * 3.6

        seriesCollection.Series
        |> List.fold
            (fun (acc, prevAngle) s ->
                let value =

                    ({ MaxValue = maxValue
                       MinValue = minValue
                       Value = s.Value }
                    : NormalizerParameters<'T>)
                    |> seriesCollection.Normalizer

                let newAngle = prevAngle + (toDeg value)

                printfn $"{value} {prevAngle} {newAngle}"

                acc
                @ [ createPath
                        (settings.ChartDimensions.CenterPoint)
                        radius
                        prevAngle
                        newAngle
                        (s.Style.Color.GetValue())
                        settings.IsDonut ],
                newAngle)
            ([], 0)
        |> fun (acc, _) ->
            match settings.IsDonut with
            | true ->
                acc
                @ [ ({ CX = settings.ChartDimensions.XMiddle
                       CY = settings.ChartDimensions.YMiddle
                       R = settings.ChartDimensions.ActualWidth / 2.
                       Style =
                         { Fill = (SvgColor.Named "white").GetValue() |> Some
                           Stroke = None
                           StrokeWidth = None
                           Opacity = Some 1.
                           GenericValues = Map.empty } }
                    : CircleElement)
                    |> Element.Circle
                    |> Element.GetString
                    |> "<><>" ]
            | false -> acc
            |> String.concat Environment.NewLine
            |> boilerPlate true 100. 100.
