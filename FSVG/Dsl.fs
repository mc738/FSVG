namespace FSVG

open System.IO
open FSVG

module Dsl =

    let svg elements = ({ Elements = elements }: SvgDocument)

    let saveToFile path vbHeight vbWidth (svg: SvgDocument) = File.WriteAllText(path, svg.Render(vbHeight, vbWidth))

    let point x y = SvgPoint.Create(x, y)

    let points (ps) = SvgPoints.Create ps

    let style (genericValues: Map<string, string>) fill stroke strokeWidth opacity =
        ({ Fill = fill
           Stroke = stroke
           StrokeWidth = strokeWidth
           Opacity = opacity
           GenericValues = genericValues }: Style)

    let rect (style: Style) (rx: float) (ry: float) (height: float) (width: float) (x: float) (y: float) =
        ({ Height = height
           Width = width
           RX = rx
           RY = ry
           X = x
           Y = y
           Style = style }: RectElement)
        |> Element.Rect

    let circle (style: Style) (cx: float) (cy: float) (r: float) =
        ({ CX = cx
           CY = cy
           R = r
           Style = style }: CircleElement)
        |> Element.Circle

    let ellipse (style: Style) height width cx cy rx ry =
        ({ Height = height
           Width = width
           CX = cx
           CY = cy
           RX = rx
           RY = ry
           Style = style }: EllipseElement)
        |> Element.Ellipse

    let line style x1 y1 x2 y2 =
        ({ X1 = x1
           X2 = x2
           Y1 = y1
           Y2 = y2
           Style = style }: LineElement)
        |> Element.Line

    let polygon style points =
        ({ Points = points; Style = style }: PolygonElement)
        |> Element.Polygon

    let polyline style points =
        ({ Points = points; Style = style }: PolylineElement)
        |> Element.Polyline

    let path style commands =
        ({ Commands = commands; Style = style }: PathElement)
        |> Element.Path

    let text style x y contents =
        ({ X = x
           Y = y
           Value = contents
           Style = style }: TextElement)
        |> Element.Text

    let m point =
        ({ Point = point; IsRelative = true }: MoveToCommand)
        |> PathCommand.MoveTo

    let M point =
        ({ Point = point; IsRelative = false }: MoveToCommand)
        |> PathCommand.MoveTo

    let l point =
        ({ Point = point; IsRelative = true }: LineToCommand)
        |> PathCommand.LineTo

    let L point =
        ({ Point = point; IsRelative = false }: LineToCommand)
        |> PathCommand.LineTo

    let h x =
        ({ X = x; IsRelative = false }: HorizontalLineToCommand)
        |> PathCommand.HorizontalLineTo

    let H x =
        ({ X = x; IsRelative = true }: HorizontalLineToCommand)
        |> PathCommand.HorizontalLineTo

    let v y =
        ({ Y = y; IsRelative = false }: VerticalLineToCommand)
        |> PathCommand.VerticalLineTo

    let V y =
        ({ Y = y; IsRelative = true }: VerticalLineToCommand)
        |> PathCommand.VerticalLineTo

    let c cp1 cp2 point =
        ({ ControlPoint1 = cp1
           ControlPoint2 = cp2
           FinalPoint = point
           IsRelative = false }: CurveToCommand)
        |> PathCommand.CurveTo

    let C cp1 cp2 point =
        ({ ControlPoint1 = cp1
           ControlPoint2 = cp2
           FinalPoint = point
           IsRelative = true }: CurveToCommand)
        |> PathCommand.CurveTo

    let s cp2 point =
        ({ ControlPoint2 = cp2
           FinalPoint = point
           IsRelative = false }: SmoothCurveToCommand)
        |> PathCommand.SmoothCurveTo

    let S cp2 point =
        ({ ControlPoint2 = cp2
           FinalPoint = point
           IsRelative = true }: SmoothCurveToCommand)
        |> PathCommand.SmoothCurveTo

    let q cp1 point =
        ({ ControlPoint1 = cp1
           FinalPoint = point
           IsRelative = false }: QuadraticBezierCurveToCommand)
        |> PathCommand.QuadraticBezierCurveTo

    let Q cp1 point =
        ({ ControlPoint1 = cp1
           FinalPoint = point
           IsRelative = true }: QuadraticBezierCurveToCommand)
        |> PathCommand.QuadraticBezierCurveTo

    let t point =
        ({ Point = point; IsRelative = false }: SmoothQuadraticBezierCurveToCommand)
        |> PathCommand.SmoothQuadraticBezierCurveTo

    let T point =
        ({ Point = point; IsRelative = true }: SmoothQuadraticBezierCurveToCommand)
        |> PathCommand.SmoothQuadraticBezierCurveTo

    let a radiiPoint xRotation largeArcFlag sweepFlag point =
        ({ RadiiPoint = radiiPoint
           XRotation = xRotation
           LargeArcFlag = largeArcFlag
           SweepFlag = sweepFlag
           FinalPoint = point
           IsRelative = true }: EllipticalArcCommand)
        |> PathCommand.EllipticalArc

    let A radiiPoint xRotation largeArcFlag sweepFlag point =
        ({ RadiiPoint = radiiPoint
           XRotation = xRotation
           LargeArcFlag = largeArcFlag
           SweepFlag = sweepFlag
           FinalPoint = point
           IsRelative = false }: EllipticalArcCommand)
        |> PathCommand.EllipticalArc

    let z = PathCommand.ClosePath
    
    let literal value = TextType.Literal value

    let tspan style x y value =
        ({ X = x
           Y = y
           Value = value
           Style = style }: TextSpan)
