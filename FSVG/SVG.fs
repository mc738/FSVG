namespace FSVG

open FSVG

module Helpers =

    open System

    type Line =
        { Length: double
          Angle: double }

        static member Create(pointA: SvgPoint, pointB: SvgPoint) =
            let lengthX = pointB.X - pointA.X |> double
            let lengthY = pointB.Y - pointA.Y |> double

            { Length = Math.Sqrt(Math.Pow(lengthX, 2.) + Math.Pow(lengthY, 2.))
              Angle = Math.Atan2(lengthY, lengthX) }

    let createControlPoint (current: SvgPoint) (previous: SvgPoint option) (next: SvgPoint option) (reverse: bool) =
        let pPoint =
            match previous with
            | Some p -> p
            | None -> current

        let nPoint =
            match next with
            | Some p -> p
            | None -> current

        let smoothing = 0.2
        let opLine = Line.Create(pPoint, nPoint)

        let angle =
            match reverse with
            | true -> opLine.Angle + Math.PI
            | false -> opLine.Angle

        let length = opLine.Length * smoothing

        { X = current.X + Math.Cos(angle) * length
          Y = current.Y + Math.Sin(angle) * length }

    let createBezierCommands (points: SvgPoints) =
        points.Values
        |> Array.mapi (fun i p ->
            match i = 0 with
            | true ->
                ({ Point = { X = p.X; Y = p.Y }
                   IsRelative = false }: MoveToCommand)
                |> PathCommand.MoveTo
            | false ->
                let point = points.Values.[i - 1]

                let startP =
                    createControlPoint point (points.Get(i - 2)) (Some p) false

                let endP =
                    createControlPoint p (Some point) (points.Get(i + 1)) true

                ({ ControlPoint1 = { X = startP.X; Y = startP.Y }
                   ControlPoint2 = { X = endP.X; Y = endP.Y }
                   FinalPoint = { X = p.X; Y = p.Y }
                   IsRelative = false }: CurveToCommand)
                |> PathCommand.CurveTo)
        |> List.ofArray

    let createBezierElement (style: Style) (points: SvgPoints) =
        createBezierCommands points
        |> fun cmds -> ({ Commands = cmds; Style = style }: PathElement)
        |> Element.Path
