namespace FSVG.Diagrams.Common.Shared

// No warning needed for use of DiagramNode and Diagram
#nowarn "10001"

module Rendering =

    open FSVG.Diagrams.Common.Shared

    [<RequireQualifiedAccess>]
    type RenderingMode = | Grid

    type RendererSettings = Grid of GridRendererSettings

    and GridRendererSettings =
        { Rows: GridRow list }

        member grs.RowCount() = grs.Rows.Length

        member grs.ColumnCount() =
            grs.Rows |> List.maxBy (fun r -> r.ColumnCount()) |> (fun r -> r.ColumnCount())

    and GridRow =

        { Order: int
          Nodes: GridNode list }

        member gr.ColumnCount() = gr.Nodes.Length

    and GridNode =
        { Node: Definitions.DiagramNode
          Column: int }

    type  
