namespace FSVG.Diagrams.FlowCharts

// No warning needed for use of DiagramNode and Diagram,
// they are only used for internal purposes here.
#nowarn "10001"

module Definitions =

    open FSVG.Diagrams.Common.Shared

    type FlowChartNode =
        { Id: string
          Class: FlowChartNodeClass }

        member fcn.ToNode() =
            ({ Id = fcn.Id
               Connections = []
               Classes = [ fcn.Class.Serialize() ]
               Properties = Map.empty }
            : Definitions.DiagramNode)

    and FlowChartNodeConnection =
        { ToId: string }

        member fnc.ToNodeConnection() =
            Definitions.DiagramNodeConnection.Create(fnc.ToId)

    and [<RequireQualifiedAccess>] FlowChartNodeClass =
        | Rectangle
        | Unknown of Name: string

        static member TryDeserialize(str: string) =
            match str.ToLower() with
            | "rectangle" -> Some FlowChartNodeClass.Rectangle
            | _ -> None

        static member Deserialize(str: string) =
            FlowChartNodeClass.TryDeserialize str
            |> Option.defaultValue (FlowChartNodeClass.Unknown str)

        member fnc.Serialize() =
            match fnc with
            | Rectangle -> "rectangle"
            | Unknown v -> v
